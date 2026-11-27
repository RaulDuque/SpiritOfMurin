# Continuar o projeto com outra conta do Claude ou com o Cursor — 2026-11-27

## Pedido

O criador tem uma segunda conta do Claude e uma conta no Cursor. Ele perguntou como continuar o projeto com elas quando os tokens desta conta acabarem, e aprovou a criação dos dois arquivos abaixo.

## O que foi criado

| Arquivo | Para quê |
|---|---|
| `docs/contexto-para-assistentes.md` | Resumo das decisões que estavam só na memória local do Claude Code: preferências do criador, regra de fechamento, direção do jogo, caminhos do Godot, como rodar os testes, proibição de capturar a área de trabalho. Qualquer ferramenta consegue ler. |
| `.cursor/rules/spiritofmurin.mdc` | Regra do Cursor sempre ativa: manda ler esse resumo, o `CLAUDE.md`, a regra de commits e o checkpoint antes de qualquer tarefa. |

## Como usar

- **Outra conta do Claude:** no Claude Code, `/logout` e `/login` com a outra conta, na mesma pasta. Tudo continua funcionando: skills, hooks, agentes e memória local.
- **Cursor:** abra `D:\Progr\SpiritOfMurin`. A regra é carregada sozinha. As skills do Claude Code não rodam lá como comandos, mas o Cursor pode ler o `SKILL.md` e seguir os passos (por exemplo, `.claude/skills/subir-git/SKILL.md`).
- **Antes de trocar de ferramenta:** deixe a entrega em andamento com commit e push.
- **Mantenha o resumo em dia:** quando surgir uma decisão nova que valha para todas as ferramentas, atualize `docs/contexto-para-assistentes.md`.

## Verificação

Esta entrega só muda documentação e configuração do editor. `dotnet build` foi rodado para confirmar que o projeto continua compilando. Nenhum teste de jogo se aplica. O último resultado completo (494/494) está em [2026-11-27-pedido13-final.md](2026-11-27-pedido13-final.md).
