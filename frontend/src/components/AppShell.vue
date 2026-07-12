<template>
  <div class="shell" :class="shellClass">
    <header class="topbar" :class="topbarClass">
      <div class="leading">
        <slot name="leading" />
        <div v-if="title" class="brand" :class="titleClass">{{ title }}</div>
        <div v-if="subtitle" class="meta">{{ subtitle }}</div>
      </div>
      <div v-if="$slots.trailing" class="trailing">
        <slot name="trailing" />
      </div>
    </header>

    <slot />
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    title?: string
    subtitle?: string
    shellClass?: string
    topbarClass?: string
    titleClass?: string
  }>(),
  {
    title: '',
    subtitle: '',
    shellClass: '',
    topbarClass: '',
    titleClass: ''
  }
)
</script>

<style scoped>
.trailing {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}
</style>
