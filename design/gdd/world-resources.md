# GDD: Recursos do mundo — árvores, rochas, bambus e arbustos

*Criado: 2026-11-27 (pedido 14, entrega 5 da campanha) · Status: implementado · Código: `src/Elements/` · Dados: `assets/data/elements/`, `assets/data/plains/plains_items.json`*

## 1. Overview

Na Planície, o jogador que acordou sem nada recolhe sílex e galhos, fabrica um **machado de pedra** e uma **picareta de pedra** e trabalha o cenário:

- **pinheiros:** derrubados a machado, deixam toco e dão **toras**;
- **rochas com veio:** quebradas a picareta, deixam cascalho e dão **pedra bruta**, às vezes **quartzo**;
- **bambus:** cortados a faca ou machado, dão **varas**;
- **arbustos:** não se trabalham. Eles reagem a quem passa por dentro: folhas e galhos se abrem e voltam.

Cada elemento tem identidade fixa pela semente e pelo tile. O trabalho feito fica salvo, e os materiais saem uma vez só.

## 2. Player Fantasy

"Eu acordei sem nada e, com as próprias mãos, transformei pedra e galho em ferramenta, e ferramenta em material." O golpe tem peso: o tronco recebe entalhes, a rocha racha, a haste cede. O mundo guarda as marcas: o toco continua onde a árvore caiu.

## 3. Detailed Rules

1. **Ferramentas** (`plains_items.json`, campo `tool`):
   - Machado de Pedra: 2 sílex + 2 galhos, força 1.
   - Picareta de Pedra: 3 sílex + 1 galho, força 1.
   - Machado Lapidado: 1 tora + 2 pedras brutas + 1 tira de amarração, força 2.
   - Picareta Lapidada: 1 vara de bambu + 2 pedras brutas + 1 tira de amarração, força 2.
   - A faca de pedra é ferramenta `faca`, de força 1, e corta bambu.
   - Equipar é pela bolsa (E ou clique). O item na mão é salvo (`equipped_item`).
2. **Golpe:** o corte (J ou clique) com uma ferramenta acerta o elemento que está no arco da lâmina, com o mesmo teste dos animais. Cada elemento leva no máximo um golpe por movimento.
3. **Ferramenta certa:** o trabalho sobe a força da ferramenta. A arte mostra o progresso:
   - entalhes no tronco;
   - rachaduras que crescem na rocha;
   - hastes cortadas no bambu.

   Uma barra pequena sobre o elemento mostra quanto falta durante 3 s.
4. **Ferramenta errada:** não trabalha e mostra a frase `refusal` do elemento (no máximo uma vez a cada 1,5 s). Uma **pedra sem veio** com a picareta avisa: "Esta pedra não tem veio".
5. **Último golpe:**
   - o elemento cai ou quebra (árvore 0,9 s, rocha 0,45 s, haste 0,6 s);
   - a colisão muda no mesmo instante: o toco do pinheiro fica com uma base de 4×5 px, e o cascalho e as hastes cortadas não colidem;
   - os materiais vão direto para a bolsa;
   - vem XP.
6. **Persistência:** o trabalho é guardado por id de lugar (`pine@x,y`, `boulder@x,y`, `bamboo@x,y`) no `WorldState` e no save (`worked`).
   - Sair do bloco, meditar, fechar e continuar mantém o meio-cortado e o esgotado.
   - Um elemento esgotado volta como toco ou cascalho e não dá mais nada.
7. **Onde nasce:** o gerador continua decidindo **onde** há árvore, rocha e arbusto, com os mesmos sorteios de antes. O **bambu** tem sorteio próprio, só nos tiles que não ganharam nada; assim as sementes antigas mantêm tudo o que tinham.
   - **Qual** árvore, rocha ou bambu nasce sai dos `habitats` dos arquivos de elemento, com um sorteio fixo pelo tile.
   - A rocha com veio é cerca de 1/3 das rochas.
   - Os elementos garantidos da clareira inicial continuam decorativos.
8. **Arbustos:** não colidem; personagem e animais passam por dentro. Quem passa a até `contact_radius` dobra as folhas na direção em que vai, e uma mola as traz de volta. Durante uma rajada, o vento e o contato se somam. O capim alto também se abre para os animais.

## 4. Formulas

