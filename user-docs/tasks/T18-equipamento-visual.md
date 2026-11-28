# T18 — Armas, roupas e capacetes trocáveis (equipamento visual)

**Estado:** **pronta (2026-11-28)** — relatório [2026-11-28-t18-equipamento](../relatorios/2026-11-28-t18-equipamento.md) · **Frente:** personagem · **Depende de:** T06 (bolsa com categorias e comparação) · **Origem:** pedido 14 itens 2, 3 e 4 (`/create arma`, `/create roupa`, `/create capacete`)

## Objetivo do jogador

Encontrar ou fabricar uma arma, uma túnica ou um chapéu, vestir pela bolsa e ver no personagem, de frente, de costas e de perfil, em todas as poses.

## O que já existe hoje

- **O protagonista** é desenhado inteiro em `src/Player/Player.cs` (`DrawFighter`), com as cores no topo da classe. É a referência visual aprovada pelo criador: túnica, faixa, cabelo e armas fazem parte desse desenho.
- **Armas:** espada (prólogo) e faca de pedra, desenhadas no `FrontArm`. As ferramentas (machado, picareta) já usam um arquivo próprio (`src/Elements/Tools/ToolArt.cs`) e dados no item (`tool`): é o modelo a seguir.
- **A Vitrine e a Inspeção** mostram o personagem em várias direções.

## O que falta

1. **Camadas:** separar do `Player.cs`, sem mudar um pixel do visual aprovado (comparação pixel por pixel), as camadas cabeça/cabelo, túnica e arma em mão. Cada camada recebe a pose (direção, quadro, mão, ponta) e desenha por cima ou por baixo conforme a vista.
2. **Armas:** um item com `equip: "weapon"` e `weapon`: {forma, alcance, dano, encadeamento, tempos}, desenhado por uma classe `<Nome>Art` (por exemplo, `SabreArt`). A espada e a faca viram as primeiras.
3. **Roupa e capacete:** itens com `wear: "corpo"` ou `"cabeça"`, desenhados por uma arte com as 4 vistas. Defesa: decisão aberta.
4. **Bolsa:** equipar e comparar (depende da T06).
5. **Inspeção:** trocar arma, roupa e capacete no personagem, em 8 direções e nas poses de corte, passo e Ki.
6. **`/create`:** os tipos `arma`, `roupa` e `capacete` passam de "planejamento" a "jogável".

## Decisões do criador (2026-11-28, sobre a [prancha](../relatorios/2026-11-28-t18-prancha-equipamento.html))

1. **A roupa dá defesa?** **Defesa e peso no Vigor** (`defense`, `weight` no JSON de cada peça).
2. **Armas novas:** **técnicas próprias**. As três propostas para o sabre (Meia-Lua, Saque do Viajante e Corte Fluido) são
   técnicas **descobertas com o tempo**. A Meia-Lua já é jogável (12 acertos com o sabre); as outras duas ficam como futuras.
3. **O chapéu e o coque:** cada modelo define o quanto cobre (`HairCover`). O chapéu de bambu **cobre o coque e a faixa** (sem fita).
4. **Primeiro conjunto:** **túnica de viajante + chapéu de bambu trançado + sabre de ferro gasto**, como desenhados na prancha.

## Onde conseguir (Painel → Jogar)

- **Sabre de Ferro Gasto** e **Túnica de Viajante:** na **trouxa de viajante** sob o pinheiro da margem, ao norte da clareira (E pega).
- **Chapéu de Bambu Trançado:** Fabricar, com 3 Varas de Bambu (corte uma touceira com a faca) + 1 Tira de Amarração (de pele rasgada).
- **Túnica Verde:** sempre na bolsa; tirar a outra roupa a devolve.

## Pendências que ficaram

- **Saque do Viajante** e **Corte Fluido** (técnicas futuras do sabre): só a ficha em `assets/data/equipamento/_tecnicas_armas.json`.
- **Emenda à bíblia de arte §5.1** (regra dos chapéus: aba nivelada, ≤ 16 px, olhos à vista): proposta, ainda não escrita na bíblia.
- Deitado e ajoelhado (o despertar), o personagem continua com o desenho fixo, sem chapéu e sem a roupa trocada.

## Critérios de aceite

- O personagem padrão sai **idêntico** ao aprovado, pixel por pixel, nas 8 direções e nas poses.
- Vestir e tirar a túnica e o chapéu, e trocar a arma, aparece nas 4 vistas, e isso fica salvo na jornada.
- `/create roupa`, `/create capacete` e `/create arma` geram arte, item e prévia jogáveis, e um teste passa.

## Roteiro de teste

Painel → Inspeção: Tab arma, R roupa, C chapéu, Z pose, Q/E giram o personagem nas 8 direções, Espaço anima.
Painel → Jogar → Novo jogo: vá à trouxa sob o pinheiro ao norte da clareira; I abre a bolsa.

## Prompt pronto para executar

```text
Tarefa T18 do SpiritOfMurin — armas, roupas e capacetes trocáveis.

Leia GUIA.md, user-docs/tasks/README.md, docs/elementos/PADRAO.md e user-docs/tasks/T18-equipamento-visual.md. O src/Player/Player.cs é a referência visual aprovada pelo criador: separe as camadas (cabeça/cabelo, túnica, arma) sem mudar nada do que se vê, provando com uma comparação pixel por pixel nas 8 direções e nas poses (só imagens do viewport do jogo).

Pergunte-me as decisões abertas uma por vez; para escolhas visuais (cores, forma do chapéu), mostre uma prancha HTML em user-docs/relatorios/ antes. Use o fluxo /asset-spec para as fichas visuais das peças novas. Faça o primeiro conjunto (túnica de viajante, chapéu de bambu trançado, sabre de ferro gasto) e torne jogáveis os tipos arma, roupa e capacete do /create (.claude/skills/create/SKILL.md).

Valores em assets/data, saves isolados, sementes 20261126 e 777, jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, monitor 2. Termine tudo e rode a bateria completa uma vez no fim. Feche pelo CLAUDE.md e atualize a T18 no README de tasks.
```
