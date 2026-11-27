# T05 — Árvores, pedras, bambus e arbustos (Entrega 5 da campanha)

**Estado:** pronta (2026-11-27, pedido 14) · **Frente:** mundo · **Depende de:** — · **Origem:** pedido 10 (Entrega 5), pedido 14 item 5

## Objetivo do jogador

Sair da fogueira, recolher sílex e galhos, fabricar e equipar um machado e uma picareta, e trabalhar o cenário. A árvore cai e deixa toco, a pedra racha e vira cascalho, o bambu cede. Os materiais saem uma vez só e viram ferramentas melhores. Os arbustos se abrem quando alguém passa.

## O que existe agora

- **Design:** `design/gdd/world-resources.md` (regras, fórmulas, casos limite, critérios).
- **Código:**
  - `src/Elements/` (`WorldElement`, `ResourceRules`, as artes);
  - `PlainsGame` (`StrikeElement`, `Equip`, `OnElementUsedUp`);
  - `PlainsWorld.MakeElement`.
- **Dados:** `assets/data/elements/**` (pinheiro, salgueiro, pedra, rocha_quartzo, rocha_ferro, bambu_verde, arbusto) e `plains_items.json` (ferramentas, materiais, receitas).
- **Prévia:** Painel → **Inspeção (elementos)** (`src/Dev/Inspecao.tscn`).
- **Testes:**
  - `tests/integration/RecursosCheck.tscn` (work, resume e bush nas duas sementes; oldsave);
  - `tests/integration/ElementosCheck.tscn` (catalogo; mundo nas duas sementes);
  - `tests/unit/Plains/ResourceRulesTests.cs` (inclui a SOR reproduzível).
- **Relatório:** [2026-11-27-pedido14-organizacao-e-entrega5.md](../relatorios/2026-11-27-pedido14-organizacao-e-entrega5.md).
- **Evidências:** `production/qa/evidence/2026-11-27_recursos/`, `2026-11-27_create-salgueiro/`, `2026-11-27_create-rocha-ferro/`.
- **Commits:** `8e71ee4` (artes), `cbfcacf` (entrega), `cad585f` (testes), `c4e47b7` (correção das listras), `bb6bed3` (inspeção), `fe5c988` e `5a792e6` (ElementosCheck), `ace82af` (salgueiro), `59d40c3` (rocha de ferro).

## Critérios de aceite (todos verificados)

Os critérios 1 a 9 do `design/gdd/world-resources.md` §8.

## Como testar à mão

Siga o roteiro "Painel → Jogar" no relatório da entrega.

## O que ficou para outras tarefas

- **Mais espécies de árvore, rocha e bambu:** use o `/create`.
- **Pá e escavação:** T10.
- **Madeira e pedra em construções e forja:** T14/T15 (ferreiro).
- **Aves pousando nas árvores:** T08.
