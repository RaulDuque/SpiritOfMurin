# Asset Specs — System: equipamento (T18)

> **Source**: user-docs/tasks/T18-equipamento-visual.md, design/gdd/equipment.md
> **Art Bible**: design/art/art-bible.md
> **Generated**: 2026-11-28 (/asset-spec, modo full: art-director e technical-artist consultados)
> **Status**: 3 assets specced / 3 approved (decisão 4 do criador, 2026-11-28) / 3 done (desenhados em código)

O jogo não usa sprites: a arte é desenhada em código (bíblia §8.12). Por isso "dimensões" quer dizer pixels a 1× sobre o
protagonista, e não há prompts de geração de imagem para uso no jogo. O prompt abaixo serve só como referência de conceito.

**Conflito registrado:** a bíblia §5.1 diz "sem troca de visual por equipamento no MVP". O criador decidiu fazer a troca
na T18. Para manter a leitura, os especialistas propõem uma regra para os chapéus do protagonista, como emenda à §5.1: aba
nivelada, com no máximo 16 px, olhos à vista. Houve uma **divergência entre os agentes** sobre a copa do chapéu: o
technical-artist sugeriu cone alto e o art-director, copa baixa. Ficou a copa baixa, porque o cone, a 1×, lembra o chapéu
do discípulo.

## ASSET-003 — Túnica de Viajante (`tunica_viajante`)

| Field | Value |
|-------|-------|
| Category | Roupa (camada do corpo) |
| Dimensions | matrizes 20×21 (frente e costas) e 16×21 (perfil), mangas em linhas de 2 a 5 px |
| Format | código C#: `src/Elements/Roupas/TunicaViajanteArt.cs` |
| Naming | id `tunica_viajante`; classe `TunicaViajanteArt` |

**Visual Description:** linho cru gasto, com o corte e a silhueta da túnica verde. Tem gola cruzada em diagonal de 1 px,
faixa em jade gasto com nó no lugar da fivela, punhos arregaçados, um remendo 2×2 na saia e barra irregular.

**Art Bible Anchors:** §4.4 (túnica clara `#d6d2bf`/`#b3ad97`/`#8a846f`, faixa jade gasto `#4f7a64`/`#3a5a4b`); §5.1 (a
túnica continua clara e com faixa jade); §5.6 (2×2 é o menor detalhe).

**Generation Prompt (conceito):** pixel art top-down 3/4, traveller's raw linen tunic, crossed collar, worn jade sash with
a knot, small patch, uneven hem, muted wuxia palette — negative: bright colours, embroidery, cyan.

**Status:** Done

## ASSET-004 — Chapéu de Bambu Trançado (`chapeu_bambu`)

| Field | Value |
|-------|-------|
| Category | Capacete (camada da cabeça) |
| Dimensions | matrizes 16×6, centradas (left −8); aba de 16 px na frente e nas costas, de 15 px no perfil |
| Format | código C#: `src/Elements/Capacetes/ChapeuBambuArt.cs` |
| Naming | id `chapeu_bambu`; classe `ChapeuBambuArt` |

**Visual Description:** copa baixa de topo achatado, em bambu seco esverdeado, com massa de 3 tons e sem textura, cordão
verde-escuro e aba estreita nivelada. Cobre o coque e a faixa (decisão 3). A lateral do cabelo aparece sob a aba e os
olhos ficam livres.

**Art Bible Anchors:** §4.2 (mundo, L* 40–63), §4.4 (oliva ~70°, longe da palha `#b0a382` do discípulo), §5.3 (expressão
pela aba), §5.6 (massa de 2 a 3 tons).

**Status:** Done

## ASSET-005 — Sabre de Ferro Gasto (`sabre_ferro`)

| Field | Value |
|-------|-------|
| Category | Arma (na mão) |
| Dimensions | 1,05 × a linha da espada; curva de 1 px em 55% do comprimento; guarda 4×2 px; cabo de 3 px atrás da mão |
| Format | código C#: `src/Elements/Armas/SabreFerroArt.cs` |
| Naming | id `sabre_ferro`; classe `SabreFerroArt` |

**Visual Description:** lâmina de um fio, levemente curva, de aço acinzentado `#b9b8ae`. O fio `#dcd8c8` tem dois dentes e
a ponta é o pixel mais claro. Guarda e pomo de ferro escuro `#4a4640`, sem ouro; cabo `#3b2a1f`; 1 px de ferrugem
`#6e5844` junto à guarda; bainha nas costas com ponteira de ferro.

**Art Bible Anchors:** §3.2 (ponta e cabo diferentes), §4.4 (lâmina contra a túnica), §5.7 (arma de uma mão, mão livre visível).

**Status:** Done
