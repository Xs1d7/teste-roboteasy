# Evolucao com IA generativa

Visao de arquiteto — **nao implementado** neste desafio. Amarra o chat que ja existe a casos de uso reais de IA (RAG, assistentes, governanca).

## Por que IA aqui

O enunciado pede chat em tempo real. Para uma vaga de **Arquiteto de Software IA / RPA**, o proximo passo natural e: *como este historico (Mongo) e estes eventos (Rabbit) viram contexto util para um LLM*, sem virar chatbot generico colado no produto.

## Casos de uso (amarrados ao que ja existe)

| Caso | Valor | Dados de entrada |
|------|-------|------------------|
| **Resumo de thread** | Usuario volta a uma conversa longa e pede "o que combinamos?" | Ultimas N mensagens do par `(from, to)` no Mongo |
| **Sugestao de resposta** | Acelera atendimento humano (copilot, nao auto-reply) | Thread atual + tom opcional |
| **Busca semantica** | "Quando falamos de deploy?" em meses de historico | Embeddings das mensagens + vector store (Qdrant, pgvector, Chroma) |

Todos reutilizam o **mesmo dominio** (`ChatMessage`, usuarios do Auth) — nao precisam de novo monolito.

## Desenho alto nivel

```
Frontend / API Gateway
        |
        v
  Chat Service (.NET) ---- Mongo (historico bruto)
        |
        +--> AI Orchestrator (novo servico ou modulo)
                 |
                 +--> LLM (OpenAI / Azure OpenAI / modelo interno)
                 +--> Vector DB (busca semantica, opcional)
                 +--> Policy layer (PII, rate limit, audit log)
```

Fluxo tipico (resumo):

1. Cliente chama `POST /api/ai/summarize?with={userId}` com JWT
2. Orchestrator busca mensagens autorizadas no Mongo (so o par do usuario logado)
3. Monta prompt com template fixo + contexto truncado (token budget)
4. Chama LLM; persiste audit log (quem pediu, tokens, modelo, hash do contexto)
5. Devolve texto; **nao** grava resposta como mensagem automaticamente (evita poluir historico)

RabbitMQ pode notificar jobs assincronos (indexar embedding quando chega `message`).

## Governanca

| Tema | Abordagem |
|------|-----------|
| **PII** | Mascarar email/telefone antes do prompt; lista de campos proibidos; opt-in por tenant |
| **Autorizacao** | Mesmo JWT — so acessa threads onde o usuario participa |
| **Rate limit** | Por usuario e por feature (ex.: 10 resumos/hora) |
| **Custo** | Cache de resumo por `(threadId, lastMessageId)`; modelo menor pra draft, maior sob demanda |
| **Qualidade** | Golden set de threads anonimizadas + revisao humana; metricas de alucinacao / utilidade |
| **Seguranca** | Prompt injection: delimitar contexto, nunca executar conteudo de mensagem como instrucao |

## O que nao faria no MVP deste desafio

- LangChain/RAG no `docker-compose` do teste — aumenta superficie sem provar o core
- Bot que responde sozinho no hub SignalR — muda UX e exige moderacao
- Fine-tuning com dados de producao sem pipeline de consentimento
- Enviar historico completo sem truncamento/token budget

Spike experimental (repo separado ou pasta `spikes/`) seria o lugar certo pra provar uma chamada LLM + 20 mensagens fake — linkado no README como "fora do escopo do desafio".
