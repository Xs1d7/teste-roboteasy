import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import axios from 'axios'
import type { AuthResponse, AvatarUploadResponse, StoredAuth } from '../types/api'

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
  const avatarUrl = ref<string | null>(saved?.avatarUrl ?? null)

  const isLogged = computed(() => !!token.value)

  function persist() {
    if (!token.value) {
      localStorage.removeItem(KEY)
      return
    }
    const payload: StoredAuth = {
      token: token.value,
      userId: userId.value ?? '',
      username: username.value ?? '',
      avatarUrl: avatarUrl.value
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

  async function uploadAvatar(file: File) {
    const form = new FormData()
    form.append('file', file)
    const { data } = await axios.post<AvatarUploadResponse>(
      '/api/users/avatar',
      form,
      { headers: authHeader() }
    )
    avatarUrl.value = data.avatarUrl
    persist()
    return data.avatarUrl
  }

  function apply(data: AuthResponse) {
    token.value = data.token
    userId.value = data.userId
    username.value = data.username
    avatarUrl.value = data.avatarUrl ?? null
    persist()
  }

  function logout() {
    token.value = null
    userId.value = null
    username.value = null
    avatarUrl.value = null
    persist()
  }

  function authHeader(): { Authorization: string } {
    return { Authorization: `Bearer ${token.value}` }
  }

  return {
    token,
    userId,
    username,
    avatarUrl,
    isLogged,
    register,
    login,
    uploadAvatar,
    logout,
    authHeader
  }
})
