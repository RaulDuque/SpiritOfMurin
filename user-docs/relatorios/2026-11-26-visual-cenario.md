# Como o cenário é desenhado (Vale e Planície)

**Data:** 2026-11-26
**Tipo:** documentação (leitura do código; nenhum arquivo do jogo foi alterado)
**Para quem:** o criador, para entender de onde vem cada pedaço do visual do cenário.

As referências seguem o formato `arquivo:linha`. Todos os caminhos partem da raiz do repositório.

---

## 1. Resumo

- **Não há imagens de cenário.** Chão, água, árvores, pedras, bambus, ponte, túmulo, tendas, fogueira e objetos são desenhados por código. Cada nó tem uma função `_Draw` que pinta **retângulos** de cor chapada, um por um. O "pixel art" sai desses retângulos de 1, 2, 3 px, com as mesmas cores do protótipo em HTML.
- **O Vale de Bambus (prólogo) é feito à mão.** É um mapa fixo de 60 × 40 tiles, escrito no código tile a tile, igual ao `buildMap` do protótipo (`src/Vale/ValeMap.cs:57-78`).
- **A Planície Verdejante é gerada por uma semente.** Um número (por exemplo 20261126) define o riacho, as margens, os lamaçais, os campos de arbustos e cada árvore, pedra e toca. A mesma semente sempre gera o mesmo mundo (`src/Plains/PlainsGenerator.cs:46-53`). O mundo é montado em **blocos (chunks)** de 16 × 16 tiles conforme o jogador anda.
- As **variações pequenas** (onde cai um tufo de grama, a altura de um bambu, o tom de um tile) não são sorteadas a cada vez. Saem de uma conta fixa ("hash") sobre a coordenada do tile, por isso o desenho é sempre igual (`src/Vale/ValeTerrain.cs:25-34`, `src/Plains/PlainsGenerator.cs:91-99`).
- A origem do visual é o protótipo `prototypes/spiritofmurin/index.html`: `hash` (linha 233), `buildMap` (240), `drawGround` (1287), `drawTall` (1355) e `drawWorld` (1496). O C# copia essas funções. Os comentários do código dizem isso (`src/Vale/ValeTerrain.cs:7-12`, `src/Vale/ValeMap.cs:8-13`).

---

## 2. Camadas, ordem de desenho, tile e escala

### 2.1 O tile

- **32 × 32 px** no Vale (`src/Vale/ValeMap.cs:17`) e na Planície (`assets/data/plains/plains_worldgen.json:18`, lido em `src/Plains/PlainsGenSettings.cs:40`).
- O campo de testes antigo usa o mesmo tamanho (`src/World/Ground.cs:15`).

### 2.2 Ordem de desenho no Vale

A cena `src/Vale/Vale.tscn` tem três nós de mundo e o HUD:

| Ordem | Nó / camada | O que contém | Onde |
|---|---|---|---|
| 1 (fundo) | `Ground` → `ValeGround`, z = −10 | grama, trilha, chão do acampamento, fenda, tábuas da ponte, piso do túmulo | `src/Vale/ValeTerrain.cs:53`, `src/Vale/Vale.tscn:11` |
| 2 | `Ground` → `ValeWater`, z = −9 | o Rio Nublado, animado | `src/Vale/ValeTerrain.cs:55` |
| 3 | `Ground` → `ValeDecals`, z = −5 | esteira de meditação, as 3 pegadas do túmulo, marcas de passo do jogador | `src/Vale/ValeGame.cs:196` |
| 4 | `World` (y-sort ligado) | peças "altas": pinheiros, pedras, paredes, bambus, tendas, estela, portão, porta; potes, ervas, baú, corpo, inimigos, estátua e jogador | `src/Vale/Vale.tscn:13-14`, `src/Vale/ValeTerrain.cs:57-65`, `src/Vale/ValeGame.cs:198-207` |
| 5 | `Bodies` | só colisões, invisíveis | `src/Vale/Vale.tscn:19`, `src/Vale/ValeTerrain.cs:99` |
| 6 | Onda de Qi, z = 10; `ValeFx`, z = 20 | faíscas, anéis, palavras ("aparado", "passo"), golpes inimigos, colchetes do alvo | `src/Vale/ValeGame.cs:576`, `src/Vale/ValeGame.cs:195`, `src/Vale/ValeFx.cs:6-12` |
| 7 (frente) | `Hud` (CanvasLayer) | barras, minimapa, painéis | `src/Vale/Vale.tscn:21-22` |

**O que é o y-sort:** dentro de `World`, quem tem o pé mais abaixo na tela é desenhado por cima. Por isso cada peça alta tem a origem **no pé**: o centro da base do tile, `(tx·32+16, (ty+1)·32−1)` (`src/Vale/ValeTerrain.cs:62`). Assim o personagem passa atrás de um pinheiro quando está acima dele e na frente quando está abaixo. O protótipo fazia o mesmo com uma lista ordenada por `y` (`index.html:1504-1508`).

**Quais tiles são "altos":** Rock, TombWall, CutWall, Bamboo, Tent, Stele, Gate, Door e Pine (`src/Vale/ValeMap.cs:90`). Embaixo deles, o chão desenha o piso adequado: piso do túmulo sob paredes e porta, chão de acampamento sob tendas e portão, grama ou acampamento sob o resto (`src/Vale/ValeTerrain.cs:114-120`).

### 2.3 Ordem de desenho na Planície

A cena `src/Plains/Plains.tscn` tem `World` (script `PlainsWorld`) com o filho `Objects` (y-sort ligado) e o HUD:

| Ordem | Camada | O que contém | Onde |
|---|---|---|---|
| 1 | `GroundLayer`, z = −10, um `ChunkGround` por bloco | chão de cada bloco e as marcas no chão (bocas de toca, terra revirada, cascos) | `src/Plains/PlainsWorld.cs:66`, `src/Plains/PlainsWorld.cs:106-107` |
| 2 | dentro de `Objects`: pegadas de trilha, z = −5; esteira, z = −4 | ficam sempre sob as figuras | `src/Plains/PlainsWorld.cs:177`, `src/Plains/PlainsWorld.cs:182` |
| 3 | `Objects` (y-sort) | pinheiros, pedras, arbustos, itens no chão, fogueira, animais, carcaças e jogador | `src/Plains/Plains.tscn:19-22`, `src/Plains/PlainsWorld.cs:111-117` |
| 4 | Onda de Qi, z = 10 | | `src/Plains/PlainsGame.cs:524` |
| 5 | `Hud` | | `src/Plains/Plains.tscn:25-26` |

