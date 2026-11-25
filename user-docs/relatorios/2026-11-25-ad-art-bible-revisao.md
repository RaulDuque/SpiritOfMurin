# Revisão final do art bible (AD-ART-BIBLE)

- **Data:** 2026-11-25
- **Documento revisado:** `design/art/art-bible.md`, Seções 1–9, lido do disco
- **Checklist:** `.claude/docs/director-gates/ad-art-bible.md` com o guia `.claude/skills/art-bible/references/lens-art-director.md`
- **Quem revisou:** a própria conversa do `/art-bible`, aplicando o checklist do diretor de arte. A regra da skill é não abrir outro agente para esta revisão.

## Veredito

**AD-ART-BIBLE: CONCERNS** (Seções 1, 2, 4.6/4.8)

A bíblia é coerente e permite produzir assets, mas tem duas contradições entre seções e uma lacuna pequena.

## Achados

| # | Seção | Tipo | Achado | Correção proposta |
|---|---|---|---|---|
| 1 | 1 (Princípio 2) | Contradição | O exemplo descreve o repouso como "pés juntos e lâmina baixa junto ao corpo" (a pose antiga do protótipo). Contradiz a Seção 3, 4.1 (pé à frente, lâmina fora do corpo) e a Seção 5.2 (pés juntos = NPC neutro). | Reescrever o exemplo com o repouso novo (texto do diretor de arte em `prompts/03-correcoes-secoes-1-e-2.md`). |
| 2 | 2 (duelo do discípulo) | Contradição | O lampejo ocre aparece "quando ele fecha a guarda ou contra-ataca". As Seções 4.3 e 5.4 reservam o ocre só para a intenção de ataque. Ocre ao fechar a guarda ensinaria "ocre = defesa". | Ocre só quando ele prepara o contra-ataque; fechar a guarda nunca acende ocre. Ajustar também o quadro-resumo da Seção 2. |
| 3 | 4.6 / 4.8 | Lacuna | A cor dos losangos de vida do jogador não está definida. No protótipo eles são verdes (`#4fb889`), quase na rampa do qi. A soma da distribuição da paleta (4.8) dá ~53, não ~51. | Escolha do criador: **vermelho escuro gasto** (~`#7a3a32`), com reforço para daltônicos (vida × ocre) na 4.7. Contagem: ~54 de 64. |

## Observações (não são defeitos)

- **Seção 8:** o projeto ainda não tem motor nem orçamento de desempenho. A estimativa (~2 MB por personagem) cabe folgada em PC. A revisão no `/setup-engine` já está registrada na própria seção.
- **Quadros por animação:** ainda não definidos. É assunto do `/asset-spec`, não do art bible.
- **Nível de detalhe:** as medidas em pixels da Seção 3 são metas, e o teste no bambuzal (Seção 3, 4.5) é o que decide. Não é especificação demais para um projeto solo.

## Perguntas do checklist

| Pergunta | Resposta |
|---|---|
| As cores servem aos alvos de clima? | Sim. A temperatura por área (4.5) segue a Seção 2. Única falha: o ocre do achado 2. |
| A linguagem de formas segue a identidade visual? | Sim. As três famílias de forma e o arco-linha reservado derivam da regra "linha antes da cor". |
| Os padrões de asset cabem nas restrições da plataforma? | Sim para PC, por estimativa. Não há orçamento formal (ver Seção 8). |
| A direção de personagens dá base sem especificar demais? | Sim. Medidas, os três estados e o teste de aprovação estão definidos. |
| Há contradições entre seções? | Sim: os achados 1 e 2. |
| Uma equipe externa conseguiria produzir a partir dele? | Sim, depois das correções e com o `/asset-spec` definindo os quadros por animação. |

## Estado

**Corrigido e gravado em 2026-11-25.** O criador aprovou as três correções, escolhendo o tom **A, vermelho escuro gasto `#7a3a32`**, na seção 1b da prancha `2026-11-25-escolhas-visuais.html`. O cabeçalho do art bible registra `REVISED 2026-11-25`.
