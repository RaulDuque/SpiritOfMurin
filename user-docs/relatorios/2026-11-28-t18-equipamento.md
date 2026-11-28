# T18 — Armas, roupas e capacetes trocáveis — 2026-11-28

*Pedido: [prompts/16-t18-equipamento.md](prompts/16-t18-equipamento.md). Tarefa: [T18](../tasks/T18-equipamento-visual.md).
Design: `design/gdd/equipment.md`. Fichas visuais: `design/assets/specs/t18-equipamento-assets.md`. Como editar:
`docs/elementos/EQUIPAMENTO.md`.*

## 1. O seu protagonista continua idêntico (antes e depois das camadas)

Antes de mexer no desenho, guardei o visual original: o personagem congelado em **12 poses reproduzíveis** (repouso, dois
quadros de caminhada, os três cortes, o início do corte, a estocada, o passo, o gesto de Ki, o Reforço parado e no
corte), nas **8 direções**, com **5 coisas na mão** (espada, faca, machado, picareta, mãos vazias): 480 figuras em 15
pranchas, capturadas do viewport do jogo, sempre na mesma escala.

Depois separei as camadas: cabeça e cabelo (`CabecaArt`), túnica verde (`TunicaVerdeArt`), espada e faca (`EspadaArt`,
`FacaPedraArt`); o corpo, as pernas e as mãos ficaram no `Player.cs`. Repeti as capturas e comparei pixel por pixel:
**15/15 pranchas, 0 px de diferença.**

- **Uma causa encontrada e corrigida antes de usar a referência:** na primeira rodada, as pranchas do Reforço mudavam
  entre duas capturas iguais (176 a 576 px). As faíscas do Reforço (`KiPalm`) tinham relógio próprio. Agora elas também
  congelam na pose de captura, e duas rodadas seguidas dão 0 px. Isso também mostrou que o comparador acusa diferença
  quando ela existe.
- Comparação lado a lado: `production/qa/evidence/2026-11-28_t18-equipamento/comparacao-antes-depois.png`, com antes,
  depois e o mapa de diferença (vermelho onde muda; está todo apagado). As 15 pranchas estão em `antes/` e `depois/`.

## 2. Decisões (suas, uma por vez, sobre a [prancha](2026-11-28-t18-prancha-equipamento.html))

| # | Pergunta | Sua escolha |
|---|---|---|
| 1 | A roupa só muda a aparência? | **Defesa e peso no Vigor** |
| 2 | Armas novas trazem técnicas próprias? | **Sim.** As três do sabre (Meia-Lua, Saque do Viajante, Corte Fluido) são **descobertas com o tempo**: a Meia-Lua entrou agora, as outras duas ficaram como futuras |
| 3 | Quanto do coque o chapéu cobre? | Cada chapéu define. O de bambu **cobre o coque e a faixa** (sem fita enquanto estiver na cabeça) |
| 4 | Primeiro conjunto | **Túnica de viajante + chapéu de bambu trançado + sabre de ferro gasto**, como na prancha |

A prancha foi feita pelo `/asset-spec` (art-director e technical-artist em paralelo) com os pixels reais do jogo. Os dois
especialistas divergiram num ponto: o technical-artist sugeriu copa em cone, e o art-director avisou que a 1× o cone vira
o chapéu do discípulo. Ficou a copa baixa. A bíblia de arte §5.1 dizia "sem troca de visual por equipamento"; a sua
decisão prevalece. Fica proposta uma regra para todo chapéu do protagonista: aba nivelada, no máximo 16 px, olhos à vista.

## 3. O que você consegue fazer agora

- **Achar e fabricar:** uma **trouxa de viajante** está sob o pinheiro da margem, ao norte da clareira, no mesmo lugar em
  todas as sementes. **E** pega a trouxa, e o **sabre** e a **túnica de viajante** vão para a bolsa. O **chapéu de bambu**
  se fabrica com 3 Varas de Bambu + 1 Tira de Amarração.
- **Bolsa (I):** na aba Equipamentos, cada peça tem **ícone** e **✓** quando está em uso. O detalhe diz "na mão",
  "vestindo" ou "na cabeça".
  - **Armas** comparam alcance, dano por corte, cortes no combo, dano do combo e tempo de um corte.
  - **Roupas e chapéus** comparam defesa e peso.
  - Todas as peças dizem **onde se consegue**.
  - **E** veste ou empunha, e **E** de novo tira. Arma, roupa e chapéu são independentes.
