<template>
  <div class="landing">
    <header class="nav">
      <span class="brand">Roboteasy</span>
      <nav>
        <RouterLink class="nav-link" to="/login">Entrar</RouterLink>
        <RouterLink class="btn nav-cta" to="/login">Comecar gratis</RouterLink>
      </nav>
    </header>

    <section class="hero">
      <div class="hero-copy">
        <h1 class="reveal">Mensagens em tempo real,<br />sem atrito.</h1>
        <p class="lede reveal delay">
          Roboteasy conecta sua equipe em conversas diretas, com historico seguro e presencia ao vivo.
        </p>
        <div class="cta reveal delay2">
          <RouterLink class="btn" to="/login">Abrir o chat</RouterLink>
          <a class="btn ghost" href="#produto">Ver como funciona</a>
        </div>
      </div>

      <div class="hero-visual" aria-hidden="true">
        <div class="pulse"></div>
        <div class="thread">
          <div class="line them">oi, conseguiu olhar o deploy?</div>
          <div class="line me">subi agora. ja esta no ar.</div>
          <div class="line them">perfeito — seguimos no canal</div>
        </div>
      </div>
    </section>

    <section id="produto" class="product">
      <h2>Feito para conversa direta</h2>
      <p class="product-lede">
        Cadastro rapido, usuarios online e mensagens instantaneas — o essencial de um messenger moderno.
      </p>
      <ul class="points">
        <li>
          <strong>Presenca ao vivo</strong>
          <span>Veja quem esta disponivel e inicie o chat em um clique.</span>
        </li>
        <li>
          <strong>Historico persistente</strong>
          <span>Volte depois e continue de onde parou.</span>
        </li>
        <li>
          <strong>Tempo real de verdade</strong>
          <span>WebSockets com entrega imediata entre os dois lados.</span>
        </li>
      </ul>
    </section>

    <footer class="foot">
      <span class="brand">Roboteasy</span>
      <span>mensageria em tempo real</span>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.landing {
  min-height: 100vh;
  background:
    radial-gradient(1100px 560px at 8% -8%, #1b463c 0%, transparent 58%),
    radial-gradient(900px 520px at 95% 8%, #0e2d38 0%, transparent 52%),
    linear-gradient(180deg, #0a1613 0%, #0c1916 45%, #091411 100%);
  overflow-x: hidden;
}

.nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.15rem 6vw;
  position: relative;
  z-index: 2;
}

.nav nav {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}

.nav-link {
  color: var(--muted);
  font-weight: 500;
}
.nav-link:hover { color: var(--text); }

.nav-cta { padding: 0.55rem 0.9rem; font-size: 0.92rem; }

.brand {
  font-family: 'Sora', sans-serif;
  font-weight: 700;
  font-size: 1.15rem;
  letter-spacing: -0.03em;
}

.hero {
  min-height: calc(100vh - 72px);
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 2rem;
  align-items: center;
  padding: 2rem 6vw 4rem;
  position: relative;
}

.hero::before {
  content: '';
  position: absolute;
  inset: 8% 4% auto auto;
  width: min(42vw, 520px);
  height: min(42vw, 520px);
  border-radius: 50%;
  background: radial-gradient(circle, rgba(61, 214, 140, 0.16), transparent 68%);
  animation: drift 12s ease-in-out infinite alternate;
  pointer-events: none;
}

.hero-copy { position: relative; z-index: 1; max-width: 640px; }

h1 {
  margin: 0 0 1rem;
  font-size: clamp(2.4rem, 5.2vw, 4rem);
  line-height: 1.05;
  font-weight: 700;
}

.lede {
  margin: 0 0 1.75rem;
  color: var(--muted);
  font-size: clamp(1.02rem, 1.6vw, 1.2rem);
  line-height: 1.55;
  max-width: 34rem;
}

.cta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.hero-visual {
  position: relative;
  z-index: 1;
  min-height: 320px;
  display: grid;
  place-items: center;
}

.pulse {
  position: absolute;
  width: 220px;
  height: 220px;
  border-radius: 50%;
  border: 1px solid rgba(61, 214, 140, 0.28);
  animation: pulse 3.4s ease-out infinite;
}

.thread {
  width: min(100%, 340px);
  display: flex;
  flex-direction: column;
  gap: 0.7rem;
  animation: rise 900ms ease both;
}

.line {
  max-width: 86%;
  padding: 0.75rem 0.95rem;
  border-radius: 14px;
  border: 1px solid var(--line);
  font-size: 0.95rem;
  line-height: 1.35;
  animation: pop 700ms ease both;
}

.line.them {
  align-self: flex-start;
  background: rgba(26, 46, 42, 0.9);
  animation-delay: 200ms;
}

.line.me {
  align-self: flex-end;
  background: rgba(31, 77, 66, 0.95);
  border-color: #2f6d5d;
  animation-delay: 480ms;
}

.line.them:last-child { animation-delay: 760ms; }

.product {
  padding: 5rem 6vw 4rem;
  border-top: 1px solid rgba(42, 69, 63, 0.7);
  max-width: 1100px;
  margin: 0 auto;
}

.product h2 {
  margin: 0 0 0.6rem;
  font-size: clamp(1.6rem, 3vw, 2.2rem);
}

.product-lede {
  margin: 0 0 2rem;
  color: var(--muted);
  max-width: 36rem;
  line-height: 1.5;
}

.points {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 1.4rem;
}

.points li {
  display: grid;
  gap: 0.25rem;
  padding-top: 1.1rem;
  border-top: 1px solid var(--line);
}

.points strong {
  font-family: 'Sora', sans-serif;
  font-size: 1.05rem;
}

.points span { color: var(--muted); line-height: 1.45; }

.foot {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.5rem 6vw 2rem;
  color: var(--muted);
  font-size: 0.9rem;
  border-top: 1px solid rgba(42, 69, 63, 0.55);
}

.reveal {
  animation: rise 800ms ease both;
}
.delay { animation-delay: 120ms; }
.delay2 { animation-delay: 220ms; }

@keyframes rise {
  from { opacity: 0; transform: translateY(16px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes pop {
  from { opacity: 0; transform: translateY(10px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

@keyframes pulse {
  0% { transform: scale(0.85); opacity: 0.7; }
  70% { transform: scale(1.25); opacity: 0; }
  100% { opacity: 0; }
}

@keyframes drift {
  from { transform: translate(-8px, 0); }
  to { transform: translate(12px, 18px); }
}

@media (max-width: 900px) {
  .hero {
    grid-template-columns: 1fr;
    min-height: auto;
    padding-top: 1.5rem;
  }

  .hero-visual { order: -1; min-height: 260px; }

  .nav-link { display: none; }
}
</style>
