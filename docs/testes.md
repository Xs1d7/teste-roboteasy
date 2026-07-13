# Testes

Projeto: `tests/Chat.Api.Tests` (xUnit).

## Rodar tudo (unitarios)

Nao precisa de Mongo — os testes de integracao pulam sozinhos se a URL nao existir.

```bash
dotnet test tests/Chat.Api.Tests
```

## Com integracao Mongo (MessageStore)

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

## O que cada teste cobre

| Arquivo | Tipo | Depende de Mongo? |
|---------|------|-------------------|
| `ChatMessageSerializationTests` | Unit | Nao — regressao Guid no BSON |
| `PresenceTrackerTests` | Unit | Nao — multi-aba / online (**in-memory**; Redis e coberto no compose, nao nestes unitarios) |
| `MessageStoreTests` | Integracao | Sim — save + busca conversa |

Se `MONGO_TEST_URL` nao estiver setada, `MessageStoreTests` retorna sem falhar (unitarios ja cobrem o bug critico do Guid).

## CI

No GitHub Actions (`.github/workflows/ci.yml`), o job sobe service `mongo:7` e exporta `MONGO_TEST_URL` antes do `dotnet test`.
