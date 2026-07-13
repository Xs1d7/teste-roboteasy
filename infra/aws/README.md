# AWS — Roboteasy

Sobe EC2 Ubuntu (VPC dedicada + Elastic IP) com Docker Compose.

## Auth

```bash
aws configure
# ou AWS_PROFILE / variaveis de ambiente
```

## Apply

```bash
cp terraform.tfvars.example terraform.tfvars
# edite ssh_public_key, git_repo_url, jwt_key, region
terraform init
terraform plan
terraform apply
```

Abra o `app_url` do output. Logs:

```bash
ssh -i <chave> ubuntu@<IP> 'sudo cat /var/log/cloud-init-output.log'
ssh -i <chave> ubuntu@<IP> 'cd /opt/roboteasy && sudo docker compose logs -f'
```

## Destroy

```bash
terraform destroy
```

Custo tipico do demo: `t3.medium` + EIP + disco 40 GB — destruir apos a avaliacao.
