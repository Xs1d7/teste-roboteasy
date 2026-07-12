# Anotacoes do desafio

Antes de sair codando, anotei o que o README pede de verdade e o que da pra "esticar" sem overengineering.

## O que e obrigatorio

- Auth: cadastro + login com JWT
- Listar usuarios **conectados** (online), nao so cadastrados
- Chat em tempo real via WebSocket
- Historico de mensagens em banco
- Frontend Vue com 3 telas (login, lista, conversa)
- Docker pra subir tudo

Backend pode ser .NET ou Spring. Fui de **.NET** porque ja e o stack que quero mostrar no teste.

## Leitura do requisito vs. interpretacao

| Pedido | Como interpretei |
|--------|------------------|
| Usuarios disponiveis | Quem esta com SignalR conectado agora |
| WebSockets | SignalR (WebSocket por baixo, API mais limpa no .NET) |
| Historico | Persistencia das mensagens 1:1, buscavel por par de usuarios |
| Docker | compose com app + dependencias, um comando e sobe |

## O que **nao** esta no enunciado (e nao vou forcar)

- Grupos / salas publicas
- Upload de arquivo
- Refresh token / OAuth social
- Notificacoes push
- Testes e2e pesados

## Criterios que de avaliação

Codigo organizado, boas praticas, seguranca no auth, WebSocket certo, banco ok, README claro.

Entao o desenho precisa mostrar **separacao de responsabilidade** e **motivo** de cada escolha — nao so "funcionou na minha maquina".


## Arquitetura

Deixei a arquitetura salva no seguinte arquivo [Diagrama](.\roboteasy-diagram.excalidraw). Para visualizar, abra o site [Excalidraw](https://excalidraw.com/) e importe o arquivo. Além disso, irei criar um arquivo mais explicativo do que pensei para essa aplicação. Este se chamará [02-arquitetura](./02-arquitetura.md)