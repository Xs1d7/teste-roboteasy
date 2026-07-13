# Docker

Um comando sobe tudo: infra + auth + **duas replicas do Chat** + frontend (nginx).

```bash
docker compose up --build
```

App: http://localhost:8080  
RabbitMQ management (opcional): http://localhost:15672 — `guest` / `guest`

## O que sobe

| Servico | Papel |
|---------|--------|
| postgres | Auth (usuarios) |
| mongo | Historico de mensagens |
| rabbitmq | Eventos message / presence |
| redis | Backplane SignalR + presenca (TTL 60s) |
| auth | API JWT |
| **chat-a**, **chat-b** | Duas instancias do Chat (mesma imagem) |
| frontend | SPA + nginx (proxy + **sticky** `ip_hash`) |

Healthchecks: Auth, chat-a e chat-b precisam ficar healthy antes do frontend.

### Por que 2 chats + Redis + sticky

1. **Sticky (`ip_hash`)** — o mesmo browser tende a cair sempre no mesmo pod (WebSocket estavel)
2. **Redis backplane** — se A esta no `chat-a` e B no `chat-b`, `Clients.User` ainda funciona entre pods
3. **Presenca no Redis + TTL** — lista online compartilhada; pod morto some em ~60s (heartbeat a cada 20s)

Detalhe: [02-arquitetura.md](02-arquitetura.md#escala-horizontal--ponto-critico).

## Teste rapido

1. Abra http://localhost:8080
2. Cadastre usuario A e entre
3. Em aba anonima (ou outro browser), cadastre usuario B
4. A lista de online deve mostrar os dois (presenca via Redis)
5. Troque mensagens — pode estar cada um em um pod diferente; Redis + RabbitMQ amarram a entrega

Para ver as duas replicas no ar:

```bash
docker compose ps
# deve listar chat-a e chat-b (healthy)
```

## Dev local (sem rebuild das APIs)

So a infra (inclui Redis — o Chat em `appsettings.json` aponta pra `localhost:6379`):

```bash
docker compose up postgres mongo rabbitmq redis -d
```

APIs (uma instancia de Chat e suficiente em dev; sem Redis, cai no tracker in-memory):

```bash
dotnet run --project services/auth --urls http://localhost:5001
dotnet run --project services/chat --urls http://localhost:5002
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Vite em http://localhost:5173 (proxy pras APIs).
