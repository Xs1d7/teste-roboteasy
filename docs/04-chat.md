# Chat — status

Hub SignalR + historico Mongo + eventos RabbitMQ.

## REST (Swagger)

- `GET /api/users/online`
- `GET /api/messages?with={userId}`

Mesma chave JWT do Auth.

```bash
dotnet run --project services/chat --urls http://localhost:5002
```

Swagger UI (so em Development): http://localhost:5002/swagger

## SignalR (nao coberto pelo Swagger)

Endpoint: `/hubs/chat`

Autenticacao: JWT via query `?access_token={token}` (WebSocket nao envia header facil).

| Metodo (cliente → servidor) | Parametros | Descricao |
|-----------------------------|------------|-----------|
| `SendMessage` | `toUserId`, `toUsername`, `content` | Envia mensagem 1:1 |

| Evento (servidor → cliente) | Payload | Descricao |
|-----------------------------|---------|-----------|
| `OnlineUsers` | `OnlineUser[]` | Lista ao conectar |
| `PresenceChanged` | `{ type, userId, username }` | `user.online` ou `user.offline` |
| `ReceiveMessage` | `MessageDto` | Nova mensagem na conversa |

Obs: precisei setar Guid como string no BSON — o driver 3.x reclama se deixar Unspecified.
