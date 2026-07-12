# Chat — status

Hub SignalR + historico Mongo + eventos RabbitMQ.

- Hub: `/hubs/chat` (`SendMessage`)
- `GET /api/users/online`
- `GET /api/messages?with={userId}`

Mesma chave JWT do Auth.

```bash
dotnet run --project services/chat --urls http://localhost:5002
```

Obs: precisei setar Guid como string no BSON — o driver 3.x reclama se deixar Unspecified.
