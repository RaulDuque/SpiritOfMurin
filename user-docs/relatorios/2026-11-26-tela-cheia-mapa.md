# Tela cheia, imagem e mapa do mundo (entrega 3) — 2026-11-26

> O pedido está em [prompts/10-campanha-inicial.md](prompts/10-campanha-inicial.md), entrega 3. O design está em `design/gdd/display-and-map.md`.

## O problema e a causa

O problema do vídeo foi reproduzido: uma janela grande ou maximizada mostrava o jogo pequeno no centro, com bordas pretas nos quatro lados.

**Causa:** o projeto usava o modo de esticar `viewport` com **escala inteira** e a proporção padrão `keep`, sobre uma resolução fixa de 640×360. A imagem só cresce em múltiplos inteiros, então:
- uma janela maximizada (1600×877) ou a tela cheia do monitor 2 (1600×900) ficam presas em 2×: o jogo ocupa só 1280×720 e o resto é borda;
- só tamanhos que são múltiplos exatos (1280×720, 1920×1080) enchiam a tela.

O F5 embutido no editor (aba "Game") roda com o mesmo projeto: o painel do editor quase nunca é múltiplo exato de 640×360, por isso a borda aparecia lá também.

**Correção:**
- a escala continua inteira, para o pixel ficar nítido, mas a **resolução do jogo passa a ser a janela dividida por essa escala**;
- a janela maior mostra mais mundo, em vez de bordas;
- o HUD acompanha o tamanho real (`src/Game/DisplayManager.cs`, autoload "Display").

## Medido no monitor 2 (1600×900), mesmo jogo, regra antiga × nova

| Situação | Antes (640×360 fixo) | Depois (janela ÷ escala inteira) |
|---|---|---|
| Janela 1280×720 | jogo 1280×720, 2×, sem borda | jogo 1280×720, 2×, mundo 640×360, sem borda |
| **Janela maximizada** (1600×877) | jogo 1280×720 em (160,79): **bordas de 320 + 157 px** | jogo **1600×876**, 2×, mundo 800×438, sobra de 1 px |
| **Tela cheia real** (1600×900) | jogo 1280×720 em (160,90): **bordas de 320 + 180 px** | jogo **1600×900**, 2×, mundo 800×450, sem borda |
| Janela 1920×1080 | jogo 1920×1080, 3×, sem borda | jogo **1920×1080, 3×, mundo 640×360**, sem borda |

**Comparação visual (antes e depois na tela cheia do monitor 2):** `production/qa/evidence/2026-11-26_tela-cheia-mapa/tela-cheia-antes-depois.png`.
- Nas capturas de tela inteira, a faixa de baixo foi cortada: o Windows desenha a barra de tarefas por cima de uma tela cheia que não tem o foco, e o teste roda ao lado do seu uso normal.
- As capturas do mapa são do próprio jogo. Uma primeira captura da tela inteira pegou o navegador aberto no monitor 2 e foi apagada antes de qualquer commit.

**Em 1920×1080** o mundo fica em 3×, exatamente a resolução base: o personagem fica com ~130 px de altura na tela e o HUD com letras de 24–27 px.

**F5 embutido no editor:** **não foi medido automaticamente.** O teste roda o jogo numa janela própria e não consegue abrir a aba Game do editor. Como a regra agora segue o tamanho real da área, a aba Game também fica sem borda. Confira apertando F5 com a aba Game grande. Se ainda aparecer borda, me avise com o tamanho do painel.

## O que ficou jogável

