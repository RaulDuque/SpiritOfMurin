# Como criar e editar elementos do SpiritOfMurin — 2026-11-27

> Guia para você mudar o jogo à mão, pedido no item 3 do [pedido 14](prompts/14-organizacao-criacao-conteudo.md).
>
> - **A regra:** [docs/elementos/PADRAO.md](../../docs/elementos/PADRAO.md).
> - **Onde está cada coisa hoje:** [docs/elementos/INDICE.md](../../docs/elementos/INDICE.md).
>
> Todos os caminhos abaixo existem de verdade.

## 0. O ciclo de três passos: abrir, editar, ver

1. **Abra** o arquivo indicado na tabela (no VS Code, Rider ou no próprio Godot).
2. **Edite** o trecho e salve.
3. **Veja o resultado**, de um destes dois jeitos:
   - **Painel → Inspeção (elementos):** com o "Modo ao vivo" marcado, o Painel recompila e reabre sozinho ao salvar um `.cs` ou um `.json`. Use ← → para escolher o elemento, ↑ ↓ para o estado (inteiro, meio, quase, caindo, esgotado), G para uma rajada de vento, A para um corpo atravessando e Tab para trocar a ferramenta na mão do personagem.
   - **Painel → Jogar:** para ver no mundo.

> **Por que a mudança "não aparece":** o jogo usa o código compilado. O Painel compila sempre antes de abrir. Já o Godot aberto direto (sem o F5 do editor) não compila.

## 1. Tabela rápida: quero mudar → abro → edito → testo

