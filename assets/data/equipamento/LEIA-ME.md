# Equipamento — uma peça por arquivo (T18)

Cada `.json` em `armas/`, `roupas/` e `capacetes/` é uma peça. O jogo lê a pasta inteira ao começar a jornada
(`src/Items/ItemCatalog.cs`, `LoadEquipment`) e põe as peças na bolsa junto com os itens de
`assets/data/plains/plains_items.json`. Arquivos que começam com `_` são regras, não peças. Como editar o desenho de
cada camada, com três exemplos: `docs/elementos/EQUIPAMENTO.md`. Para criar uma peça: `/create arma|roupa|capacete "…"`.

| Chave | O que é |
|---|---|
| `id` | nome estável, igual ao nome do arquivo. Um id publicado nunca muda: para algo parecido, crie uma variante |
| `name`, `description` | o que a bolsa mostra |
| `onde` | onde se consegue (aparece na bolsa e no relatório) |
| `base` | `true` só na túnica verde: nunca sai da bolsa e é vestida quando nada mais está no corpo |
| `equip`, `tool` | só na faca: `knife` e o bloco de ferramenta da T05 (corta bambu, esfola) |

**`weapon`** (armas):

| Chave | O que é |
|---|---|
| `art` | o desenho na mão (`espada`, `faca_pedra`, `sabre_ferro`: classe `<Nome>Art` em `src/Elements/Armas/`) |
| `kind` | `longa` (como a espada: três cortes e a estocada) ou `curta` (como a faca) |
| `reach` | alcance do corte em px. **É o mesmo número que a bolsa mostra e que o golpe usa** |
| `damage`, `chain` | dano de um corte; cortes no combo |
| `startup`, `active`, `recovery`, `cancel_at`, `lunge` | tempos do corte (s) e o avanço do corpo (px) |
| `third_extra`, `third_recovery`, `third_cancel`, `third_lunge`, `third_mult` | o último corte de um combo de 3 ou mais: tempos a mais, avanço e multiplicador de dano |
| `techniques` | as técnicas próprias da arma (ids em `_tecnicas_armas.json`), descobertas usando-a |

**`wear`** (roupas e capacetes):

| Chave | O que é |
|---|---|
| `slot` | `corpo` (roupa) ou `cabeca` (chapéu) |
| `art` | o desenho (`src/Elements/Roupas/` ou `src/Elements/Capacetes/`) |
| `defense` | fração do dano que segura (0.08 = 8%), somada às outras peças até `defense_cap`, depois da CON |
| `weight` | fração a mais de Vigor por corte e por passo (0.1 = +10%) |

**Regras** (`_regras.json`): `defense_cap` (o máximo que as roupas juntas seguram) e `trouxa` (o que a trouxa de viajante
da Planície contém). **Técnicas das armas** (`_tecnicas_armas.json`): nome, arma, `state` (`jogavel` ou `futura`),
`discover.cuts` (acertos com a arma para descobrir) e os números de cada uma.
