output "instance_id" {
  value       = aws_instance.app.id
  description = "ID da instancia EC2"
}

output "public_ip" {
  value       = aws_eip.app.public_ip
  description = "IP publico (Elastic IP)"
}

output "app_url" {
  value       = "http://${aws_eip.app.public_ip}:8080"
  description = "URL do frontend (nginx)"
}

output "ssh_command" {
  value       = "ssh -i <sua-chave-privada> ubuntu@${aws_eip.app.public_ip}"
  description = "Comando SSH"
}
