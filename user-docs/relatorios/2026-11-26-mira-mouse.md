# Mira livre pelo mouse (pedido 13, entrega 1) — 2026-11-26

> O pedido está em [prompts/13-mira-tecnicas-bolsa-moedas.md](prompts/13-mira-tecnicas-bolsa-moedas.md), item 7.

## O que mudou

- **Sem alvo travado, o ataque vai para o cursor.** Você pode andar para a esquerda, apontar o mouse para a direita e cortar para a direita. Vale para:
  - a espada (cada corte do combo mira de novo);
  - a faca;
  - o Disparo de Qi;
  - qualquer tecla ou botão configurado em Controles.
- **Uma direção só:** o corpo vira para o alvo no instante em que o golpe começa. A pose, o risco da lâmina, a área que acerta e o projétil usam essa mesma direção. O teste confere que o pote do lado do cursor é o que quebra.
- **De tela para mundo:** a direção vai do peito do personagem (14 px acima dos pés) até o ponto do mundo sob o cursor. A posição do mouse passa pela escala da janela e pela câmera (`GetCanvasTransform`), por isso funciona em janela, maximizada e tela cheia.
- **Alvo travado (Espaço) tem prioridade:** o ataque vai para o alvo, mesmo com o cursor atrás de você. Ao destravar, o próximo ataque já volta ao cursor.
- **Comandos relativos ao oponente continuam iguais:** a estocada (frente + corte com trava) e o passo lateral (lado + passo com trava).
- **Só teclado:** em **Esc → Controles**, o botão **"Mira: pelo mouse / direção do personagem"** troca para mirar pela direção em que você anda, como antes. A escolha fica salva.
- **O clique da interface fica na interface:** fechar o menu de pausa com um clique não corta (testado de novo).

## Testes

**Teste novo** `tests/integration/NovidadesCheck --phase=aim`, com janela no monitor 2: **12/12**.

| Verificação | Resultado |
|---|---|
| Andando para a esquerda com o cursor à direita, J corta para a direita (ângulo 7°) | OK |
| A área de acerto concorda: o pote da direita quebra | OK |
| Andando para a esquerda com o cursor à direita, K atira para a direita | OK |
| Corte remapeado (U) mira para cima, onde está o cursor | OK |
| A faca mira para a esquerda, onde está o cursor | OK |
| Travado na estátua com o cursor atrás: o corte vai para o alvo | OK |
| Destravado: o próximo corte volta ao cursor | OK |
| Mira por teclado: o corte segue a direção do personagem | OK |
| Maximizada e tela cheia: o cursor a 45° dá um corte a 45° | OK |
| O clique que fecha a pausa não corta | OK |

**Regressões com janela:**

| Teste | Resultado |
|---|---|
| Unidade | 30/30 |
| Controles | 14/14 |
| Planície, sementes 20261126 · 777 | 44/44 · 44/44 |
| Jornada nova · retomada | 31/31 · 11/11 |
| Prólogo do Vale | 32/32 |
| Recuperação: vale · vale2 · controls · controls2 | 20/20 · 3/3 · 13/13 · 4/4 |
| Técnicas seladas | 19/19 |

**Ajuste nos testes antigos:** com janela, o cursor real do Windows sobre a janela de teste também mirava, e dois testes que esperam o golpe na direção do personagem falharam (controles e contra-ataque ao javali). Agora os testes antigos usam a mira pela direção só durante a execução (`Aim.UseForSession`), sem gravar nas preferências.

**Painel e script:** o teste novo entrou no `PainelDeTestes.exe` (grupo "Novidades (pedido 13)") e no `tools/godot/testes-janela.ps1`.

**Evidências:** `production/qa/evidence/2026-11-26_mira-mouse/`
- `01-mira-anda-esquerda-corta-direita.png`
- `02-mira-travado-cursor-atras.png`

## Como experimentar

PainelDeTestes.exe → **Jogar** → Novo jogo. Ande para um lado com A ou D e aponte o mouse para o outro:
- corte com J ou com o clique esquerdo;
- solte o Qi com K;
- trave na estátua ou num discípulo com Espaço: o golpe vai para ele;
- Espaço de novo destrava, e o golpe volta ao cursor.
