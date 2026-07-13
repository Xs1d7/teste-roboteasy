<template>
  <li>
    <button
      type="button"
      :class="cn(
        'group flex w-full items-center gap-3 rounded-xl border border-transparent px-3 py-3 text-left transition-colors',
        'hover:bg-accent/60',
        unread > 0 && 'border-primary/20 bg-primary/8',
      )"
      @click="$emit('select')"
    >
      <Avatar size="default" class="size-10">
        <AvatarImage v-if="avatarUrl" :src="avatarUrl" :alt="username" />
        <AvatarFallback class="bg-secondary text-sm font-semibold text-secondary-foreground">
          {{ initials(username) }}
        </AvatarFallback>
        <AvatarBadge class="bg-primary ring-background size-2.5 ring-2" />
      </Avatar>

      <div class="min-w-0 flex-1">
        <div class="flex items-center gap-2">
          <span
            :class="cn(
              'truncate font-medium',
              unread > 0 && 'font-semibold',
            )"
          >
            {{ username }}
          </span>
          <Badge
            v-if="unread > 0"
            class="h-5 min-w-5 shrink-0 justify-center px-1.5 text-[0.7rem]"
            :aria-label="`${unread} nao lidas`"
          >
            {{ unread > 99 ? '99+' : unread }}
          </Badge>
        </div>
        <p
          v-if="preview"
          :class="cn(
            'truncate text-sm text-muted-foreground',
            unread > 0 && 'text-foreground/80',
          )"
        >
          {{ preview }}
        </p>
      </div>

      <span
        :class="cn(
          'shrink-0 text-xs text-muted-foreground opacity-70 transition-opacity group-hover:opacity-100',
          unread > 0 && 'font-semibold text-primary opacity-100',
        )"
      >
        {{ unread > 0 ? 'nova' : 'abrir' }}
      </span>
    </button>
  </li>
</template>

<script setup lang="ts">
import { Avatar, AvatarBadge, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { initials } from '@/lib/initials'
import { cn } from '@/lib/utils'

withDefaults(
  defineProps<{
    username: string
    avatarUrl?: string | null
    unread?: number
    preview?: string
  }>(),
  { avatarUrl: null, unread: 0, preview: '' }
)
defineEmits<{ select: [] }>()
</script>
