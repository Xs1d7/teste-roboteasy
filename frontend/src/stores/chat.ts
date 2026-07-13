import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'
import { useAuthStore } from './auth'
import type { ChatMessage, DirectoryUser, OnlineUser, TypingEvent } from '../types/api'
import {
  currentPermission,
  ensureNotificationPermission,
  maybeNotifyIncoming
} from '../lib/notifications'

function normId(id: string | number): string {
  return String(id).toLowerCase()
}

export const useChatStore = defineStore('chat', () => {
  const connection = ref<signalR.HubConnection | null>(null)
  const online = ref<OnlineUser[]>([])
  const connected = ref(false)
  /** userId(lowercase) -> avatarUrl */
  const avatarByUserId = ref<Record<string, string | null>>({})
  /** Conversa aberta agora. */
  const activePeerId = ref<string | null>(null)
  /** Contagem de nao lidas por userId (lowercase). */
  const unreadByUser = ref<Record<string, number>>({})
  /** Ultima mensagem recebida (preview na lista). */
  const lastPreviewByUser = ref<Record<string, string>>({})
  /** Quem esta digitando pra mim agora (userId lowercase). */
  const typingByUser = ref<Record<string, boolean>>({})
  /** Evita badge +2 se o hub entregar o mesmo id mais de uma vez. */
  const seenIncomingIds = new Set<string>()
  const notificationPermission = ref(currentPermission())
  const typingClearTimers = new Map<string, ReturnType<typeof setTimeout>>()

  const totalUnread = computed(() =>
    Object.values(unreadByUser.value).reduce((a, n) => a + n, 0)
  )

  function syncDocumentTitle() {
    const n = totalUnread.value
    document.title = n > 0 ? `(${n}) Roboteasy` : 'Roboteasy'
  }

  function avatarOf(userId: string): string | null {
    return avatarByUserId.value[normId(userId)] ?? null
  }

  function withAvatars(list: OnlineUser[]): OnlineUser[] {
    return list.map(u => ({
      ...u,
      avatarUrl: avatarOf(u.userId) ?? u.avatarUrl ?? null
    }))
  }

  function setActivePeer(userId: string | null) {
    activePeerId.value = userId
    if (userId) markRead(userId)
  }

  function markRead(userId: string) {
    const key = normId(userId)
    if (!unreadByUser.value[key]) return
    const next = { ...unreadByUser.value }
    delete next[key]
    unreadByUser.value = next
    syncDocumentTitle()
  }

  function unreadCount(userId: string): number {
    return unreadByUser.value[normId(userId)] ?? 0
  }

  function lastPreview(userId: string): string {
    return lastPreviewByUser.value[normId(userId)] ?? ''
  }

  function isPeerTyping(userId: string): boolean {
    return !!typingByUser.value[normId(userId)]
  }

  function applyTyping(evt: TypingEvent) {
    const key = normId(evt.userId)
    const prev = typingClearTimers.get(key)
    if (prev) clearTimeout(prev)

    if (!evt.isTyping) {
      const next = { ...typingByUser.value }
      delete next[key]
      typingByUser.value = next
      typingClearTimers.delete(key)
      return
    }

    typingByUser.value = { ...typingByUser.value, [key]: true }
    // fallback se o peer sumir sem mandar isTyping=false
    typingClearTimers.set(
      key,
      setTimeout(() => {
        const next = { ...typingByUser.value }
        delete next[key]
        typingByUser.value = next
        typingClearTimers.delete(key)
      }, 3000)
    )
  }

  function registerIncoming(msg: ChatMessage) {
    const auth = useAuthStore()
    if (!auth.userId) return

    const msgId = String(msg.id ?? '')
    if (msgId) {
      if (seenIncomingIds.has(msgId)) return
      seenIncomingIds.add(msgId)
      if (seenIncomingIds.size > 500) {
        const first = seenIncomingIds.values().next().value
        if (first) seenIncomingIds.delete(first)
      }
    }

    const me = normId(auth.userId)
    const from = normId(msg.fromUserId)
    const to = normId(msg.toUserId)

    if (from === me) return
    if (to !== me) return

    // mensagem chega = nao esta mais "digitando"
    applyTyping({ userId: from, username: msg.fromUsername, isTyping: false })

    const viewing =
      activePeerId.value != null &&
      from === normId(activePeerId.value)

    if (viewing) return

    unreadByUser.value = {
      ...unreadByUser.value,
      [from]: (unreadByUser.value[from] ?? 0) + 1
    }
    lastPreviewByUser.value = {
      ...lastPreviewByUser.value,
      [from]: msg.content.trim()
    }
    syncDocumentTitle()
    maybeNotifyIncoming(msg, auth.userId, activePeerId.value)
  }

  async function enableNotifications() {
    notificationPermission.value = await ensureNotificationPermission()
    return notificationPermission.value
  }

  function refreshNotificationPermission() {
    notificationPermission.value = currentPermission()
  }

  async function refreshDirectory() {
    const auth = useAuthStore()
    try {
      const { data } = await axios.get<DirectoryUser[]>('/api/users', {
        headers: auth.authHeader()
      })
      const map: Record<string, string | null> = {}
      for (const u of data ?? []) {
        map[normId(u.id)] = u.avatarUrl ?? null
      }
      avatarByUserId.value = map
      online.value = withAvatars(online.value)
    } catch {
      // lista online ainda funciona sem avatares
    }
  }

  async function connect() {
    const auth = useAuthStore()
    if (!auth.token) return
    if (connection.value) return

    refreshNotificationPermission()

    const hub = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', {
        accessTokenFactory: () => auth.token ?? ''
      })
      .withAutomaticReconnect()
      .build()

    hub.on('OnlineUsers', (list: OnlineUser[]) => {
      online.value = withAvatars(list ?? [])
    })

    hub.on('PresenceChanged', async () => {
      await refreshOnline()
    })

    hub.on('ReceiveMessage', (msg: ChatMessage) => {
      registerIncoming(msg)
    })

    hub.on('UserTyping', (evt: TypingEvent) => {
      applyTyping(evt)
    })

    hub.onclose(() => { connected.value = false })
    hub.onreconnected(async () => {
      connected.value = true
      await refreshDirectory()
      await refreshOnline()
    })

    await hub.start()
    connection.value = hub
    connected.value = true
    await refreshDirectory()
    await refreshOnline()
  }

  async function refreshOnline() {
    const auth = useAuthStore()
    try {
      const { data } = await axios.get<OnlineUser[]>('/api/users/online', {
        headers: auth.authHeader()
      })
      online.value = withAvatars(data)
    } catch {
      // hub pode ja ter mandado a lista
    }
  }

  async function loadHistory(withUserId: string): Promise<ChatMessage[]> {
    const auth = useAuthStore()
    const { data } = await axios.get<ChatMessage[]>('/api/messages', {
      params: { with: withUserId },
      headers: auth.authHeader()
    })
    return data
  }

  async function sendMessage(toUserId: string, toUsername: string, content: string) {
    if (!connection.value) throw new Error('Sem conexao')
    await notifyTyping(toUserId, false)
    await connection.value.invoke('SendMessage', toUserId, toUsername, content)
  }

  async function notifyTyping(toUserId: string, isTyping: boolean) {
    if (!connection.value || connection.value.state !== signalR.HubConnectionState.Connected) return
    try {
      await connection.value.invoke('Typing', toUserId, isTyping)
    } catch {
      // tipagem e best-effort
    }
  }

  function onMessage(handler: (msg: ChatMessage) => void): () => void {
    if (!connection.value) return () => {}
    connection.value.on('ReceiveMessage', handler)
    return () => connection.value?.off('ReceiveMessage', handler)
  }

  async function disconnect() {
    if (connection.value) {
      await connection.value.stop()
    }
    connection.value = null
    connected.value = false
    online.value = []
    avatarByUserId.value = {}
    activePeerId.value = null
    unreadByUser.value = {}
    lastPreviewByUser.value = {}
    typingByUser.value = {}
    for (const t of typingClearTimers.values()) clearTimeout(t)
    typingClearTimers.clear()
    seenIncomingIds.clear()
    syncDocumentTitle()
  }

  return {
    online,
    connected,
    activePeerId,
    unreadByUser,
    lastPreviewByUser,
    typingByUser,
    totalUnread,
    notificationPermission,
    connect,
    disconnect,
    refreshOnline,
    refreshDirectory,
    loadHistory,
    sendMessage,
    notifyTyping,
    onMessage,
    setActivePeer,
    markRead,
    unreadCount,
    lastPreview,
    avatarOf,
    isPeerTyping,
    enableNotifications,
    refreshNotificationPermission
  }
})
