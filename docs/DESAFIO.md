# Desafio Tecnico — Desenvolvedor Full Stack

Documento original do teste. A solucao implementada esta descrita no [README](../README.md).

## Objetivo

Criar um **chat em tempo real** com autenticacao de usuarios, listagem de usuarios disponiveis e trocas de mensagens.

## Como participar

1. **Fork** este repositorio para a sua conta do GitHub.
2. Desenvolva a solucao no seu fork.
3. Apos finalizar, **abra um Pull Request (PR)** para este repositorio.
4. Aguarde o feedback da equipe.

## Requisitos do desafio

### Backend

API REST + WebSockets em **C# (.NET)** ou **Java (Spring Boot)**:

- Autenticacao e registro com **JWT**
- Endpoint de usuarios **conectados** (online)
- Mensagens em tempo real via **WebSockets**
- **Historico** de mensagens (MongoDB ou outro banco)

### Frontend

Aplicacao **Vue.js** com tres telas:

- Login (usuario/senha + cadastro)
- Usuarios disponiveis (clique abre chat)
- Conversa (historico + envio em tempo real)

### Docker

- Dockerfile + `docker-compose.yml`
- README com instrucoes de execucao

## Tecnologias sugeridas

**Backend:** .NET ou Spring Boot, JWT, WebSockets, banco relacional/documento, Docker.

**Frontend:** Vue.js, Axios/Fetch, WebSockets.

## O que sera avaliado

- Codigo estruturado e organizado
- Boas praticas (Clean Code, SOLID)
- Seguranca na autenticacao
- Uso correto de WebSockets
- Uso eficiente do banco de dados
- Documentacao clara

## Prazo

5 dias.
