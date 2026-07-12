# Auth — status

Servico de autenticacao pronto:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/users` (JWT)
- Postgres via EF (`EnsureCreated` por enquanto)

Rodar local (precisa do Postgres no ar):

```bash
dotnet run --project services/auth --urls http://localhost:5001
```

Swagger UI (so em Development): http://localhost:5001/swagger

Proximo passo: Chat (SignalR + Mongo + Rabbit).
