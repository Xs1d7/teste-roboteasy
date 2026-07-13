# Testes

Projetos xUnit:

| Projeto | Foco |
|---------|------|
| `tests/Auth.Api.Tests` | Register, login, conflito de username (EF InMemory) |
| `tests/Chat.Api.Tests` | Serializacao Guid, presenca, MessageStore (Mongo opcional) |

## Rodar tudo (unitarios)

Nao precisa de Postgres nem Mongo — Auth usa InMemory; integracao Chat com Mongo pula sozinha se a URL nao existir.

```bash
dotnet test tests/Auth.Api.Tests
dotnet test tests/Chat.Api.Tests
```

## Auth

Cobre o `AuthController` direto (sem subir HTTP):

| Teste | Esperado |
|-------|----------|
| Register valido | 200 + JWT |
| Username duplicado | 409 Conflict |
| Login ok | 200 + JWT |
| Senha errada | 401 |
| Username curto | 400 |

## Chat — com integracao Mongo (MessageStore)

1. Suba o Mongo:

```bash
docker compose up mongo -d
```

2. Defina a URL (uma das opcoes):

**PowerShell**

```powershell
$env:MONGO_TEST_URL = "mongodb://localhost:27017"
dotnet test tests/Chat.Api.Tests
```

**Bash**

```bash
export MONGO_TEST_URL=mongodb://localhost:27017
dotnet test tests/Chat.Api.Tests
```

**Arquivo `.env` local**

Copie `.env.example` para `.env` e ajuste. O `.env` nao sobe no git; use-o so na sua maquina se tiver ferramenta que carrega env automaticamente.

> O `dotnet test` **nao le `.env` sozinho**. A variavel precisa estar no shell ou no CI (GitHub Actions).

## O que cada teste Chat cobre

| Arquivo | Tipo | Depende de Mongo? |
|---------|------|-------------------|
| `ChatMessageSerializationTests` | Unit | Nao — regressao Guid no BSON |
| `PresenceTrackerTests` | Unit | Nao — multi-aba / online (**in-memory**; Redis e coberto no compose, nao nestes unitarios) |
| `MessageStoreTests` | Integracao | Sim — save + busca conversa |

Se `MONGO_TEST_URL` nao estiver setada, `MessageStoreTests` retorna sem falhar (unitarios ja cobrem o bug critico do Guid).

## CI

No GitHub Actions (`.github/workflows/ci.yml`), o job restaura/builda Auth e Chat, roda `Auth.Api.Tests` e `Chat.Api.Tests` (com service `mongo:7` + `MONGO_TEST_URL`).