Os colisores da água não têm desenho; são filhos de `World` (`src/Plains/PlainsWorld.cs:109-110`).

### 2.4 Câmera e escala de pixel

- **Resolução base: 640 × 360** (`project.godot:24-25`). A janela abre em 1280 × 720 (`project.godot:26-27`).
- **Escala inteira:** `stretch/mode="viewport"`, `scale_mode="integer"` e `aspect="expand"` (`project.godot:28-30`). Cada pixel do jogo vira um quadrado de 2 × 2, 3 × 3 etc., sem borrar.
- O `DisplayManager` escolhe a maior escala inteira que ainda mostra pelo menos 640 × 360 e aumenta a área vista com o que sobra. Exemplos do próprio código: 1600 × 900 → 2×, 800 × 450 de mundo; 1920 × 1080 → 3×, 640 × 360 (`src/Game/DisplayManager.cs:93-107`). Com `--estica-antiga`, volta ao modo com faixas pretas (`src/Game/DisplayManager.cs:45-46`).
- Texturas sem filtro (nítidas): `default_texture_filter=0` (`project.godot:134`). Posições presas ao pixel: `snap_2d_transforms_to_pixel=true` (`project.godot:138`). Os desenhos arredondam as coordenadas com `Mathf.Round` (por exemplo `src/Vale/ValeTerrain.cs:242`, `src/Plains/ChunkGround.cs:46`).
- **Câmera:** é um `Camera2D` dentro do jogador, com suavização de velocidade 9 (`src/Player/Player.tscn:18-21`). No Vale, ela fica presa às bordas do mapa (0 a 60·32 por 0 a 40·32) (`src/Vale/ValeGame.cs:225-226`). Na Planície não há limite, porque o mundo não tem borda.

---

## 3. O Vale de Bambus (feito à mão)

### 3.1 O mapa de tiles

- **60 × 40 tiles** (`src/Vale/ValeMap.cs:19`). São 16 tipos de tile (`src/Vale/ValeMap.cs:6`).
- O mapa é montado com retângulos de tiles, na ordem do protótipo (`src/Vale/ValeMap.cs:62-77`):

| Elemento | Tiles (x, y) | Linha |
|---|---|---|
| Acampamento (chão batido) | x 2–57, y 2–12 | `ValeMap.cs:62` |
| Paliçada de rocha com portão | rocha nas linhas 13–14; portão em x 27–30, linha 13 | `ValeMap.cs:63` |
| Rio Nublado com ponte | água nas linhas 17–19; ponte em x 27–30 | `ValeMap.cs:64` |
| Bordas do mapa | 2 tiles de rocha em volta | `ValeMap.cs:65` |
| Trilha | do início (x 28–29, até y 36) ao vale e ao túmulo | `ValeMap.cs:66` |
| Fenda | x 21–22, de y 20 até a borda de baixo | `ValeMap.cs:67` |
| Túmulo | paredes x 45–56, y 21–30; piso por dentro; porta (50–51, 30); parede cortada (50–51, 21) | `ValeMap.cs:68` |
| Pedras gêmeas | (48, 32) e (53, 32) | `ValeMap.cs:69` |
| Tendas | 4 blocos no acampamento | `ValeMap.cs:70` |
| Touceiras de bambu | 8 retângulos | `ValeMap.cs:71-72` |
| Pedras soltas | 11 posições | `ValeMap.cs:73` |
| Estela (inscrição) | (17, 32) | `ValeMap.cs:74` |
| Pinheiros | 20 posições, só onde havia grama ou acampamento | `ValeMap.cs:75-77` |

- Fora do mapa, tudo conta como rocha (`src/Vale/ValeMap.cs:81`).
- **Pedra solta × paredão:** uma rocha sem nenhuma rocha vizinha é uma "pedra solta" (boulder), desenhada arredondada e com colisão só na base. Rochas encostadas formam paredão (`src/Vale/ValeMap.cs:87`). As pedras gêmeas são pedras soltas.
- **Lugares com nome** (para o mapa): Trilha da Montanha, Ponte do Rio Nublado, Acampamento, Vale de Bambus, Túmulo da Mestra e Além da Fenda (`src/Vale/ValeMap.cs:48-52`). O nome da área no topo da tela vem de faixas de coordenadas (`src/Vale/ValeMap.cs:93-102`).

### 3.2 Como cada tile é desenhado

As cores estão todas no topo de `src/Vale/ValeTerrain.cs:15-22`. "hash" é a conta fixa por tile (`ValeTerrain.cs:25-34`).

| Tile | Cor base | Detalhes e variação | Linhas |
|---|---|---|---|
| **Grama** | `#4c5a3e` | 1 em 3 tiles ganha uma mancha `#526142` de tamanho e lugar variáveis. Até 5 tufos por tile, cada um com 3 hastes `#65774c` sobre uma base `#3d4a33`; cerca de 1 em 3 tufos é pulado. 1 em 13 tiles tem uma florzinha `#8e9a6c` com miolo `#d6d8c4`. | `ValeTerrain.cs:134-145` |
| **Trilha** | `#86704f` | 4 riscos `#77623f` de 2 × 1 e um seixo `#9c8763` de 2 × 2. **Borda de grama irregular:** de cada lado que encosta em grama, pinheiro, bambu ou rocha, pinta uma franja de grama de 1 a 4 px a cada 2 px. É a única transição suave do Vale. | `ValeTerrain.cs:146-159` |
| **Acampamento** | `#77684a` ou `#6b5d41` (alterna pelo hash) | um risco `#77623f` de 3 × 1 | `ValeTerrain.cs:160-163` |
| **Fenda** | quase preto `#090a0b` | parede superior de 10 px `#2a2924` com aresta de 2 px `#4a473e`; paredes laterais de 3 px `#2a2924` | `ValeTerrain.cs:164-169` |
| **Ponte** | madeira `#7a5a3a` | uma linha `#5d432d` a cada 6 px (as tábuas); corrimão de 3 px `#4b3524` nas bordas esquerda e direita | `ValeTerrain.cs:170-175` |
| **Piso do túmulo** | xadrez `#3a3a3f` / `#323237` | junta de 1 px `#28282c` em cima e à esquerda de cada laje | `ValeTerrain.cs:176-179` |
| **Água** | xadrez `#2c5a5e` / `#336669` | brilhos `#78aca7` de 7 × 1 e 5 × 1 que acendem e apagam com o tempo (fase por tile). Na margem de cima, uma faixa de pedra de 5 px `#5d5b51` e uma linha clara | `ValeTerrain.cs:201-217` |

