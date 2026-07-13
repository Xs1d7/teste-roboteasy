import type { ChatMessage } from '../types/api'
import router from '../router'

export function notificationsSupported(): boolean {
  return typeof window !== 'undefined' && 'Notification' in window
}

export function currentPermission(): NotificationPermission | 'unsupported' {
  if (!notificationsSupported()) return 'unsupported'
  return Notification.permission
}

export async function ensureNotificationPermission(): Promise<NotificationPermission | 'unsupported'> {
  if (!notificationsSupported()) return 'unsupported'
  if (Notification.permission === 'granted' || Notification.permission === 'denied') {
    return Notification.permission
  }
  try {
    return await Notification.requestPermission()
  } catch {
    return Notification.permission
  }
}

function truncate(text: string, max = 120): string {
  const t = text.trim()
  if (t.length <= max) return t
  return `${t.slice(0, max - 1)}…`
}

/** Notifica so quando a aba esta em segundo plano ou o usuario nao esta na conversa. */
export function maybeNotifyIncoming(
  msg: ChatMessage,
  meId: string | null,
  activePeerId: string | null
): void {
  if (!notificationsSupported() || Notification.permission !== 'granted') return
  if (!meId) return

  const me = meId.toLowerCase()
  const from = String(msg.fromUserId).toLowerCase()
  const to = String(msg.toUserId).toLowerCase()

  if (from === me) return
  if (to !== me) return

  const viewingPeer =
    document.visibilityState === 'visible' &&
    activePeerId != null &&
    from === activePeerId.toLowerCase()

  if (viewingPeer) return

  const n = new Notification(`${msg.fromUsername} — Roboteasy`, {
    body: truncate(msg.content),
    tag: `chat-${msg.fromUserId}`,
    icon: '/favicon.svg'
  })

  n.onclick = () => {
    window.focus()
    void router.push({
      name: 'chat',
      params: { userId: msg.fromUserId },
      query: { name: msg.fromUsername }
    })
    n.close()
  }
}
