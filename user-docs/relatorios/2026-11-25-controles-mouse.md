# Controles no mouse — 2026-11-25

> O pedido está em [prompts/06-controles-mouse.md](prompts/06-controles-mouse.md). Este relatório não contém spoiler do túmulo.

## Para jogar no Windows

`file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html`

O endereço é o mesmo de antes e funciona no Edge e no Chrome. A jornada salva e as teclas ficam no navegador em que você joga.

| Ação | Teclado | Mouse (padrão) |
|---|---|---|
| Corte, encadeia até 3 | J | **Mouse esquerdo** (clique de novo durante o golpe) |
| Passo; com o alvo travado e o passo lateral aprendido, direção para o lado + passo contorna | L | **Mouse direito** (com a direção em WASD) |
| Onda de qi | K | (livre para atribuir) |
| Travar no alvo | Espaço | (livre) |
| Andar | W A S D | — |

Teclado e mouse funcionam juntos, e dá para alternar no meio da luta.

## O que mudou

Tudo em `prototypes/spiritofmurin/index.html`.

**Os botões do mouse viram entradas como as teclas**
- O esquerdo, o do meio e o direito (e os laterais, se houver) passam pela mesma regra do teclado.
- O clique esquerdo usa a mesma janela de encadeamento do J.
- O clique direito usa a mesma lógica do L: sem trava é o passo curto; com trava e o passo lateral aprendido, contorna o alvo para o lado indicado em WASD.

**Menu do navegador:** o clique direito não abre mais o menu do navegador sobre o jogo.

**Duas colunas por ação na tela de Teclas** (O no título ou na pausa)
- "Tecla" e "Mouse / extra". A segunda já vem com o corte no Mouse esquerdo e o passo no Mouse direito.
- **Clique** num quadrinho para trocá-lo e depois aperte a tecla ou o botão do mouse que quiser. Também funciona com as setas e Enter.
- Os nomes aparecem em português: "Mouse esquerdo", "Mouse do meio", "Mouse direito".
- Na coluna Mouse / extra, Backspace limpa o espaço.
- Se o botão já está em outra ação, as duas trocam de lugar. A tecla principal de uma ação nunca fica vazia: quando isso aconteceria, a tela avisa e não troca.
- Clique direito ou Esc volta.
- "Restaurar o padrão" volta tudo, inclusive o mouse.

**Os cliques nos menus ficam nos menus**
- Clicar no título (Continuar, Novo jogo, confirmação), na tela de Teclas, na pausa ou no Livro não faz o personagem atacar ao voltar ao jogo.
- Um clique esquerdo também fecha os textos de examinar, sem atacar.

**Textos de ajuda:** as listas de controles e a dica "contornar" mostram as duas entradas, por exemplo "L/Mouse dir. + W / S: contornar".

**Teclas que você já tinha:**
- continuam na mesma chave do navegador e não mudam;
- o mouse fica numa chave nova (`spiritofmurin.teclas2`);
- um botão de mouse que você já tenha colocado como tecla principal não é repetido na coluna nova.

## Testes executados

Todos rodaram em **perfil de navegador de teste**, abrindo o mesmo endereço local pela tela de título, com cliques e teclas de verdade.

**1. `tools/playtest/controles-mouse.mjs` — 24 de 24 OK**

Evidências em `production/qa/evidence/2026-11-25_mouse/`.

- **Sessão 1:**
  - um clique no título começa o jogo e não vira ataque;
  - três cliques esquerdos encadeiam os golpes 1, 2 e 3;
  - na ponte, o clique bate na guarda como o J (aparado);
  - S + clique direito dá o passo, e o menu do navegador não abre;
  - K lança a onda de qi;
  - o passo lateral é aprendido pelo percurso normal;
  - um clique fecha o texto da marca sem atacar;
  - na estátua, com o alvo travado, a tecla de lado + clique direito deu 3 passos laterais, e cada clique esquerdo em seguida acertou o flanco;
  - cliques dentro do Livro e na pausa/Teclas não chegam ao jogo;
  - na tela de Teclas, um clique no quadrinho "Mouse / extra" da feitiçaria de qi seguido do botão do meio fez a atribuição, que funcionou no jogo.
- **Sessão 2:** **o navegador foi fechado e reaberto** com o mesmo perfil.
  - A troca (meio = qi) continuou.
  - Meio = qi, J e esquerdo = corte, L e direito = passo, todos funcionando.
- **Sessão 3:** com teclas personalizadas antigas (qi na tecla U) e uma jornada salva antes desta versão:
  - as teclas foram mantidas e o mouse padrão foi acrescentado;
  - "Continuar" segue disponível e um clique nele retoma a jornada sem atacar;
  - U continua lançando o qi.
- Nenhum erro de página.

Na sessão 3, as teclas antigas e a jornada foram escritas direto no armazenamento do navegador de teste, para simular o que você já tinha. Todo o resto foi jogado.

**2. Regressões**
- `ciclo-completo.mjs --lose`: **20 de 20**.
- `meditar-continuar.mjs`: **34 de 34**.

Estão em `production/qa/evidence/2026-11-25_mouse/regressao-*`.

**Ajustes no próprio teste, não no jogo**
- Nas primeiras rodadas, três verificações falharam por erro do teste:
  - os cliques foram calculados para a escala 2, mas a janela de teste mostra o jogo em escala 1;
  - o encadeamento foi testado de frente para o discípulo, e a aparada reinicia a sequência;
  - os cliques saíam devagar demais para encadear.
- Corrigido o teste, tudo passou.

## Limitações

- Não há mira com o mouse. O corte vai para onde o personagem olha, ou para o alvo travado, como no teclado. A posição do cursor só serve nos menus.
- Os botões laterais do mouse podem ser atribuídos, mas não foram testados (o teste usou esquerdo, meio e direito).
- O som não foi verificado.

## Próximas etapas combinadas

Depois do seu teste:
1. `/setup-engine` com **Godot e C#**.
2. O primeiro trecho de geração por semente na Planície Verdejante: coelho, javali, ervas e faca de pedra.

O conteúdo atual vira a base do prólogo. A visão está em `user-docs/futuro-do game/opiniao-claude.md`.
