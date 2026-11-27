# GUIA.md para qualquer agente de IA — 2026-11-27

## Pedido

O criador quer um único arquivo que baste abrir em qualquer agente de IA para ele saber o que estudar e entender o projeto.

## O que foi criado

- **`GUIA.md`** (raiz do projeto). Tem quatro partes:
  1. o projeto em 30 segundos;
  2. a ordem de estudo, com os itens obrigatórios marcados com ★: regras de trabalho → onde paramos → visão e design → código (com o mapa de `src/`) → testes → o que só ler se precisar;
  3. as regras que não podem ser quebradas;
  4. a primeira mensagem sugerida ao criador.

  Ele também avisa que o `README.md` e o `AGENTS.md` da raiz são do framework, não do jogo, e que os arquivos `*-SOLUCAO.md` contêm spoilers.
- **`.cursor/rules/spiritofmurin.mdc`** agora manda ler o `GUIA.md` primeiro.

## Como usar

Em qualquer ferramenta, comece com: **"Leia o `GUIA.md` e estude o que ele indica antes de começar."**

Quando o projeto mudar bastante (sistema novo, pasta nova em `src/`, GDD novo), peça para atualizar o guia.

## Verificação

- Todos os caminhos citados no guia foram conferidos no disco.
- `dotnet build`: 0 erros.
- Nenhum teste de jogo se aplica, porque a entrega só muda documentação.