| Quero mudar isto | Abro este arquivo | Edito este trecho | Testo assim |
|---|---|---|---|
| A **copa do pinheiro** (largura, andares, altura) | `src/Elements/Flora/PinheiroArt.cs` | `DrawPineAt`: `w = (17 - i * 3 + hv % 3)` (largura), `top = … (24 + i * 11)` (altura de cada andar), `tiers = 3 + hv % 3` (quantos andares) | Inspeção → pinheiro |
| Os **verdes do pinheiro** | o mesmo | `Greens(int hv)`: três paletas | Inspeção → pinheiro (as 3 variações) |
| O **balanço do pinheiro** com o vento | o mesmo | `sway = … Sin(p.Time * 1.2f + hv) * 0.6f + windPush * (i + 1) / tiers` | Inspeção → G (rajada) |
| O **tronco** mais alto e a inclinação | `assets/data/elements/arvores/pinheiro.json` | `"look": { "trunk_extra": 4, "lean": 1 }` | Inspeção |
| **Quantos golpes** derrubam o pinheiro | `assets/data/elements/arvores/pinheiro.json` | `"toughness": 6` | Inspeção → J; ou Jogar |
| **O que o pinheiro dá** | o mesmo | `"drops"` (tora 2-3) e `"bonus"` (resina 10% + 3%/SOR) | Jogar → derrube um |
| **Onde nasce** cada árvore | o JSON da árvore | `"habitats": { "grass": 1.0, "bank": 0.5 }` (pesos dentro da família) | Jogar, ou `ElementosCheck --phase=mundo` |
| Os **galhos pendentes do salgueiro** | `src/Elements/Flora/SalgueiroArt.cs` e `arvores/salgueiro.json` | `Strands(...)`; `"look": { "strands": 7, "strand_length": 24 }` | Inspeção → salgueiro, G |
| As **hastes do bambu** (altura, cor) | `src/Elements/Flora/BambuArt.cs` | `Stalk`: `ht = 34 + h2 % 14`; cores em `Green[]` | Inspeção → bambu |
| A **queda da haste** de bambu | o mesmo | `Falling` (giro de 1,4 rad) e `FallSeconds = 0.6f` | Inspeção → J |
| O **veio** e as **rachaduras** da rocha | `src/Elements/Rocks/RochaArt.cs` | `Vein`, `Cracks`, `Rust`; cor do veio em `"look": { "vein": "#e6efe8" }` no JSON | Inspeção → rocha_quartzo / rocha_ferro |
| Quanto o **arbusto se abre** quando alguém passa | `src/Elements/Flora/ArbustoArt.cs` + `arbustos/arbusto.json` | `PartScale = 0.8f`; `"contact_radius": 16`; mola em `WorldElement.BendStiffness/BendDamping` | Inspeção → arbusto → A |
| As **orelhas do coelho** | `src/Creatures/Rabbit.cs` | `_Draw`, bloco `// ears`: em pé quando assustado (`Brain.EarsUp`) e deitadas | Painel → Vitrine (coelho) |
| A **cor do coelho** | o mesmo | `Fur = "#8a7a5e"`, `FurLight`, `Belly`, `EarIn` (linhas 111-112) | Vitrine |
| A **animação de corrida** do coelho | o mesmo | `stretch = running ? 3 : 0`, `lift` do pulo | Jogar: chegue perto de um |
| A **distância de fuga** do coelho | `assets/data/plains/RabbitTuning.tres` (abre no Godot ou num editor de texto) | `HearWalk` (95 px andando), `HearSneak` (26 px com Shift), `DashSpeed`, `HideTime` | Jogar: ande e depois use Shift |
| **Alcance e dano** da faca e da espada | `assets/data/PlayerTuning.tres` | `KnifeReach`, `KnifeDamage`, `SwordReach`, `SwordDamage` | Jogar ou Vitrine (boneco controlado) |
| **Alcance e força** do machado e da picareta | `assets/data/plains/plains_items.json` | `"tool": { "kind": "machado", "power": 1, "reach": 22, "damage": 1 }` | Jogar |
| A **forma do machado na mão** | `src/Elements/Tools/ToolArt.cs` | `case "machado":` (cunha) e `Handle` (cabo) | Inspeção → Tab até "machado" |
| O **ícone de um item** na bolsa | `src/Game/ItemIcons.cs` | `case "<id>":`, onde `R(x, y, w, h, "#cor")` é um retângulo e `O(...)` o contorno | Jogar → I |
| Nome e descrição de um item | `assets/data/plains/plains_items.json` | `"name"`, `"description"` | Jogar → I |
| Uma **receita** | o mesmo | `"recipes": [ { "id", "output", "inputs", "station" } ]` | Jogar → I → Fabricar |
| **Força, defesa e crítico** | `assets/data/plains/plains_progression.json` | `for_melee_damage`, `con_damage_reduction`, `crit_chance`, `sor_crit_chance`, `crit_multiplier` | Jogar → C (a prévia mostra) |
| **Andar sobre a água** (custo de Ki) | `assets/data/plains/plains_ambient.json` | `"deep_water": { "walk_ki_per_second": 10, "swim_speed": 0.45 }` | Jogar → riacho fundo + Shift |
| **Brilho azul** do Reforço de Ki | `src/Player/Player.cs` | `ReinforceHalo`, `ReinforceLight` (cores) e o bloco `// the Reforço de Ki: a blue glow` | Painel → Vitrine (boneco) ou Jogar |
| **Tamanho do rastro** do corte | `src/Player/Player.cs` | `SlashSword = 52f, SlashSwordThird = 66f, SlashKnife = 26f`, `SlashThick`, `SlashLife` | Jogar |
| A **barra de vida do inimigo** com números | `src/Plains/PlainsHud.cs` | `DrawFocusBar`, `FocusBarW = 120, FocusBarH = 9` | Jogar → Espaço num javali |
| Os **números de dano** (cor, tamanho) | `src/Game/CombatText.cs` | `DealtC`, `CritC`, `TakenC`, `Size`, `CritSize`, `Life` | Esc → Modo de assistência → números |
| Os **ícones do minimapa** | `src/Game/MapIcons.cs` | um `case` por figura em `Draw`, nomes em `Name` | Jogar → M |
| Uma linha do **menu de pausa** | `src/Game/GamePanels.cs` | `PauseRows()` | Jogar → Esc |
| O **Livro de Técnicas** | `src/Game/BookPanel.cs` | páginas e comandos | Jogar → B |

## 2. Passo a passo — já dá para fazer hoje

### 2.1 Mudar a silhueta, a cor e o balanço de uma árvore ou de um bambu

1. Painel → **Inspeção (elementos)**, com o "Modo ao vivo" marcado. Escolha o pinheiro com ← →.
2. Abra `src/Elements/Flora/PinheiroArt.cs`.
   - **Silhueta:** em `DrawPineAt`, o `w` é a largura de cada andar. Troque `17` por `20` para copas mais largas, ou `11` por `9` para andares mais juntos.
   - **Cor:** em `Greens`, troque os quatro verdes de uma paleta (base, sombra, brilho, borda).
   - **Balanço:** em `sway`, o `0.6f` é o balanço parado e `windPush` é a rajada. Aperte G na Inspeção para ver a rajada.
