# Prompt 01: revisão da Seção 3 (personagens)

- **Data:** 2026-11-25
- **Para:** agente `art-director` (diretor de arte)
- **Origem:** retorno do criador sobre o protótipo (protagonista e discípulo do chapéu de palha)
- **Resultado:** Seção 3 aprovada e gravada em `design/art/art-bible.md`
- **Idioma:** o pedido ao agente foi escrito em inglês; a resposta veio em português

## Pedido enviado ao agente

```text
You are revising Section 3 (Shape Language) of the SpiritOfMurin art bible, which you drafted in a previous session. Do NOT write any files — return the draft text only; the orchestrating conversation handles approval and writing.

Read for context (by section; don't read whole files unnecessarily):
- `design/art/art-bible.md` — Sections 1 (Visual Identity Statement) and 2 (Mood & Atmosphere) are approved and locked. Section 1's one-line rule is roughly "Tudo o que importa se lê pela linha antes da cor"; respect it.
- `design/art/art-bible-pendente.md` — your previous Section 3 draft (Princípios 1–7) and your Section 4 colour draft (not yet shown to the creator).
- `design/gdd/game-concept.md` — pillars and visual anchor "Todo Golpe Deixa Marca" (pixel art, 32×32 tiles, 64×64 character frames, 4 directions, 640×360 native, top-down 3/4 camera, PC).

The creator's feedback on the Section 3 characters (translated faithfully from Portuguese):
- Main reference: the manhwa *Tales of a Regressor's Cultivation* (Contos de Cultivação de um Regressor). Characters must be ORIGINAL, but with the posture and presence of Murim martial artists — more human, and clearly distinguishable from each other.
- Protagonist (swordsman-cultivator), problems with the current prototype sprite: the tunic reads as a straight block; arms merge into the body; you can barely tell he carries a sword.
  - Wanted: a 3/4 rest pose that already reveals the swordsman — one foot forward, weight on the back leg, shoulders slightly turned, blade visible beside the body.
  - Clothing to try: light cross-collar tunic (gola cruzada), jade sash (faixa jade), split hem (barra dividida) that lets the legs show.
  - If ~4 heads makes the figure too compact, test a slightly more elongated proportion within the 64×64 frame.
- Closed-Guard Disciple (straw-hat): keep the straw hat. The closed guard must be his most striking trait: tilted brim, wide base, elbows close to the body, sword on the diagonal protecting the chest. At match/game size the player must be able to see where he plants his feet and which way the blade points.
- The master's statue IS the same master as the tomb's. The position of her feet gives the clue for the sidestep (passo lateral); the cut in the stone shows the sword's trajectory. (Previous draft listed her as a fused monolith with "no space under the feet" — reconcile: her feet must now be readable as a stance clue, while she stays environment-rule art.)
- For visual approval, the plan is one pose each first: protagonist at rest, disciple in guard (prompts come later in /asset-spec; a before/after comparison in the prototype bamboo grove at the same camera/zoom will decide, before animations are produced). You may add a short note in the section about what those two approval poses must show and how to judge them, but don't write the prompts.

Deliver:
1. The full revised Section 3 text in PT-BR (the creator reads Portuguese), at `thorough` density (full prose with rationale per choice, discarded alternatives, design tests), keeping Princípios 1, 2, 5, 6, 7 essentially as before unless the character changes force an edit, and rewriting Princípios 3 and 4 (and the archetype table) with the feedback above. Give concrete pixel measurements (height, head size, proportion in heads, shoulder width, blade length/thickness, foot spacing, the rest-pose stance offsets in px) that work at 64×64 / 640×360. State the elongated-proportion decision explicitly (e.g. test ~4.5 heads vs 4) with what stays readable.
2. Separately, a short list (PT-BR) of any knock-on effects on your Section 4 colour draft. In particular: the "jade sash" collides with the draft's rule that the qi jade ramp (`#24603f`/`#3f9e6a`/`#9be3b0`) is used ONLY for what the player emits — propose how to resolve (e.g. a desaturated jade worn colour outside the qi ramp, or making the sash the one sanctioned exception) with your recommendation. Also the player robe `#d6d2bf` vs "light tunic".
3. Any open questions for the creator, at most 2.

Keep it a constraint document: every rule should narrow the solution space. Ensure the characters remain legible against the environment (unoutlined, low contrast, bamboo vertical 2–4 px).
```

## Resposta do agente (resumo)

**1. Seção 3 revisada.** O texto completo foi aprovado sem mudanças de conteúdo e está gravado em `design/art/art-bible.md`, Seção 3. Pontos principais:
- proporção alongada de ~4,6 cabeças (46 px, cabeça de 10 px; o ganho vai para as pernas);
- protagonista em repouso 3/4: pé à frente, peso atrás, ombros girados, gola cruzada em diagonal, barra dividida, lâmina baixa a ~27° fora da perna;
- discípulo em "ampulheta": aba de 21 px inclinada, cotovelos colados, lâmina a 45° cruzando o peito, base de 14–16 px com perneiras claras;
- estátua da mestra: pés em passo lateral, pegada gravada que acende em ciano, lâmina de pedra apontando para o início do arco;
- regras gerais: linhas internas no tom da veste, sombra de contato sob cada pé, lâmina só em ângulos limpos e nunca vertical em repouso;
- aprovação visual: duas poses (protagonista em repouso, discípulo em guarda) julgadas no bambuzal do protótipo com cinco testes.

**2. Efeitos na Seção 4 (cores):**
1. A faixa jade colide com a regra "a rampa do qi é só para o que o jogador emite". Opções: (a) jade gasto fora da rampa do qi, `#3a5a4b` / `#4f7a64` (recomendado); (b) faixa na rampa do qi; (c) variante futura de (a), uma versão gasta por caminho.
2. Túnica do jogador: rampa `#d6d2bf` / `#b3ad97` / `#8a846f`. Colisão de valor com a lâmina (aço L\* ~80) quando ela passa na frente da túnica; resolve com contorno dos dois lados.
3. Calça escura nova: L\* ~24–28, pelo menos 10 abaixo do chão.
4. Discípulo: corpo mais escuro (L\* ~38–42), chapéu e perneiras claros (L\* 65–70), esquema de valor invertido em relação ao protagonista.
5. Orçamento: faixa e calça levam a paleta de 47 para ~51 cores, acima de 48.

**3. Questões levantadas:**
1. Traço comum entre estátua e túmulo. Proposta: coque alto com grampo na diagonal. **Decidido:** coque com grampo.
2. Mão da espada no espelhamento (3 direções + 1 espelhada, ou 4 desenhadas). **Decidido:** aceitar o espelhamento.
