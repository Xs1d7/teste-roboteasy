<template>
  <Message :align="mine ? 'end' : 'start'" class="animate-in fade-in slide-in-from-bottom-1 duration-200">
    <MessageAvatar v-if="!mine">
      <Avatar size="sm" class="size-8">
        <AvatarImage v-if="avatarUrl" :src="avatarUrl" :alt="author" />
        <AvatarFallback class="bg-secondary text-[0.65rem] font-semibold">
          {{ initials(author) }}
        </AvatarFallback>
      </Avatar>
    </MessageAvatar>

    <MessageContent class="max-w-[min(80%,28rem)] gap-1">
      <Bubble :variant="mine ? 'default' : 'secondary'" :align="mine ? 'end' : 'start'">
        <BubbleContent class="whitespace-pre-wrap">
          {{ content }}
        </BubbleContent>
      </Bubble>
      <MessageFooter>
        {{ time }}
      </MessageFooter>
    </MessageContent>
  </Message>
</template>

<script setup lang="ts">
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Bubble, BubbleContent } from '@/components/ui/bubble'
import {
  Message,
  MessageAvatar,
  MessageContent,
  MessageFooter
} from '@/components/ui/message'
import { initials } from '@/lib/initials'

withDefaults(
  defineProps<{
    content: string
    time: string
    author?: string
    avatarUrl?: string | null
    mine?: boolean
  }>(),
  { author: '?', avatarUrl: null, mine: false }
)
</script>