| Fórmula | Definição | Exemplo |
|---|---|---|
| Golpes até esgotar | `ceil((toughness − trabalho) / força)` | Pinheiro (6) com machado de pedra: 6; com lapidado: 3 |
| Progresso | `trabalho / toughness` | 2 golpes no pinheiro: 0,33 (2 entalhes) |
| Materiais | `min + floor(rolagem(semente, lugar, "drop:"+item) · (max − min + 1))` | Pinheiro: 2 ou 3 toras |
| Chance de bônus | `min(cap, chance + per_sor · SOR)` | Quartzo: SOR 0 → 12%, SOR 5 → 32%, teto 60% |
| Bônus | sai quando `rolagem(semente, lugar, "bonus:"+item) < chance` | a rolagem é fixa: mais SOR só acrescenta bônus |

## 5. Edge Cases

- **Golpe na ferramenta errada:** não gasta trabalho, só o vigor normal do corte. A mensagem não se repete mais de uma vez a cada 1,5 s.
- **Golpe num toco ou cascalho:** nada acontece (`AlreadyDepleted`).
- **Dois elementos no mesmo arco:** cada um recebe o seu golpe.
- **Arbusto e animal no mesmo arco:** o animal leva o golpe; o arbusto não é trabalhável e é ignorado.
- **Bloco descarregado durante a queda:** o trabalho já foi salvo no último golpe. O elemento volta como esgotado.
- **Jornada salva antes desta entrega:** carrega com tudo em pé (`worked` vazio). A faca continua na mão: `equipped` era "knife", e isso vira `faca_pedra`.
- **Clareira inicial:** os pinheiros e as pedras forçados ficam decorativos, para a clareira nunca mudar.
- **Toco no caminho de fuga dos animais:** o toco tem base pequena (4×5 px) e o cascalho não colide. O espaço de fuga só cresce.
- **Nadando** (pedido 14.3): o personagem não ataca, portanto não trabalha.

## 6. Dependencies

- `plains-first-stretch.md` (gerador, habitats, coleta de sílex e galho) e `plains-progression.md` (SOR, XP).
- O save (`src/Save/SaveGame.cs`: `worked`, `equipped_item`) e o `WorldState`.
- O corte do personagem (`Player.BladeActive`, `InBladeArc`).
- A bolsa (`Equip`, receitas).
- Recíproca: `plains-first-stretch.md` gera os lugares que este sistema transforma em elementos.

## 7. Tuning Knobs

| Ajuste | Onde | Faixa segura | Efeito |
|---|---|---|---|
| `toughness` | cada `assets/data/elements/**.json` | 2–12 | golpes por elemento |
| `drops.min/max` | idem | 1–5 | quanto material |
| `bonus.chance / per_sor / cap` | idem | 0–0,3 / 0–0,06 / 0,3–0,8 | raridade e peso da SOR |
| `habitats` | idem | pesos ≥ 0 | onde cada elemento nasce e a proporção dentro da família |
| `contact_radius` | `arbustos/arbusto.json` | 10–24 | a que distância o arbusto reage |
| `tool.power / reach` | `plains_items.json` | 1–3 / 18–28 | força e alcance das ferramentas |
| `bamboo` por habitat | `plains_worldgen.json` | 0–0,1 | quantas touceiras |

## 8. Acceptance Criteria

Verificados por `tests/integration/RecursosCheck.cs`, nas sementes 20261126 e 777, e por `tests/unit/Plains/ResourceRulesTests.cs`:

1. Com 5 sílex e 3 galhos, dá para fabricar o machado e a picareta.
2. O pinheiro recusa a faca e a picareta. Com o machado, 2 golpes deixam o trabalho em 2 de 6, com os entalhes visíveis. O 6º golpe o derruba e deixa um toco de base 4×5. Ele dá 2 a 3 toras uma vez, e golpear o toco não dá mais nada.
3. A rocha com veio recusa o machado. A picareta faz rachaduras, e no 5º golpe a rocha vira cascalho que não colide, dando 2 a 3 pedras. A pedra sem veio avisa que não tem veio.
4. O bambu cede à faca, uma haste por golpe, e dá 2 a 3 varas.
5. Sair do bloco e voltar mantém o meio-cortado, o toco e o inventário, sem material em dobro. Meditar salva. Continuar restaura tudo.
6. O Machado Lapidado (tora + pedra) derruba um pinheiro em 3 golpes.
7. Um coelho atravessando um arbusto inclina as folhas na direção do movimento, e elas voltam ao repouso. O personagem atravessando, durante uma rajada, faz o mesmo e não é bloqueado.
8. SOR: em 400 lugares fixos, o quartzo sai em cerca de 12% com SOR 0, 32% com SOR 5 e 60% com SOR 20 (teto). Mais SOR nunca tira um bônus.
9. Uma jornada de antes desta entrega carrega com tudo em pé e a faca na mão.
