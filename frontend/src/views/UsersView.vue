<template>
  <div class="shell">
    <header class="topbar">
      <div class="brand">Roboteasy</div>
      <div class="meta">ola, {{ auth.username }}</div>
    </header>
    <section class="panel" style="padding: 1.25rem">
      <h2 style="margin-top:0">Usuarios online</h2>
      <p style="color: var(--muted)">Proximo passo: conectar no SignalR e listar quem esta disponivel.</p>
      <button class="btn ghost" @click="sair">Sair</button>
    </section>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()

async function sair() {
  auth.logout()
  await router.push('/login')
}
</script>
