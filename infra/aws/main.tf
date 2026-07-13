data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_ami" "ubuntu" {
  most_recent = true
  owners      = ["099720109477"] # Canonical

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

resource "aws_vpc" "main" {
  cidr_block           = "10.20.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = {
    Name = "${var.name_prefix}-vpc"
  }
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.main.id

  tags = {
    Name = "${var.name_prefix}-igw"
  }
}

resource "aws_subnet" "public" {
  vpc_id                  = aws_vpc.main.id
  cidr_block              = "10.20.1.0/24"
  availability_zone       = data.aws_availability_zones.available.names[0]
  map_public_ip_on_launch = true

  tags = {
    Name = "${var.name_prefix}-public"
  }
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }

  tags = {
    Name = "${var.name_prefix}-public-rt"
  }
}

resource "aws_route_table_association" "public" {
  subnet_id      = aws_subnet.public.id
  route_table_id = aws_route_table.public.id
}

resource "aws_security_group" "app" {
  name        = "${var.name_prefix}-app-sg"
  description = "Roboteasy app (HTTP + SSH)"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = [var.allowed_cidr]
  }

  ingress {
    description = "App HTTP"
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = [var.allowed_cidr]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.name_prefix}-app-sg"
  }
}

resource "aws_key_pair" "deployer" {
  key_name   = "${var.name_prefix}-key"
  public_key = var.ssh_public_key
}

locals {
  user_data = <<-EOT
    #!/bin/bash
    set -euxo pipefail
    export DEBIAN_FRONTEND=noninteractive

    apt-get update -y
    apt-get install -y ca-certificates curl git

    if ! command -v docker >/dev/null 2>&1; then
      curl -fsSL https://get.docker.com | sh
      usermod -aG docker ubuntu || true
    fi

    systemctl enable docker
    systemctl start docker

    APP_DIR=/opt/roboteasy
    rm -rf "$APP_DIR"
    git clone --branch ${var.git_branch} --depth 1 "${var.git_repo_url}" "$APP_DIR"
    cd "$APP_DIR"

    cat > docker-compose.override.yml <<OVERRIDE
    services:
      auth:
        environment:
          Jwt__Key: "${var.jwt_key}"
      chat:
        environment:
          Jwt__Key: "${var.jwt_key}"
    OVERRIDE

    sed -i 's/^    //' docker-compose.override.yml

    docker compose up -d --build
  EOT
}

resource "aws_instance" "app" {
  ami                    = data.aws_ami.ubuntu.id
  instance_type          = var.instance_type
  subnet_id              = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.app.id]
  key_name               = aws_key_pair.deployer.key_name
  user_data              = local.user_data

  root_block_device {
    volume_size = var.disk_size_gb
    volume_type = "gp3"
  }

  tags = {
    Name    = "${var.name_prefix}-ec2"
    App     = "roboteasy"
    Managed = "terraform"
  }
}

resource "aws_eip" "app" {
  instance = aws_instance.app.id
  domain   = "vpc"

  tags = {
    Name = "${var.name_prefix}-eip"
  }
}
