# Índice dos elementos — onde está cada um hoje

*Atualizado em 2026-11-28 (T07: javali, coelho, pegadas e efeitos do combate no padrão; antes: pedido 14). Padrão: [PADRAO.md](PADRAO.md).*

**Como ler o estado:**
- **no padrão:** desenho, dados, comportamento e prévia nos lugares do padrão.
- **parcial:** os dados estão em arquivo, mas o desenho está dentro de uma classe maior.
- **antigo:** funciona, mas ainda não foi reorganizado. A tarefa da coluna final organiza.

## Mundo (Planície e Vale)

| Elemento (id) | Desenho | Dados | Comportamento | Onde nasce | Estado | Tarefa |
|---|---|---|---|---|---|---|
| Pinheiro (`pinheiro`) | `src/Elements/Flora/PinheiroArt.cs` | `assets/data/elements/arvores/pinheiro.json` | `src/Elements/WorldElement.cs` | Planície: `habitats` + feature `pine`; Vale: `src/Vale/ValeMap.cs` (decor) | no padrão | — |
| Pedra decorativa (`pedra`) | `src/Elements/Rocks/RochaArt.cs` | `assets/data/elements/rochas/pedra.json` | `WorldElement` | `habitats` + feature `boulder` | no padrão | — |
| Rocha com veio (`rocha_quartzo`) | `RochaArt.cs` (veio, rachaduras, cascalho) | `rochas/rocha_quartzo.json` | `WorldElement` + `ResourceRules` | `habitats` + feature `boulder` | no padrão | — |
| Rocha enferrujada (`rocha_ferro`) — criada com `/create rocha` | `RochaArt.cs` (manchas `look.rust`) | `rochas/rocha_ferro.json` | `WorldElement` + `ResourceRules` | `habitats` (lamaçal, relva, margem) | no padrão | — |
| Salgueiro de margem (`salgueiro`) — criado com `/create arvore` | `src/Elements/Flora/SalgueiroArt.cs` | `arvores/salgueiro.json` | `WorldElement` | `habitats` (margem) + feature `pine` | no padrão | — |
| Touceira de bambu (`bambu_verde`) | `src/Elements/Flora/BambuArt.cs` | `bambus/bambu_verde.json` | `WorldElement` | feature `bamboo` (`plains_worldgen.json`) + `habitats` | no padrão | — |
| Arbusto (`arbusto`) | `src/Elements/Flora/ArbustoArt.cs` | `arbustos/arbusto.json` | `WorldElement` (contato) | feature `bush` | no padrão | — |
| Capim alto | `src/Plains/Ambient.cs` (`GrassSway`) | `plains_ambient.json` (`tall_grass`) | o mesmo | por tile de relva | parcial | T17 |
| Terreno da Planície (relva, margem, água, vau, lamaçal) | `src/Plains/ChunkGround.cs` | `plains_worldgen.json` | `PlainsGenerator.cs` | semente | antigo | T17 |
| Terreno do Vale (tiles, ponte, rio, túmulo) | `src/Vale/ValeTerrain.cs` | `src/Vale/ValeMap.cs` (mapa à mão) | `ValeTerrain.Build` | à mão | antigo | T17 |
| Céu, vento, folhas, pássaros, ondinhas | `src/Plains/Ambient.cs` (`DayNight`, `Wind`, `LeafFx`, `Birds`, `Ripples`) | `plains_ambient.json` | o mesmo | — | parcial | T08 / T09 |

## Objetos com interação

