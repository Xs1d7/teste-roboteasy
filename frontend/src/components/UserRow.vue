<template>
  <li>
    <button class="user-row" type="button" @click="$emit('select')">
      <span class="dot"></span>
      <span class="name">{{ username }}</span>
      <span class="cta">conversar</span>
    </button>
  </li>
</template>

<script setup lang="ts">
defineProps<{ username: string }>()
defineEmits<{ select: [] }>()
</script>

<style scoped>
.user-row {
  width: 100%;
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 0.75rem;
  padding: 0.95rem 0.35rem;
  background: transparent;
  border: 0;
  border-top: 1px solid var(--line);
  color: var(--text);
  cursor: pointer;
  text-align: left;
}

.user-row:hover .name { color: var(--accent); }
.user-row:hover .cta { opacity: 1; }

.dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: var(--accent);
  box-shadow: 0 0 0 4px rgba(61, 214, 140, 0.15);
}

.name { font-weight: 600; }

.cta {
  color: var(--muted);
  font-size: 0.85rem;
  opacity: 0.7;
}
</style>
