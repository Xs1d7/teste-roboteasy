# Roboteasy

Chat em tempo real para conversas diretas — solucao do desafio full stack Roboteasy.

Auth com JWT, presenca ao vivo, mensagens via SignalR e historico persistido. Frontend em Vue 3 + TypeScript; backend em dois servicos .NET (Auth e Chat).

## Preview

### Landing

Pagina inicial posicionando o produto antes do login.

![Landing](screenshots/site-presentation.png)

### Usuarios online

Lista quem esta conectado agora; clique inicia a conversa.

![Usuarios disponiveis](screenshots/available-users.png)

### Conversa

Historico carregado do Mongo + envio em tempo real.

![Chat](screenshots/chat.png)

### Mensagens nao lidas

Com o site aberto na lista (sem chat aberto), novas mensagens aparecem com badge, preview e destaque no usuario.

![Nao lidas / notificacoes](screenshots/notification.png)

## O que foi entregue

| Requisito | Como foi feito |
|-----------|----------------|
| Login / cadastro + JWT | `services/auth` — Postgres, BCrypt, Bearer token |
| Usuarios online | SignalR + presenca in-memory no Chat |
| Mensagens realtime | Hub `/hubs/chat`, RabbitMQ entre publish/consume |
| Historico | MongoDB, filtro por par de usuarios |
| Frontend Vue (3 telas) | Login, lista online, conversa (+ landing) |
| Mensagens nao lidas | Badge + preview na lista; titulo `(N) Roboteasy`; Notification API opcional |
| Docker | `docker compose up --build` sobe tudo |

## Stack

| Camada | Tecnologia |
|--------|------------|
| Frontend | Vue 3, TypeScript, Pinia, Vue Router, SignalR client |
| Auth | .NET 10, EF Core, PostgreSQL |
| Chat | .NET 10, SignalR, MongoDB, RabbitMQ |
| Infra | Docker Compose, nginx (proxy unico) |

## Arquitetura

```
Vue 3 + TS  --HTTP-->  Auth (.NET)  --> PostgreSQL
           \--SignalR-->  Chat (.NET)  --> MongoDB
                              |  ^
                              v  |
                           RabbitMQ
```

Diagrama detalhado: [roboteasy-diagram.excalidraw](roboteasy-diagram.excalidraw) (abrir no [Excalidraw](https://excalidraw.com/)).

Documentacao do processo:

- [Entendimento](01-entendimento.md)
- [Arquitetura](02-arquitetura.md)
- [Auth](03-auth.md) · [Chat](04-chat.md) · [Frontend](05-frontend.md) · [Docker](06-docker.md) · [Testes](testes.md)
- [Evolucao com IA](07-evolucao-ia.md) — visao de arquiteto (nao implementado)

## Como rodar

Requisitos: **Docker** + **Docker Compose**.

```bash
docker compose up --build
```

Abrir **http://localhost:8080**

### Teste rapido

1. Cadastre usuario A e entre
2. Em aba anonima, cadastre usuario B
3. Confira os dois na lista de online
4. Clique e troque mensagens

RabbitMQ management (opcional): http://localhost:15672 — `guest` / `guest`

### Desenvolvimento local

Infra only:

```bash
docker compose up postgres mongo rabbitmq -d
```

APIs:

```bash
dotnet run --project services/auth --urls http://localhost:5001
dotnet run --project services/chat --urls http://localhost:5002
```

Frontend:

```bash
cd frontend && npm install && npm run dev
```

Vite em http://localhost:5173 (proxy para as APIs).

## CI

Workflow: [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) — build .NET, testes (Mongo no job) e build do frontend.

Comandos locais equivalentes: ver [README raiz](../README.md#ci).

## Enunciado original

O texto completo do desafio esta em [DESAFIO.md](DESAFIO.md).