- **Tela cheia de verdade:** **F11** em qualquer tela, ou no menu de pausa. A preferência fica salva e vale ao abrir o jogo.
- **Menu de pausa (Esc ou P):** Voltar ao jogo · Mapa · Tela cheia (ligada/desligada) · Controles (a lista completa) · Sair. Funciona com teclado e mouse.
- **Mouse em todos os painéis:** título, inventário, ficha, Livro e pausa. A linha sob o cursor é a selecionada, e o clique age nela, em janela ou em tela cheia.
- **Fechar um painel com o clique não corta:** o personagem ignora o botão até ele ser solto.
- **Mapa (M):**
  - **Vale:** só o que você já viu, com os nomes dos lugares vistos, a esteira de meditação e o ponto de missão em andamento.
  - **Planície:** os blocos em que você entrou, montados pela semente: relva, margem, água e vau, lamaçal, arbustos, mais a fogueira, a esteira, as tocas, os javalis vivos e a "clareira".
  - **Comandos:** seta com a posição e a direção do personagem, legenda curta, **WASD** move, **roda ou + / −** dá zoom, **C** recentraliza, **X ou clique** marca ou desmarca.
  - **Salvo com a jornada:** os blocos explorados e os marcadores, no Vale e na Planície.

## Testes (com janela no monitor 2)

| Teste | Resultado |
|---|---|
| `DisplayCheck --phase=measure`: a tabela acima, nas duas regras, mais o HUD acompanhando a tela cheia; capturas antes e depois | **5/5** |
| `DisplayCheck --phase=click`, em tela cheia:<br>- a linha do inventário tem lugar na tela;<br>- o mouse sobre ela a seleciona e o clique age (equipa a faca);<br>- Esc abre a pausa;<br>- o clique em "Voltar ao jogo" fecha sem cortar;<br>- a pausa desliga e religa a tela cheia;<br>- F11 alterna | **8/8** |
| `DisplayCheck --phase=map`: M abre; mostra o bloco e a clareira; X marca; WASD move; C recentraliza; + aproxima; M fecha; explorar revela mais; meditar salva os blocos e o marcador | **9/9** |
| `DisplayCheck --phase=map2` (outro processo): os blocos explorados e o marcador voltam; o mapa abre com eles | **2/2** |
| `ValeCheck --phase=prologue`: agora com o mapa do Vale (só o visto) e o marcador | **32/32** |
| `ValeCheck --phase=resume-vale`: o marcador e o visto voltam | **5/5** |
| Regressões: unidade, controles, Planície e jornada nas sementes 20261126 e 777, Vale (resume-ending, old-save, skip), TechCheck (6 fases) | **30/30 · 14/14 · 44/44 · 44/44 · 31/31 · 31/31 · 11/11 · 11/11 · 2/2 · 2/2 · 2/2 · 3/3 · 19/19 · 2/2 · 1/1 · 2/2 · 1/1** |

**O que é controlado:**
- os tamanhos de janela são aplicados pelo teste;
- o teste alterna a regra antiga e a nova para medir;
- teletransporte para explorar outro bloco.

O mouse e as teclas são eventos de entrada de verdade.

## Evidências

Pasta `production/qa/evidence/2026-11-26_tela-cheia-mapa/`:
- `tela-cheia-antes-depois.png`: **a comparação pedida**, lado a lado. `tela-cheia-antes.png` e `tela-cheia-depois.png` são as capturas separadas;
- `mapa-planicie.png` e `mapa-planicie-depois-de-carregar.png`: o mapa da Planície, com o marcador recuperado depois de carregar;
- `vale/05b-mapa-vale.png`: o mapa do Vale;
- `displaycheck-*-janela.log` (as linhas `MEDIDA`) e `regressao/*.log` (as regressões) ficam só na sua máquina: o `.gitignore` do projeto exclui `*.log`. Os números estão na tabela acima.

## Pendências

- **F5 embutido:** não foi medido pelo teste (explicado acima).
- **Fauna no mapa:** os javalis aparecem nos blocos explorados mesmo que você não os tenha visto. É uma simplificação: o bloco revelado mostra o que há nele.
- **Mapa mais fino:** a revelação é por bloco inteiro de 16×16 tiles. Se quiser uma revelação mais fina, pelo raio de visão, dá para trocar.
