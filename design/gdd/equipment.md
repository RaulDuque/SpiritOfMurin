# Equipamento: armas, roupas e chapéus trocáveis (T18)

*Criado em 2026-11-28 com a T18. As decisões estão na prancha `user-docs/relatorios/2026-11-28-t18-prancha-equipamento.html`
e em `user-docs/tasks/T18-equipamento-visual.md`. Os valores ficam em `assets/data/equipamento/`. O desenho e a edição
das camadas estão descritos em `docs/elementos/EQUIPAMENTO.md`.*

## 1. Overview

O protagonista passa a ser desenhado em camadas (corpo, cabeça e cabelo, roupa, chapéu, arma), sem mudar nenhum pixel do
visual aprovado. Armas, roupas e chapéus são itens da bolsa, cada um num arquivo próprio. Equipar uma peça muda o que se
vê nas oito direções e em todas as poses, e muda também os números: a arma traz alcance, dano, combo e tempos próprios; a
roupa e o chapéu seguram dano e pesam no Vigor. As armas podem ter técnicas próprias, descobertas com o uso. O primeiro
conjunto é a túnica de viajante, o chapéu de bambu trançado e o sabre de ferro gasto. A espada do prólogo e a faca de
pedra são as primeiras armas do sistema.

## 2. Player Fantasy

"Achei a trouxa de um viajante e agora me pareço com quem anda pela estrada." O jogador vê no próprio personagem cada
escolha feita na bolsa e sente a troca: o sabre alcança um pouco mais e volta um pouco mais devagar; a túnica de linho
segura um golpe a mais, mas cansa nos passos. O protagonista continua reconhecível: túnica clara, faixa jade e lâmina
baixa fora do eixo.

## 3. Detailed Rules

### 3.1 Espaços e base
- Há três espaços: **mão** (uma arma ou ferramenta, como na T06), **corpo** (uma roupa) e **cabeça** (um chapéu).
- Com o corpo vazio, o personagem veste a **Túnica Verde** (`tunica_verde`, `base: true`). Ela sempre aparece na bolsa e
  não pode ser descartada. Tirar outra roupa devolve a túnica verde.
- Com a cabeça vazia, aparecem o cabelo, o coque, a faixa e a fita.

### 3.2 Bolsa (decisão 1 + T06)
- **E** sobre uma peça a veste ou empunha. **E** sobre uma peça já vestida a tira.
- Cada peça mostra seu ícone e a marca ✓ quando está em uso. O detalhe diz "na mão", "vestindo" ou "na cabeça".
- A comparação é feita contra o que está em uso:
  - armas: alcance (px), dano por corte, cortes no combo, dano do combo, tempo de um corte;
  - roupas e chapéus: defesa (%) e peso (+% de Vigor por corte e por passo);
  - em todas: **onde se consegue**.
- O alcance mostrado é o `reach` da arma, o mesmo número que o golpe usa.

### 3.3 Armas (decisão 2)
- `kind: longa` segue as regras da espada: três cortes, estocada com alvo travado, Reforço de Ki. `kind: curta` segue as
  da faca: corte curto, esfolar, bambu.
- Os números vêm do JSON da arma. A espada e a faca usam os valores que o `PlayerTuning` já tinha, então jogam igual.
- **Técnicas da arma** são descobertas pelo uso. Cada golpe com a arma que acerta um animal ou um elemento conta 1. Ao
  atingir `discover.cuts`, a técnica é anunciada, entra no Livro de Técnicas e passa a valer.
  - **Meia-Lua** (sabre, jogável): o último corte do combo varre ±120° em vez de ±75°, custa 4 de Vigor a mais e deixa um
    rastro 1,4× maior. Sem Vigor para o extra, sai o corte comum.
  - **Saque do Viajante** e **Corte Fluido** (sabre): registradas como futuras, ainda sem efeito no jogo.

### 3.4 Roupas e chapéus (decisões 1 e 3)
- Cada peça vestida soma `defense` à defesa (até `defense_cap` = 40%). O dano recebido vira dano × multiplicador da CON ×
  (1 − defesa).
- Cada peça soma `weight`. O Vigor de um corte e o de um passo são multiplicados por (1 + peso).
- O chapéu define quanto do cabelo cobre (`HairCover`). O chapéu de bambu cobre o coque e a faixa, e a fita some enquanto
  ele estiver na cabeça.

### 3.5 Onde conseguir (Planície)
| Peça | Como |
|---|---|
| Sabre de Ferro Gasto, Túnica de Viajante | na **trouxa de viajante** sob o pinheiro da margem, ao norte da clareira (lugar fixo para todas as sementes); **E** pega a trouxa |
| Chapéu de Bambu Trançado | **Fabricar:** 3 Varas de Bambu + 1 Tira de Amarração |
| Faca de Pedra | **Fabricar:** 1 Sílex + 1 Galho Rijo (como antes) |
| Espada | só no prólogo; ela se perde na explosão |