**Portão aberto:** depois que o portão cai, o chão desenha três pedaços de paliçada `#5a3f28` caídos ao lado (`ValeTerrain.cs:182-186`). Quando isso acontece, o chão é redesenhado.

### 3.3 O rio e a ponte

- O rio é um nó próprio (`ValeWater`), redesenhado **a cada quadro** para os brilhos piscarem (`ValeTerrain.cs:198`). Para economizar, ele só percorre as linhas 16 a 20 (`ValeTerrain.cs:204`).
- A ponte é desenhada **pelo chão**, não pela água. A camada da água pula os tiles da ponte (`ValeTerrain.cs:210`). Antes ela pintava água por cima das tábuas; foi o defeito corrigido no commit `2da0aff`.
- A água colide: ninguém atravessa o rio fora da ponte (`ValeTerrain.cs:82`).

### 3.4 A fenda

- É chão escuro com paredes desenhadas (seção 3.2).
- **Colisão especial:** a fenda fica numa camada própria, a `GapLayer` (`ValeTerrain.cs:68`, `ValeTerrain.cs:83`). Para quem anda, ela é parede. O passo lateral e o passo longo atravessam (`ValeTerrain.cs:10-11`, `design/gdd/campaign-prologue.md:34`). O jogador colide com as camadas `1 | GapLayer` (`src/Vale/ValeGame.cs:210`).

### 3.5 Peças altas (`ValeTall`)

Cada peça alta é um nó separado, criado para cada tile alto (`ValeTerrain.cs:57-65`). Pinheiros, bambus, estela e a parede cortada se redesenham a cada quadro para animar; as outras peças são desenhadas uma vez (`ValeTerrain.cs:239`).

| Peça | Como é desenhada | Linhas |
|---|---|---|
| **Pinheiro** | Duas sombras ovais translúcidas no pé. Tronco de 4 px `#5a3e28` com lado escuro `#3e2a1a` e contorno `#16140f`. Copa de **4 andares triangulares** de 15 linhas cada; a largura cai 3 px por andar, mais 0–2 px pelo hash. Tons: `#2f5a3a`, lado direito `#23462d`, brilho `#4c7c4a` à esquerda, base `#1d3a25`. Contorno escuro em volta de tudo. **Balanço:** os 2 andares de cima oscilam 1 px para os lados (seno lento, fase própria por árvore). | `ValeTerrain.cs:251-272` |
| **Pedra solta** | Perfil arredondado com 24–29 px de altura e 13–16 px de meia largura (pelo hash). Três faixas de tom: brilho `#a3a091` no topo, `#8a877a` no meio, face `#5d5b51` embaixo. Lado direito mais escuro, duas rachaduras `#3b3a34`, sombra no chão e contorno. | `ValeTerrain.cs:273-287` |
| **Paredão de rocha** | Bloco de 32 px **erguido 10 px** acima do tile, com topo `#8a877a`. Aresta clara de 2 px quando não há rocha acima e riscos escuros variados pelo hash. Quando não há rocha abaixo, ganha **face frontal** de 10 px `#5d5b51` com base `#3b3a34`. Isso dá a impressão de altura em vista 3/4. | `ValeTerrain.cs:289-303` |
| **Parede do túmulo** | Mesma lógica, em pedra mais fria: topo `#6c6b72`, brilho `#83828a`, face `#48474e`. Juntas de cantaria `#28282c` na face, desencontradas a cada linha. | `ValeTerrain.cs:291-302` |
| **Porta selada** | Placa `#55545b` sobre os dois tiles, com fresta no meio. No tile da direita, um selo em "C" âmbar `#e2a640`, que fica `#ffd27a` quando o jogador tem o manual. Aberta, a porta some e aparece o piso. | `ValeTerrain.cs:304-313`, `ValeTerrain.cs:289` |
| **Parede cortada** | Sobre os dois tiles, uma linha curva de 40 px (o "arco-linha"), cinza `#4b4a51`. Depois de ler a marca, ela **pulsa em ciano** (`#6fd8d2` / `#d4fbf6`) com um halo translúcido. | `ValeTerrain.cs:314-323` |
| **Bambu** | 3 colmos por tile, de 34–47 px de altura e 3 px de largura (`#5f8f4a`, com 1 px de brilho `#7fae5d`). Um nó escuro `#3b6332` a cada 9 px e uma base escura. Três folhas `#4d7a3c` no alto que **balançam ±1,5 px**, cada colmo com sua fase. | `ValeTerrain.cs:325-336` |
| **Tenda** | Lona vermelha `#8f3d2c` com listras `#74301f` a cada 8 px, erguida 8 px. Na fileira de cima, uma cumeeira de palha `#c4ab80`. Na fileira de baixo, uma barra escura `#5a2418` e, nos tiles de x par, uma entrada `#1e1410`. | `ValeTerrain.cs:337-344` |
| **Estela** | Laje de pedra `#7c7a70` com contorno e uma grade de 5 × 2 sinais **ciano que pulsam**; depois de lida, ficam fixos. Ao lado, uma lanterna âmbar com chama que pisca e um halo âmbar translúcido. | `ValeTerrain.cs:345-353` |
| **Portão** | 4 estacas `#6d4f33` por tile, com brilho `#8a6844` e pontas, mais duas travessas `#4a3422`. Aberto, não é desenhado. | `ValeTerrain.cs:354-362` |

