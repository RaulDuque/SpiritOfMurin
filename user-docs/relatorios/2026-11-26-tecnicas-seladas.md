# Selo e retorno enfraquecido das técnicas no nível 3 (entrega 2) — 2026-11-26

> O pedido está em [prompts/10-campanha-inicial.md](prompts/10-campanha-inicial.md), entrega 2. O design está em `design/gdd/campaign-prologue.md`, seção 3.5. Os valores das duas versões estão em `assets/data/combat_techniques.json`.

## O que ficou jogável

**Selado (do despertar até o nível 2):** as técnicas do prólogo não saem por nenhuma entrada.
| Entrada | O que acontece | O que se gasta |
|---|---|---|
| **K** longe de carcaça | "Disparo de Qi está selada: o Qi não responde." | nada |
| **L + lado** com um animal focado (Espaço) | "Passo Lateral está selada: fica o passo comum." e o passo comum acontece | só o vigor do passo comum (12) |
| **Shift + direção + L** | "Passo longo está selada: fica o passo comum." | só o vigor do passo comum |
| **frente + J** com foco | o corte comum da faca; a estocada não sai | o vigor do corte |
| passo lateral → corte → Qi | a Lâmina não sai, porque o passo lateral e o Qi estão selados | — |
| clique esquerdo / direito | são o corte e o passo comuns; nunca acionam técnica | — |

A mesma recusa não se repete a cada toque: há um intervalo de 2,5 s.

Andar, andar quieto, caçar, cortar com a faca e o passo comum (L, com ou sem direção) continuam como antes.

**No nível 3:**
- aparecem a faixa "**As técnicas do Vale voltam**" e uma mensagem curta: "Nível 3: as técnicas do Vale voltam, enfraquecidas. Custam Qi e vigor; veja o Livro (B).";
- no **Livro (B)**, cada técnica do Vale passa a "**Enfraquecida**", com o comando, o custo e o efeito em vigor. Exemplo: "Enfraquecida · custo: 45 Qi, 10 vigor · alcance 105 px · dano 1";
- a mensagem aparece **uma vez por jornada**. Subir mais níveis, meditar e carregar não a repetem.

**K conforme o contexto:**
- perto de uma carcaça, a faixa de atalhos diz "**K esfolar com Qi**" e o K esfola (−15 Qi);
- longe dela, diz "**K disparo de Qi**", com "(selado)" enquanto estiver selado;
- um toque faz uma ação só: esfolar com K não dispara nada.

**Sem recurso:** a técnica é recusada dizendo o que falta ("falta Qi (10/45)" ou "falta vigor (5/10)"), e o outro recurso não é gasto.

**Passo longo na Planície:** passa a ser **Shift + direção + L**. Assim, L + direção continua sendo o passo comum e não gasta Qi a cada esquiva. No Vale continua como no protótipo: direção + L.

## Plena × enfraquecida: os números medidos no jogo

As medições foram feitas no motor, pelo `TechCheck`: plena no Vale (`--phase=full`) e enfraquecida na Planície (`--phase=sealed`). Para medir o custo exato, o teste desliga a recuperação natural de Qi durante a medição (controlado).

| Técnica | Medida | Plena (dados → medido) | Enfraquecida (dados → medido) |
|---|---|---|---|
| Disparo de Qi | custo de Qi | 30 → **30** | 45 → **45** |
| | custo de vigor | 0 → **0** | 10 → **10** |
| | distância percorrida | 210 → **210 px** | 105 → **105 px** |
| | dano | 2 → **2** | 1 → **1** (o disparo derrubou um coelho no caminho) |
| Passo Lateral | arco em volta do alvo | 95° → **96,7°** | 55° → **61,7°**\* |
| | custo de Qi / vigor | 0 / 0 → **0 / —** | 15 / 12 → **15 / 12** |
| Passo longo | distância | 112 → **113,8 px** | 60 → **62,4 px** |
| | custo de Qi | 0 | 10 → **10** |
| Lâmina Silenciosa | dano do disparo | 6 (dados) | 3 → **3** (o disparo saiu da sequência passo lateral → corte → Qi) |
| | alcance | 252 px (dados) | 126 → **126 px** |

