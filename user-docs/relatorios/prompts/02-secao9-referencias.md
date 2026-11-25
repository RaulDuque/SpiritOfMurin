# Prompt 02: Seção 9 (referências)

- **Data:** 2026-11-25
- **Para:** agente `art-director` (diretor de arte)
- **Resultado:** Seção 9 aprovada e gravada em `design/art/art-bible.md` (itens 9.1–9.8)
- **Idioma:** o pedido ao agente foi escrito em inglês; a resposta veio em português

## Pedido enviado ao agente

```text
Draft Section 9 (Reference Direction) of the SpiritOfMurin art bible. Do NOT write any files — return the draft text only, in PT-BR (the creator reads Portuguese), at `thorough` density.

Inputs:
- `design/art/art-bible.md` — Sections 1–8 are approved and written. Read by section (Grep `^## ` / `^### ` then Read ranges); Section 1 (visual rule "Tudo o que importa se lê pela linha antes da cor"), Section 3 (shape language, characters) and Section 4 (colour) matter most.
- `design/gdd/game-concept.md` — sections "Visual Identity Anchor" and "Inspiration and References".

Constraint decided earlier by the creator: **references come only from those already in the concept** — the manhwa *Tales of a Regressor's Cultivation* (main reference for character presence), *Logging into the Murim*, *Leveling Up Infinitely: Murim*; games Castlevania: Symphony of the Night, Hollow Knight, Noita, Magicka, Minecraft, Vampire Survivors. Pick 3–5 of these that give genuinely distinct visual lessons (no two pointing the same way). Do not add new titles. If you believe an important visual gap remains that only an outside reference would fill, mention it in one line at the end as a suggestion, clearly marked as not part of the section.

For each reference: name it; the SPECIFIC visual element to take (a technique, a colour choice, a compositional rule — never "the general aesthetic"), tied to the section/rule in this bible it supports; and what to explicitly avoid or diverge from so the result doesn't read as a copy (characters must be original — take presence/posture, never costumes, faces or designs). Remember the game is 640×360 pixel art, top-down 3/4, 32×32 tiles, 64×64 character frames — note where a reference's medium (e.g. manhwa full-colour illustration, side-view platformer) must be translated rather than imitated.

Also give a short "como usar as referências" rule (e.g. a reference never overrides a section of this bible) and a design test.
```

## Resposta do agente (resumo)

O texto completo foi aprovado e está gravado em `design/art/art-bible.md`, Seção 9. As cinco referências escolhidas:

| Referência | Lição |
|---|---|
| *Contos de Cultivação de um Regressor* | presença: peso num pé, ombros girados, lâmina parada, quietude antes do golpe |
| *Hollow Knight* | separação entre figura e fundo pelo valor; névoa em camadas planas |
| *Castlevania: Symphony of the Night* | animação: pose-chave segurada, movimento secundário atrasado, traço da arma por cima do sprite |
| *Noita* | efeito na mesma grade de pixels do mundo; contra-lição sobre o excesso de efeitos |
| *Magicka* | fila de comandos visível enquanto se digita |

Ficaram fora *Logando no Murim* e *Upando Infinitamente* (repetiriam a primeira), *Vampire Survivors* e *Minecraft* (contribuem com design, não com visual).

**Sugestão do agente fora da seção:** *Hyper Light Drifter* cobriria a falta de uma referência de combate de espada visto de cima. **Decidido:** descartada, para manter só as referências do conceito.
