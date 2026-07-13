export interface AuthResponse {
  token: string
  userId: string
  username: string
  avatarUrl?: string | null
}

export interface StoredAuth {
  token: string
  userId: string
  username: string
  avatarUrl?: string | null
}

export interface DirectoryUser {
  id: string
  username: string
  avatarUrl?: string | null
}

export interface OnlineUser {
  userId: string
  username: string
  avatarUrl?: string | null
}

export interface ChatMessage {
  id: string
  fromUserId: string
  fromUsername: string
  toUserId: string
  toUsername: string
  content: string
  sentAt: string
}

export interface TypingEvent {
  userId: string
  username: string
  isTyping: boolean
}

export interface ApiErrorBody {
  message?: string
}

export interface AvatarUploadResponse {
  avatarUrl: string
}
