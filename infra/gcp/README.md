# Google Cloud — Roboteasy

Sobe uma VM Ubuntu com Docker Compose (mesmo stack do repo).

## Auth

```bash
gcloud auth application-default login
gcloud config set project SEU_PROJECT_ID
```

Habilite APIs (uma vez):

```bash
gcloud services enable compute.googleapis.com
```

## Apply

```bash
cp terraform.tfvars.example terraform.tfvars
# edite project_id, ssh_public_key, git_repo_url, jwt_key
terraform init
terraform plan
terraform apply
```

Abra o `app_url` do output. Logs do boot:

```bash
ssh ubuntu@<IP> 'sudo journalctl -u google-startup-scripts -f'
# ou
ssh ubuntu@<IP> 'cd /opt/roboteasy && sudo docker compose logs -f'
```

## Destroy

```bash
terraform destroy
```
