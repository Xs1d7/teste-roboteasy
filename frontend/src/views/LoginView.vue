<template>
  <div class="auth-page flex min-h-dvh items-center justify-center p-6">
    <Card class="w-full max-w-md border-border/80 shadow-xl shadow-black/20">
      <CardHeader>
        <p class="text-xs font-semibold tracking-[0.14em] text-primary uppercase">
          Roboteasy
        </p>
        <CardTitle class="font-heading text-2xl">
          {{ title }}
        </CardTitle>
        <CardDescription>
          {{ description }}
        </CardDescription>
      </CardHeader>

      <CardContent>
        <!-- Passo: login / cadastro -->
        <form v-if="step === 'auth'" class="space-y-4" @submit.prevent="submit">
          <div class="space-y-2">
            <Label for="user">Usuario</Label>
            <Input
              id="user"
              v-model.trim="username"
              autocomplete="username"
              required
              minlength="3"
              class="h-10"
            />
          </div>
          <div class="space-y-2">
            <Label for="pass">Senha</Label>
            <Input
              id="pass"
              v-model="password"
              type="password"
              autocomplete="current-password"
              required
              minlength="4"
              class="h-10"
            />
          </div>

          <Alert v-if="error" variant="destructive">
            <AlertDescription>{{ error }}</AlertDescription>
          </Alert>

          <div class="flex flex-wrap gap-2 pt-1">
            <Button type="submit" size="lg" class="min-w-28" :disabled="loading">
              <LoaderCircle v-if="loading" class="animate-spin" />
              {{ loading ? 'Aguarde...' : mode === 'login' ? 'Login' : 'Cadastrar' }}
            </Button>
            <Button
              type="button"
              variant="outline"
              size="lg"
              :disabled="loading"
              @click="toggle"
            >
              {{ mode === 'login' ? 'Cadastrar-se' : 'Ja tenho conta' }}
            </Button>
          </div>
        </form>

        <!-- Passo: avatar opcional apos cadastro -->
        <div v-else class="space-y-5">
          <div class="flex flex-col items-center gap-4">
            <Avatar size="lg" class="size-24">
              <AvatarImage v-if="previewUrl" :src="previewUrl" alt="Pré-visualizacao do avatar" />
              <AvatarFallback class="bg-secondary text-2xl font-semibold text-secondary-foreground">
                {{ initials(auth.username || username) }}
              </AvatarFallback>
            </Avatar>

            <div class="w-full space-y-2">
              <Label for="avatar">Escolher imagem</Label>
              <Input
                id="avatar"
                type="file"
                accept="image/jpeg,image/png,image/webp,image/gif"
                class="h-10 cursor-pointer file:mr-3 file:rounded-md file:border-0 file:bg-secondary file:px-3 file:py-1.5 file:text-sm file:font-medium"
                :disabled="loading"
                @change="onFileChange"
              />
              <p class="text-xs text-muted-foreground">
                JPEG, PNG, WebP ou GIF · maximo 2 MB
              </p>
            </div>
          </div>

          <Alert v-if="error" variant="destructive">
            <AlertDescription>{{ error }}</AlertDescription>
          </Alert>

          <div class="flex flex-wrap gap-2 pt-1">
            <Button
              type="button"
              size="lg"
              class="min-w-28"
              :disabled="loading || !selectedFile"
              @click="uploadAvatar"
            >
              <LoaderCircle v-if="loading" class="animate-spin" />
              {{ loading ? 'Enviando...' : 'Salvar avatar' }}
            </Button>
            <Button
              type="button"
              variant="outline"
              size="lg"
              :disabled="loading"
              @click="skipAvatar"
            >
              Pular por agora
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { LoaderCircle } from '@lucide/vue'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { initials } from '@/lib/initials'
import { useAuthStore } from '@/stores/auth'
import type { ApiErrorBody } from '@/types/api'

const auth = useAuthStore()
const router = useRouter()

type AuthMode = 'login' | 'register'
type Step = 'auth' | 'avatar'

const mode = ref<AuthMode>('login')
const step = ref<Step>('auth')
const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const selectedFile = ref<File | null>(null)
const previewUrl = ref<string | null>(null)

const title = computed(() => {
  if (step.value === 'avatar') return 'Adicionar avatar'
  return mode.value === 'login' ? 'Entrar' : 'Criar conta'
})

const description = computed(() => {
  if (step.value === 'avatar') {
    return 'Deseja adicionar uma foto de perfil? Voce pode pular e fazer isso depois.'
  }
  return mode.value === 'login'
    ? 'Acesse sua conta para continuar a conversa.'
    : 'Crie um usuario e comece a falar em segundos.'
})

function toggle() {
  mode.value = mode.value === 'login' ? 'register' : 'login'
  error.value = ''
}

function clearPreview() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = null
  }
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0] ?? null
  error.value = ''
  clearPreview()
  selectedFile.value = file
  if (file) previewUrl.value = URL.createObjectURL(file)
}

async function submit() {
  error.value = ''
  loading.value = true
  try {
    if (mode.value === 'login') {
      await auth.login(username.value, password.value)
      await router.push('/users')
      return
    }

    await auth.register(username.value, password.value)
    step.value = 'avatar'
    selectedFile.value = null
    clearPreview()
  } catch (e: unknown) {
    if (axios.isAxiosError<ApiErrorBody>(e)) {
      error.value = e.response?.data?.message || 'Nao foi possivel autenticar.'
    } else {
      error.value = 'Nao foi possivel autenticar.'
    }
  } finally {
    loading.value = false
  }
}

async function uploadAvatar() {
  if (!selectedFile.value) return
  error.value = ''
  loading.value = true
  try {
    await auth.uploadAvatar(selectedFile.value)
    await router.push('/users')
  } catch (e: unknown) {
    if (axios.isAxiosError<ApiErrorBody>(e)) {
      error.value = e.response?.data?.message || 'Nao foi possivel enviar o avatar.'
    } else {
      error.value = 'Nao foi possivel enviar o avatar.'
    }
  } finally {
    loading.value = false
  }
}

async function skipAvatar() {
  await router.push('/users')
}

onBeforeUnmount(() => clearPreview())
</script>
