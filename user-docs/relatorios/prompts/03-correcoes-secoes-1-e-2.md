# Prompt 03: correções das Seções 1 e 2 (revisão final)

- **Data:** 2026-11-25
- **Para:** agente `art-director` (diretor de arte)
- **Origem:** revisão final AD-ART-BIBLE (ver `../2026-11-25-ad-art-bible-revisao.md`)
- **Resultado:** aprovado pelo criador e gravado no art bible em 2026-11-25, com as duas frases extras e o ajuste do quadro-resumo.

## Pedido enviado ao agente

```text
Small revision of two lines in the SpiritOfMurin art bible (`design/art/art-bible.md`), which you drafted. Do NOT write files — return only the replacement text, in PT-BR, matching the surrounding style.

The AD-ART-BIBLE review found two contradictions with sections approved later:

1. Section 1, line 40 (read lines 34–43 for context). Current text:
"- Repouso, guarda e início de corte diferem em **pelo menos duas das três linhas** (ex.: repouso com pés juntos e lâmina baixa junto ao corpo; guarda fechada com base larga e lâmina cobrindo o tronco; início de corte com peso transferido e lâmina saindo do eixo do corpo)."
Problem: Section 3 §4.1 (lines ~190–225) now defines the player's rest as one foot forward, weight on the back leg, shoulders turned, blade low at ~27° OUTSIDE the leg silhouette; and Section 5.2 (line ~466) makes "pés juntos (base ≤ 6 px)" the sign of a neutral NPC. Section 5.4 (read the table, lines ~485–495) has the three states for player and disciple. Rewrite the example so it agrees with Sections 3 and 5.4 (keep it a single bullet, keep the "pelo menos duas das três linhas" rule).

2. Section 2, line 106 (Duelo: Discípulo da Guarda Fechada). Current "*Elemento:*" sentence ends: "o vento que para quando o duelo começa e o lampejo ocre ao longo da lâmina quando ele fecha a guarda ou contra-ataca."
Problem: Sections 4.3 and 5.4 reserve ocre ONLY for attack intent (the início de corte, held ≥ 2 frames); an ocre flash on closing the guard would teach "ocre = defense". Rewrite that Elemento sentence so the ocre flash is tied only to preparing the counter-attack. Give the replacement for the whole *Elemento:* sentence.

Return two labelled blocks: "Seção 1 — linha 40" and "Seção 2 — Elemento do duelo do discípulo", each with the exact replacement text.
```

## Resposta do agente (completa)

**Seção 1, linha 40**

> - Repouso, guarda e início de corte diferem em **pelo menos duas das três linhas** (ex.: repouso com pé à frente, peso atrás e lâmina baixa a ~27°, fora da silhueta da perna; guarda com base aberta, joelhos flexionados e lâmina erguida diante do tronco; início de corte com o peso passando ao pé da frente e a lâmina saindo do eixo do corpo). Pés juntos não são repouso de combatente: ficam reservados ao NPC neutro (Seção 5.2). Medidas por estado na Seção 5.4.

**Seção 2, Elemento do duelo do discípulo**

> *Elemento:* o vento que para quando o duelo começa e o lampejo ocre que corre a borda da lâmina só quando ele prepara o contra-ataque (a lâmina deixa a diagonal da guarda e fica parada ≥ 2 quadros); fechar a guarda nunca acende ocre.

**Notas do agente:**
- No bloco 1, o exemplo repete as três linhas da tabela 5.4. As duas frases finais impedem que "pés juntos = NPC" volte a ser usado para o jogador. Se o criador preferir a correção só no parêntese, as duas frases podem sair; a regra continua valendo sem elas.
- No bloco 2, a trava "nunca acende ocre" deixa explícito que ocre não significa defesa.
- A linha 76 do quadro-resumo da Seção 2 diz "lampejos ocre na lâmina" sem dizer quando. Sugestão: "O vento para; lampejo ocre só no contra-ataque".
