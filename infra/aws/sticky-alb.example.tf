# Exemplo — sticky session no Application Load Balancer (ECS Fargate + N tasks Chat)
#
# Nao aplicado automaticamente pelo `terraform apply` desta pasta (demo = 1 EC2).
# Use quando evoluir para ECS/ALB com varias tasks do Chat.
#
# Complementa Redis backplane: sticky reduz troca de pod no meio do WebSocket;
# Redis continua obrigatorio para Clients.User / presenca entre tasks.

# resource "aws_lb_target_group" "chat" {
#   name        = "${var.name_prefix}-chat"
#   port        = 8080
#   protocol    = "HTTP"
#   vpc_id      = aws_vpc.main.id
#   target_type = "ip"
#
#   stickiness {
#     enabled         = true
#     type            = "lb_cookie"
#     cookie_duration = 86400
#   }
#
#   health_check {
#     path = "/health"
#   }
# }
#
# No listener rule / target group attachment das tasks Chat:
# stickiness.type = "lb_cookie"  (acima)
#
# Para WebSocket long-lived, cookie stickiness + Redis backplane e o combo padrao.
