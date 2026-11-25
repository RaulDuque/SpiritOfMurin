# Passar atrás de pedras, árvores e bambus — 2026-11-25

## Pedido do criador

"Uma coisa: a colisão com pedra, árvores e o bambu, pode colocar só na base dele para que eu possa passar por trás?"

## O que mudou

Tudo em `prototypes/spiritofmurin/index.html`. Segue a decisão do art bible (Seção 6.5: o bambu tem base e corpo; 8.11: a silhueta aparece através do cenário).

**Colisão só na base**

| Objeto | Faixa que bloqueia (provisória) | O que dá para fazer |
|---|---|---|
| Pedra solta (inclusive as pedras gêmeas) | os 13 px de baixo, 24 px de largura | passar atrás da pedra |
| Pinheiro | o tronco: 11 px de baixo, 14 px de largura | passar atrás da copa |
| Bambu | os 7 px de baixo de cada fileira | entrar no bambuzal, andar ao longo das fileiras e ficar escondido entre os colmos |

Os números ficam no início do código, em `BASE`, para ajuste.

**Silhueta:** quando um desses objetos cobre o personagem, uma silhueta clara dele aparece por cima, para você não se perder atrás do cenário.

**Continuam sólidos por inteiro:** paredões, bordas do mapa, água, muros do túmulo, tendas, estela, portão e porta.

**A onda de qi** também passa por cima das copas e só para na base.

**Limitação:** dentro do bambuzal você anda ao longo das fileiras, mas não atravessa de uma fileira para a outra. A base de cada fileira bloqueia, como uma moita densa.

## Testes

Todos foram feitos com teclado real, num perfil de navegador de teste.

**`tools/playtest/passar-atras.mjs` — 13 de 13**, rodado duas vezes:
- dá para entrar na fileira do bambuzal da trilha e andar ao longo dela;
- dá para ficar atrás do pinheiro (25,22) e da pedra (33,29), com a silhueta ligada;
- a base de cada um continua bloqueando;
- os paredões continuam paredes.

Evidências em `production/qa/evidence/2026-11-25_passar-atras/`.

Na primeira rodada uma verificação falhou: o jogador automático parou na borda de cima da pedra, onde ela ainda não cobre o corpo, e por isso a silhueta, corretamente, não ligou. Aprofundei o ponto de parada no teste e as duas rodadas seguintes deram 13 de 13.

**Regressões:**

| Teste | Resultado |
|---|---|
| `ciclo-completo.mjs --lose` | 20 de 20 |
| `meditar-continuar.mjs` | 34 de 34 |
| `controles-mouse.mjs` | 24 de 24 |

## Para jogar no Windows

`file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html`

Para ver: na tela de título, comece um jogo novo, suba a trilha e entre pela parte de cima do bambuzal à direita, antes da ponte.
