# Infraestrutura (Terraform)

Declaracoes para subir o stack Roboteasy em **Google Cloud** e **AWS**.

Estrategia escolhida para o desafio: **uma VM por cloud** rodando o `docker compose` do repo.

O compose **ja inclui** escala do Chat no host:

- `redis` + `chat-a` + `chat-b`
- nginx com sticky (`ip_hash`)
- presenca Redis com TTL/heartbeat

Ou seja: mesmo na VM unica do Terraform, o stack demonstra **N processos de Chat** cooperando via Redis — nao e um unico container de chat.

Em producao “gerenciada” eu separaria em Cloud Run / ECS + Cloud SQL / RDS + Mongo Atlas + Amazon MQ / CloudAMQP + ElastiCache / Memorystore — ver notas no final.

| Pasta | Cloud | Recursos principais |
|-------|-------|---------------------|
| [`gcp/`](gcp/) | Google Cloud | VPC, firewall, Compute Engine |
| [`aws/`](aws/) | AWS | VPC, Security Group, EC2 |

## Pre-requisitos

- [Terraform](https://www.terraform.io/downloads) >= 1.5
- Credenciais da cloud (gcloud / AWS CLI)
- Chave SSH publica
- Repositorio acessivel via HTTPS (ou ajuste o `git_repo_url`)

## Fluxo comum

```bash
cd infra/gcp   # ou infra/aws
cp terraform.tfvars.example terraform.tfvars
# edite project_id / region / ssh_public_key / git_repo_url
terraform init
terraform plan
terraform apply
```

Output `app_url` aponta para `http://<IP>:8080`.

O `startup_script` instala Docker, clona o repo e sobe o compose. Primeira subida leva alguns minutos (build das imagens).

## Segredos

- `jwt_key` e senhas devem ir em `terraform.tfvars` (nao commitado) ou secret manager
- `.tfvars` real esta no `.gitignore` via padrao `*.tfvars` (exceto `*.tfvars.example`)

## Proximo passo (producao)

| Peca | GCP | AWS |
|------|-----|-----|
| Auth / Chat / Frontend | Cloud Run | ECS Fargate + ALB |
| Postgres | Cloud SQL | RDS |
| Mongo | Atlas ou GCE | DocumentDB / Atlas |
| RabbitMQ | CloudAMQP ou GCE | Amazon MQ |
| Imagens | Artifact Registry | ECR |
| **Escala do Chat (N>1)** | **Memorystore (Redis)** + sticky no LB | **ElastiCache (Redis)** + sticky no ALB |

### Escala horizontal

Auth e Frontend escalam horizontalmente sem mudanca de codigo.

**Chat (implementado no compose + codigo):**

| Peca | Como |
|------|------|
| 2 replicas | `chat-a` + `chat-b` no `docker-compose.yml` |
| Sticky | nginx `ip_hash` → mesmo cliente no mesmo pod |
| Redis backplane | SignalR `AddStackExchangeRedis` |
| Presenca Redis + TTL 60s | `RedisPresenceTracker` + heartbeat a cada 20s |
| RabbitMQ | eventos entre qualquer replica |

**Nuvem (quando sair da VM unica):** ElastiCache / Memorystore + sticky no ALB / Cloud LB — ver exemplos:

- [`infra/aws/sticky-alb.example.tf`](aws/sticky-alb.example.tf)
- [`infra/gcp/sticky-lb.example.tf`](gcp/sticky-lb.example.tf)

Mais contexto: [README raiz](../README.md#escala-horizontal--ponto-critico-que-identifiquei).
