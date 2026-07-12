<template>
  <div class="bubble" :class="{ mine }">
    <div class="content">{{ content }}</div>
    <div class="time">{{ time }}</div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  content: string
  time: string
  mine?: boolean
}>()
</script>

<style scoped>
.bubble {
  max-width: min(75%, 420px);
  padding: 0.7rem 0.85rem;
  border-radius: 12px;
  background: var(--them);
  border: 1px solid var(--line);
  align-self: flex-start;
}

.bubble.mine {
  align-self: flex-end;
  background: var(--me);
  border-color: #2f6d5d;
}

.content {
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.4;
}

.time {
  margin-top: 0.35rem;
  font-size: 0.72rem;
  color: var(--muted);
  text-align: right;
}
</style>
