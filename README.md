# Roboteasy

Chat em tempo real — solucao do desafio full stack.

Auth JWT, usuarios online, mensagens via SignalR e historico no Mongo. Dois servicos .NET (Auth + Chat), frontend Vue 3 + TypeScript, tudo sobe com Docker.

**Documentacao completa:** [docs/README.md](docs/README.md) (screenshots, arquitetura, como rodar)

**Enunciado original:** [docs/DESAFIO.md](docs/DESAFIO.md)

## Preview

| Landing | Usuarios online | Conversa |
|---------|-----------------|----------|
| ![Landing](docs/screenshots/site-presentation.png) | ![Online](docs/screenshots/available-users.png) | ![Chat](docs/screenshots/chat.png) |

## Rodar em 1 comando

```bash
docker compose up --build
```

http://localhost:8080

## Entregue

- Login/cadastro com JWT (Postgres)
- Lista de quem esta conectado agora
- Chat 1:1 com historico (Mongo + SignalR)
- Docker Compose + nginx

## Stack

Vue 3 · .NET 10 · PostgreSQL · MongoDB · RabbitMQ · SignalR
