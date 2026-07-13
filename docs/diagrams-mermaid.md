# Diagramas Mermaid

Cole um bloco por vez no [Excalidraw](https://excalidraw.com/) (**Insert → Mermaid**) ou visualize direto no GitHub/Markdown.

---

## 1 — Local

```mermaid
flowchart LR
  Browser[Browser] --> Nginx[nginx]

  Nginx -->|auth / avatar| Auth[Auth]
  Nginx -->|chat / hub| ChatA[chat-a]
  Nginx -->|chat / hub| ChatB[chat-b]

  Auth --> Postgres[(Postgres)]
  Auth -->|Put/Get object| MinIO[(MinIO)]
  ChatA --> Mongo[(Mongo)]
  ChatB --> Mongo
  ChatA --> Rabbit[(RabbitMQ)]
  ChatB --> Rabbit
  ChatA --> Redis[(Redis)]
  ChatB --> Redis
```

---

## 2 — AWS

```mermaid
flowchart LR
  Browser[Browser] --> ALB[Load Balancer]

  ALB --> Frontend[Frontend]
  ALB --> Auth[Auth]
  ALB --> Chat[Chat x N]

  Auth --> RDS[(RDS Postgres)]
  Auth -->|Put/Get object| S3[(S3)]
  Chat --> Mongo[(Mongo / DocumentDB)]
  Chat --> MQ[(Amazon MQ)]
  Chat --> Cache[(ElastiCache)]
```

---

## 3 — GCP

```mermaid
flowchart LR
  Browser[Browser] --> LB[Load Balancer]

  LB --> Frontend[Frontend]
  LB --> Auth[Auth]
  LB --> Chat[Chat x N]

  Auth --> SQL[(Cloud SQL)]
  Auth -->|Put/Get object| GCS[(Cloud Storage)]
  Chat --> Mongo[(Mongo)]
  Chat --> MQ[(RabbitMQ)]
  Chat --> Cache[(Memorystore)]
```

---

## 4 — Como uma mensagem chega no outro lado

```mermaid
sequenceDiagram
  actor Alice
  participant Auth
  participant Chat
  participant Mongo
  participant RabbitMQ
  participant Redis
  actor Bob

  Alice->>Auth: login
  Auth-->>Alice: JWT

  Alice->>Chat: conecta
  Bob->>Chat: conecta

  Alice->>Chat: envia mensagem
  Chat->>Mongo: salva
  Chat->>RabbitMQ: publica
  RabbitMQ-->>Chat: consome
  Chat->>Redis: roteia
  Redis-->>Chat: ok
  Chat-->>Bob: ReceiveMessage
  Chat-->>Alice: ReceiveMessage
```

---

## 5 — Upload de avatar

```mermaid
sequenceDiagram
  actor User
  participant Nginx
  participant Auth
  participant Postgres
  participant Storage as MinIO / S3 / GCS

  User->>Nginx: POST /api/users/avatar (JWT + file)
  Nginx->>Auth: proxy
  Auth->>Storage: PutObject (user-avatars)
  Storage-->>Auth: ok
  Auth->>Postgres: salva AvatarKey
  Auth-->>User: AvatarUrl (/api/users/avatar/{key})

  User->>Nginx: GET /api/users/avatar/{key}
  Nginx->>Auth: proxy
  Auth->>Storage: GetObject
  Storage-->>Auth: bytes
  Auth-->>User: image/*
```
