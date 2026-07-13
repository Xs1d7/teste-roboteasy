# Auth — status

Servico de autenticacao pronto:

- `POST /api/auth/register`
- `POST /api/auth/login` (retorna `avatarUrl` se houver)
- `GET /api/users` (JWT)
- `POST /api/users/avatar` (JWT, multipart — MinIO/S3)
- `GET /api/users/avatar/{fileName}` (proxy do arquivo)
- Postgres via EF (`EnsureCreated` + coluna `AvatarKey`)
- Object storage via SDK S3 (MinIO no compose; S3/GCS-compativel em prod)

Rodar local (precisa do Postgres + MinIO no ar):

```bash
dotnet run --project services/auth --urls http://localhost:5001
```

Swagger UI: http://localhost:5001/swagger

No Docker Compose tambem fica disponivel (porta publicada + `Swagger__Enabled=true`).
