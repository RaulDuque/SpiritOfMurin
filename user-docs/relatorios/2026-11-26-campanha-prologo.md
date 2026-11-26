# Campanha: Novo jogo começa no prólogo do Vale (entregas 0 e 1) — 2026-11-26

> O pedido completo, com as entregas de 0 a 8, está em [prompts/10-campanha-inicial.md](prompts/10-campanha-inicial.md). O design está em `design/gdd/campaign-prologue.md`.

## Entrega 0: o que estava aguardando a janela

Já estava publicado:
- as correções da pegada perto da erva e do XP que faltava para o nível 2 entraram no commit [`7e65dc2`](https://github.com/RaulDuque/SpiritOfMurin/commit/7e65dc2);
- os testes com janela no monitor 2, o percurso normal e o relatório entraram nos commits seguintes, até [`453c2f3`](https://github.com/RaulDuque/SpiritOfMurin/commit/453c2f3).

Esse foi o ponto de partida. Nenhum commit novo foi preciso.

**A sua jornada pessoal foi preservada:**
- `%APPDATA%\Godot\app_userdata\SpiritOfMurin\jornada_planicie.json` (última gravação às 15:51, pelo seu jogo) não foi tocada;
- todos os testes usam arquivos próprios (`teste_vale*.json`, `teste_jornada*.json`, `teste_percurso.json`) e preferências próprias (`teste_vale_prefs*.json`).

## Entrega 1: o que ficou jogável

O **F5** agora abre a **tela de título da campanha** (cena principal `res://src/Campaign/Campaign.tscn`).

### Novo jogo: o prólogo no Vale de Bambus
O mapa e o percurso foram portados do protótipo HTML, tile a tile, com a mesma aparência procedural:
1. A trilha da montanha e o **Discípulo da Guarda Fechada** na ponte do Rio Nublado: de frente, ele apara tudo ("aparado").
2. O **corpo no bambuzal** com a metade do manual.
3. O **selo do túmulo**, que o manual completa.
4. A **marca na pedra** e as **três pegadas** junto à estátua da mestra: pisar nelas na ordem ensina o **Passo Lateral** e o **Passo longo**.
5. A **estátua**, um alvo de treino: apara de frente e responde ao golpe no flanco.
6. A **inscrição além da fenda**, que se atravessa com o passo longo, e o baú.
7. O **Passo da Lâmina Silenciosa** (passo lateral → corte → Qi), descoberto fazendo: quebra a guarda.
8. O guardião cai e o **portão do acampamento** se abre.
9. O **Espadachim Sênior**: guarda fechada, postura de ataque com combo de três golpes e abertura depois.

Também vieram do protótipo:
- Travar no alvo (Espaço), estocada (frente + J com trava), disparo de Qi (K), passo lateral (L + lado do alvo travado) e passo longo (L + direção, sem trava).
- Potes, ervas, bálsamo (H), pedra de Qi, missões, área no topo, falas dos inimigos e **Livro de Técnicas (B)**.
- **Meditar** na esteira das pedras gêmeas grava a jornada.
- **Derrota:** você volta ao início da trilha, ou à entrada do acampamento depois do portão. Os inimigos voltam com a vida cheia e se pode tentar de novo.
- O **som** agora existe no Godot: sintetizado em código, como no protótipo, sem arquivos.

### A vitória sobre o Sênior e o fim do prólogo
A luta precisa ser vencida de verdade: a vida dele chega a 0 pelos golpes. Depois disso:
1. **Vitória:** a faixa "Vitória" e o som de vitória. A vitória já é **gravada**.
2. **Ele revive** depois de 2 s: muda de postura e a aura cresce até levantar **poeira**, lançar **folhas** e fazer **tiras de tecido** tremularem. Um ronco grave e a tela treme.
3. Ele encara você e diz **"Você acha que pode me derrotar? Pois vou garantir que não."** **E** avança a fala; sozinha, ela avança em 6 s.
4. Ele puxa a energia para o corpo e **se explode**: clarão e anéis de choque. A câmera fica parada no lugar da explosão, e o espadachim é **lançado girando para fora do quadro**, por cima das montanhas. Esses dois ajustes vieram dos seus pedidos durante a sessão.
5. **Escuridão e silêncio:** o volume vai a zero e se ouvem duas batidas fracas de coração.
6. **Claridade:** uma luz pálida se desfaz, o vento e os pássaros voltam e você está **deitado na relva da Planície**. Mover-se (WASD) ou **E** o levanta.
7. **Mãos vazias:** espada, bálsamos e tudo o mais se perderam. O **Livro (B)** continua com o que você aprendeu, e as técnicas do Vale aparecem como **"Selada"**.

**Pular o fim:** depois de vê-lo uma vez, **Esc** ou **Enter** pulam direto para o despertar.

### Etapas persistentes
| Se você fechar o jogo… | Ao escolher Continuar |
|---|---|
| no Vale, depois de meditar | volta às pedras gêmeas com o manual, o túmulo aberto, o passo lateral, o portão aberto e os inimigos derrotados |
| em qualquer ponto do fim (vitória, revivescência, fala, explosão, escuridão) | vai para o **despertar** na Planície. A perda acontece aí, uma vez |
| depois do despertar | continua na Planície. A perda não se repete; o que ganhou e gravou meditando fica |
| com um save da Planície de antes da campanha | continua na Planície com os seus itens, atributos, experiência e proficiências, sem perda e sem Livro do Vale |

A **Planície direta** continua acessível pela cena de teste `res://src/Plains/Plains.tscn`: no editor, abra a cena e use **F6** ("Rodar cena atual").

### O selo, já nesta entrega
**K** na Planície, longe de uma carcaça, é recusado: "Disparo de Qi está selada: o Qi não responde." Não gasta Qi. Perto de uma carcaça, K continua sendo "esfolar com Qi". O resto da regra do selo (nível 3, versões enfraquecidas) é a entrega 2.

## Roteiro rápido pelo F5

1. **F5** → título → **E** em **Novo jogo**. Se você tem jornada salva, aparece **Continuar** e o Novo jogo pede confirmação. **Atenção:** confirmar apaga a sua jornada da Planície.
2. Suba a trilha. Na ponte, **Espaço** trava no discípulo; corte de frente e veja "aparado". Fuja e desça para o vale, a leste.
3. No bambuzal, **E** no corpo → o manual. No túmulo, **E** no selo → a porta abre. **E** na marca na pedra; pise nas três pegadas na ordem → **Passo Lateral**.
4. **Espaço** na estátua, **L + A ou D** para contornar, **J** → "flanco".
5. Volte à ponte: trave no discípulo, **L + lado**, **J**, **K** → **Lâmina Silenciosa**. Repita até ele cair; o portão abre.
6. Medite na esteira das pedras gêmeas, se quiser poder voltar ali.
7. Suba ao acampamento e vença o Sênior. Na postura dele de ataque, desvie com o passo lateral e use a Lâmina na abertura.
8. Assista ao fim (**E** avança a fala) e acorde deitado na Planície. **WASD** levanta. **B** mostra o Livro selado.

## Como rodar os testes (sua pergunta)

O teste parece "extremamente rápido" porque ele teletransporta o personagem até cada lugar; as ações são teclas de verdade. Para assistir no monitor 2, em ritmo mais lento:
```powershell
cd D:\Progr\SpiritOfMurin
.\tools\godot\testes-janela.ps1 -SoPrologo -Lento   # só o prólogo, com pausa a cada passo
.\tools\godot\testes-janela.ps1 -Lento              # tudo (prólogo, unidade, controles, Planície, jornada)
```

## Testes

**Observado num fluxo normal:** o `ValeCheck --phase=prologue` entra **pela tela de título** (só "Novo jogo" quando não há jornada) e aperta E. O `--phase=resume-vale` entra pelo título com **Continuar (Vale de Bambus)**. As ações em todo o prólogo são teclas: Espaço, J, L com direção, K, E.

**Controlado** (dito em cada verificação):
- teletransportes entre os lugares;
- a Vida reposta pelo teste durante as trocas com o Sênior e o guardião;
- na fase `skip`, a morte do Sênior por chamadas diretas, porque a luta já é conferida na fase `prologue`.

| Teste (com janela no monitor 2 e sem janela) | Resultado |
|---|---|
| `ValeCheck --phase=prologue`: título; Vale com espada e Livro inicial; trava; Guarda Fechada apara; manual; selo; marca; pegadas ensinam o passo lateral; estátua responde ao flanco; passo longo atravessa a fenda; inscrição; Lâmina Silenciosa; guardião cai e portão abre; meditar grava; **o Sênior vencido pela luta**; vitória gravada (fase `senior_won`); ele revive com aura (0,70); a fala exata; E avança; a carga; a explosão lança o espadachim **560 px, para fora do quadro**; escuridão (overlay 1,0) e silêncio (volume < 0,6); despertar na Planície com mãos vazias e sem itens; Livro com as técnicas "Selada"; a região muda para Planície uma vez; claridade; **deitado ao acordar**; mover-se o levanta; K recusado como selado, sem gastar Qi | **30/30** |
| `ValeCheck --phase=resume-vale`: título com Continuar (Vale de Bambus); a história restaurada; técnicas plenas; nas pedras gêmeas | **4/4** |
| `ValeCheck --phase=resume-ending`: fechado durante o fim → despertar; meditar ali grava como Planície, perda aplicada uma vez e itens mantidos | **2/2** |
| `ValeCheck --phase=old-save`: save formato 1 (Planície de antes) → Planície com faca, peles, nível 3, VIT 2, Esfolar Adepto, sem perda | **2/2** |
| `ValeCheck --phase=skip`: com o fim já visto, Esc pula para o despertar sem a explosão | **2/2** |
| Regressões com janela no monitor 2: unidade, `ControlsCheck` (espada), `PlainsCheck` e `JourneyCheck` (nova e retomada) nas sementes 20261126 e 777 | **28/28 · 14/14 · 44/44 · 44/44 · 31/31 · 31/31 · 11/11 · 11/11** |

Todos os logs registram "window on screen 1 of 2".

## Evidências

Pasta `production/qa/evidence/2026-11-26_campanha-prologo/`:
- `fim-do-prologo-ate-a-planicie.gif`: **o vídeo pedido do fim do prólogo até os primeiros passos na Planície**. Mostra a luta com o Sênior, a vitória, a revivescência com poeira e folhas, a fala, a explosão, a escuridão, a claridade e o despertar.
- `prologo/`: as capturas do teste, de `01-vale-inicio` a `12-levantou` (guarda apara, manual, passo lateral, portão aberto, vitória, aura, fala, explosão, claridade, mãos vazias e se levantando).
- `valecheck-*-janela.log` e `valecheck-*-headless.log`: todas as fases. `regressao-*-janela.log`: as regressões.

## Pendências desta entrega

- **Tela de título da Planície direta:** a cena de teste da Planície ainda tem o título próprio, com "Novo jogo" começando direto na Planície. É para testes; o jogo de verdade é a campanha.
- **Mapa (M) e menu de pausa:** são da entrega 3. As teclas já estão no mapa de entradas (M, F11, Esc/P).
- **Tempos da encenação do fim:** estão no código (`SeniorFinale.cs`), não em dados. São tempos de cena, não de balanceamento.
- **Ritmo do jogo:** a luta com o Sênior foi vencida no teste com a Vida reposta. Num jogo real, ela pede o passo lateral na hora certa. Vale você jogar e me dizer se está difícil demais.
