<template>
  <AppShell
    title="Roboteasy"
    :subtitle="subtitle"
  >
    <template #trailing>
      <span class="status" :class="{ on: chat.connected }">
        {{ chat.connected ? 'online' : 'conectando...' }}
      </span>
      <button class="btn ghost" type="button" @click="sair">Sair</button>
    </template>

    <section class="panel list-wrap">
      <div class="list-head">
        <h2>
          Usuarios disponiveis
          <span v-if="chat.totalUnread > 0" class="head-badge">{{ chat.totalUnread }}</span>
        </h2>
        <div class="list-actions">
          <button
            v-if="chat.notificationPermission === 'default'"
            class="btn ghost"
            type="button"
            @click="pedirNotificacoes"
          >
            Ativar notificacoes
          </button>
          <button class="btn ghost" type="button" @click="chat.refreshOnline()">Atualizar</button>
        </div>
      </div>

      <p v-if="notifHint" class="hint" :class="{ warn: notifHintWarn }">{{ notifHint }}</p>

      <p v-if="others.length === 0" class="empty">
        Ninguem online no momento. Abra outra aba com outro usuario pra testar.
      </p>

      <ul v-else class="user-list">
        <UserRow
          v-for="u in others"
          :key="u.userId"
          :username="u.username"
          :unread="chat.unreadCount(u.userId)"
          :preview="chat.lastPreview(u.userId)"
          @select="abrir(u)"
        />
      </ul>
    </section>
  </AppShell>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppShell from '../components/AppShell.vue'
import UserRow from '../components/UserRow.vue'
import { useAuthStore } from '../stores/auth'
import { useChatStore } from '../stores/chat'
import type { OnlineUser } from '../types/api'

const auth = useAuthStore()
const chat = useChatStore()
const router = useRouter()

const notifHint = ref('')
const notifHintWarn = ref(false)

const subtitle = computed(() => {
  const base = `Logado como ${auth.username}`
  if (chat.totalUnread > 0) {
    return `${base} · ${chat.totalUnread} nao lida${chat.totalUnread === 1 ? '' : 's'}`
  }
  return base
})

const others = computed(() => {
  const me = String(auth.userId).toLowerCase()
  return [...chat.online]
    .filter(u => String(u.userId).toLowerCase() !== me)
    .sort((a, b) => {
      const ua = chat.unreadCount(a.userId)
      const ub = chat.unreadCount(b.userId)
      if (ua !== ub) return ub - ua
      return a.username.localeCompare(b.username)
    })
})

onMounted(async () => {
  chat.refreshNotificationPermission()
  if (chat.notificationPermission === 'denied') {
    notifHint.value = 'Notificacoes do sistema bloqueadas neste navegador. O indicador de nao lidas na lista continua funcionando.'
    notifHintWarn.value = true
  }
  try {
    await chat.connect()
  } catch (e) {
    console.error(e)
  }
})

async function pedirNotificacoes() {
  const result = await chat.enableNotifications()
  if (result === 'granted') {
    notifHint.value = 'Notificacoes do sistema ativadas.'
    notifHintWarn.value = false
  } else if (result === 'denied') {
    notifHint.value = 'Permissao negada. Voce ainda ve mensagens nao lidas na lista abaixo.'
    notifHintWarn.value = true
  } else if (result === 'unsupported') {
    notifHint.value = 'Este navegador nao suporta Notification API.'
    notifHintWarn.value = true
  }
}

function abrir(u: OnlineUser) {
  chat.markRead(u.userId)
  void router.push({
    name: 'chat',
    params: { userId: u.userId },
    query: { name: u.username }
  })
}

async function sair() {
  await chat.disconnect()
  auth.logout()
  await router.push('/login')
}
</script>

<style scoped>
.status {
  font-size: 0.8rem;
  color: var(--muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.status.on { color: var(--accent); }

.list-wrap { padding: 1.1rem 1.2rem 0.6rem; }

.list-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 0.75rem;
}

.list-head h2 {
  margin: 0;
  font-size: 1.15rem;
  display: flex;
  align-items: center;
  gap: 0.55rem;
}

.head-badge {
  min-width: 1.4rem;
  height: 1.4rem;
  padding: 0 0.4rem;
  border-radius: 999px;
  background: var(--accent);
  color: #042015;
  font-size: 0.75rem;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.list-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.hint {
  margin: 0 0 0.85rem;
  font-size: 0.85rem;
  color: var(--muted);
}

.hint.warn { color: #d4a574; }

.empty {
  color: var(--muted);
  padding: 1.5rem 0.25rem 1.75rem;
}

.user-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
</style>