| Elemento | Desenho | Dados | Comportamento | Onde nasce | Estado | Tarefa |
|---|---|---|---|---|---|---|
| Sílex, galho, erva, raiz, bolsinha de moedas | `src/Plains/Pickup.cs` | `plains_items.json` | `PlainsGame.TakePickup` | features `flint`, `branch`, `herb`, `root`, `coinpouch` | antigo | T17 |
| Fogueira, esteira de meditação | `src/Plains/Landmarks.cs` (`Campfire`, `MeditationMat`) | `plains_worldgen.json`, `plains_ambient.json` | `PlainsGame.TryInteract` | clareira (forçados) | antigo | T17 |
| Pegada (gerada na trilha ou fresca) | `src/Elements/Fauna/PegadaArt.cs` | `assets/data/plains/plains_fauna.json` (`track_seconds` = quanto dura a fresca, `track_spacing_*`, `track_max`, `track_sector_degrees`) | `TrackPrint` (`Landmarks.cs`), `src/Creatures/FreshTracks.cs` (pegadas pelo caminho real), `TrackingRules` (`FaunaSettings.cs`), `PlainsGame.Examine` | trilhas da clareira (gerador) + atrás de coelhos e javalis em movimento | no padrão | — |
| Carcaça | `src/Creatures/Carcass.cs` | `plains_progression.json` (`hides`, `spiritual`) | `PlainsGame.TakePart` | onde o animal morre | antigo | T17 |
| Baú, vaso, erva, corpo, objeto caído do Vale | `src/Vale/ValeProps.cs` (`ValeChest`, `ValePot`, `ValeHerb`, `ValeCorpse`, `ValeDrop`) | `src/Vale/ValeState.cs` | `ValeGame` | `ValeMap.cs` | antigo | T17 |

## Criaturas e lutadores

| Elemento | Desenho | Dados | Comportamento | Onde nasce | Estado | Tarefa |
|---|---|---|---|---|---|---|
| Coelho de Campo (`rabbit`) | `src/Elements/Fauna/CoelhoArt.cs` (orelhas em `Ears`, agachado e corrida em `Draw`, mergulho em `Dive`) | `assets/data/plains/RabbitTuning.tres`, `plains_progression.json` (`creatures.rabbit`), `plains_fauna.json` (`rabbit_dive_seconds`) | `src/Creatures/RabbitBrain.cs` + `Rabbit.cs` | feature `burrow` | no padrão | — |
| Javali da Mata (`boar`) | `src/Elements/Fauna/JavaliArt.cs` (poeira em `Dust`, faixa da investida em `Strip`, resvalo em `Glance`, corpo e recuo em `Body`); áreas da assistência em `Boar.DrawAssist` | `BoarTuning.tres`, `creatures.boar`, `plains_fauna.json` (recuo, empurrão do aparo, fator da Lâmina) | `src/Creatures/BoarBrain.cs` + `Boar.cs` | feature `boar` | no padrão | — |
| Efeitos do combate com a fauna | `src/Combat/KiFizzle.cs` (onde o Disparo de Ki se desfaz), `src/Game/NearBar.cs` (barra junto ao animal), `src/Game/TrackHint.cs` (cone da direção ao examinar) | `plains_fauna.json` (`near_bar_seconds`, `combat_radius`, `track_hint_seconds`) | `PlainsGame` (`UpdateNearBar`, `UpdateWaves`, `Examine`) | — | no padrão | — |
| Discípulo da Guarda Fechada | `src/Combat/FighterArt.cs` | `assets/data/vale_combat.json` | `src/Vale/ValeFighter.cs` (`Kind.Disciple`) | `ValeMap.cs` | parcial | T08 |
| Espadachim Sênior (chefe) | `FighterArt.cs` + `src/Vale/SeniorFinale.cs` (cena final) | `vale_combat.json` | `ValeFighter` (`Kind.Senior`) | `ValeMap.cs` | parcial | T08 |
| Estátua da mestra | `FighterArt.cs` | `vale_combat.json` | `ValeFighter` (`Kind.Statue`) | `ValeMap.cs` | parcial | — |
| NPCs | — | — | — | — | não existe | T14 / T15 |

## Personagem, armas, ferramentas e roupas

