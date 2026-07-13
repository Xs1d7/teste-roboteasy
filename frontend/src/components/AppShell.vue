<template>
  <div :class="cn('mx-auto w-full max-w-3xl px-4 py-5 sm:px-6', shellClass)">
    <header
      :class="cn(
        'mb-5 flex flex-wrap items-center justify-between gap-3',
        topbarClass,
      )"
    >
      <div class="flex min-w-0 flex-1 items-center gap-3">
        <slot name="leading" />
        <div class="min-w-0">
          <div
            v-if="title"
            :class="cn('font-heading text-lg font-semibold tracking-tight', titleClass)"
          >
            {{ title }}
          </div>
          <div v-if="subtitle" class="text-sm text-muted-foreground">
            {{ subtitle }}
          </div>
        </div>
      </div>
      <div v-if="$slots.trailing" class="flex shrink-0 items-center gap-2">
        <slot name="trailing" />
      </div>
    </header>

    <slot />
  </div>
</template>

<script setup lang="ts">
import { cn } from '@/lib/utils'

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
