import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import axios from 'axios'
import type { AuthResponse, StoredAuth } from '../types/api'

const KEY = 're_auth'

function readSaved(): StoredAuth | null {
  try {
    return JSON.parse(localStorage.getItem(KEY) || 'null') as StoredAuth | null
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const saved = readSaved()
  const token = ref<string | null>(saved?.token ?? null)
  const userId = ref<string | null>(saved?.userId ?? null)
  const username = ref<string | null>(saved?.username ?? null)

  const isLogged = computed(() => !!token.value)

  function persist() {
    if (!token.value) {
      localStorage.removeItem(KEY)
      return
    }
    const payload: StoredAuth = {
      token: token.value,
      userId: userId.value ?? '',
      username: username.value ?? ''
    }
    localStorage.setItem(KEY, JSON.stringify(payload))
  }

  async function register(user: string, pass: string) {
    const { data } = await axios.post<AuthResponse>('/api/auth/register', {
      username: user,
      password: pass
    })
    apply(data)
  }

  async function login(user: string, pass: string) {
    const { data } = await axios.post<AuthResponse>('/api/auth/login', {
      username: user,
      password: pass
    })
    apply(data)
  }

  function apply(data: AuthResponse) {
    token.value = data.token
    userId.value = data.userId
    username.value = data.username
    persist()
  }

  function logout() {
    token.value = null
    userId.value = null
    username.value = null
    persist()
  }

  function authHeader(): { Authorization: string } {
    return { Authorization: `Bearer ${token.value}` }
  }

  return { token, userId, username, isLogged, register, login, logout, authHeader }
})
