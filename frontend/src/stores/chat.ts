import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'
import { useAuthStore } from './auth'
import type { ChatMessage, OnlineUser } from '../types/api'
import {
  currentPermission,
  ensureNotificationPermission,
  maybeNotifyIncoming
} from '../lib/notifications'

export const useChatStore = defineStore('chat', () => {
  const connection = ref<signalR.HubConnection | null>(null)
  const online = ref<OnlineUser[]>([])
  const connected = ref(false)
  /** Conversa aberta agora (para nao notificar se ja esta olhando). */
  const activePeerId = ref<string | null>(null)
  const notificationPermission = ref(currentPermission())

  function setActivePeer(userId: string | null) {
    activePeerId.value = userId
  }

  async function enableNotifications() {
    notificationPermission.value = await ensureNotificationPermission()
  }

  async function connect() {
    const auth = useAuthStore()
    if (!auth.token) return
    if (connection.value) return

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
      maybeNotifyIncoming(msg, auth.userId, activePeerId.value)
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

    // pede permissao na primeira conexao (se ainda default)
    if (notificationPermission.value === 'default') {
      void enableNotifications()
    }
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
  }

  return {
    online,
    connected,
    activePeerId,
    notificationPermission,
    connect,
    disconnect,
    refreshOnline,
    loadHistory,
    sendMessage,
    onMessage,
    setActivePeer,
    enableNotifications
  }
})
