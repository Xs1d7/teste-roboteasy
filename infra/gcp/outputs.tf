output "instance_name" {
  value       = google_compute_instance.app.name
  description = "Nome da VM"
}

output "external_ip" {
  value       = google_compute_instance.app.network_interface[0].access_config[0].nat_ip
  description = "IP publico"
}

output "app_url" {
  value       = "http://${google_compute_instance.app.network_interface[0].access_config[0].nat_ip}:8080"
  description = "URL do frontend (nginx)"
}

output "ssh_command" {
  value       = "ssh ${var.ssh_user}@${google_compute_instance.app.network_interface[0].access_config[0].nat_ip}"
  description = "Comando SSH"
}
