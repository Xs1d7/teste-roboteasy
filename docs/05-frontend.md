# Frontend

App Vue no ar com:

1. Landing (posicionamento do produto)
2. Auth (login/cadastro + passo opcional de avatar apos cadastro)
3. Online + chat SignalR (alterar/adicionar foto na lista de usuarios)
4. Indicador de mensagens **nao lidas** na lista (+ Notification API opcional)

UI: **Tailwind CSS v4** + **shadcn-vue** (Reka UI) — `Button`, `Card`, `Avatar`, `Badge`, `Bubble`/`Message`, `ScrollArea`, etc.

Avatar: `POST /api/users/avatar` (multipart) via Auth store; exibicao via URL relativa `/api/users/avatar/{key}`.

## Nao lidas (in-app)

Quando o usuario esta na lista (sem chat aberto) e recebe mensagem:

- Badge com contagem no usuario
- Preview da ultima mensagem
- Destaque visual na linha
- Contagem no titulo da pagina: `(3) Roboteasy`
- Ordenacao: quem tem nao lidas sobe

Ao abrir a conversa, marca como lida.

## Proxy (Docker)

No container, o `nginx.conf` faz:

- `/api/auth`, `/api/users` → Auth
- `/api/messages`, `/api/users/online`, `/hubs/` → upstream **chat-a + chat-b** com `ip_hash` (sticky)

Assim o WebSocket nao salta de replica no meio da sessao; Redis cobre o cruzamento entre pods.
