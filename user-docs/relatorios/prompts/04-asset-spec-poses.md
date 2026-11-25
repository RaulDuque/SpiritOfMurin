# Prompt 04: /asset-spec das poses de aprovação

- **Data:** 2026-11-25
- **Para:** agentes `art-director` e `technical-artist` (em paralelo)
- **Resultado:** specs aprovadas e gravadas em `design/assets/specs/approval-poses-assets.md`
- **Prancha das escolhas:** `../2026-11-25-asset-spec-poses.html`

## Os dois prompts de imagem (para usar no ChatGPT)

Estão completos, prontos para copiar, em `design/assets/specs/approval-poses-assets.md`:
- **ASSET-001**, protagonista em repouso. Anexe `user-docs/assets/protagonista/image.png` ao gerar; o prompt diz para usá-la só para traje e cores.
- **ASSET-002**, discípulo do chapéu de palha em guarda.

Depois de gerar, siga a "Limpeza à mão" de cada asset na mesma spec.

## Pedido enviado ao art-director

```text
Asset specs for two SpiritOfMurin approval poses. Do NOT write files — return text only, in PT-BR.

Read (by section — Grep `^#` then Read ranges) `design/art/art-bible.md`: Section 3 (Princípio 3 incl. 3.1–3.2, Princípio 4 §4.1 protagonist, §4.2 disciple, §4.5 approval poses), Section 4 (§4.4 qi e personagens, §4.2 value bands), Section 5 (§5.4 three states, §5.5 pivot, §5.6 detail), Section 8.2 (AI images are reference only, cleaned by hand), Section 9.1 (prompts never name the manhwa or "in the style of") and 9.9 (creator's reference images and the mandatory translations). Also read `user-docs/assets/protagonista/promp.md` (the creator's own prompt) and look at `user-docs/assets/protagonista/image.png`.

Assets:
- ASSET-001 — Espadachim-cultivador (protagonista), pose de REPOUSO, direção S (frente 3/4), one 64×64 frame.
- ASSET-002 — Discípulo da Guarda Fechada, pose de GUARDA, direção S, one 64×64 frame.

Context: the creator generates images with ChatGPT (image generation in chat, no API), writes prompts in Portuguese, then cleans the result by hand in LibreSprite/Pixelorama/Krita down to native 64×64 with the 64-colour master palette. The two poses will be judged by a before/after inside the prototype's bamboo grove (same camera/zoom) before any animation is made. The creator's reference image shows the protagonist with a wide, guard-like stance and a near-horizontal blade and a vivid green sash — Section 9.9 says to translate these (rest base 9–10 px, weight on back foot, blade ~27° from vertical with tip 3–5 px above ground, worn jade #4f7a64/#3a5a4b).

For EACH asset give:
1. Visual description, 2–3 sentences, specific enough that two artists would produce consistent results.
2. Art bible anchors (cite sections).
3. A ready-to-use image-generation prompt IN PORTUGUESE, written for ChatGPT image generation (natural-language paragraphs like the creator's prompt, not Midjourney keyword soup). [...] Include a short "Evite:" list [...]. Never name the manhwa or any existing work.
4. A short "limpeza à mão" checklist [...] and the five approval tests of §4.5.

For ASSET-001, write the prompt as a REVISION of the creator's existing prompt (keep what matches the bible, fix what the bible translates), and list in one line what changed from their prompt.

Also: one line on how the before/after should be composed (what stays identical) — no engine/tool choices.
```

(Os trechos `[...]` resumem instruções de formato já refletidas na spec: pose em termos de corpo, roupa e cores com hex, fundo, contorno, luz e altura na grade de 64×64.)

## Resposta do art-director (resumo)

- **Conflito de medidas:** com o cabo a 20–22 px, uma lâmina de 22 px a 1:2 toca o chão. Opções: A (mão na faixa, recomendada), B (lâmina a 45°), C (lâmina mais curta). **Decidido: A.**
- **Latão contra âmbar:** a guarda em latão cairia no âmbar reservado (4.3). Trocada por bronze escuro e opaco.
- **Cores do discípulo** propostas: corpo `#555d5e` / `#3f4647`, palha `#b0a382`. **Aprovadas.**
- **Braço livre do discípulo:** a bíblia não dizia se é o braço livre ou se as duas mãos seguram o cabo. **Decidido:** espada de uma mão, braço livre no peito. O criador pediu que a pegada siga a arma, porque haverá outras armas no futuro; isso virou a regra 5.7 do art bible.
- **Fundo dos prompts:** cinza chapado em vez de transparente, porque o "transparente" do gerador costuma vir com xadrez pintado ou bordas suavizadas.
- Os prompts completos, as descrições e as listas de limpeza estão na spec.

## Pedido enviado ao technical-artist

```text
Technical asset spec for two SpiritOfMurin sprites. Do NOT write files — return text only, in PT-BR.

Assets:
- ASSET-001 — protagonist (espadachim-cultivador), REST pose, direction S, single 64×64 frame.
- ASSET-002 — Closed-Guard Disciple, GUARD pose, direction S, single 64×64 frame.

Read: `design/art/art-bible.md` Section 8 [...], Section 5.5, Section 4.8. `project.yaml` has NO engine and NO performance/naming blocks; `.claude/docs/technical-preferences.md` is all "[TO BE CONFIGURED]". Do not invent budgets or assert engine-specific facts; mark engine-dependent items as "não confirmado — rever no /setup-engine".

The current game is a one-shot browser prototype at `prototypes/spiritofmurin/index.html` [...]. The creator wants a BEFORE/AFTER comparison: the same bamboo-grove spot, same camera and zoom, once with the current procedural characters and once with the new PNG sprites. [...]

For each asset specify: exact dimensions; file format and export settings; file name per the bible's convention; source file name/location; pivot/anchor and how it maps to the prototype's actor position; palette check; anything that would break the comparison.

Then propose, as a short technical note (not code), the least invasive way to do the before/after in the prototype [...]. Also say where the evidence screenshots should go and suggest file names.

Flag any conflict between the art bible standard and technical constraints.
```

## Resposta do technical-artist (resumo)

- A origem (32, 59) coincide com o `(x, y)` que o protótipo já usa para os pés. Os dois assets cabem em 64×64 sem camada de efeito.
- **Riscos da comparação:**
  - a diferença de altura é intencional (proporção nova);
  - a sombra pode sair duplicada;
  - a câmera se move por interpolação e precisa ser fixada;
  - o contorno do sprite não acende como o procedural;
  - só existe a direção S;
  - o cenário ainda não usa a paleta-mestra.
- **Proposta:** interruptor `?sprites=1` na URL, um único ponto de troca em `drawFighter`, PNGs copiados para o protótipo com procedência, captura determinística e evidência em `production/qa/evidence/`.
- **Conflitos levantados e como ficaram:**
  - sombra: no sprite só os pixels de contato, a elipse fica no jogo (**adotado**);
  - linha 59: é o contorno da sola (**adotado**);
  - sufixo `_00` (**adotado**);
  - a paleta `pal_master_64.gpl` não existe (**pendente**);
  - `_ph`: estes assets são candidatos de aprovação, não placeholders;
  - há duas cópias dos PNGs (raiz e protótipo);
  - a subpasta de `assets/` não está definida (**pendente**).
