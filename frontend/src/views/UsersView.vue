<template>
  <AppShell
    title="Roboteasy"
    :subtitle="`Logado como ${auth.username}`"
  >
    <template #trailing>
      <span class="status" :class="{ on: chat.connected }">
        {{ chat.connected ? 'online' : 'conectando...' }}
      </span>
      <button class="btn ghost" type="button" @click="sair">Sair</button>
    </template>

    <section class="panel list-wrap">
      <div class="list-head">
        <h2>Usuarios disponiveis</h2>
        <button class="btn ghost" type="button" @click="chat.refreshOnline()">Atualizar</button>
      </div>

      <p v-if="others.length === 0" class="empty">
        Ninguem online no momento. Abra outra aba com outro usuario pra testar.
      </p>

      <ul v-else class="user-list">
        <UserRow
          v-for="u in others"
          :key="u.userId"
          :username="u.username"
          @select="abrir(u)"
        />
      </ul>
    </section>
  </AppShell>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import AppShell from '../components/AppShell.vue'
import UserRow from '../components/UserRow.vue'
import { useAuthStore } from '../stores/auth'
import { useChatStore } from '../stores/chat'
import type { OnlineUser } from '../types/api'

const auth = useAuthStore()
const chat = useChatStore()
const router = useRouter()

const others = computed(() =>
  chat.online.filter(u => String(u.userId).toLowerCase() !== String(auth.userId).toLowerCase())
)

onMounted(async () => {
  try {
    await chat.connect()
  } catch (e) {
    console.error(e)
  }
})

function abrir(u: OnlineUser) {
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
}

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