### 3.6 Objetos

Objetos com contorno usam o `PixelPainter`, que junta os retângulos e desenha o contorno de ator `FighterArt.Outline` em volta (`src/Vale/ValeProps.cs:58`). O desenho do protótipo vem das funções `drawPot`, `drawHerb` e `drawChest` (`index.html:1514-1515`).

| Objeto | Desenho | Colisão | Linhas |
|---|---|---|---|
| **Pote** (8 no mapa, `ValeMap.cs:44`) | Barro `#9a5b36` com boca `#b8774a`, lado escuro `#6e3f24` e faixa clara `#d9b48a`; sombra no chão | caixa de 12 × 5 na base | `ValeProps.cs:42-60` |
| **Erva espiritual** (8, `ValeMap.cs:46`) | Haste e 3 folhas verdes que balançam 1 px; 2 flores `#f2f7e0` | nenhuma (é pega andando por cima) | `ValeProps.cs:63-82` |
| **Baú** (`ValeMap.cs:38`) | Madeira `#6d4a2c` com frisos âmbar `#e2a640` e fecho `#ffd27a`. Aberto: tampa levantada e interior escuro | caixa de 20 × 6 | `ValeProps.cs:85-104` |
| **Corpo do discípulo** (`ValeMap.cs:30`) | Desenhado pela arte dos lutadores (`FighterArt.DrawDead`), com a página do manual brilhando até ser pega | nenhuma | `ValeProps.cs:107-119` |
| **Item caído** | Quica com gravidade e depois flutua; sombra, halo âmbar translúcido e o ícone do item | nenhuma | `ValeProps.cs:122-159`; ícones em `ValeProps.cs:24-38` |

A **estátua da mestra** não é cenário: é um "lutador" parado, criado com `Spawn("statue", …)` (`src/Vale/ValeGame.cs:204`) e desenhado pela arte dos personagens.

### 3.7 Marcas no chão (`ValeDecals`)

- **Esteira de meditação** junto à pedra gêmea oeste (`ValeMap.cs:40`). É palha `#8f8466` / `#b0a382` / `#c4b893` com contorno. Ao lado sobe um fio de qi `#a6f2e2` que brilha mais logo depois de meditar (`src/Vale/ValeFx.cs:141-147`).
- **As 3 pegadas do túmulo,** a 60 px da estátua, nos ângulos de 90°, 140° e 192° (`ValeMap.cs:41-42`, `ValeMap.cs:55`). São cinza `#4c4c53` até a marca ser lida. Depois ficam ciano, e a próxima pegada a pisar pisca (`ValeFx.cs:148-161`).
- **Marcas de passo do jogador:** pares de pontos ciano que somem em 1,4 s (`ValeFx.cs:126`, `ValeFx.cs:162-166`).

### 3.8 Colisões: o que bloqueia

As colisões são caixas invisíveis montadas em `ValeTerrain.Build`. Tiles sólidos vizinhos na mesma linha viram uma caixa só (`ValeTerrain.cs:66-98`).

| Bloqueia o tile inteiro | Bloqueia só a base | Camada especial | Pode abrir |
|---|---|---|---|
| paredão de rocha, parede do túmulo, parede cortada, tenda, estela, água (`ValeTerrain.cs:82`) | pinheiro: 14 × 11; bambu: 32 × 7; pedra solta: 24 × 13 (`ValeTerrain.cs:94-96`) | fenda, na `GapLayer` (`ValeTerrain.cs:83`) | portão e porta são corpos à parte, removidos ao abrir (`ValeTerrain.cs:84-85`, `ValeTerrain.cs:36-47`) |

Como pinheiro, bambu e pedra solta só colidem na base, o personagem passa **atrás** deles. É a regra da bíblia de arte, seção 6.5 (`design/art/art-bible.md:576-585`).

---

## 4. A Planície Verdejante (gerada por semente)

### 4.1 De onde vem a semente

- Pela linha de comando, `--seed=N`. Se não houver, vale o campo **Seed** da cena. Se ele for 0, o jogo sorteia uma semente nova a partir do relógio (`src/Plains/PlainsGame.cs:205-212`, `src/Plains/PlainsGame.cs:142-143`).
- Uma jornada salva guarda a semente e a reusa (`src/Plains/PlainsGame.cs:283`, `src/Plains/PlainsGame.cs:863`).
- O gerador nasce com a semente e as regras do JSON (`src/Plains/PlainsGame.cs:284`).

### 4.2 A ferramenta básica: hash e ruído

- **Hash** (`PlainsGenerator.cs:91-99`): mistura semente, x, y e um "sal" (um número que separa os usos) e devolve um número que parece aleatório, mas é sempre o mesmo para as mesmas entradas. `Rand01` transforma esse número num valor de 0 a 1 (`PlainsGenerator.cs:101`).
- **Ruído suave** (`PlainsGenerator.cs:104-111`): sorteia valores nos cantos de uma grade e faz uma mistura suave entre eles. Produz manchas grandes e arredondadas, em vez de pontos soltos. É o que desenha o contorno dos lamaçais e dos campos de arbustos.
- **Regra de ouro:** cada tile depende só de (semente, coordenada), nunca da ordem em que os blocos foram visitados. Por isso os blocos se encaixam nas bordas e saem iguais em qualquer ordem (`PlainsGenerator.cs:46-53`; `design/gdd/plains-first-stretch.md:30-33`).

### 4.3 Passo a passo do chão (`TileAt`, `PlainsGenerator.cs:138-158`)

Para cada tile, nesta ordem de prioridade:

1. **Lugar feito à mão (carimbo):** se o tile foi reservado com `AddStamp`, vale o que foi reservado (`PlainsGenerator.cs:140`, `PlainsGenerator.cs:182-186`). Hoje nenhum código chama `AddStamp`. É o encaixe previsto para cidades e túmulos.
2. **Riacho:**
   - A linha central é `CreekX`: 7 tiles a leste do despertar, mais a soma de dois senos (meandro longo, amplitude 5, e curto, amplitude 2), com fases tiradas da semente (`PlainsGenerator.cs:80-81`, `PlainsGenerator.cs:123-130`).
   - Perto do despertar o meandro fica mais calmo (35% da força em y = 0, 100% a partir de 30 tiles), para o riacho não invadir a clareira (`PlainsGenerator.cs:127`).
   - A até 1,2 tile do centro é **água**. Nas linhas de vau, é **vau** (raso, atravessável) (`PlainsGenerator.cs:143`).
