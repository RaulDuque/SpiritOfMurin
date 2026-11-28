# Padrão dos elementos do SpiritOfMurin

*Criado em 2026-11-27 (pedido 14, item 2). A regra curta está no `CLAUDE.md`, seção "Elementos do jogo". Onde está cada elemento hoje: [INDICE.md](INDICE.md). Como editar à mão, passo a passo: `user-docs/relatorios/2026-11-27-como-criar-e-editar-elementos.md`.*

## 1. A ideia

Todo elemento do jogo, de uma pedra ao Livro de Técnicas, tem:

1. **Um nome estável (id)** em português, minúsculo e com `_`: `pinheiro`, `rocha_quartzo`, `machado_pedra`, `ki_wave`. Esse id é o que o save e o inventário guardam. **Um id publicado nunca muda.** Para mudar a aparência ou os números, edite o elemento; para fazer outro, crie uma **variante** com outro id.
2. **Sete coisas fáceis de achar**, cada uma num lugar previsível:

| O quê | Onde fica (regra) | Exemplo real |
|---|---|---|
| **Desenho** (forma, paleta, medidas, pivô, animação) | uma classe `<Nome>Art` num arquivo só, em `src/Elements/<Família>/` | `src/Elements/Flora/PinheiroArt.cs` |
| **Dados editáveis** (vida, alcance, velocidade, resistência, custo, materiais) | um JSON por elemento em `assets/data/elements/<família>/<id>.json` (ou o catálogo da família, quando já existe) | `assets/data/elements/arvores/pinheiro.json` |
| **Comportamento** (interação, colisão, ordem de desenho) | uma classe da **família**, compartilhada | `src/Elements/WorldElement.cs` (árvore, rocha, bambu, arbusto) |
| **Ícone e textos** (bolsa, HUD, mapa, Livro) | ícone em `src/Game/ItemIcons.cs` (um `case` por id) ou `src/Game/MapIcons.cs`; nome e descrição no JSON | `case "tora":` em `ItemIcons.cs` |
| **Onde nasce** (Vale à mão ou Planície por semente) | Planície: `habitats` no JSON do elemento + `plains_worldgen.json`; Vale: `src/Vale/ValeMap.cs` | `"habitats": { "grass": 1.0, "bank": 0.5 }` |
| **Id no save e no inventário** | o `id` do JSON; o estado no mundo usa o **id do lugar** (`pine@3,-7`) | `worked: { "pine@3,-7": 6 }` no save |
| **Prévia rápida** | a cena de inspeção `src/Dev/Inspecao.tscn` (ou a Vitrine) | `-- --elemento=pinheiro --estado=meio` |

## 2. Famílias e o que elas compartilham

Uma família tem **um comportamento** e **várias aparências**. Um elemento novo de uma família existente precisa só de **dados** (JSON) e, se a forma for nova, de **uma arte**.

| Família | Comportamento compartilhado | Aparências (artes) | Dados |
|---|---|---|---|
| Árvore | `WorldElement` + `ResourceRules` (golpe, ferramenta, queda, toco, materiais, persistência) | `PinheiroArt` (+ variantes criadas pelo `/create`) | `assets/data/elements/arvores/` |
| Rocha | o mesmo (rachaduras, cascalho, veio) | `RochaArt` | `assets/data/elements/rochas/` |
| Bambu | o mesmo (haste a haste) | `BambuArt` | `assets/data/elements/bambus/` |
| Arbusto | `WorldElement` (reação ao contato, sem colisão) | `ArbustoArt` | `assets/data/elements/arbustos/` |
| Ferramenta | `PlainsGame.Equip` + `ToolDef` (tipo, força, alcance) | `src/Elements/Tools/ToolArt.cs` (na mão), `ItemIcons.cs` (na bolsa) | `plains_items.json` (`tool`) |
| Item / erva / material | `ItemCatalog`, `Inventory`, receitas | `ItemIcons.cs` | `plains_items.json` |
| Animal | cérebro (`RabbitBrain`, `BoarBrain`) + corpo (`Rabbit`, `Boar`) | `src/Elements/Fauna/CoelhoArt.cs`, `JavaliArt.cs`; a pegada em `PegadaArt.cs` (T07) | `RabbitTuning.tres`, `BoarTuning.tres`, `plains_progression.json` (`creatures`), `plains_fauna.json` (pegadas, leitura do combate) |
| Inimigo humano / chefe | `ValeFighter` (estados, guarda, golpe) | `src/Combat/FighterArt.cs` | `assets/data/vale_combat.json` |
| Técnica | `TechniqueData` / `TechniqueSet` (custo, estado, números) | o efeito no `Player` e no `KiWave` | `assets/data/combat_techniques.json` |
| Interface | uma classe por painel (ver §5) | a própria classe | — |

**Um coelho novo** reaproveita `RabbitBrain` (a fuga para a toca), mas tem aparência e números próprios. **Uma árvore nova** reaproveita o corte do `WorldElement`, com outra copa (arte), outra resistência e outra tabela de materiais (JSON). **Uma técnica nova** usa o `TechniqueData`, que calcula o custo e o acerto mostrados no Livro.

## 3. Estrutura de pastas

