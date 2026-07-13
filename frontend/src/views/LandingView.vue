<template>
  <div class="min-h-dvh overflow-x-hidden">
    <header class="relative z-10 flex items-center justify-between px-[6vw] py-4">
      <span class="font-heading text-lg font-bold tracking-tight">Roboteasy</span>
      <nav class="flex items-center gap-2">
        <Button as-child variant="ghost" size="sm" class="hidden sm:inline-flex">
          <RouterLink to="/login">Entrar</RouterLink>
        </Button>
        <Button as-child size="sm">
          <RouterLink to="/login">Comecar gratis</RouterLink>
        </Button>
      </nav>
    </header>

    <section class="relative grid min-h-[calc(100dvh-72px)] items-center gap-8 px-[6vw] py-8 lg:grid-cols-[1.1fr_0.9fr]">
      <div
        class="pointer-events-none absolute inset-y-[8%] right-[4%] size-[min(42vw,520px)] rounded-full bg-[radial-gradient(circle,oklch(0.78_0.15_155/0.16),transparent_68%)] animate-[drift_12s_ease-in-out_infinite_alternate]"
        aria-hidden="true"
      />

      <div class="relative z-1 max-w-xl animate-in fade-in slide-in-from-bottom-3 duration-700">
        <h1 class="font-heading mb-4 text-[clamp(2.4rem,5.2vw,4rem)] leading-[1.05] font-bold">
          Mensagens em tempo real,<br />sem atrito.
        </h1>
        <p class="mb-7 max-w-md text-[clamp(1.02rem,1.6vw,1.2rem)] leading-relaxed text-muted-foreground">
          Roboteasy conecta sua equipe em conversas diretas, com historico seguro e presencia ao vivo.
        </p>
        <div class="flex flex-wrap gap-3">
          <Button as-child size="lg">
            <RouterLink to="/login">Abrir o chat</RouterLink>
          </Button>
          <Button as-child variant="outline" size="lg">
            <a href="#produto">Ver como funciona</a>
          </Button>
        </div>
      </div>

      <div class="relative z-1 grid min-h-80 place-items-center" aria-hidden="true">
        <div class="absolute size-56 animate-[pulse-ring_3.4s_ease-out_infinite] rounded-full border border-primary/30" />
        <div class="flex w-full max-w-xs flex-col gap-3 animate-in fade-in slide-in-from-bottom-4 duration-700">
          <Bubble variant="secondary" align="start" class="animate-in fade-in slide-in-from-bottom-2 duration-500">
            <BubbleContent>oi, conseguiu olhar o deploy?</BubbleContent>
          </Bubble>
          <Bubble variant="default" align="end" class="animate-in fade-in slide-in-from-bottom-2 duration-500 delay-150">
            <BubbleContent>subi agora. ja esta no ar.</BubbleContent>
          </Bubble>
          <Bubble variant="secondary" align="start" class="animate-in fade-in slide-in-from-bottom-2 duration-500 delay-300">
            <BubbleContent>perfeito — seguimos no canal</BubbleContent>
          </Bubble>
        </div>
      </div>
    </section>

    <section id="produto" class="mx-auto max-w-5xl border-t border-border/70 px-[6vw] py-16">
      <h2 class="font-heading mb-2 text-[clamp(1.6rem,3vw,2.2rem)] font-semibold">
        Feito para conversa direta
      </h2>
      <p class="mb-8 max-w-lg text-muted-foreground leading-relaxed">
        Cadastro rapido, usuarios online e mensagens instantaneas — o essencial de um messenger moderno.
      </p>
      <ul class="grid gap-5">
        <li
          v-for="item in points"
          :key="item.title"
          class="grid gap-1 border-t border-border pt-4"
        >
          <strong class="font-heading text-base">{{ item.title }}</strong>
          <span class="text-muted-foreground leading-relaxed">{{ item.body }}</span>
        </li>
      </ul>
    </section>

    <footer class="flex justify-between gap-4 border-t border-border/55 px-[6vw] py-6 text-sm text-muted-foreground">
      <span class="font-heading font-semibold text-foreground">Roboteasy</span>
      <span>mensageria em tempo real</span>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { Bubble, BubbleContent } from '@/components/ui/bubble'
import { Button } from '@/components/ui/button'

const points = [
  {
    title: 'Presenca ao vivo',
    body: 'Veja quem esta disponivel e inicie o chat em um clique.'
  },
  {
    title: 'Historico persistente',
    body: 'Volte depois e continue de onde parou.'
  },
  {
    title: 'Tempo real de verdade',
    body: 'WebSockets com entrega imediata entre os dois lados.'
  }
]
</script>

<style>
@keyframes drift {
  from { transform: translate(-8px, 0); }
  to { transform: translate(12px, 18px); }
}

@keyframes pulse-ring {
  0% { transform: scale(0.85); opacity: 0.7; }
  70% { transform: scale(1.25); opacity: 0; }
  100% { opacity: 0; }
}
</style>
