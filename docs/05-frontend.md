# Frontend

App Vue no ar com:

1. Landing (posicionamento do produto)
2. Auth (login/cadastro)
3. Online + chat SignalR
4. Notificacao do navegador ao receber mensagem (Notification API)

## Notificacoes

- Pedido de permissao ao conectar no hub (ou botao **Ativar notificacoes** na lista)
- Dispara quando chega `ReceiveMessage` de outro usuario
- **Nao** notifica se a aba esta visivel **e** a conversa com o remetente ja esta aberta
- Clique na notificacao abre o chat com aquele usuario
