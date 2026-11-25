# Relatório da sessão: art bible (/art-bible)

- **Data:** 2026-11-25 (segunda sessão do dia)
- **Documento:** `design/art/art-bible.md`
- **Escopo:** guia completo (9 seções). As Seções 1 e 2 já estavam aprovadas desde a sessão anterior.

## O que foi feito

| Seção | Estado | Quem escreveu | Decisões do criador nesta sessão |
|---|---|---|---|
| 1. Identidade Visual | aprovada antes; corrigida na revisão final | diretor de arte (prompt 03) | exemplo dos três estados atualizado |
| 2. Clima e Atmosfera | aprovada antes; corrigida na revisão final | diretor de arte (prompt 03) | ocre só no contra-ataque do discípulo |
| 3. Linguagem de Formas | aprovada e gravada | diretor de arte (prompt 01) | protagonista e discípulo redesenhados; proporção ~4,6 cabeças; estátua = mestra do túmulo; espelhamento aceito (3 direções + 1 espelhada); traço da mestra = coque alto com grampo na diagonal |
| 4. Sistema de Cores | aprovada e gravada | diretor de arte (rascunho da sessão anterior + efeitos do prompt 01) | qi em jade verde ~145°; faixa em jade gasto; sênior em índigo-ardósia; sem ciano nos menus; paleta de 64 cores |
| 5. Direção de Personagens | aprovada e gravada | conversa, pelo guia do diretor de arte | a lâmina visível distingue inimigo de NPC; os três estados; ponto de apoio (32, 59) |
| 6. Linguagem do Ambiente | aprovada e gravada | conversa, pelo guia do diretor de arte | duas eras pelo material; bambu com base sólida e corpo passável; travessia de água legível pela linha |
| 7. Interface e HUD | aprovada e gravada | conversa (visão de arte + visão de UX) | conflito arte × UX: rótulo de estado do alvo só até aprender, com opção para manter sempre |
| 8. Padrões de Asset | aprovada e gravada | conversa (visão de arte + visão técnica) | corte em 64×64 com camada de efeito; silhueta do ator através do cenário |
| 9. Referências | aprovada e gravada | diretor de arte (prompt 02) | cinco referências do conceito; *Hyper Light Drifter* descartado |
| 9.9 Referências visuais do projeto | gravada nesta etapa | conversa | imagens do protagonista e do mundo registradas, com as traduções obrigatórias |

## Retorno do criador sobre o protótipo (registrado)

- Gosta do cenário atual: bambuzal, trilha e água.
- O bambuzal bloqueia por inteiro. O criador quer passar por trás dele ou por dentro (Seção 6.5).
- A água ainda não pode ser atravessada. A expectativa é uma técnica de movimento (Seção 6.5, depende do design).
- Prioridade desta etapa: **os personagens**.

## Imagens de referência do criador

| Imagem | Prompt | Uso |
|---|---|---|
| `user-docs/assets/protagonista/image.png` | `user-docs/assets/protagonista/promp.md` | túnica marfim de gola cruzada, faixa jade gasta, fita no cabelo, espaço entre a espada e o corpo |
| `user-docs/assets/mundo/image.png` | `user-docs/assets/mundo/mundo-promp.md` (**arquivo vazio** em 2026-11-25) | ambientação e expansão do vale |

## Pendências

1. ~~Aprovar as correções da revisão final~~ **Feito:** gravadas; a vida do jogador ficou com o vermelho escuro gasto `#7a3a32`; revisão `REVISED 2026-11-25`.
2. ~~Registrar o resultado da revisão no cabeçalho~~ **Feito.**
3. **Próxima etapa, `/asset-spec`:**
   - primeiro, dois prompts de imagem: protagonista em repouso e discípulo em guarda;
   - depois, antes e depois sobre o mesmo bambuzal do protótipo, com a mesma câmera e o mesmo zoom;
   - o criador escolhe jogando, **antes** de qualquer animação.
4. `/setup-engine`: rever a Seção 8 (motor, orçamento, troca de paleta, técnica da silhueta através do cenário).
5. Corrigir no protótipo os desvios listados na Seção 7: barra de vida do inimigo em ocre, seleção em ciano, fonte Courier New, vida do jogador em verde.
