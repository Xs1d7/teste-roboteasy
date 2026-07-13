import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'
import { useAuthStore } from './auth'
import type { ChatMessage, OnlineUser } from '../types/api'
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
  /** Conversa aberta agora. */
  const activePeerId = ref<string | null>(null)
  /** Contagem de nao lidas por userId (lowercase). */
  const unreadByUser = ref<Record<string, number>>({})
  /** Ultima mensagem recebida (preview na lista). */
  const lastPreviewByUser = ref<Record<string, string>>({})
  const notificationPermission = ref(currentPermission())

  const totalUnread = computed(() =>
    Object.values(unreadByUser.value).reduce((a, n) => a + n, 0)
  )

  function syncDocumentTitle() {
    const n = totalUnread.value
    document.title = n > 0 ? `(${n}) Roboteasy` : 'Roboteasy'
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

  function registerIncoming(msg: ChatMessage) {
    const auth = useAuthStore()
    if (!auth.userId) return

    const me = normId(auth.userId)
    const from = normId(msg.fromUserId)
    const to = normId(msg.toUserId)

    if (from === me) return
    if (to !== me) return

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
      online.value = list ?? []
    })

    hub.on('PresenceChanged', async () => {
      await refreshOnline()
    })

    hub.on('ReceiveMessage', (msg: ChatMessage) => {
      registerIncoming(msg)
    })

    hub.onclose(() => { connected.value = false })
    hub.onreconnected(async () => {
      connected.value = true
      await refreshOnline()
    })

    await hub.start()
    connection.value = hub
    connected.value = true
    await refreshOnline()
  }

  async function refreshOnline() {
    const auth = useAuthStore()
    try {
      const { data } = await axios.get<OnlineUser[]>('/api/users/online', {
        headers: auth.authHeader()
      })
      online.value = data
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
    await connection.value.invoke('SendMessage', toUserId, toUsername, content)
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
    activePeerId.value = null
    unreadByUser.value = {}
    lastPreviewByUser.value = {}
    syncDocumentTitle()
  }

  return {
    online,
    connected,
    activePeerId,
    unreadByUser,
    lastPreviewByUser,
    totalUnread,
    notificationPermission,
    connect,
    disconnect,
    refreshOnline,
    loadHistory,
    sendMessage,
    onMessage,
    setActivePeer,
    markRead,
    unreadCount,
    lastPreview,
    enableNotifications,
    refreshNotificationPermission
  }
})
