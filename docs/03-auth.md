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

Swagger UI: http://localhost:5001/swagger

No Docker Compose tambem fica disponivel (porta publicada + `Swagger__Enabled=true`).

Proximo passo: Chat (SignalR + Mongo + Rabbit).
