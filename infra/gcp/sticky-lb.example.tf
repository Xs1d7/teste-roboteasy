# Exemplo — session affinity no Cloud Load Balancing (Cloud Run / GCE NEG + N instancias Chat)
#
# Nao aplicado automaticamente pelo `terraform apply` desta pasta (demo = 1 GCE).
# Use quando evoluir para LB na frente de varias instancias do Chat.
#
# Complementa Redis (Memorystore) backplane: affinity reduz troca de instancia;
# Redis continua obrigatorio para Clients.User / presenca entre instancias.

# resource "google_compute_backend_service" "chat" {
#   name                  = "${var.name_prefix}-chat"
#   protocol              = "HTTP"
#   load_balancing_scheme = "EXTERNAL_MANAGED"
#   timeout_sec           = 3600 # WebSocket longo
#
#   # GENERATED_COOKIE = sticky no LB
#   session_affinity = "GENERATED_COOKIE"
#   affinity_cookie_ttl_sec = 86400
#
#   backend {
#     group = google_compute_region_network_endpoint_group.chat.id
#   }
#
#   health_checks = [google_compute_health_check.chat.id]
# }
#
# Alternativa CLIENT_IP (similar ao ip_hash do nginx local):
#   session_affinity = "CLIENT_IP"