```text
src/Elements/                 elementos do mundo (padrão novo, pedido 14)
  ElementDef.cs               dados de um elemento + catálogo (lê assets/data/elements/**)
  ElementArt.cs               base das artes + registro ElementArts (id da arte → classe)
  WorldElement.cs             o nó no mundo (estado, colisão, golpe, contato)
  ResourceRules.cs            regras puras (golpe, progresso, materiais, SOR)
  Flora/  PinheiroArt.cs BambuArt.cs ArbustoArt.cs …
  Rocks/  RochaArt.cs …
  Tools/  ToolArt.cs
  Fauna/  CoelhoArt.cs JavaliArt.cs PegadaArt.cs   (T07: o corpo e o cérebro continuam em src/Creatures/)
assets/data/elements/<família>/<id>.json   um arquivo por elemento (esquema no LEIA-ME.md da pasta)
src/Dev/Inspecao.tscn         prévia de qualquer elemento no tamanho real
```

As famílias que já existiam antes do padrão (animais, lutadores, itens, técnicas, interface) continuam nas pastas atuais. O [INDICE.md](INDICE.md) diz onde cada uma está e qual tarefa vai organizá-la. **A migração é por partes:** só se move um elemento quando uma entrega mexer nele, para não arriscar o que já funciona.

## 4. Regras ao criar ou mudar

1. **Variante ou edição?**
   - Mudar um elemento existente: edite o JSON ou a arte dele. Isso muda todos os lugares onde ele aparece.
   - Fazer um elemento parecido: crie outro arquivo com **outro id** (`pinheiro_alto`) e, se a forma mudar, outra arte (`PinheiroAltoArt`). O original continua igual. O `/create` pergunta qual dos dois você quer quando o nome já existe.
2. **Valores de jogo nunca no código.** Vida, golpes, alcance, materiais e chances ficam no JSON. Cores e medidas do desenho ficam como constantes no topo da arte, ou em `look` no JSON quando variam entre elementos da mesma arte.
3. **O save guarda ids de lugar e de item, nunca posições de pixel do desenho.** Mudar o desenho não estraga jornadas.
4. **Sem dados, a arte desenha o original.** O Vale usa as mesmas artes sem JSON. Assim, uma variante nova da Planície não muda o prólogo.
5. **Um elemento só é "jogável"** quando carrega, aparece na situação prevista e responde à interação descrita, com um teste que prova isso. Sem isso, ele é **planejamento**: uma ficha e uma tarefa em `user-docs/tasks/`.
6. **Prévia antes de jogar:** abra a cena de inspeção (`src/Dev/Inspecao.tscn`, ou `-- --elemento=<id>`) e confira o elemento inteiro, trabalhado, caindo e esgotado.

## 5. Interface organizada por responsabilidade

| Parte | Arquivo | O que mexer |
|---|---|---|
| HUD (barras de Vida/Vigor/Ki, nível, mãos, relógio) | `src/Plains/PlainsHud.cs` (`DrawBars`, `SetTime`) | cores no topo da classe |
| Barra do inimigo focado (com números) | `src/Plains/PlainsHud.cs` (`DrawFocusBar`, `FocusBarW/H`) | tamanho e cores |
| Números de dano (assistência) | `src/Game/CombatText.cs` | cores, tamanhos, duração |
| Bolsa e fabricação (T06) | `src/Game/BagPanel.cs` (abas, grade, detalhe), `src/Items/BagRules.cs` (abas, "tenho X de Y", comparação em golpes), `PlainsGame.RefreshRows` (o que entra em cada aba) | `DrawTabs`, `DrawSlot`, `DrawDetail`; categoria de cada item em `plains_items.json` (`category`) |
| Atalhos de item (T06) | `src/Game/QuickBar.cs` (a fileira no HUD), `PlainsGame.AssignQuick/UseQuick/RefreshQuickBar` | teclas em Esc → Controles (`quick_1`…`quick_8`) |
| Alvo do E (T06) | `src/Game/TargetMarker.cs` (anel e lista), `TargetChoice` em `src/Items/BagRules.cs`, `PlainsGame.Interactables` | cores no topo da classe |
| Ganhos sobre a cabeça (T06) | `src/Game/GainFeed.cs`, `GainLog` em `src/Items/BagRules.cs` | cores; tempo em `plains_hud.json` |
| Tempos das mensagens (T06) | `assets/data/plains/plains_hud.json` (`src/Game/HudSettings.cs`) | `gain_seconds`, `toast_seconds`, `important_seconds` |
| Rodapé contextual (T06) | `PlainsGame.ContextHints` + `PlainsHud.SetContext` | quais ações aparecem |
| Ícones dos itens | `src/Game/ItemIcons.cs` | um `case` por id |
| Ficha (T06) | `src/Game/SheetPanel.cs` (desenho, botões "+"), `PlainsGame.RefreshSheet` + `SheetExtras` (prévia da arma e do teto de CON), `PlayerStats.Preview` | `PlusRect`, cores |
| Vida completa ao inspecionar (T06) | `PlainsHud.LifeFull`, `UpdateLifeTip` | texto e posição |
| Mapa e minimapa | `src/Game/MapView.cs` (moldura, caderno), `src/Game/MapIcons.cs` (figuras e legenda), `PlainsGame.DrawMap` (o que aparece) | figuras e nomes |
| Livro de Técnicas | `src/Game/BookPanel.cs` | páginas, comandos |
| Pausa e Modo de assistência | `src/Game/GamePanels.cs` (`PauseRows`) | linhas do menu |
| Controles | `src/Game/ControlsPanel.cs`, `src/Game/Bindings.cs` | ações e teclas |

## 6. O comando de autoria

`/create <tipo> "<descrição>"` (skill em `.claude/skills/create/SKILL.md`) aplica este padrão: acha o exemplo mais próximo, cria os arquivos, registra o elemento e diz como pré-visualizar e testar. Os tipos e o que já é jogável hoje estão na skill.