3. **Vau:** a cada 28 linhas há 3 linhas de vau. A posição do primeiro depende da semente (`PlainsGenerator.cs:82`, `PlainsGenerator.cs:133`).
4. **Margem:** os 3 tiles seguintes de cada lado da água são margem de areia (`PlainsGenerator.cs:144`).
5. **Clareira do despertar:** até 5,5 tiles do início é sempre relva (`PlainsGenerator.cs:145`).
6. **Lamaçal garantido** e depois **campo de arbustos garantido:**
   - São manchas de raio 6,5 tiles centradas nos "âncoras". Cada âncora fica a 12–16 tiles do início, num ângulo de 145° (lamaçal) ou 215° (arbustos), com ±20° de variação pela semente (`PlainsGenerator.cs:83-84`, `PlainsGenerator.cs:113-118`).
   - A borda é irregular: o raio é multiplicado por 0,8 a 1,2 conforme o ruído (`PlainsGenerator.cs:147-149`).
7. **Longe do início, ruído:** o ruído com escala 0,07 vira lamaçal acima de 0,72 e arbustos acima de 0,66. O efeito entra aos poucos numa faixa de 6 tiles, para não aparecer lamaçal colado ao início (`PlainsGenerator.cs:151-156`).
8. **Relva** no resto (`PlainsGenerator.cs:157`).

Os tipos de chão são Grass, Bank, Water, Ford, BushGrass e Mud (`PlainsGenerator.cs:8`). Só a água funda não é pisável (`PlainsGenerator.cs:174`).

### 4.4 Passo a passo dos objetos (`FeaturesAt`, `PlainsGenerator.cs:228-264`)

1. **Objetos garantidos do início** (`PlainsGenerator.cs:196-223`):
   - uma fogueira em (2, 1) e uma esteira em (1, −2), na borda leste da clareira;
   - na margem: 2 sílex, 2 galhos, 1 pinheiro e 1 pedra com erva;
   - no campo de arbustos: 2 tocas, 2 arbustos e 1 pedra com erva;
   - no lamaçal: 1 javali e 3 terras reviradas, 2 delas com raiz.
2. **Onde não nasce nada:** água funda, vau e a clareira (raio 3,5) (`PlainsGenerator.cs:244`).
3. **Sorteio por tile:** cada tile tem **no máximo um objeto principal**. Um número de 0 a 1 é comparado com as chances do habitat, em sequência: sílex, galho, pinheiro, pedra, arbusto, toca, terra revirada, javali (`PlainsGenerator.cs:245-255`).
4. **Posição dentro do tile:** x de 6 a 20 px e y de 14 a 28 px, pelo hash. Assim os objetos não ficam alinhados na grade (`PlainsGenerator.cs:256`).
5. **Extras:**
   - pedra → erva ao lado, com 75% de chance (`PlainsGenerator.cs:260-261`);
   - terra revirada → raiz, com 80% (`PlainsGenerator.cs:262-263`).
6. **Trilhas de pegadas:**
   - A toca e o javali garantidos sempre têm uma trilha, que começa na borda da clareira.
   - Os outros animais têm trilha com 60% de chance, vinda de uma direção da semente, com 4 a 7 tiles de comprimento e uma pegada a cada 22 px, alternando os lados (`PlainsGenerator.cs:272-297`).
7. **Id estável:** cada objeto se chama `tipo@tx,ty`, por exemplo `pine@12,-3`. É por esse id que o jogo lembra o que foi pego (`PlainsGenerator.cs:17-28`, `PlainsGenerator.cs:257`).

**Grupos:** não existe um algoritmo de "bosque" ou de agrupamento. Cada tile sorteia sozinho. A concentração vem dos habitats: no campo de arbustos a chance de arbusto é 17%, contra 3% na relva (`assets/data/plains/plains_worldgen.json:42-43`).

**Densidade esperada por bloco** (256 tiles, conta feita com o JSON):
- bloco só de relva: cerca de 20 objetos (5–6 pinheiros, 3–4 pedras, 7–8 arbustos, 2 galhos, 1 sílex);
- bloco só de arbustos: cerca de 63 objetos, quase todos arbustos.

### 4.5 Streaming por blocos e persistência

- **Bloco:** 16 × 16 tiles (512 px) (`plains_worldgen.json:17`).
- **Carregar e descarregar:** ficam montados os blocos a até 2 do bloco do jogador, ou seja, uma área de 5 × 5 blocos. Blocos a mais de 3 são liberados (`plains_worldgen.json:19-20`; `src/Plains/PlainsWorld.cs:71-87`). A margem entre 2 e 3 evita montar e desmontar sem parar na fronteira.
- **Quando atualiza:** só quando o jogador muda de bloco (`src/Plains/PlainsGame.cs:394`).
- **Montar um bloco** (`PlainsWorld.cs:99-118`):
  1. gera os dados (`GenerateChunk`, `PlainsGenerator.cs:311-323`);
  2. cria um `ChunkGround`, que desenha o chão **uma vez só**;
  3. cria o colisor da água, juntando cada fileira de água num retângulo (`PlainsWorld.cs:121-142`);
  4. cria um nó para cada objeto (`PlainsWorld.cs:160-190`).
- **Memória (`WorldState`):** objetos pegos, animais mortos (com o lugar da carcaça), partes tiradas e trilhas examinadas (`src/Plains/WorldState.cs:6-59`). Ao remontar um bloco, o que foi pego não volta e o animal morto volta como carcaça (`PlainsWorld.cs:165`, `PlainsWorld.cs:196-201`). A memória é gravada ao meditar e restaurada ao continuar (`src/Plains/PlainsGame.cs:307`, `src/Plains/PlainsGame.cs:871`).

### 4.6 Como cada elemento é desenhado

