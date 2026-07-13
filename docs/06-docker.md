# Docker

Um comando sobe tudo: infra + auth + chat + frontend (nginx).

```bash
docker compose up --build
```

App: http://localhost:8080  
RabbitMQ management (opcional): http://localhost:15672 — `guest` / `guest`

Auth e Chat expoem `GET /health` (porta 8080 interna). O compose usa isso como healthcheck; o frontend so sobe depois dos dois APIs ficarem healthy.

Servicos: postgres, mongo, rabbitmq, **redis** (backplane SignalR + presenca), auth, chat, frontend.

## Teste rapido

1. Abra http://localhost:8080
2. Cadastre usuario A e entre
3. Em aba anonima, cadastre usuario B
4. A lista de online deve mostrar os dois
5. Clique e troque mensagens

## Dev local (sem rebuild das APIs)

So a infra:

```bash
docker compose up postgres mongo rabbitmq redis -d
```

APIs:

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