- **Tirar:** sem o chapéu, o cabelo, o coque, a faixa e a fita voltam. Sem a túnica de viajante, a **túnica verde** volta.
  A túnica verde nunca sai da bolsa.
- **Números (decisão 1):** a túnica de viajante segura **8%** do dano e custa **+10%** de Vigor por corte e por passo. O
  chapéu segura **3%** e custa **+2%**. A defesa vem depois da CON, com teto de 40%.
- **Sabre:** alcance **38 px** (a espada tem 34), dano 2 por corte, combo de 3 (o último dobra), um pouco mais lento
  (0,43 s contra 0,39 s). Funcionam a estocada, a mira pelo mouse, o alvo travado e o Reforço de Ki (o brilho azul segue a
  curva da lâmina).
- **Meia-Lua:** depois de **12 golpes que acertam** com o sabre, aparece "Técnica descoberta: Meia-Lua!" e ela entra no
  Livro de Técnicas. O 3º corte do combo passa a varrer **±120°** (o normal é ±75°), com rastro maior, por 4 de Vigor a mais.
- **Espada e faca** são as primeiras armas do sistema, com os mesmos números de antes (o teste de unidade compara). A faca
  continua sendo ferramenta (bambu, esfolar) e os atalhos da T06 continuam funcionando.
- **Jornadas antigas** carregam com a túnica verde, sem chapéu e com a arma que tinham.

## 4. Inspeção (Painel → Inspeção)

A figura grande (×3) e a fileira com as 8 direções mostram as peças escolhidas. **Tab** troca a arma (espada, faca,
sabre, machado, picareta, mãos vazias), **R** a roupa, **C** o chapéu, **Z** a pose (repouso, caminhada, cortes 1–3,
estocada, passo, Ki, Reforço), **Q/E** giram a figura grande e **Espaço** anima. O texto diz qual arquivo desenha cada
peça. Captura: `inspecao_conjunto.png`.

## 5. Onde mexer

| Para mudar… | Arquivo |
|---|---|
| Cabeça, cabelo, coque, faixa, fita | `src/Elements/Corpo/CabecaArt.cs` |
| Túnica verde (a sua) | `src/Elements/Roupas/TunicaVerdeArt.cs` |
| Túnica de viajante | `src/Elements/Roupas/TunicaViajanteArt.cs` |
| Chapéu de bambu (e quanto cabelo cobre: `HairCover`) | `src/Elements/Capacetes/ChapeuBambuArt.cs` |
| Espada / faca / sabre (desenho, comprimento: `Length`) | `src/Elements/Armas/EspadaArt.cs`, `FacaPedraArt.cs`, `SabreFerroArt.cs` |
| Números de cada peça (alcance, dano, combo, tempos, defesa, peso, onde conseguir) | `assets/data/equipamento/<armas|roupas|capacetes>/<id>.json` (esquema no `LEIA-ME.md` da pasta) |
| Técnicas das armas (Meia-Lua: acertos, arco, Vigor) | `assets/data/equipamento/_tecnicas_armas.json` |
| Teto da defesa, conteúdo da trouxa | `assets/data/equipamento/_regras.json` |
| Receita do chapéu | `assets/data/plains/plains_items.json` (`recipes`) |
| Ícones na bolsa | `src/Game/ItemIcons.cs` (`case "sabre_ferro"`, …) |
| Corpo, pernas, mãos e a ordem das camadas | `src/Player/Player.cs` (`Pose`, `DrawFighter`) |

**Três exemplos passo a passo** em `docs/elementos/EQUIPAMENTO.md`: alongar a lâmina do sabre (`Length` e `reach`),
alargar a faixa da túnica (as linhas 8–11 nas três vistas) e baixar a aba do chapéu (uma linha de copa a mais).

**`/create arma`, `/create roupa` e `/create capacete` agora são jogáveis** (`.claude/skills/create/SKILL.md`, §2b). Cada
um cria o JSON da peça, a classe `<Nome>Art`, o ícone, a receita ou a origem, e confere tudo com
`EquipCheck --phase=catalogo`. Essa fase lê **todos** os arquivos da pasta, falha se faltar desenho registrado, ícone
próprio, números, "onde" ou origem, e salva a prévia da peça no personagem nas 8 direções (`catalogo/previa_<id>.png`). O
conjunto aprovado passou pelo fluxo: 6/6 peças. Também vi a fase falhar com um arquivo quebrado de propósito (4 problemas
apontados), depois apagado.

## 5b. Mapa de testes (seu pedido no fim da sessão)

