variable "region" {
  type        = string
  description = "Regiao AWS"
  default     = "us-east-1"
}

variable "name_prefix" {
  type        = string
  description = "Prefixo dos recursos"
  default     = "roboteasy"
}

variable "instance_type" {
  type        = string
  description = "Tipo da instancia EC2"
  default     = "t3.medium"
}

variable "disk_size_gb" {
  type        = number
  description = "Tamanho do disco root (GB)"
  default     = 40
}

variable "ssh_public_key" {
  type        = string
  description = "Chave publica SSH"
}

variable "git_repo_url" {
  type        = string
  description = "URL HTTPS do repositorio"
}

variable "git_branch" {
  type        = string
  description = "Branch a clonar"
  default     = "main"
}

variable "jwt_key" {
  type        = string
  description = "Segredo JWT"
  sensitive   = true
  default     = "roboteasy-dev-secret-key-change-me-32chars!"
}

variable "allowed_cidr" {
  type        = string
  description = "CIDR liberado para HTTP :8080 e SSH :22"
  default     = "0.0.0.0/0"
}
