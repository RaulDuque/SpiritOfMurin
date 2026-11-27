# T17 — Levar os elementos antigos para o padrão

**Estado:** planejada · **Frente:** organização · **Depende de:** — (fazer por partes, junto da entrega que mexer em cada família) · **Origem:** pedido 14 item 2

## Objetivo do criador

Perguntar "onde eu mudo X?" sobre **qualquer** elemento e receber um arquivo só, com o desenho, e um JSON com os números, como já acontece com as árvores, as rochas, os bambus e os arbustos (`docs/elementos/PADRAO.md`).

## O que já existe hoje

No padrão: árvores, rochas, bambus, arbustos e ferramentas. Fora dele (ver `docs/elementos/INDICE.md`):
- **coelho e javali:** o desenho fica dentro de `Rabbit.cs` e `Boar.cs`;
- **lutadores do Vale:** todos num `FighterArt.cs`;
- **itens recolhíveis, fogueira, esteira e pegada:** `Pickup.cs` e `Landmarks.cs`;
- **carcaça:** `Carcass.cs`;
- **objetos do Vale:** `ValeProps.cs`;
- **terreno:** `ChunkGround.cs` e `ValeTerrain.cs`;
- **capim alto:** `Ambient.cs`;
- **ícones de itens:** tudo num `ItemIcons.cs`.

## O que falta (um passo por vez, cada um com antes/depois)

1. **Animais:** separar o desenho em `src/Elements/Fauna/CoelhoArt.cs` e `JavaliArt.cs`; números num `assets/data/elements/fauna/<id>.json` (hoje nos `.tres` e em `plains_progression.json`). Junto da T07, que mexe nos dois.
2. **Lutadores:** `FighterArt.cs` por tipo (`DiscipuloArt`, `SeniorArt`, `EstatuaArt`). Junto da T08.
3. **Objetos com interação:** `Pickup`, `Campfire`, `MeditationMat`, `TrackPrint`, `Carcass` como famílias com arte própria.
4. **Ícones:** um arquivo por família de item (`src/Elements/Icons/...`), em vez do `switch` único.
5. **Terreno e capim:** paletas e formas dos tiles em arquivos de arte.
6. **Pré-visualização:** acrescentar cada família à cena de inspeção.

## Decisões abertas (do criador)

1. **Quando migrar:** junto de cada entrega que mexer na família (recomendado) ou numa entrega só de organização.
2. **Ícones:** um arquivo por família (recomendado) ou um por item.

## Critérios de aceite

- Cada família migrada tem desenho, dados e prévia nos lugares do padrão, e o `INDICE.md` atualizado.
- **Antes/depois no mesmo cenário e pose, pixel por pixel**, para provar que o visual não mudou.
- Saves antigos carregam, com os mesmos ids.
- A bateria completa passa.

## Roteiro de teste

Pelo Painel → Inspeção, confira cada família migrada em todos os estados. Pela Vitrine, os lutadores e animais.

## Prompt pronto para executar

```text
Tarefa T17 do SpiritOfMurin — levar uma família antiga para o padrão de elementos.

Leia GUIA.md, user-docs/tasks/README.md, docs/elementos/PADRAO.md, docs/elementos/INDICE.md e esta tarefa (user-docs/tasks/T17-organizar-elementos-antigos.md). Pergunte-me qual família migrar agora (uma por vez) e a decisão aberta.

Mova o desenho para uma classe <Nome>Art em src/Elements/<Família>/ e os números para um JSON em assets/data/elements/<família>/, sem mudar nada do que se vê: faça uma comparação pixel por pixel no mesmo cenário e pose (imagens só do viewport do jogo) em production/qa/evidence/<data>_padrao-<família>/. Preserve ids e saves (tests/fixtures/saves/jornada_antes_do_ki.json deve carregar). Ponha a família na cena src/Dev/Inspecao.tscn e atualize docs/elementos/INDICE.md.

Termine tudo e rode a bateria completa com janela (monitor 2) uma vez no fim. Feche pelo CLAUDE.md: build, testes, relatório + índice, commits atômicos, push, e atualize a T17 no README de tasks.
```