3. Salve. O Painel recompila e reabre a Inspeção, e você compara as 3 variações lado a lado.
4. **Bambu:** o mesmo em `BambuArt.cs` (`Stalk`, `Green[]`). O número de hastes está no JSON (`"look": { "stalks": 3 }`).
5. **Só para uma árvore parecida, sem mudar o pinheiro:** use `/create arvore "…"` (seção 3). Ele cria uma variante com outro id.

### 2.2 Mudar as orelhas, a cor, a animação e a distância de fuga do coelho

1. **Orelhas e cor:** `src/Creatures/Rabbit.cs`, método `_Draw`.
   - As cores ficam nas linhas 111-112 (`Fur`, `FurLight`, `Belly`, `EarIn`).
   - As orelhas estão no bloco `// ears`: em pé com `R(hx + 1, hy - 6, 2, 6, …)`; aumente o `6` para orelhas mais longas.
2. **Animação:** no mesmo método, `stretch` (o corpo estica na corrida) e `lift` (o pulo).
3. **Distância de fuga:** `assets/data/plains/RabbitTuning.tres`.
   - `HearWalk = 95` é de quantos px ele ouve você andando.
   - `HearSneak = 26` é o mesmo com Shift.
   - `DashSpeed` é a velocidade das arrancadas.
4. **Prévia:** Painel → **Vitrine** (o coelho parado). Para ver a fuga, Painel → **Jogar**: ande até um coelho e depois repita com Shift.
5. **Hoje** o desenho do coelho ainda fica dentro da classe do bicho. A T17 o separa num `CoelhoArt.cs`, como o pinheiro.

### 2.3 Criar uma ferramenta (desenho, empunhadura, ícone, alcance, força)

Hoje são jogáveis as ferramentas dos tipos `machado`, `picareta` e `faca`.

1. `/create ferramenta "Foice de sílex que corta bambu de uma vez"`. Ou à mão:
   1. Em `assets/data/plains/plains_items.json`, copie o `machado_pedra`, troque o id, o nome e a descrição, e ajuste `"tool": { "kind": "faca", "power": 3, "reach": 20 }`.
   2. Acrescente uma receita.
   3. **Desenho na mão:** em `src/Elements/Tools/ToolArt.cs`, um `case` novo. O `hand` é a empunhadura e o `tip` a ponta; desenhe a lâmina a partir deles.
   4. **Ícone:** em `src/Game/ItemIcons.cs`, um `case "<id>":`.
2. **Prévia:** Inspeção → Tab, para o personagem segurar a ferramenta nas 4 direções.
3. **Jogo:** fabrique, equipe (I, depois E) e golpeie um bambu.

### 2.4 Criar um item ou um material e fazê-lo aparecer na bolsa

1. `plains_items.json`: `"meu_item": { "name": "…", "description": "…" }`.
   - Se for usável, acrescente `"vigor_now": 12` ou `"regen_amount": 4, "regen_seconds": 6`.
2. Ícone: `case "meu_item":` em `ItemIcons.cs`.
3. **De onde vem:**
   - de uma receita (`"recipes"`);
   - ou de um elemento (`"drops"`/`"bonus"` no JSON do elemento, por exemplo o `minerio_ferro` da `rocha_ferro`).
4. **Teste:** `ElementosCheck --phase=catalogo` confere que os materiais dos elementos existem no catálogo.

### 2.5 Ajustar um componente da interface sem procurar no HUD inteiro

