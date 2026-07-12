<template>
  <div class="shell">
    <header class="topbar">
      <div>
        <div class="brand">Roboteasy</div>
        <div class="meta">Logado como {{ auth.username }}</div>
      </div>
      <div class="right">
        <span class="status" :class="{ on: chat.connected }">
          {{ chat.connected ? 'online' : 'conectando...' }}
        </span>
        <button class="btn ghost" @click="sair">Sair</button>
      </div>
    </header>

    <section class="panel list-wrap">
      <div class="list-head">
        <h2>Usuarios disponiveis</h2>
        <button class="btn ghost" @click="chat.refreshOnline()">Atualizar</button>
      </div>

      <p v-if="others.length === 0" class="empty">
        Ninguem online no momento. Abra outra aba com outro usuario pra testar.
      </p>

      <ul v-else class="user-list">
        <li v-for="u in others" :key="u.userId">
          <button class="user-row" @click="abrir(u)">
            <span class="dot"></span>
            <span class="name">{{ u.username }}</span>
            <span class="cta">conversar</span>
          </button>
        </li>
      </ul>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
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
.right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

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

.user-row {
  width: 100%;
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 0.75rem;
  padding: 0.95rem 0.35rem;
  background: transparent;
  border: 0;
  border-top: 1px solid var(--line);
  color: var(--text);
  cursor: pointer;
  text-align: left;
}

.user-row:hover .name { color: var(--accent); }
.user-row:hover .cta { opacity: 1; }

.dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: var(--accent);
  box-shadow: 0 0 0 4px rgba(61, 214, 140, 0.15);
}

.name { font-weight: 600; }

.cta {
  color: var(--muted);
  font-size: 0.85rem;
  opacity: 0.7;
}
</style>
