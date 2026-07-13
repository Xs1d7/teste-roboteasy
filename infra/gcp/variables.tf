variable "project_id" {
  type        = string
  description = "ID do projeto GCP"
}

variable "region" {
  type        = string
  description = "Regiao GCP"
  default     = "us-central1"
}

variable "zone" {
  type        = string
  description = "Zona GCP"
  default     = "us-central1-a"
}

variable "name_prefix" {
  type        = string
  description = "Prefixo dos recursos"
  default     = "roboteasy"
}

variable "machine_type" {
  type        = string
  description = "Tipo da VM (precisa de RAM para compose: mongo+rabbit+apis)"
  default     = "e2-standard-2"
}

variable "disk_size_gb" {
  type        = number
  description = "Disco boot da VM (GB)"
  default     = 40
}

variable "ssh_user" {
  type        = string
  description = "Usuario SSH"
  default     = "ubuntu"
}

variable "ssh_public_key" {
  type        = string
  description = "Chave publica SSH (conteudo de id_rsa.pub / id_ed25519.pub)"
}

variable "git_repo_url" {
  type        = string
  description = "URL HTTPS do repositorio (sera clonado no startup)"
}

variable "git_branch" {
  type        = string
  description = "Branch a clonar"
  default     = "main"
}

variable "jwt_key" {
  type        = string
  description = "Segredo JWT (min ~32 chars). Troque em producao."
  sensitive   = true
  default     = "roboteasy-dev-secret-key-change-me-32chars!"
}

variable "allowed_cidr" {
  type        = string
  description = "CIDR liberado para HTTP :8080 e SSH :22"
  default     = "0.0.0.0/0"
}