**Chão (`src/Plains/ChunkGround.cs`).** As cores estão nas linhas 19-23. Cada detalhe usa o hash do tile **no mundo**, com sal 900, então o bloco redesenha igual e casa com o vizinho (`ChunkGround.cs:5-10`, `ChunkGround.cs:50`).

| Chão | Cor base | Detalhes | Linhas |
|---|---|---|---|
| **Relva** | `#4c5a3e`, 1 em 4 tiles `#526142` | 3 tufos (hastes `#65774c` / `#3d4a33`); 1 em 23 tiles tem uma flor `#8e9a6c` | `ChunkGround.cs:80-84`, `ChunkGround.cs:90-98` |
| **Campo de arbustos** | `#3f4d34`, 1 em 4 `#46553a` (mais escuro) | 4 tufos | `ChunkGround.cs:76-79` |
| **Margem** | areia `#8f8466`, 1 em 4 `#7f755a` | 3 seixos de 2 × 2 (`#a3a091` ou `#7f755a`); 1 em 5 tiles tem um tufo | `ChunkGround.cs:66-70` |
| **Água** | `#2f5560` | um risco `#3b6a73` de 8 × 1 e um brilho `#8fb3b0` de 5 × 1; linha de areia de 2 px onde encosta em terra | `ChunkGround.cs:53-65` |
| **Vau** | raso `#557f7c` | os mesmos riscos, mais **3 pedras** `#8a877a` de 5 × 3 para mostrar a passagem | `ChunkGround.cs:56-60` |
| **Lamaçal** | `#4a3d2c`, 1 em 4 `#463a2a` | uma poça úmida `#5a4b37` de 10 × 3 e um torrão `#33271b` | `ChunkGround.cs:71-75` |

Diferente do Vale, a água da Planície **não é animada**: o bloco é desenhado uma vez.

**Marcas no chão desenhadas pelo bloco:**
- **Toca:** monte `#6b5a3e`, boca preta `#16140f` e 3 pegadinhas (`ChunkGround.cs:100-106`).
- **Terra revirada:** mancha `#5e4630` de 22 × 11 com 6 torrões (`ChunkGround.cs:108-116`).
- **Cascos** em volta do ponto do javali: 5 pares (`ChunkGround.cs:118-126`).

**Objetos altos (`src/World/Obstacle.cs`).** São a mesma arte do Vale:
- **Pinheiro:** igual ao do Vale, com balanço (`Obstacle.cs:57-76`).
- **Pedra:** igual (`Obstacle.cs:78-93`).
- **Arbusto:** só existe na Planície. São 3 lóbulos redondos com contorno, em três tons (`#5f8a4a` em cima, `#456a3a` no meio, `#2f4a2c` embaixo), e balançam 0,6 px. 1 em 3 arbustos tem bagas `#7a3a32` (`Obstacle.cs:110-126`).
- **Colisão só na base:** pinheiro 14 × 11, pedra 24 × 13, arbusto 18 × 6 (`Obstacle.cs:24-31`).
- O bambu existe na classe (`Obstacle.cs:95-108`), mas a Planície não gera bambu. Ele só aparece no campo de testes (`src/Main/Main.cs:32`).

**Itens no chão (`src/Plains/Pickup.cs`).** Todos têm contorno `#16140f` e nenhuma colisão.

| Item | Desenho | Linhas |
|---|---|---|
| sílex | pedra cinza em 3 tons; de vez em quando dá um brilho de 1 px | `Pickup.cs:45-49` |
| galho | madeira `#7a5a3a` / `#4e3824` com um ramo | `Pickup.cs:50-53` |
| Grama Sangue-de-Coelho | 5 folhas que balançam, com veios vermelhos `#b0403a` | `Pickup.cs:54-66` |
| Raiz Amarga | raiz bifurcada `#a08350` com broto verde | `Pickup.cs:67-72` |

**Lugares do início (`src/Plains/Landmarks.cs`):**
- **Fogueira:** um anel de 7 pedras, lenha e três chamas (`#c9532e`, `#e8a13a`, `#f4dc8a`) que tremulam, mais 2 fagulhas subindo. Tem colisão de 16 × 6 (`Landmarks.cs:65-87`).
- **Esteira de meditação:** palha `#a3925f` com listras `#7a6b44` e um fio de qi `#6fd8d2` subindo (`Landmarks.cs:90-111`).
- **Pegadas de trilha:**
  - **Coelho:** marcas de pata `#2a2219` / `#5e4a34`.
  - **Javali:** marcas de casco.
  - Brilham em `#d4fbf6` quando examinadas (`Landmarks.cs:10-62`).

O nome do arquivo é "Landmarks", mas hoje ele só tem esses três elementos. A Planície ainda não tem marcos grandes, como ruínas ou vilas.

### 4.7 Variedade entre sementes (20261126 e 777)

As duas sementes são as de teste (`tests/unit/Plains/PlainsFixtures.cs:13`). A **estrutura** é sempre a mesma:
- clareira no centro;
- riacho a leste;
- arbustos a noroeste e lamaçal a sudoeste (ângulos 215° e 145°, medidos com 0° = leste e 90° = sul).

O que muda são as fases do riacho, a posição exata das âncoras e todos os objetos. Os números abaixo são uma conta à parte, que reproduz as fórmulas do gerador:

| | 20261126 | 777 |
|---|---|---|
| Campo de arbustos (centro, em tiles) | (−12,3, −6,1), a 13,7 tiles, ângulo 206° | (−11,6, −7,9), a 14,1 tiles, ângulo 214° |
| Lamaçal (centro, em tiles) | (−12,7, 8,8), a 15,5 tiles, ângulo 145° | (−9,2, 12,6), a 15,6 tiles, ângulo 126° |
| Riacho entre as linhas −60 e 60 | o centro varia de x ≈ −1 a 12 | o centro varia de x ≈ 3,5 a 16 |

As capturas amplas das duas sementes estão em `production/qa/evidence/2026-11-26_planicie/planicie-ampla-semente-20261126.png` e `planicie-ampla-semente-777.png`.

---

## 5. Paletas e a bíblia de arte

### 5.1 Paleta do cenário em uso