\* O arco medido sai um pouco maior que o dos dados porque o corpo desliza ao terminar a volta e o alvo se move.

## Testes

| Teste (com janela no monitor 2) | Resultado |
|---|---|
| `TechCheck --phase=full`: versões plenas no Vale (disparo 30 Qi, 210 px, dano 2; passo lateral ~95°; passo longo ~112 px) | **3/3** |
| `TechCheck --phase=sealed`:<br>- nível 2: K recusado sem gastar; a faixa diz "K disparo de Qi (selado)"; foco no javali; L lateral recusado com o passo comum (só vigor); Shift + direção + L recusado;<br>- perto da carcaça a faixa diz "K esfolar com Qi", e um K esfola (−15 Qi, Pele Fina) sem disparar;<br>- nível 3: técnicas de volta com uma mensagem; o Livro mostra custo e efeito; subir de nível de novo não repete;<br>- disparo enfraquecido medido e acertando;<br>- recusa por falta de Qi (vigor intacto) e por falta de vigor (Qi intacto);<br>- passo lateral e Lâmina enfraquecidos medidos;<br>- passo longo com Shift medido;<br>- meditar grava o retorno | **19/19** |
| `TechCheck --phase=resume2`: save no nível 2 continua selado, sem mensagem; K recusado | **2/2** |
| `TechCheck --phase=resume3`: save gravado depois do retorno continua enfraquecido, sem segunda mensagem | **1/1** |
| `TechCheck --phase=migrate` e `migrate2`: save no nível 4 ainda selado recebe o retorno uma vez ao carregar; carregar de novo não repete nada | **2/2 · 1/1** |
| Unidade (inclui 2 novos testes das técnicas) | **30/30** |
| Regressões: `ControlsCheck`; `PlainsCheck`, `JourneyCheck` nova e retomada nas sementes 20261126 e 777; `ValeCheck` em todas as fases | **14/14 · 44/44 · 44/44 · 31/31 · 31/31 · 11/11 · 11/11 · 30/30 · 4/4 · 2/2 · 2/2 · 2/2** |

**O que é controlado:**
- os saves de teste (nível 2 com 20 XP, nível 4 ainda selado) foram montados para o caso;
- os teletransportes;
- o XP que falta para o nível 3, dado pelo teste;
- um coelho abatido por chamada direta, para criar a carcaça do teste do K;
- a recuperação de Qi desligada durante as medições.

Todas as teclas (K, L, Shift, J, Espaço, B, E) são de verdade.

**Para assistir aos testes no monitor 2:** `.\tools\godot\testes-janela.ps1 -SoPrologo -Lento`. Agora o script roda o prólogo e as técnicas.

## Evidências

Pasta `production/qa/evidence/2026-11-26_tecnicas-seladas/`:
- `01-nivel2-disparo-selado.png`: a recusa do K no nível 2, com a faixa "K disparo de Qi (selado)";
- `02-nivel3-tecnicas-voltam.png`: a faixa e a mensagem do nível 3;
- `03-livro-disparo-enfraquecido.png`: o Livro com todas as técnicas "Enfraquecida" e a linha de custo e efeito do Disparo de Qi;
- `*-janela.log`: todos os testes, com as linhas `MEDIDA` dos números acima.

## Pendências

- **Javali e Lâmina:** a Lâmina enfraquecida não fura o aviso e a investida do javali, onde o couro desvia tudo. Ela vale na abertura, como os outros golpes. Se quiser que ela quebre a guarda dos animais como quebrava a dos discípulos, é uma decisão de design.
- **Teclas:** a tela para configurar as teclas ainda não existe no Godot (o protótipo tinha). Os comandos estão no mapa de entradas e são listados na faixa de atalhos.
