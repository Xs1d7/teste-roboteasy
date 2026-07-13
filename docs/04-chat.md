# Chat — status

Hub SignalR + historico Mongo + eventos RabbitMQ + **Redis** (backplane e presenca).

No Docker Compose ha **duas replicas**: `chat-a` e `chat-b` (nginx faz sticky).

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

## Escala (ponto critico)

Com `ConnectionStrings:Redis` (padrao no compose):

- backplane SignalR (`AddStackExchangeRedis`)
- `RedisPresenceTracker` com **TTL 60s** por conexao
- heartbeat a cada **20s** (`PresenceHeartbeatService` + filtro no hub)
- prune de conexoes expiradas (pod morto sem disconnect)

Compose sobe **duas replicas** (`chat-a`, `chat-b`); nginx faz **sticky** com `ip_hash`.

Sem Redis: fallback `PresenceTracker` in-memory.

Ver tambem [06-docker.md](06-docker.md) e o README raiz (secao de escala).