| Material | Vale (`ValeTerrain.cs:15-22`) | Planície (`ChunkGround.cs:19-23`, `Obstacle.cs`) |
|---|---|---|
| Grama / relva | `#4c5a3e`, `#526142`, tufos `#65774c` / `#3d4a33`, flor `#8e9a6c` | mesmas cores |
| Grama escura | — | `#3f4d34`, `#46553a` (campo de arbustos) |
| Terra | trilha `#86704f` / `#77623f` / `#9c8763`; acampamento `#77684a` / `#6b5d41` | lamaçal `#4a3d2c` / `#463a2a` / `#5a4b37`; terra revirada `#5e4630` / `#33271b` |
| Areia | — | `#8f8466`, `#7f755a`, seixo `#a3a091` |
| Água | `#2c5a5e`, `#336669`, brilho `#78aca7` | `#2f5560`, `#3b6a73`, brilho `#8fb3b0`, vau `#557f7c` |
| Pedra | `#a3a091`, `#8a877a`, `#5d5b51`, `#3b3a34` | mesmas cores (pedra solta) |
| Túmulo | piso `#3a3a3f` / `#323237`; parede `#6c6b72` / `#83828a` / `#48474e` | — |
| Pinheiro | `#2f5a3a`, `#23462d`, `#4c7c4a`, `#1d3a25`; tronco `#5a3e28` | mesmas cores |
| Bambu | `#5f8f4a`, `#7fae5d`, `#3b6332`, folha `#4d7a3c` | (só no campo de testes) |
| Arbusto | — | `#5f8a4a`, `#456a3a`, `#2f4a2c`, baga `#7a3a32` |
| Madeira | ponte `#7a5a3a` / `#5d432d` / `#4b3524`; portão `#6d4f33` | galho `#7a5a3a`, lenha `#5a3e28` |
| Tenda | `#8f3d2c`, `#74301f`, palha `#c4ab80` | — |
| Conhecimento (ciano) | `#d4fbf6`, `#6fd8d2` (marca, pegadas, estela) | `#d4fbf6` (pegada examinada), `#6fd8d2` (qi da esteira) |
| Âmbar | `#e2a640`, `#ffd27a` (selo, baú, lanterna) | chamas `#e8a13a`, `#f4dc8a` |
| Contorno | `#16140f` | `#16140f` |

### 5.2 Cores do mapa e do minimapa

O mapa grande e o minimapa usam a mesma classe `MapView`, sobre pergaminho `#d8c7a0` com tinta `#2a2118` (`src/Game/MapView.cs:58`). Cada tile vira **1 pixel** de uma textura.

- **Vale:** só aparecem os tiles já vistos (`src/Vale/ValeGame.cs:107-127`). Cores em `ValeGame.cs:95-101`: trilha `#9a7d55`, acampamento `#b59c72`, água `#5f8a8a`, fenda `#2a2620`, rocha `#6f6a5c`, túmulo `#55545a`, bambu `#6f8f55`, pinheiro `#4c6a48`, grama `#b3a882`. Os nomes de lugar e a esteira (ponto ciano) também aparecem.
- **Planície:** só aparecem os blocos explorados (`src/Plains/PlainsGame.cs:89-113`). Cores em `PlainsGame.cs:82-86`: água `#5f8a8a`, vau `#86a8a0`, margem `#c4b48a`, lamaçal `#8a6f4c`, arbustos `#6f8a55`, relva `#a8ae7e`. Pontos marcam a fogueira, a esteira, as tocas e o javali vivo, e há o rótulo "clareira".
- **Zoom do minimapa:** 0,085 no Vale e 0,06 na Planície (`ValeGame.cs:230`, `PlainsGame.cs:342`).

### 5.3 Relação com a bíblia de arte

A bíblia (`design/art/art-bible.md`) define a família **terra-musgo** para todo o cenário: chão `#4c5a3e`, caminho `#86704f`, rocha `#8a877a`, túmulo `#3a3a3f` (`art-bible.md:337`). O código usa exatamente esses valores.

**O que bate com a bíblia:**
- **Ciano só para o que se aprende** (`art-bible.md:45`, `art-bible.md:358`): no Vale, aparece só na marca da parede, nas pegadas e na estela.
- **Natureza vertical e em camadas** (`art-bible.md:285-292`): o bambu tem 3 px de espessura e nós a cada 9 px, dentro da faixa pedida (2–4 px, nós a cada 6–10 px).
- **Variação por tile para a grade não aparecer** (`art-bible.md:550`): feita pelo hash.
- **Base sólida e corpo atravessável** (`art-bible.md:580-582`): feito nas colisões de base.

**Diferenças entre a bíblia e o código** (só observação; não foram corrigidas):
- A bíblia diz que **o ambiente não tem contorno** (`art-bible.md:549`, `art-bible.md:310`). Os pinheiros, as pedras soltas, os arbustos e a estela têm contorno `#16140f` (`ValeTerrain.cs:265`, `Obstacle.cs:70`, `Obstacle.cs:122`), herdado do protótipo.
- A bíblia aponta a água `#2c5a5e` do protótipo como vizinha do ciano e pede uma água mais cinza (`art-bible.md:326`, `art-bible.md:397`). O Vale ainda usa `#2c5a5e` (`ValeTerrain.cs:18`). A Planície usa `#2f5560`, um pouco mais escura.
- A paleta-mestra de 64 cores (`assets/palettes/pal_master_64.gpl`, `art-bible.md:434`) **não é lida pelo código**. As cores estão escritas direto em cada arquivo `.cs`.

---

## 6. Transições entre terrenos e limitações atuais

**O que já tem algum acabamento:**
- **Vale, trilha → grama:** franja irregular de grama de 1–4 px na borda da trilha (`ValeTerrain.cs:150-158`).
- **Vale, rio:** faixa de pedra na margem de cima (`ValeTerrain.cs:215`).
- **Vale, fenda:** paredes desenhadas em cima e nos lados (`ValeTerrain.cs:166-168`).
- **Vale, paredões:** topo erguido e face frontal para dar profundidade (`ValeTerrain.cs:295-303`).
- **Planície, água → terra:** linha de areia de 2 px (`ChunkGround.cs:62-63`).
- **Planície, bordas dos habitats:** o contorno das manchas é irregular graças ao ruído (`PlainsGenerator.cs:147`), mas cada tile ainda é de um tipo só.

