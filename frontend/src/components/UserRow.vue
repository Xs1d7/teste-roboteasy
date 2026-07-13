<template>
  <li>
    <button
      class="user-row"
      type="button"
      :class="{ unread: unread > 0 }"
      @click="$emit('select')"
    >
      <span class="dot"></span>
      <span class="info">
        <span class="name-row">
          <span class="name">{{ username }}</span>
          <span v-if="unread > 0" class="badge" :aria-label="`${unread} nao lidas`">
            {{ unread > 99 ? '99+' : unread }}
          </span>
        </span>
        <span v-if="preview" class="preview">{{ preview }}</span>
      </span>
      <span class="cta">{{ unread > 0 ? 'nova mensagem' : 'conversar' }}</span>
    </button>
  </li>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    username: string
    unread?: number
    preview?: string
  }>(),
  { unread: 0, preview: '' }
)
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

.user-row.unread {
  background: linear-gradient(90deg, rgba(61, 214, 140, 0.1), transparent 70%);
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

.info {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.name-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 0;
}

.name {
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-row.unread .name { font-weight: 700; }

.badge {
  flex-shrink: 0;
  min-width: 1.35rem;
  height: 1.35rem;
  padding: 0 0.4rem;
  border-radius: 999px;
  background: var(--accent);
  color: #042015;
  font-size: 0.72rem;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}

.preview {
  color: var(--muted);
  font-size: 0.85rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-row.unread .preview { color: var(--text); opacity: 0.85; }

.cta {
  color: var(--muted);
  font-size: 0.85rem;
  opacity: 0.7;
  white-space: nowrap;
}

.user-row.unread .cta {
  color: var(--accent);
  opacity: 1;
  font-weight: 600;
}
</style>
