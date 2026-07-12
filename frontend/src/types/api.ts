export interface AuthResponse {
  token: string
  userId: string
  username: string
}

export interface StoredAuth {
  token: string
  userId: string
  username: string
}

export interface OnlineUser {
  userId: string
  username: string
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

export interface ApiErrorBody {
  message?: string
}
