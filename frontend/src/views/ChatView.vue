<template>
  <div class="shell chat-shell">
    <header class="topbar">
      <div>
        <button class="btn ghost back" @click="router.push('/users')">Voltar</button>
        <div class="brand peer">{{ peerName }}</div>
      </div>
      <div class="meta">voce: {{ auth.username }}</div>
    </header>

    <section class="panel thread">
      <div ref="scroller" class="messages">
        <p v-if="loading" class="empty">Carregando historico...</p>
        <p v-else-if="messages.length === 0" class="empty">Nenhuma mensagem ainda. Manda um oi.</p>

        <div
          v-for="m in messages"
          :key="m.id + m.sentAt"
          class="bubble"
          :class="{ mine: isMine(m) }"
        >
          <div class="content">{{ m.content }}</div>
          <div class="time">{{ formatTime(m.sentAt) }}</div>
        </div>
      </div>

      <form class="composer" @submit.prevent="enviar">
        <input
          v-model="draft"
          placeholder="Escreva uma mensagem..."
          maxlength="2000"
          autocomplete="off"
        />
        <button class="btn" type="submit" :disabled="!draft.trim() || sending">Enviar</button>
      </form>
      <p v-if="error" class="error pad">{{ error }}</p>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useChatStore } from '../stores/chat'
import type { ChatMessage } from '../types/api'

const auth = useAuthStore()
const chat = useChatStore()
const route = useRoute()
const router = useRouter()

const peerId = computed(() => String(route.params.userId))
const peerName = computed(() => {
  const name = route.query.name
  return typeof name === 'string' && name.length > 0 ? name : 'Usuario'
})

const messages = ref<ChatMessage[]>([])
const draft = ref('')
const loading = ref(true)
const sending = ref(false)
const error = ref('')
const scroller = ref<HTMLElement | null>(null)
let off: (() => void) | null = null

function isMine(m: ChatMessage) {
  return String(m.fromUserId).toLowerCase() === String(auth.userId).toLowerCase()
}

function sameThread(m: ChatMessage) {
  const a = String(m.fromUserId).toLowerCase()
  const b = String(m.toUserId).toLowerCase()
  const me = String(auth.userId).toLowerCase()
  const peer = peerId.value.toLowerCase()
  return (a === me && b === peer) || (a === peer && b === me)
}

function formatTime(value: string) {
  try {
    return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } catch {
    return ''
  }
}

async function scrollBottom() {
  await nextTick()
  if (scroller.value) scroller.value.scrollTop = scroller.value.scrollHeight
}

onMounted(async () => {
  try {
    await chat.connect()
    messages.value = await chat.loadHistory(peerId.value)
  } catch {
    error.value = 'Falha ao carregar historico.'
  } finally {
    loading.value = false
    await scrollBottom()
  }

  off = chat.onMessage((msg) => {
    if (!sameThread(msg)) return
    if (messages.value.some(x => x.id === msg.id)) return
    messages.value.push(msg)
    void scrollBottom()
  })
})

onUnmounted(() => {
  if (off) off()
})

async function enviar() {
  const text = draft.value.trim()
  if (!text) return
  sending.value = true
  error.value = ''
  try {
    await chat.sendMessage(peerId.value, peerName.value, text)
    draft.value = ''
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Erro ao enviar.'
  } finally {
    sending.value = false
  }
}
</script>

<style scoped>
.chat-shell { max-width: 820px; }

.topbar { align-items: flex-end; }

.back {
  margin-bottom: 0.45rem;
  padding: 0.35rem 0.7rem;
  font-size: 0.85rem;
}

.peer { margin-top: 0.15rem; }

.thread {
  display: flex;
  flex-direction: column;
  min-height: 70vh;
  overflow: hidden;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 1.1rem;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.empty {
  color: var(--muted);
  margin: 1rem 0;
}

.bubble {
  max-width: min(75%, 420px);
  padding: 0.7rem 0.85rem;
  border-radius: 12px;
  background: var(--them);
  border: 1px solid var(--line);
  align-self: flex-start;
}

.bubble.mine {
  align-self: flex-end;
  background: var(--me);
  border-color: #2f6d5d;
}

.content {
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.4;
}

.time {
  margin-top: 0.35rem;
  font-size: 0.72rem;
  color: var(--muted);
  text-align: right;
}

.composer {
  display: flex;
  gap: 0.6rem;
  padding: 0.9rem;
  border-top: 1px solid var(--line);
  background: rgba(0, 0, 0, 0.15);
}

.composer input {
  flex: 1;
  background: #0d1f1b;
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 0.75rem 0.85rem;
  color: var(--text);
  outline: none;
}

.composer input:focus { border-color: var(--accent-2); }

.pad { padding: 0 0.9rem 0.8rem; }
</style>
