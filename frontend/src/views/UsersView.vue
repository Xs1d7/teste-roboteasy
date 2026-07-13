<template>
  <AppShell title="Roboteasy" :subtitle="subtitle">
    <template #leading>
      <button
        type="button"
        class="group relative shrink-0 rounded-full outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        :title="auth.avatarUrl ? 'Alterar foto de perfil' : 'Adicionar foto de perfil'"
        :disabled="avatarLoading"
        @click="abrirSeletorAvatar"
      >
        <Avatar size="default" class="size-10">
          <AvatarImage v-if="auth.avatarUrl" :src="auth.avatarUrl" :alt="auth.username ?? 'avatar'" />
          <AvatarFallback class="bg-secondary text-sm font-semibold text-secondary-foreground">
            {{ initials(auth.username || '?') }}
          </AvatarFallback>
        </Avatar>
        <span
          class="absolute inset-0 flex items-center justify-center rounded-full bg-black/45 text-white opacity-0 transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100"
        >
          <Camera class="size-4" />
        </span>
      </button>
      <input
        ref="avatarInput"
        type="file"
        accept="image/jpeg,image/png,image/webp,image/gif"
        class="sr-only"
        @change="onAvatarChange"
      />
    </template>
    <template #trailing>
      <Button
        variant="outline"
        size="sm"
        type="button"
        :disabled="avatarLoading"
        @click="abrirSeletorAvatar"
      >
        <Camera />
        {{ auth.avatarUrl ? 'Alterar foto' : 'Adicionar foto' }}
      </Button>
      <Badge
        :variant="chat.connected ? 'default' : 'secondary'"
        class="uppercase tracking-wide"
      >
        <span
          :class="cn(
            'mr-1.5 size-1.5 rounded-full',
            chat.connected ? 'bg-primary-foreground' : 'bg-muted-foreground animate-pulse',
          )"
        />
        {{ chat.connected ? 'online' : 'conectando...' }}
      </Badge>
      <Button variant="outline" size="sm" type="button" @click="sair">
        <LogOut />
        Sair
      </Button>
    </template>

    <Alert v-if="avatarHint" :variant="avatarHintWarn ? 'destructive' : 'default'" class="mb-4">
      <AlertDescription>{{ avatarHint }}</AlertDescription>
    </Alert>

    <Card class="overflow-hidden border-border/80 shadow-xl shadow-black/20">
      <CardHeader class="border-b border-border/60 pb-4">
        <div class="flex flex-wrap items-center justify-between gap-3">
          <CardTitle class="font-heading flex items-center gap-2 text-lg">
            usuários disponiveis
            <Badge v-if="chat.totalUnread > 0" class="h-5 min-w-5 justify-center px-1.5">
              {{ chat.totalUnread }}
            </Badge>
          </CardTitle>
          <div class="flex flex-wrap gap-2">
            <Button
              v-if="chat.notificationPermission === 'default'"
              variant="outline"
              size="sm"
              type="button"
              @click="pedirNotificacoes"
            >
              <Bell />
              Ativar notificacoes
            </Button>
            <Button
              variant="outline"
              size="sm"
              type="button"
              @click="atualizar"
            >
              <RefreshCw />
              Atualizar
            </Button>
          </div>
        </div>
      </CardHeader>

      <CardContent class="p-0">
        <Alert
          v-if="notifHint"
          :variant="notifHintWarn ? 'destructive' : 'default'"
          class="m-4"
        >
          <AlertDescription>{{ notifHint }}</AlertDescription>
        </Alert>

        <Empty v-if="others.length === 0" class="border-0 py-14">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <Users />
            </EmptyMedia>
            <EmptyTitle>Ninguem online</EmptyTitle>
            <EmptyDescription>
              Abra outra aba com outro usuario pra testar o chat em tempo real.
            </EmptyDescription>
          </EmptyHeader>
        </Empty>

        <ul v-else class="space-y-0.5 p-2">
          <UserRow
            v-for="u in others"
            :key="u.userId"
            :username="u.username"
            :avatar-url="u.avatarUrl ?? chat.avatarOf(u.userId)"
            :unread="chat.unreadCount(u.userId)"
            :preview="chat.lastPreview(u.userId)"
            @select="abrir(u)"
          />
        </ul>
      </CardContent>
    </Card>
  </AppShell>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { Bell, Camera, LogOut, RefreshCw, Users } from '@lucide/vue'
import AppShell from '@/components/AppShell.vue'
import UserRow from '@/components/UserRow.vue'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle
} from '@/components/ui/card'
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle
} from '@/components/ui/empty'
import { initials } from '@/lib/initials'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { cn } from '@/lib/utils'
import type { ApiErrorBody, OnlineUser } from '@/types/api'

const auth = useAuthStore()
const chat = useChatStore()
const router = useRouter()

const notifHint = ref('')
const notifHintWarn = ref(false)
const avatarInput = ref<HTMLInputElement | null>(null)
const avatarLoading = ref(false)
const avatarHint = ref('')
const avatarHintWarn = ref(false)

function abrirSeletorAvatar() {
  avatarHint.value = ''
  avatarInput.value?.click()
}

async function onAvatarChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return

  avatarLoading.value = true
  avatarHint.value = ''
  try {
    await auth.uploadAvatar(file)
    await chat.refreshDirectory()
    avatarHint.value = 'Foto de perfil atualizada.'
    avatarHintWarn.value = false
  } catch (e: unknown) {
    avatarHintWarn.value = true
    if (axios.isAxiosError<ApiErrorBody>(e)) {
      avatarHint.value = e.response?.data?.message || 'Nao foi possivel enviar a foto.'
    } else {
      avatarHint.value = 'Nao foi possivel enviar a foto.'
    }
  } finally {
    avatarLoading.value = false
  }
}

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

async function atualizar() {
  await chat.refreshDirectory()
  await chat.refreshOnline()
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
