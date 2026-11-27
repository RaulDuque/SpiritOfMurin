# Conciliação dos pedidos 10 e 13 com o jogo real — 2026-11-27

Comparei o que foi pedido (prompt 10, entregas 4 a 8, e prompt 13) com o que o código faz hoje e com os testes que verificam cada coisa. Cada item recebeu uma de três marcas:

- **EXISTE:** está no jogo e um teste ou uma evidência confirma.
- **PARCIAL:** existe, mas falta uma parte ou é difícil de ver.
- **FALTA:** não há nada no código.

O retrato vale para o commit `601da7d` mais as mudanças do pedido 14. A última rodada completa aprovada é a de 494/494 (pedido 13). Os relatórios antigos dizem "Qi", que hoje é Ki.

**Como foi feito:** li o código e os testes, e conferi as capturas já tiradas pelos testes em `production/qa/evidence/`. Os casos que dependem de comportamento estão cobertos por verificações automáticas em jogo real (PlainsCheck, JourneyCheck, AmbientCheck, NovidadesCheck), que rodam o jogo com janela.

## Entrega 4 — ambientação → resto vai para T09 (clima) e T08 (aves)

| Item | Estado | Onde está / o que falta |
|---|---|---|
| Fases do dia, transições, relógio, luz da fogueira, hora salva | EXISTE | `src/Plains/Ambient.cs` (DayNight), `PlainsHud.cs`. AmbientCheck dia/noite/retomada, nas duas sementes |
| Rajada: capim → copa → folhas | PARCIAL | Os três reagem à mesma rajada **ao mesmo tempo**: a ordem não é encenada. As folhas nascem em ponto aleatório, não de uma árvore. Nenhum teste cobre a copa |
| Pássaros em revoada com sombra | EXISTE | `Ambient.cs` (Birds), só de dia |
| Aves com comportamento próprio (pousar, fugir de você) | **FALTA** | Os pássaros só atravessam o céu em linha reta → **T08** |
| Clima (chuva, trovoada…) | **FALTA** | Não há nada → **T09** |
| Variedade de árvores | PARCIAL | Pinheiro com 3 a 5 andares e 3 verdes, mas tronco de altura fixa e sem inclinação → **T05** (Entrega 5 varia troncos e copas) |
| Variedade de pedras | EXISTE | Redonda, laje e grupo |
| Grupos reconhecíveis para orientação | PARCIAL | Só os fixos da clareira |
| Transições de terreno e emendas dos blocos | PARCIAL | Franja nas 4 direções; cantos e diagonais sem acabamento |
| Ordenação pelos pés, colisão só na base | EXISTE | `Plains.tscn` y-sort, `World/Obstacle.cs` |
| Água rasa lenta com ondinhas; água funda distinta | EXISTE | A água funda **bloqueia**; o ajuste 14.3 (andar sobre a água com Shift e Ki, nadar devagar) muda isso só na Planície |
| Capim alto abre na passagem | EXISTE | Só do jogador. Arbustos reagindo a animais → **T05** |

## Entrega 5 — árvores, pedras, ferramentas → **T05** (esta rodada)

| Item | Estado |
|---|---|
| Cortar árvores, quebrar pedras, machado e picareta, tabela de materiais, bônus por SOR, estado por objeto | **FALTA (tudo)**. `Obstacle` não tem vida nem interação; `WorldState` só guarda coletáveis, mortes e partes |

## Entrega 6 — bolsa, ficha e HUD → **T06**

| Item | Estado | Nota |
|---|---|---|
| Ícones, nomes e pilhas | EXISTE | bolsa com figuras (pedido 13) |
| Categorias | **FALTA** | grade única + seção Fabricar |
| Detalhe com função, efeito e ações | PARCIAL | qualidade só no nome da erva |
| Equipar e usar por teclado e mouse | EXISTE | |
| Comparar com o que está na mão | **FALTA** | |
| Fabricação com "tenho/necessário" e motivo | PARCIAL | é uma seção, não aba; nenhuma receita exige ferramenta ou proficiência ainda (a Entrega 5 cria as primeiras) |
| Atalhos de itens configuráveis | **FALTA** | `Prefs.QuickSlots` existe, mas nada o usa |
| H para a erva | EXISTE | exige a Bolsa Simples |
| Abrir/fechar painel não age no mundo | EXISTE | o teste do clique roda no Vale, não há um para a bolsa da Planície |
| Ficha com "+" e prévia | PARCIAL | a prévia existe e a linha inteira é clicável, mas não há botão "+" visível. FOR já entrou (pedido 14.0) |
| Vida em % no HUD | EXISTE | |
| Vida absoluta ao inspecionar ("Vida 50% · 6/12") | **FALTA** | os números existem só na barra do inimigo focado (14.0) |
| Rodapé com fundo | EXISTE | |
| Rodapé contextual | PARCIAL | só a dica do K muda |
| Ganhos e XP perto do personagem, agrupados | **FALTA** | as mensagens ficam embaixo, no centro, sem agrupar |
| Escolher entre pegada e erva próximas | PARCIAL | o alvo é anunciado antes do E, mas não dá para alternar |

## Entrega 7 — combate, rastreamento e fauna → **T07**

| Item | Estado | Nota |
|---|---|---|
| Aviso do javali (casco, cabeça baixa, olho, poeira) | EXISTE (visual) | o som "warn" existe e não toca |
| Faixa da trajetória no chão, com opção de reduzir | PARCIAL | 4 marcas curtas; o corredor inteiro só aparece no Modo de assistência; `Prefs.ChargeLine` não é usado |
| Aparado × investida acerta × contra-ataque | PARCIAL | na Planície os golpes não têm som, nem pausa de impacto, nem tremor; a reação do corpo é fraca |
| Barra do animal focado | PARCIAL | fica no alto da tela (com números desde a 14.0), não junto ao animal |
| Técnicas enfraquecidas visivelmente diferentes | EXISTE (pela distância) | nenhum sinal visual de "enfraquecida" |
| Coelho: duas arrancadas e toca | EXISTE | |
| Orelha erguida | EXISTE | |
| Momento da fuga | PARCIAL | sem som nem poeira de susto |
| Entrada na toca | **FALTA** | ele simplesmente some |
| Shift faz diferença | PARCIAL | o comportamento muda, mas nada na tela mostra o ruído |
| Pegadas seguem o movimento real | PARCIAL | as trilhas são geradas e fixas; os animais não deixam rastro |
| Examinar indica a direção | PARCIAL | dá a bússola em texto e acende as pegadas; não há seta no mundo |
| Rastreamento aumenta a precisão | PARCIAL | a direção já sai exata no grau 0 |

## Pedido 13 — confirmado

Mira pelo mouse, Disparo de Ki Concentrado, Reforço de Ki, bolsa com figuras, moedas e ambientação **existem**, com testes e imagens. A única ressalva: o teste "clicar para fechar não age no mundo" roda no Vale; falta um igual para a bolsa da Planície (anotado em T06).
