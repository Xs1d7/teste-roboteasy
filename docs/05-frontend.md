# Frontend

App Vue no ar com:

1. Landing (posicionamento do produto)
2. Auth (login/cadastro)
3. Online + chat SignalR
4. Indicador de mensagens **nao lidas** na lista (+ Notification API opcional)

## Nao lidas (in-app)

Quando o usuario esta na lista (sem chat aberto) e recebe mensagem:

- Badge com contagem no usuario
- Preview da ultima mensagem
- Destaque visual na linha
- Contagem no titulo da pagina: `(3) Roboteasy`
- Ordenacao: quem tem nao lidas sobe

Ao abrir a conversa, marca como lida.

## Notificacoes do sistema (opcional)

Botao **Ativar notificacoes** so aparece se a permissao ainda e `default`.
Se o navegador bloqueou, a lista de nao lidas continua funcionando — e o feedback diz isso.