Use a tabela de [PADRAO.md §5](../../docs/elementos/PADRAO.md#5-interface-organizada-por-responsabilidade). Cada parte tem o seu arquivo:
- barra do inimigo: `PlainsHud.DrawFocusBar`;
- números de dano: `CombatText`;
- ícones do mapa: `MapIcons`;
- menu de pausa: `GamePanels.PauseRows`;
- bolsa: `BagPanel`.

## 3. O comando `/create`

**Sintaxe** (no Claude Code, dentro da pasta do projeto):

```text
/create <tipo> "<descrição em português>"
```

**Tipos jogáveis hoje:**
- `arvore`, `rocha`, `bambu`, `arbusto`: criam o JSON e, se a forma for nova, uma arte;
- `ferramenta`: tipos machado, picareta e faca;
- `item`.

**Tipos de planejamento** (geram uma ficha e uma tarefa, marcadas "PLANEJAMENTO"): `arma`, `mob`, `inimigo`, `roupa`, `capacete`. Viram jogáveis com a T18 (arma, roupa, capacete) e a T08 (mob, inimigo).

**Na sua instalação:** a skill fica em `.claude/skills/create/SKILL.md`. Nesta sessão o Claude Code a descobriu sozinho, poucos minutos depois de o arquivo ser criado, sem reiniciar. Se `/create` não aparecer, feche e abra o Claude Code na pasta do projeto.

**Se o nome já existe:** o comando pergunta se você quer uma **variante** (id novo; o original não muda) ou **editar o existente** (muda em todo lugar onde ele aparece). Ele nunca sobrescreve sem perguntar.

### Exemplo testado 1 — `/create arvore "Salgueiro de margem, com galhos baixos que balançam com o vento"`

**Arquivos:**
- criados: `assets/data/elements/arvores/salgueiro.json` e `src/Elements/Flora/SalgueiroArt.cs`;
- alterados:
  - `src/Elements/ElementArt.cs`: linha `["salgueiro"] = new SalgueiroArt()`;
  - `plains_items.json`: item novo `casca_salgueiro`, que alivia a dor;
  - `ItemIcons.cs`: o ícone.

**Onde mexer:**
- a copa em `Crown`, os galhos em `Strands`;
- cores e tamanhos em `"look"`;
- onde nasce em `"habitats"` (margem 1,5, relva 0,15);
- quantos golpes em `"toughness": 5`.

**Prévia:** Inspeção → salgueiro. As imagens estão em `production/qa/evidence/2026-11-27_create-salgueiro/` (`inspecao-*.png`, `salgueiro-no-mundo-*.png`).

**Teste:**
- `ElementosCheck --phase=catalogo`: 33/33 quando criado;
- `--phase=mundo`: 12/12 nas duas sementes; o salgueiro nasce na margem e responde ao machado.

**Resultado: jogável.**

### Exemplo testado 2 — `/create rocha "Rocha enferrujada com veio de ferro, que solta minério de ferro"`

**Arquivos:**
- criado: `assets/data/elements/rochas/rocha_ferro.json`;
- alterados:
  - `RochaArt.cs`: as manchas de ferrugem, opcionais (`"look": { "rust": … }`);
  - `plains_items.json`: `minerio_ferro`;
  - `ItemIcons.cs`: o ícone.

**Ele reaproveitou a arte `rocha`** (forma igual), como a skill manda.

**Prévia:** `production/qa/evidence/2026-11-27_create-rocha-ferro/inspecao-*.png`.

**Teste, na cena isolada `ElementosCheck --phase=catalogo`:** 39/39. A rocha recusa o machado, cede a 7 golpes de picareta e dá 2 pedras e 2 minérios.

A prévia desse exemplo revelou um defeito antigo: lajes de pedra listradas, quando a altura terminava em ,5. Corrigi no commit `c4e47b7`.

**Resultado: jogável.**

## 4. O que ainda depende de uma tarefa futura

| Quero… | Hoje | Tarefa |
|---|---|---|
| Uma **arma** nova (sabre, lança) com desenho, empunhadura, alcance e dano | só espada e faca, desenhadas no `Player.cs` | **T18** |
| **Roupa ou capacete** e conferir as camadas de frente, de costas e de perfil | túnica e cabelo fazem parte do `Player.cs` (referência aprovada) | **T18** |
| Um **inimigo** novo com aviso, golpe ativo, recuperação e materiais | só os do Vale (`ValeFighter` + `FighterArt` + `vale_combat.json`). Hoje dá para mudar os números do discípulo e do Sênior (`dis_hp`, `dis_windup`, `sen_reach`…) | **T08** |
| Uma **espécie de animal** nova | só coelho e javali. Uma variante numérica é possível pelos `.tres` | **T08**, com organização pela **T17** |
| Uma **técnica** nova no Livro | números em `combat_techniques.json`, regra em `TechniqueData.cs`, execução em `Player.cs`. Técnica nova é código novo | por pedido |
| **Vendedores** | não existem (as moedas sim) | **T15** |
