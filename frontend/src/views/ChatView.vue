<template>
  <AppShell
    shell-class="max-w-2xl"
    :title="peerName"
    title-class="font-heading"
  >
    <template #leading>
      <Button
        variant="outline"
        size="sm"
        type="button"
        @click="router.push('/users')"
      >
        <ArrowLeft />
        Voltar
      </Button>
    </template>
    <template #trailing>
      <div class="flex items-center gap-2 text-sm text-muted-foreground">
        <Avatar size="sm" class="size-7">
          <AvatarFallback class="bg-secondary text-[0.65rem] font-semibold">
            {{ initials(auth.username || '?') }}
          </AvatarFallback>
        </Avatar>
        <span class="hidden sm:inline">voce: {{ auth.username }}</span>
      </div>
    </template>

    <Card class="flex min-h-[70vh] flex-col overflow-hidden border-border/80 shadow-xl shadow-black/20">
      <ScrollArea class="flex-1">
        <div ref="scroller" class="flex max-h-[calc(70vh-5.5rem)] flex-col gap-3 overflow-y-auto p-4">
          <Empty v-if="loading" class="border-0 py-10">
            <EmptyHeader>
              <EmptyDescription>Carregando histórico...</EmptyDescription>
            </EmptyHeader>
          </Empty>

          <Empty v-else-if="messages.length === 0" class="border-0 py-10">
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <MessageSquare />
              </EmptyMedia>
              <EmptyTitle>Nenhuma mensagem ainda</EmptyTitle>
              <EmptyDescription>Manda um oi para Começar a conversa.</EmptyDescription>
            </EmptyHeader>
          </Empty>

          <MessageBubble
            v-for="m in messages"
            :key="m.id + m.sentAt"
            :content="m.content"
            :time="formatTime(m.sentAt)"
            :author="m.fromUsername"
            :mine="isMine(m)"
          />
        </div>
      </ScrollArea>

      <Separator />

      <form class="flex items-end gap-2 bg-card/80 p-3" @submit.prevent="enviar">
        <Textarea
          v-model="draft"
          placeholder="Escreva uma mensagem..."
          maxlength="2000"
          autocomplete="off"
          rows="1"
          class="max-h-32 min-h-10 flex-1 resize-none"
          @keydown.enter.exact.prevent="enviar"
        />
        <Button
          type="submit"
          size="lg"
          class="shrink-0"
          :disabled="!draft.trim() || sending"
        >
          <SendHorizontal />
          Enviar
        </Button>
      </form>

      <Alert v-if="error" variant="destructive" class="m-3 mt-0">
        <AlertDescription>{{ error }}</AlertDescription>
      </Alert>
    </Card>
  </AppShell>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, MessageSquare, SendHorizontal } from '@lucide/vue'
import AppShell from '@/components/AppShell.vue'
import MessageBubble from '@/components/MessageBubble.vue'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle
} from '@/components/ui/empty'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Separator } from '@/components/ui/separator'
import { Textarea } from '@/components/ui/textarea'
import { initials } from '@/lib/initials'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import type { ChatMessage } from '@/types/api'

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
  chat.setActivePeer(peerId.value)
  try {
    await chat.connect()
    messages.value = await chat.loadHistory(peerId.value)
  } catch {
    error.value = 'Falha ao carregar histórico.'
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

watch(peerId, (id) => {
  chat.setActivePeer(id)
})

onUnmounted(() => {
  chat.setActivePeer(null)
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