**O que ainda é bloco duro de 32 px:**
- Na Planície: relva × margem, relva × lamaçal, relva × arbustos e margem × lamaçal. A troca acontece na borda do tile, sem mistura.
- No Vale: grama × acampamento, acampamento × rocha, grama × piso do túmulo e a margem de baixo do rio (só a de cima tem acabamento).
- A água da Planície é estática; a do Vale pisca.

**Pendentes pedidos para a próxima entrega** (só registro; nada disso existe no código hoje):
- transições suaves entre terrenos;
- vento (hoje cada pinheiro, bambu e arbusto balança sozinho, sem rajadas em conjunto);
- ciclo de dia e noite (hoje não há luz nem mudança de cor ao longo do tempo).

---

## 7. Como alterar

### 7.1 Por dados (sem mexer em código)

Arquivo `assets/data/plains/plains_worldgen.json`. O jogo lê na hora de começar a jornada (`src/Plains/PlainsGenSettings.cs:111`, `src/Plains/PlainsGame.cs:284`), e o esquema está documentado no bloco `_doc` do próprio arquivo (linhas 2-16).

| Quero mudar… | Chave no JSON | Linha |
|---|---|---|
| tamanho do bloco e quantos blocos ficam montados | `chunk_tiles`, `load_radius`, `unload_radius` | 17, 19-20 |
| tamanho da clareira | `clearing_radius` | 21 |
| posição, largura e curvas do riacho | `creek_offset`, `creek_half_width`, `bank_width`, `creek_meander1/2`, `creek_freq1/2` | 22-28 |
| frequência e tamanho dos vaus | `ford_spacing`, `ford_length` | 29-30 |
| tamanho das manchas de lamaçal e arbustos, e quanto aparecem | `habitat_scale`, `mud_threshold`, `bush_threshold` | 31-33 |
| distância, tamanho e direção do lamaçal e dos arbustos do início | `anchor_*`, `bush_anchor_angle`, `mud_anchor_angle` | 34-39 |
| **densidade** de pinheiros, pedras, arbustos, sílex, galhos, tocas e javalis em cada habitat | `per_habitat` | 40-45 |
| erva junto às pedras, raiz na terra revirada | `herb_near_boulder`, `root_in_rooted` | 46-47 |
| trilhas de pegadas | `trail_*` | 48-51 |

O campo **Seed** fica no nó raiz da cena `src/Plains/Plains.tscn` (`PlainsGame.cs:142-143`); com 0, cada execução sorteia uma semente.

O Vale **não tem arquivo de dados para o cenário**: o mapa e as cores estão no código. O `vale_combat.json` só trata de combate.

### 7.2 Por código

| Quero mudar… | Arquivo e linha |
|---|---|
| **Cores** do chão, da água, das rochas, do túmulo, dos bambus e das tendas do Vale | `src/Vale/ValeTerrain.cs:15-22` (algumas cores avulsas ficam no próprio desenho, por exemplo os pinheiros em `ValeTerrain.cs:256-268`) |
| **Cores** do chão da Planície | `src/Plains/ChunkGround.cs:19-23` |
| Cores e forma de pinheiro, pedra, bambu e arbusto (Planície e campo de testes) | `src/World/Obstacle.cs:57-126` |
| Cores dos itens no chão | `src/Plains/Pickup.cs:25-27` |
| Fogueira, esteira e pegadas | `src/Plains/Landmarks.cs:28`, `Landmarks.cs:68`, `Landmarks.cs:95` |
| Potes, ervas, baú (Vale) | `src/Vale/ValeProps.cs:56-57`, `ValeProps.cs:78-79`, `ValeProps.cs:99-101` |
| **O mapa do Vale** (onde fica cada rio, trilha, tenda, pinheiro) | `src/Vale/ValeMap.cs:62-77` |
| Lugares do Vale (estátua, baú, potes, ervas, esteira) | `src/Vale/ValeMap.cs:24-46` |
| **Quantos tufos e flores** por tile | Vale: `ValeTerrain.cs:136-144`; Planície: `ChunkGround.cs:69`, `ChunkGround.cs:78`, `ChunkGround.cs:82-83` |
| **Tamanho** de pinheiro, pedra e bambu | Vale: `ValeTerrain.cs:261`, `ValeTerrain.cs:275`, `ValeTerrain.cs:329`; Planície: `Obstacle.cs:66`, `Obstacle.cs:80`, `Obstacle.cs:101` |
| Velocidade e força do balanço | pinheiro `ValeTerrain.cs:257` / `Obstacle.cs:62`; bambu `ValeTerrain.cs:333` / `Obstacle.cs:105`; arbusto `Obstacle.cs:114` |
| Colisões (o que bloqueia e o tamanho das bases) | Vale: `ValeTerrain.cs:82-96`; Planície: `Obstacle.cs:24`, água em `PlainsWorld.cs:121-142` |
| **Regras de geração** (ordem dos habitats, onde nada nasce, posição no tile) | `src/Plains/PlainsGenerator.cs:138-158`, `PlainsGenerator.cs:244`, `PlainsGenerator.cs:256` |
| Objetos garantidos do início | `src/Plains/PlainsGenerator.cs:196-223` |
| Que nó cada objeto vira | `src/Plains/PlainsWorld.cs:160-190` |
| Cores do mapa e do minimapa | Vale `src/Vale/ValeGame.cs:95-101`; Planície `src/Plains/PlainsGame.cs:82-86`; pergaminho `src/Game/MapView.cs:58` |
| Escala de pixel e resolução base | `project.godot:24-30`, `src/Game/DisplayManager.cs:91`, `DisplayManager.cs:98-107` |

**Cuidados:**
- Mudar a conta do hash ou a ordem do sorteio de objetos muda **o mundo de todas as sementes**. Uma jornada salva continuaria com a mesma semente, mas com outro mundo. Mudar só as chances no JSON tem o mesmo efeito nos objetos.
- O Vale é um mapa fixo. Mexer nele muda o prólogo para todos e pode quebrar o `RecoveryCheck` e o `ValeCheck`, que conferem posições (por exemplo, a ponte).