**Painel → "Mapa de testes (todos os itens)"** abre a Planície (semente 20261126) com uma jornada própria
(`user://teste_mapa.json`). Ela recomeça do zero a cada abertura, e a sua jornada não é tocada.
- **Na bolsa:** todos os itens do catálogo. Peças e ferramentas ×1, consumíveis ×5, materiais ×10, e 100 moedas.
- **No chão, ao sul da fogueira:** um mostruário com uma de cada arma, roupa, chapéu e ferramenta, com o nome em cima
  (**E** pega).
- A Meia-Lua já vem conhecida, para você ver a varrida.

A lista sai do próprio catálogo (`plains_items.json` + `assets/data/equipamento/`): uma peça nova feita com `/create`
aparece ali sem precisar mexer em nada. Código: `src/Plains/PlainsGame.TestMap.cs`. Teste: `EquipCheck --phase=mapa`.
Captura: `jogo_20261126/08-mapa-de-testes.png`.

## 6. Arquivos próprios (a sua pergunta)

As **armas, roupas e chapéus** desta entrega têm cada um o seu desenho (`src/Elements/<família>/<Nome>Art.cs`) e os seus
dados (`assets/data/equipamento/.../<id>.json`). **Árvores, rochas, bambus e arbustos** também já estavam assim. Ainda
**não** estão separados:
- os **itens comuns** (ervas, carnes, materiais, machado e picareta): ficam todos em `plains_items.json`;
- os **ícones**: todos em `ItemIcons.cs`, um `case` por item;
- os **números dos animais**: em arquivos compartilhados.

Separar isso é a T17. Ela segue a sua regra de migrar cada família quando uma entrega mexer nela.

## 7. Testes

**Resultado final: bateria completa pelo `PainelDeTestes.exe --auto`, com janela no monitor 2, sementes 20261126 e 777
(13 min 15 s): 993/993 verificações aprovadas, 106 execuções, todas com código 0.** O relatório do Painel está em
`production/qa/evidence/2026-11-28_t18-equipamento/painel/relatorio.md`.

As verificações aprovadas e as execuções que terminaram com código de falha, rodada por rodada:

| Rodada | Verificações | Execuções com código de falha | O que aconteceu |
|---|---|---|---|
| 1. `tools/godot/testes-janela.ps1` (monitor 2) | 796/798 | **2**: `TechCheck sealed` (18/19) e `FaunaCheck coelho`, semente 20261126 (3/4) | testes antigos e instáveis, sem relação com a T18 (veja abaixo) |
| 2. Painel `--auto` | 992/993 | **1**: `Novidades · reinforce` (16/17) | **defeito da T18, corrigido** (veja abaixo). Log: `painel/rodada1-reinforce-falha.log` |
| 3. Painel `--auto`, depois da correção | **993/993** | **0** | resultado final |

**O defeito que a rodada 2 pegou:** no Vale, a espada passou a receber os números do `espada.json`. Quando uma cena põe
a faca direto na mão, sem passar pela bolsa, os números da espada continuavam valendo para a faca (dano 2 em vez de 1).
Agora os números de uma lâmina só valem para o tipo dela, longa ou curta, e o desenho segue a mesma regra. O próprio
`reinforce` provou a correção: falhou antes (dano 2,2) e passou 17/17 duas vezes depois (dano 1,1). A rodada 1 não pegou
porque a mudança do Vale ainda não estava compilada naquele momento.

**As duas instabilidades da rodada 1:** cada uma falhou com uma verificação diferente a cada vez (ângulo do passo lateral,
dano da Lâmina, distância de audição do coelho). Rodei os mesmos dois testes **4 vezes na `main` de antes da T18** (numa
cópia separada): também falharam 1 vez em 4, com as mesmas verificações. A instabilidade já existia. Isoladas na versão
com a T18, passaram em 2 de 3 rodadas.

**Testes da T18** (`tests/integration/EquipCheck.tscn`, todos na rodada final):

| Fase | Resultado |
|---|---|
| `retrato` (padrão idêntico ao aprovado) | 17/17: 15 pranchas, 0 px |
| `conjunto` (peças aprovadas; cabeça sem o chapéu = aprovada) | 16/16 |
| `sabre` (empunhadura em 120 quadros; costas; brilho na curva) | 4/4 |
| `catalogo` (o que o `/create` cria) | 7/7 (e falhou como devia com um arquivo quebrado de propósito) |
| `bolsa`, sementes 20261126 e 777 | 18/18 e 18/18 |
| `combate`, sementes 20261126 e 777 | 9/9 e 9/9 |
| `salvar` → `continuar`, sementes 20261126 e 777 | 2/2 + 2/2 e 2/2 + 2/2 |
| `oldsave` (`jornada_antes_do_ki.json`) | 4/4 |
| `mapa` (mapa de testes) | 5/5 |
| Unidade | 64/64 (7 novos em `EquipRulesTests`) |

