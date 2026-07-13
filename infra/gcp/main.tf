resource "google_compute_network" "vpc" {
  name                    = "${var.name_prefix}-vpc"
  auto_create_subnetworks = false
}

resource "google_compute_subnetwork" "subnet" {
  name          = "${var.name_prefix}-subnet"
  ip_cidr_range = "10.10.0.0/24"
  region        = var.region
  network       = google_compute_network.vpc.id
}

resource "google_compute_firewall" "allow_app" {
  name    = "${var.name_prefix}-allow-app"
  network = google_compute_network.vpc.name

  allow {
    protocol = "tcp"
    ports    = ["8080", "22"]
  }

  source_ranges = [var.allowed_cidr]
  target_tags   = ["${var.name_prefix}-app"]
}

locals {
  startup_script = <<-EOT
    #!/bin/bash
    set -euxo pipefail
    export DEBIAN_FRONTEND=noninteractive

    apt-get update -y
    apt-get install -y ca-certificates curl git

    if ! command -v docker >/dev/null 2>&1; then
      curl -fsSL https://get.docker.com | sh
      usermod -aG docker ${var.ssh_user} || true
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

    # remove indentacao acidental do heredoc
    sed -i 's/^    //' docker-compose.override.yml

    docker compose up -d --build
  EOT
}

resource "google_compute_instance" "app" {
  name         = "${var.name_prefix}-vm"
  machine_type = var.machine_type
  zone         = var.zone
  tags         = ["${var.name_prefix}-app"]

  boot_disk {
    initialize_params {
      image = "ubuntu-os-cloud/ubuntu-2204-lts"
      size  = var.disk_size_gb
      type  = "pd-balanced"
    }
  }

  network_interface {
    subnetwork = google_compute_subnetwork.subnet.id
    access_config {}
  }

  metadata = {
    ssh-keys       = "${var.ssh_user}:${var.ssh_public_key}"
    startup-script = local.startup_script
  }

  labels = {
    app     = "roboteasy"
    managed = "terraform"
  }

  scheduling {
    automatic_restart   = true
    on_host_maintenance = "MIGRATE"
  }
}
