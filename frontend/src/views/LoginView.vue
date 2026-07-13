<template>
  <div class="auth-page flex min-h-dvh items-center justify-center p-6">
    <Card class="w-full max-w-md border-border/80 shadow-xl shadow-black/20">
      <CardHeader>
        <p class="text-xs font-semibold tracking-[0.14em] text-primary uppercase">
          Roboteasy
        </p>
        <CardTitle class="font-heading text-2xl">
          {{ mode === 'login' ? 'Entrar' : 'Criar conta' }}
        </CardTitle>
        <CardDescription>
          {{
            mode === 'login'
              ? 'Acesse sua conta para continuar a conversa.'
              : 'Crie um usuario e comece a falar em segundos.'
          }}
        </CardDescription>
      </CardHeader>

      <CardContent>
        <form class="space-y-4" @submit.prevent="submit">
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
      </CardContent>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { LoaderCircle } from '@lucide/vue'
import { Alert, AlertDescription } from '@/components/ui/alert'
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
import { useAuthStore } from '@/stores/auth'
import type { ApiErrorBody } from '@/types/api'

const auth = useAuthStore()
const router = useRouter()

type AuthMode = 'login' | 'register'

const mode = ref<AuthMode>('login')
const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

function toggle() {
  mode.value = mode.value === 'login' ? 'register' : 'login'
  error.value = ''
}

async function submit() {
  error.value = ''
  loading.value = true
  try {
    if (mode.value === 'login') await auth.login(username.value, password.value)
    else await auth.register(username.value, password.value)
    await router.push('/users')
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
</script>
