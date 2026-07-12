<template>
  <div class="auth-page">
    <div class="auth-card panel">
      <p class="eyebrow">Roboteasy</p>
      <h1>{{ mode === 'login' ? 'Entrar' : 'Criar conta' }}</h1>
      <p class="hint">
        {{ mode === 'login'
          ? 'Acesse sua conta para continuar a conversa.'
          : 'Crie um usuario e comece a falar em segundos.' }}
      </p>

      <form @submit.prevent="submit">
        <div class="field">
          <label for="user">Usuario</label>
          <input id="user" v-model.trim="username" autocomplete="username" required minlength="3" />
        </div>
        <div class="field">
          <label for="pass">Senha</label>
          <input id="pass" v-model="password" type="password" autocomplete="current-password" required minlength="4" />
        </div>

        <p v-if="error" class="error">{{ error }}</p>

        <div class="actions">
          <button class="btn" type="submit" :disabled="loading">
            {{ loading ? 'Aguarde...' : (mode === 'login' ? 'Login' : 'Cadastrar') }}
          </button>
          <button class="btn ghost" type="button" @click="toggle" :disabled="loading">
            {{ mode === 'login' ? 'Cadastrar-se' : 'Ja tenho conta' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '../stores/auth'
import type { ApiErrorBody } from '../types/api'

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

<style scoped>
.auth-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 1.5rem;
  background:
    radial-gradient(900px 480px at 15% -10%, #1a3d34 0%, transparent 55%),
    radial-gradient(700px 400px at 100% 0%, #0d2a32 0%, transparent 50%),
    var(--bg);
}

.auth-card {
  width: min(420px, 100%);
  padding: 1.75rem;
}

.eyebrow {
  margin: 0 0 0.35rem;
  color: var(--accent);
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

h1 {
  margin: 0 0 0.4rem;
  font-size: 1.8rem;
}

.hint {
  margin: 0 0 1.4rem;
  color: var(--muted);
}

.actions {
  display: flex;
  gap: 0.65rem;
  margin-top: 1rem;
  flex-wrap: wrap;
}
</style>