### 3.6 Desenho
A ordem e as matrizes estão em `docs/elementos/EQUIPAMENTO.md`. Regra visual dos chapéus do protagonista: aba nivelada,
≤ 16 px, olhos à vista (emenda proposta à bíblia de arte §5.1).

## 4. Formulas

- `defesa = min(defense_cap, Σ wear.defense)` sobre corpo e cabeça (a túnica verde vale 0).
- `peso = Σ wear.weight`.
- `dano_recebido = dano × mult_CON × (1 − defesa)` (`EquipRules.DamageTaken`).
- `vigor_corte = base_corte × (1 + peso)`; `vigor_passo = base_passo × (1 + peso)` (`EquipRules.VigorCost`).
- `dano_do_combo = Σ_{n=1..chain} damage × (n = chain ∧ chain ≥ 3 ? third_mult : 1)` (espada e sabre: 2 + 2 + 4 = 8).
- `tempo_de_um_corte = startup + active + recovery` (espada 0,39 s; sabre 0,43 s).
- Meia-Lua: `meio_arco = arc_degrees` (120) no último corte, se `acertos(sabre) ≥ discover.cuts` (12).

Exemplo com o conjunto completo: defesa 8% + 3% = 11%, peso 10% + 2% = 12%. O corte de 3 de Vigor passa a custar 3,36.

## 5. Edge Cases

- **Jornada antiga** (sem `wearing`): vem com a túnica verde, sem chapéu e com a arma salva em `equipped`/`equipped_item`.
- **Peça vestida que sumiu da bolsa ou id desconhecido:** o espaço volta a ficar vazio, sem erro.
- **Arma desconhecida** (desenho não registrado): as mãos ficam sem desenho, e o check `catalogo` falha antes de chegar ao jogo.
- **Ferramentas** (machado, picareta): não são armas, e os números delas continuam vindo de `tool`.
- **De costas e em repouso**, a lâmina fica escondida atrás da túnica, como na espada aprovada. No meio do corte ela aparece.
- **Trouxa já pega:** ela fica marcada no estado do mundo e não volta.
- **Meia-Lua sem Vigor para o extra:** sai o terceiro corte comum.

## 6. Dependencies

- **T06:** bolsa, comparação, atalhos (`BagRules`, `PlainsGame.RefreshRows`).
- **T05:** a faca como ferramenta (`tool`), o bambu, as receitas.
- **Pedido 14:** progressão (CON, FOR), Reforço de Ki, mira pelo mouse, alvo travado.
- **Save:** `wearing`, `weapon_hits`, `weapon_techniques` (`SaveData`).
- **Arte:** `PixelPainter`, bíblia de arte §4.4, §5.1, §5.7.

## 7. Tuning Knobs

| Onde | O quê |
|---|---|
| `assets/data/equipamento/armas/<id>.json` | `reach`, `damage`, `chain`, tempos, `third_*`, `techniques` |
| `assets/data/equipamento/roupas|capacetes/<id>.json` | `defense`, `weight` |
| `assets/data/equipamento/_regras.json` | `defense_cap`, conteúdo da `trouxa` |
| `assets/data/equipamento/_tecnicas_armas.json` | `discover.cuts`, `arc_degrees`, `vigor_extra`, `trail`, `state` |
| `plains_items.json` (`recipes`) | receita do chapéu |
| As classes `<Nome>Art` | comprimento da lâmina, curva, aba, faixa, cores (desenho) |

## 8. Acceptance Criteria (`tests/integration/EquipCheck.cs`)

1. `retrato`: o protagonista padrão sai **idêntico** ao aprovado (0 px) em 12 poses × 8 direções × 5 coisas na mão.
2. `conjunto`: cada peça sozinha e as três juntas saem iguais à referência aprovada. Sem o chapéu, a cabeça aprovada volta (0 px).
3. `sabre`: a empunhadura acompanha a mão em 3 cortes × 5 momentos × 8 direções. De costas, a ordem das camadas está certa. O brilho do Reforço segue a curva.
4. `catalogo`: toda peça da pasta carrega e tem desenho, ícone próprio, números, `onde` e uma origem.
5. `bolsa`, sementes 20261126 e 777: trouxa, I, comparação, equipar cada peça, fabricar e vestir o chapéu, tirar, túnica verde de volta, faca e atalho da T06.
6. `combate`, as duas sementes: alcance da bolsa = alcance do golpe; mira pelo mouse; alvo travado; Reforço; Meia-Lua descoberta e varrendo.
7. `salvar` → `continuar`, as duas sementes: meditar grava, e Continuar devolve o conjunto marcado.
8. `oldsave`: a jornada de antes do Ki volta com a túnica verde, sem chapéu e com a faca.
