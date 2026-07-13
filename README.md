# Roboteasy

Chat em tempo real — solucao do desafio full stack.

Auth JWT, usuarios online, mensagens via SignalR e historico no Mongo. Dois servicos .NET (Auth + Chat), frontend Vue 3 + TypeScript, tudo sobe com Docker.

**Documentacao completa:** [docs/README.md](docs/README.md) (screenshots, arquitetura, como rodar)

**Visao de evolucao com IA:** [docs/07-evolucao-ia.md](docs/07-evolucao-ia.md) (doc only, fora do escopo do codigo)

**Enunciado original:** [docs/DESAFIO.md](docs/DESAFIO.md)

## Preview

| Landing | Usuarios online | Conversa | Nao lidas |
|---------|-----------------|----------|-----------|
| ![Landing](docs/screenshots/site-presentation.png) | ![Online](docs/screenshots/available-users.png) | ![Chat](docs/screenshots/chat.png) | ![Notificacoes](docs/screenshots/notification.png) |

## Rodar em 1 comando

```bash
docker compose up --build
```

http://localhost:8080

## Entregue

- Login/cadastro com JWT (Postgres)
- Lista de quem esta conectado agora
- Chat 1:1 com historico (Mongo + SignalR)
- Indicador de mensagens nao lidas (badge + preview na lista)
- Docker Compose + nginx

## Stack

Vue 3 · .NET 10 · PostgreSQL · MongoDB · RabbitMQ · SignalR

## Testes

```bash
dotnet test tests/Chat.Api.Tests
```

Ver [docs/testes.md](docs/testes.md) (inclui `MONGO_TEST_URL` para integracao com Mongo).

## CI

GitHub Actions: [`.github/workflows/ci.yml`](.github/workflows/ci.yml) — build Auth/Chat, `dotnet test` (com Mongo) e `npm run build`.

Local (equivalente):

```bash
dotnet build services/auth/Auth.Api.csproj
dotnet build services/chat/Chat.Api.csproj
dotnet test tests/Chat.Api.Tests
cd frontend && npm ci && npm run build
```

## Deploy (Terraform)

Infra declarada em [`infra/`](infra/README.md):

- [Google Cloud](infra/gcp/) — Compute Engine + Docker Compose
- [AWS](infra/aws/) — EC2 + VPC + Elastic IP + Docker Compose