**Ajustes nos testes antigos:** o `InventarioCheck` conta uma linha a mais na aba "Tudo" (a Túnica Verde, que fica sempre
na bolsa): 9 → 10 e, na jornada antiga, 2 → 3.

**Observado × controlado:** a trouxa, a bolsa, o equipar e o tirar, o combate, a meditação e o Continuar rodaram pelo jogo
de verdade, com teclas (I, Tab, D, E, J, Espaço, Shift, 1) e mouse. Três coisas foram dadas pelo teste, em vez de
coletadas no mundo, para não depender de sorteio:
- os materiais do chapéu e da faca, já testados em outras checagens;
- os 11 acertos anteriores à Meia-Lua;
- a jornada com as peças já na bolsa, nas fases de combate e de salvar.

Todos os testes usam `user://teste_equip.json` e `user://teste_equip_prefs.json`; o mapa de testes usa
`user://teste_mapa.json`. A sua jornada e as suas teclas não foram tocadas.

## 8. Evidências (só a imagem do jogo)

Pasta `production/qa/evidence/2026-11-28_t18-equipamento/`:

| Arquivo | O que mostra |
|---|---|
| `comparacao-antes-depois.png` | o protagonista padrão antes, depois e o mapa de diferença (0 px) |
| `antes/`, `depois/` | as 15 pranchas (12 poses × 8 direções × 5 coisas na mão) |
| `conjunto/` | as referências do conjunto aprovado (cada peça sozinha e juntas) |
| `jogo_20261126/04-conjunto-equipado.png` | **o conjunto equipado na campanha** |
| `jogo_20261126/01-trouxa` … `07-continuar` | a trouxa, a bolsa comparando, o conjunto, o Reforço com o sabre, a Meia-Lua, o Continuar |
| `jogo_777/` | o mesmo na semente 777 |
| `video-bolsa-e-sabre.gif` | **vídeo curto:** a bolsa veste a túnica e o chapéu e empunha o sabre; o combo e o Reforço |
| `inspecao_conjunto.png` | a Inspeção com o conjunto no corte 2 |
| `catalogo/previa_<id>.png` | cada peça no personagem, 8 direções |
| `prancha/` | as faixas da prancha |
| `painel/relatorio.md` | o relatório da bateria final do Painel (993/993) |
| `jogo_20261126/08-mapa-de-testes.png` | o mapa de testes com o mostruário |

## 9. Roteiro curto

**Painel → Jogar** (Novo jogo, ou a sua jornada):
1. Da clareira, siga a margem do riacho para o norte até o pinheiro. A trouxa está ao pé dele: **E**.
2. **I** → aba Equipamentos: o sabre e a túnica comparam com o que você usa. **E** em cada um.
3. Para o chapéu: corte bambu com a faca (Varas de Bambu) e esfole mal um coelho (Pele Rasgada → Tiras); depois
   Fabricação → Chapéu de Bambu Trançado. **E** para vestir.
4. Feche a bolsa e lute com o sabre. Depois de 12 acertos vem a Meia-Lua.
5. **E** de novo numa peça vestida a tira.

**Painel → Inspeção:** Tab arma, R roupa, C chapéu, Z pose, Q/E gira, Espaço anima. Edite um arquivo da tabela acima e
reabra pelo Painel (ele recompila).

## 10. Pendências

- **Saque do Viajante** e **Corte Fluido:** técnicas futuras do sabre (só a ficha).
- **Bíblia de arte:** a emenda à §5.1 (regra dos chapéus) está proposta e ainda não foi escrita.
- **Deitado e ajoelhado** (o despertar): o desenho continua fixo, sem chapéu e sem a roupa trocada.
- **O sabre não corta bambu** (só a faca e o machado cortam). Se quiser, dá para dar a ele `tool` de faca.
- **A água do nado e o anel de Ki** continuam dentro do `Player.cs` (`DrawSwimWater`, perto da linha 1003, e o bloco
  `WaterState.Walk` no `_Draw`; cores `SwimWater`, `SwimFoam`, `WaterWalkRing`). Se quiser editar à mão num arquivo
  próprio, a separação para `src/Elements/Corpo/AguaArt.cs` fica como sugestão para a próxima entrega.