| Elemento | Desenho | Dados | Comportamento | Estado | Tarefa |
|---|---|---|---|---|---|
| Protagonista (referência visual aprovada pelo criador) | em camadas (T18): corpo, pernas e mãos em `src/Player/Player.cs` (`Pose`, `DrawFighter`); cabeça e cabelo em `src/Elements/Corpo/CabecaArt.cs`; a pose compartilhada em `src/Elements/Corpo/FighterPose.cs` | `assets/data/PlayerTuning.tres` | `Player.cs` | no padrão (T18; idêntico ao aprovado, 0 px nas 8 direções) | — |
| Espada (`espada`) | `src/Elements/Armas/EspadaArt.cs` | `assets/data/equipamento/armas/espada.json` | `Player.cs` (`Blade`) | no padrão (T18) | — |
| Faca de Pedra (`faca_pedra`) | `src/Elements/Armas/FacaPedraArt.cs`, ícone em `ItemIcons.cs` | `assets/data/equipamento/armas/faca_pedra.json` (`weapon` + `tool`); receita em `plains_items.json` | `Player.cs`, `PlainsGame.Equip` | no padrão (T18) | — |
| Sabre de Ferro Gasto (`sabre_ferro`) | `src/Elements/Armas/SabreFerroArt.cs`, ícone em `ItemIcons.cs` | `assets/data/equipamento/armas/sabre_ferro.json`; técnicas em `_tecnicas_armas.json` | `Player.cs` (`Blade`, Meia-Lua), `PlainsGame.Equipment.cs` | no padrão (T18) | — |
| Túnica Verde (`tunica_verde`, a do criador) e Túnica de Viajante (`tunica_viajante`) | `src/Elements/Roupas/TunicaVerdeArt.cs`, `TunicaViajanteArt.cs` (base `RoupaArt.cs`), ícones em `ItemIcons.cs` | `assets/data/equipamento/roupas/<id>.json` (`wear`: defesa, peso) | `PlainsGame.Equipment.cs`, `EquipRules` | no padrão (T18) | — |
| Chapéu de Bambu Trançado (`chapeu_bambu`) | `src/Elements/Capacetes/ChapeuBambuArt.cs` (base `CapaceteArt.cs`), ícone em `ItemIcons.cs` | `assets/data/equipamento/capacetes/chapeu_bambu.json`; receita em `plains_items.json` | `PlainsGame.Equipment.cs` | no padrão (T18) | — |
| Trouxa de viajante (`trouxa_viajante`, achado da Planície) | `src/Plains/Pickup.cs` (`FeatureKind.Bundle`), ícone em `ItemIcons.cs` | conteúdo em `assets/data/equipamento/_regras.json` (`trouxa`) | `PlainsGenerator` (sob o pinheiro da margem), `PlainsGame.OpenBundle` | no padrão (T18) | — |

## Itens, técnicas e progressão

| Elemento | Onde |
|---|---|
| Itens e receitas da Planície | dados em `assets/data/plains/plains_items.json`, catálogo em `src/Items/ItemCatalog.cs`, ícones em `src/Game/ItemIcons.cs` |
| Itens do prólogo | `src/Vale/ValeState.cs` (`ValeItems`), ícones em `ItemIcons.cs` |
| Técnicas (`thrust`, `ki_wave`, `side_step`, `dash`, `lamina`, `ki_charge`, `ki_reinforce`) | números em `assets/data/combat_techniques.json`, regras em `src/Combat/TechniqueData.cs`, execução em `src/Player/Player.cs` e `src/Combat/KiWave.cs`, página em `src/Game/BookPanel.cs` |
| Atributos (FOR, VIT, INT, CON, AGI, SOR, KI), XP, proficiências | `assets/data/plains/plains_progression.json`, `src/Progression/` |
| Dano, defesa, crítico | `src/Combat/DamageRules.cs` |

## Interface

Ver [PADRAO.md §5](PADRAO.md#5-interface-organizada-por-responsabilidade).
