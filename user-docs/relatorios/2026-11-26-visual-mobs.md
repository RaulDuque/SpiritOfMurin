# Como os inimigos e as criaturas são desenhados

Data: 2026-11-26 · Tipo: documentação (somente leitura do código; nada foi alterado)

## 1. Resumo

No SpiritOfMurin, nenhum inimigo ou animal usa imagem (sprite). Cada um é **desenhado por código a cada quadro**, no método `_Draw` do Godot, com retângulos de pixel. Os contornos são feitos de duas formas:

- **Figuras humanas do prólogo** (discípulos, Sênior, estátua): usam o `PixelPainter`. Primeiro ele pinta, para cada peça, um retângulo de contorno 1 px maior em todos os lados. Depois pinta as cores por cima. O resultado é uma silhueta inteira com contorno de 1 px, como no protótipo HTML.
- **Animais da Planície** (javali, coelho, carcaças): cada peça desenha o próprio retângulo de contorno logo antes da cor, sem o `PixelPainter`.

Todos os valores abaixo são **pixels nativos** (tela de 640×360). A origem (0, 0) fica **nos pés** da figura. Y negativo é para cima.

### Lista de inimigos e criaturas

| Quem | Onde aparece | Código do desenho | Comportamento |
|---|---|---|---|
| Discípulo da Guarda Fechada (4: `dis_bridge`, `dis_herbs`, `dis_gap`, `dis_camp`) | Prólogo, Vale de Bambus | `src/Combat/FighterArt.cs` + `src/Vale/ValeFighter.cs` | `ValeFighter.cs` (IA no mesmo arquivo); posições em `src/Vale/ValeGame.cs:202-207` |
| Espadachim Sênior (`senior`) | Prólogo, acampamento | idem + `src/Vale/SeniorFinale.cs` (fim do prólogo) | `ValeFighter.cs` |
| Estátua da Mestra (`statue`) | Prólogo, túmulo | idem | `ValeFighter.cs` (não se mexe nem ataca) |
| Discípulo caído com a página | Prólogo, corpo do início | `src/Vale/ValeProps.cs:110-118` → `FighterArt.DrawDead` | — |
| Javali da Mata | Planície, lamaçais | `src/Creatures/Boar.cs` | `src/Creatures/BoarBrain.cs` |
| Coelho de Campo | Planície, campos de tocas | `src/Creatures/Rabbit.cs` | `src/Creatures/RabbitBrain.cs` |
| Carcaça (coelho ou javali abatido) | Planície | `src/Creatures/Carcass.cs` | — |
| Pegadas (pata e casco) | Planície, trilhas até a toca e o lamaçal | `src/Plains/Landmarks.cs:10-60` (`TrackPrint`) | — |
| Toca (cenário ligado ao coelho) | Planície | `src/Plains/ChunkGround.cs:100-106` | — |

## 2. O que é comum a todos

| Aspecto | Como funciona | Onde |
|---|---|---|
| Origem nos pés | Toda figura é desenhada a partir de (0, 0) nos pés. A cabeça fica em Y negativo. | `FighterArt.cs:9`, `PixelPainter.cs:6-8` |
| Ordem de desenho (y-sort) | As cenas ligam `y_sort_enabled`: quem está mais abaixo na tela é desenhado por cima. Como a origem fica nos pés, um inimigo à frente do jogador cobre o jogador, e um inimigo atrás dele fica coberto. | `src/Vale/Vale.tscn:14`, `src/Plains/Plains.tscn:20`, `src/Main/Main.tscn:15` |
| Camadas fixas | Efeitos do prólogo em `ZIndex = 20`, sempre por cima de tudo. Marcas no chão em `-5`. Pegadas da Planície em `-5`. | `ValeGame.cs:195-196`, `PlainsWorld.cs:177` |
| Sombra | Retângulo escuro translúcido sob os pés, cor `rgba(8,10,8)`: humanos 22×4 a 38% de opacidade (`ValeFighter.cs:378`), javali 28×5 a 38% (`Boar.cs:106`), coelho 12×3 a 35% (`Rabbit.cs:288`), corpo caído 32×6 a 35% (`FighterArt.cs:119`). | — |
| Contorno | Cor `#16140f` (tinta). `PixelPainter.Flush` desenha todos os contornos (retângulo +1 px de cada lado) e só depois todas as cores. Por isso as peças se fundem numa só silhueta. | `src/Player/PixelPainter.cs:36-41` (`Flush`) |
| Linhas (lâmina, braços) | Linha de pixels pelo algoritmo de Bresenham, com espessura 1 a 3 px, cada ponto vira um retângulo e também recebe contorno. | `src/Player/PixelPainter.cs:20-33` |
| Clarão de dano | Humanos: `Flush` com tinta branca pinta a figura inteira de branco (o contorno continua). Animais: retângulo branco translúcido sobre o corpo. | `PixelPainter.cs` (`tint`), `Boar.cs:140`, `Rabbit.cs:310` |
| Pixel inteiro | Toda posição é arredondada para o pixel (`Mathf.Round`), para não borrar. | `PixelPainter.cs:16-17`, `Boar.cs:90`, `Rabbit.cs:277` |

### Direção (para onde olha) e espelhamento

- **Humanos** (`FighterArt.cs:33-37`): o ângulo `face` decide uma de três vistas. A vista é **"down"** (de frente) ou **"up"** (de costas) quando o componente vertical da direção passa de 0,8 vez o horizontal. Nos outros casos é **"side"** (perfil). O sinal `s` (+1 direita, −1 esquerda) espelha cabelo, olho, sombra lateral da túnica e lapela. De costas, os braços e a espada são desenhados **antes** do corpo, por isso ficam atrás dele (`FighterArt.cs:62, 74, 109`). As mãos e a ponta da lâmina são calculadas pela direção real (`fx`, `fy`). A espada aponta para qualquer ângulo, não só para 4 direções.
- **Javali** (`Boar.cs:101`): só esquerda ou direita, pelo sinal de `Facing.X`. O corpo é espelhado coluna a coluna (`bx = -x - 1`, `Boar.cs:116`). A cabeça muda de lado (`hx = 11` ou `-19`, `Boar.cs:121`).
- **Coelho** (`Rabbit.cs:283`): mesmo critério do javali. Cabeça, mancha clara e rabo trocam de lado.
- **Carcaças**: sempre viradas para a direita (não espelham).

## 3. Cada figura, peça por peça

### 3.1 Base comum das figuras humanas (`FighterArt.Draw`, `FighterArt.cs:31-114`)

As três figuras do prólogo usam o mesmo desenho, com um "Look" diferente. O Look é um conjunto de 9 cores, mais altura extra `H`, largura extra `Wide` e acessórios (`FighterArt.cs:14-15`).

| Peça | Medida e posição (com H = 0, Wide = 0) | Linha |
|---|---|---|
| Pernas | Dois retângulos 4×7 (x −5 e x +1), de y −7 a 0. Andando, cada perna sobe até 2 px alternadamente. | 77-78 |
| Túnica | De y −27 até −6, com 21 + H linhas. Largura de 12 px no alto, alargando a cada quarto: 12 → 14 → 16 → 18 px (+2·Wide). Faixa de 3 px de sombra no lado de trás. Linha da gola (cor `Trim`) na 3.ª linha. Barra em sombra na última linha. | 79-86 |
| Faixa da cintura | (12 + 2·Wide) × 2 px, em y = topo + 9 + H (y −18). | 87 |
| Lapela | Tira vertical 2×10 em `Trim`, no peito, exceto de costas. | 88 |
| Ombreiras (só o Sênior) | Dois blocos 6×4 em `Trim`, para fora dos ombros. | 89 |
| Cabeça | Rosto 10×9 + linhas de 8×1 em cima e embaixo (≈10×11), topo em y −38 − H. | 91-92 |
| Cabelo/olhos de frente | Franja 10×4 + laterais 1×4; olhos 1×1 em tinta, a 5 px um do outro. | 93-98 |
| De costas | Cabelo cobre a cabeça toda (10×10). | 99 |
| Perfil | Cabelo 7×7 + 10×3; um olho 1×1; nariz 1×2 em pele. | 100-105 |
| Braços | Linha de 3 px na cor da túnica, do ombro (y = −25 − H + 2) até a mão. Segunda mão só nas poses Guarda e Ataque. | 63-73 |
| Espada | Linha de 1 px (`Blade`). No **aviso** fica de 2 px e na cor de ameaça. Cabo 3×3 (`Hilt`) com a mão 2×2 (pele) por cima. **Sênior:** os 45% finais da lâmina ganham 2 px de espessura (lâmina mais pesada). | 68-71 |
| Respiração | Nas poses Parado e Guarda, a cabeça desce 1 px quando `sin(t·2,4) > 0,55` (cerca de 1/3 do tempo). A estátua não respira. | 90-91 |
| Balanço do atordoamento | Na pose Atordoado, o corpo oscila ±2 px de lado (rápido, `sin(t·18)`), mais no alto do que nos pés. | 76, 83 |
| Barra andando | As 5 últimas linhas da túnica balançam ±1 px. | 80, 83 |

**Poses e posição da espada** (`FighterArt.cs:39-61`):

| Pose | Mão e lâmina |
|---|---|
| Parado / Andar | Mão ao lado do quadril (y ≈ −16). Lâmina para baixo e para a frente, ≈17 px. |
| Guarda | Mãos juntas diante do peito. Lâmina atravessada para cima, em diagonal (≈13 px de lado, 7 px para cima). |
| Aviso (Windup) | Mão erguida atrás da cabeça. Lâmina para trás e para cima (12 px para trás, 13 px para cima) **em vermelho-ocre de 2 px**. |
| Golpe (Strike) | Braço estendido 12 px à frente. Lâmina projetada 22 px à frente (15 px na vertical). |
| Atordoado (Stagger) | Mão baixa e de lado. Lâmina caída, apontando para o chão. |
| Ataque (só o Sênior) | Duas mãos no alto da cabeça. Lâmina erguida acima da cabeça, ≈16 px para cima. |

**Cor do contorno conforme o estado** (`FighterArt.cs:110-113`):

- normal: `#16140f`;
- **aviso** (telegraph): pisca entre `#c8502a` e `#e8834c`, 8 vezes por segundo;
- **aura** (Sênior em postura de ataque, ou no fim do prólogo): pisca entre `#c8502a` e `#7a2a18`, 4 vezes por segundo;
- **dano**: o contorno continua, e todas as cores ficam brancas por 0,12 s (`ValeFighter.cs:290`).

### 3.2 Discípulo da Guarda Fechada

Look em `FighterArt.cs:18`: altura e largura padrão, com chapéu.

| Parte | Hex |
|---|---|
| Túnica | `#7f846c` (cinza-esverdeado) |
| Sombra da túnica / barra | `#585d4b` |
| Gola, lapela (Trim) | `#3d4136` |
| Faixa | `#a28f58` |
| Pele | `#d0a07a` |
| Cabelo | `#26221e` |
| Calça | `#3a3a30` |
| Lâmina | `#cbd1d3` |
| Cabo | `#4a3a2a` |
| Chapéu (aba / meio / topo) | `#b69858` / `#c6a866` / `#d4b676` (fixos no código, `FighterArt.cs:106`) |

**Chapéu de palha** (`FighterArt.cs:106`): aba 22×3, de x −11 a +11 (6 px além do rosto de cada lado), cone 14×3, topo 6×2. O topo do chapéu fica em y −42. **Altura total: ≈43 px** dos pés ao topo do chapéu, sem contar o contorno.

**Estados e o que muda na tela** (mapa em `ValeFighter.cs:379-388`; IA em `ValeFighter.cs:200-274`; números em `assets/data/vale_combat.json`):

| Estado da IA | Pose | O que se vê | Tempo |
|---|---|---|---|
| Idle (parado em casa) | Parado | Gira devagar para baixo (sul), respira | até ver o jogador (raio 110–130 px, por discípulo; o da ponte, 190 px) |
| Guard (guarda fechada) | Guarda | Lâmina atravessada; mantém ~52 px de distância, recuando ou avançando | intervalo entre golpes `dis_cooldown` 1,1 s × (0,8 a 1,3) |
| Windup (aviso) | Aviso | Lâmina erguida para trás em ocre de 2 px, contorno piscando ocre, **cunha de aviso no chão** | `dis_windup` 0,5 s (0,3 s se foi aparado 2 vezes seguidas) |
| Strike (golpe) | Golpe | Avança 46 px em linha; lâmina estendida; **risco do golpe** (ValeFx) aos 0,04 s | `strike_time` 0,14 s |
| Recover (recuperação) | Guarda | Mesma pose da guarda; não acompanha o jogador com tanta força | `dis_recover` 0,55 s |
| Flinch (encolhido, golpe que entrou) | Atordoado | Balança ±2 px; clarão branco 0,12 s; recua com o empurrão | 0,32 s |
| Stagger (guarda quebrada por técnica "lâmina") | Atordoado | Balança + **3 estrelas girando** sobre a cabeça | duração passada pela técnica |
| Return (volta para casa) | Andar | Pernas alternam, barra balança | até chegar |
| Dead | Corpo caído | Ver 3.6 | permanente |

### 3.3 Espadachim Sênior

Look em `FighterArt.cs:20`: **H = 5** (5 px mais alto), **Wide = 2** (túnica 4 px mais larga), cabelo comprido, ombreiras e lâmina mais grossa no final.

| Parte | Hex |
|---|---|
| Túnica | `#6e3027` (vermelho escuro) |
| Sombra | `#4a1f1a` |
| Gola, lapela, ombreiras (Trim) | `#c79e4e` (dourado) |
| Faixa | `#24130f` |
| Pele | `#cc9a74` |
| Cabelo | `#131116` |
| Calça | `#2a1714` |
| Lâmina | `#e1e8ea` |
| Cabo | `#2a1a10` |

Medidas: túnica de y −32 a −6 (26 linhas), largura 16 → 22 px. Ombreiras 6×4 nas pontas dos ombros. Cabeça com topo em y −43. Tufo de cabelo 4×3 no alto (até y −46). Mechas compridas 2×14 dos lados (de frente) ou 2×15 atrás (de perfil). **Altura total ≈46 px.**

**Estados próprios do Sênior** (`ValeFighter.cs:223, 233-238, 241, 257-258`):

| Estado | Pose | O que se vê | Tempo |
|---|---|---|---|
| Guarda fechada | Guarda | Igual ao discípulo, mas maior e vermelho | 3,4 s (`sen_closed_time`) e então muda de postura |
| Postura de ataque (Advance) | **Andar** | Avança direto a 85 px/s, **contorno com aura piscando** ocre/vinho | desiste depois de 2,5 s sem alcançar |
| Combo de 3 golpes | Aviso → Golpe (×3) | Avisos de 0,38 s, depois 0,27 s e 0,49 s. Avanços de 40, 40 e **62 px**. O 3.º golpe é **largo**: cunha de 150° em vez de 80°, alcance +6 px e risco maior e mais claro. | golpe 0,14 s cada |
| Abertura depois do combo | **Ataque** (lâmina acima da cabeça) | Parado, a aura do contorno continua; é a hora de bater | 0,8 s (`sen_combo_recover`) |
| Armadura | — | Em postura de ataque, os golpes **não interrompem** (sem pose de atordoado, só o clarão branco) | `ValeFighter.cs:152, 293` |

### 3.4 Estátua da Mestra

Look em `FighterArt.cs:22`: tudo em cinzas de pedra, com coque.

| Parte | Hex |
|---|---|
| Túnica | `#8d9089` |
| Sombra | `#6d706a` |
| Gola, grampo (Trim) | `#7b7e77` |
| Faixa | `#777a73` |
| Pele | `#9b9e97` |
| Cabelo | `#63665f` |
| Calça | `#6a6d66` |
| Lâmina | `#abaea7` |
| Cabo | `#6a6d66` |

Coque (`FighterArt.cs:107`): dois coques laterais 3×4 e um grampo 3×3 no topo. Fica sempre na pose **Guarda** (`ValeFighter.cs:387`), não respira (`FighterArt.cs:90`) e não se move (`ValeFighter.cs:186`). Ao ser golpeada de frente, mostra o mesmo brilho de aparo dos discípulos, mas sem a palavra "aparado" e com faíscas cinza `#b9b5a6` (`ValeGame.cs:490-491`). No flanco, aparece a palavra "flanco" com um anel ciano (`ValeGame.cs:500-501`).

### 3.5 O fim do prólogo (`SeniorFinale.cs`)

| Etapa | Duração | O que se vê no Sênior |
|---|---|---|
| Victory | 2,0 s | Corpo caído (3.6), faixa "Vitória" |
| Revive | 2,6 s | Levanta na pose **Atordoado**. Aos 0,8 s, muda para **Ataque** (`:107`). A **aura** cresce de 0 a 0,7. Poeira `#8f7a58` girando nos pés, folhas `#8fb35a`/`#b5a54e` voando, tiras de pano `#6e3027`/`#c79e4e`. A tela treme. (`:103-115`) |
| Speech | até 6 s ou E | Pose **Guarda**, virado para o jogador, balão com a fala (`:114-120`) |
| Charge | 2,0 s | Pose **Aviso** (lâmina ocre erguida). Aura de 0,7 a 1,3. Partículas `#e8834c` são puxadas para o corpo. Depois de 60% da etapa, pisca em branco 6 vezes por segundo. (`:82-86, 122-128`) |
| Explode | 2,0 s | Clarão na tela; dois anéis (`#fff3c4` de raio 140, `#e8834c` de raio 90); 60 + 30 faíscas; o Sênior some (`Gone`) e fica **chão queimado**: retângulo 36×6 `#1d1712` a 70% (`ValeFighter.cs:339`). O jogador gira e sai voando do quadro. (`:129-145, 149-166`) |
| Dark | 3,4 s | Tela preta, silêncio, dois batimentos |

**A aura** (`ValeFighter.cs:343-350`): 3 anéis de 20 pontinhos 2×2 cada. Eles giram em volta do peito (y −22), achatados (altura 70% da largura). Raio = 14 + 7·anel + 10·força, com pulsação de ±2 px. O 1.º anel é `#e8834c` e os outros são `#c8502a`. A opacidade vai de 25% a 60%, conforme a força, e cai 25% a cada anel. Os anéis externos giram mais depressa.

### 3.6 Corpos caídos (humanos) — `FighterArt.DrawDead` (`FighterArt.cs:117-132`)

Deitado de lado, sempre com a cabeça para a direita:

- sombra 32×6;
- corpo 22×7 na cor da túnica, com 2 linhas de sombra no alto e a faixa como tira vertical 2×7;
- cabeça 8×8 com 3 linhas de cabelo;
- espada no chão, linha de 1 px de (−18, 4) até (2, 1);
- discípulo: o chapéu caído ao lado da cabeça (12×3 + 6×2);
- Sênior: uma ombreira dourada 6×4 na ponta dos pés;
- contorno normal.

Com a **página rasgada** (só o corpo do início, `ValeProps.cs:118`), aparece um retângulo 6×5 que alterna `#6fd8d2` e `#d4fbf6` 1,5 vez por segundo, com uma linha `#5d8f8b`.

### 3.7 Javali da Mata (`Boar.cs:86-142`)

| Parte | Hex |
|---|---|
| Pelo | `#4a3a2c` |
| Pelo claro (cerdas) | `#6b5640` |
| Escuro (patas, orelha, listras) | `#2c2219` |
| Focinho | `#8a6a58` |
| Presa | `#e6dcc0` |
| Olho normal | `#c9a14a` (âmbar) |
| Olho no aviso/investida | `#d05030` (vermelho, fixo no código, `:126`) |
| Poeira | `#8f7a58` |
| Estrelinhas de tontura | `#e6dcc0` |
| Marcas no chão | `#33271b` |
| Contorno | `#16140f` |

**Anatomia** (virado para a direita; à esquerda, tudo espelhado):

- **Corpo** (`:112-119`): 24 colunas de 1 px, de x −12 a +11. A altura sobe de 9 px na traseira para 13 px nos ombros, formando uma cunha pesada. O corpo vai de y ≈ −18 a −5. Cada coluna tem contorno próprio de 3 px de largura, e por isso o contorno fica contínuo. Os 2 px do alto de cada coluna formam a **crina**: escura a cada 3.ª coluna e clara nas outras, o que dá listras.
- **Patas** (`:109-110`): quatro 3×5 escuras, em x ≈ −9, −5, +5 e +9, de y −5 a 0.
- **Cabeça** (`:121-122`): 8×7, em x 11 a 18, y −14 a −7, com contorno.
- **Focinho** (`:123-124`): 4×3 na frente da cabeça. **Presa** 2×3 em pé sobre o focinho. **Olho** 2×1. **Orelha** 2×2 no alto da cabeça.
- Comprimento total ≈34 px (da traseira à ponta do focinho); altura ≈19 px.

**Estados** (`BoarBrain.cs:6, 61-100`; números em `assets/data/plains/BoarTuning.tres`):

| Estado | O que se vê | Tempo / valor |
|---|---|---|
| Rooting (fuçando no lamaçal) | Anda devagar entre pontos perto de casa, com pausas de 0,8–2,2 s. Corpo e cabeça sobem e descem 1 px ao andar (`sin(t·8)`), e uma pata dianteira acompanha. | 16 px/s (`WanderSpeed`) |
| Warn (aviso) | **Cabeça baixa 3 px**, **olho vermelho**. A **pata traseira raspa** (±3 px, `sin(t·22)`) e solta **3 grãos de poeira** que sobem atrás. Nos primeiros 55% do aviso, a cabeça segue o jogador. Depois a direção trava e aparecem **4 marcas no chão** (3×2, a 26, 38, 50 e 62 px à frente, cada vez mais apagadas) mostrando a linha da investida. | 0,8 s (`WarnTime`); trava aos 0,44 s (`TrackFraction` 0,55) |
| Charge (investida) | Cabeça baixa, olho vermelho, sobe e desce rápido (`sin(t·30)`), **4 nuvens de poeira atrás**. Golpes resvalam: aparece uma **faísca em cruz** na cor da presa sobre a cabeça por 0,15 s (`:141`). | 240 px/s, até 170 px; acerta a 15 px (`ChargeSpeed`, `ChargeDistance`, `ChargeHitRadius`) |
| Recover (recomposição; a abertura) | Cabeça volta ao normal e treme ±1 px (`sin(t·18)`). **3 estrelinhas** 2×2 com contorno giram sobre a cabeça. | 1,0 s (`RecoverTime`), +0,15 s a cada golpe recebido (`HitFlinch`) |
| Return (volta para casa) | Igual a fuçar, andando mais rápido; sem sinal próprio | 50 px/s |
| Dead | O javali para de ser desenhado (`:97`) e uma **Carcaça** aparece no lugar | — |

**Dano**: retângulo branco 24×13 sobre o corpo, com opacidade de até 60%, sumindo em 0,2 s (`:81, 140`). Golpes no aviso e na investida não causam dano (`BoarBrain.cs:47, 135`).

### 3.8 Coelho de Campo (`Rabbit.cs:274-311`)

| Parte | Hex |
|---|---|
| Pelo | `#8a7a5e` |
| Pelo claro | `#b8a98a` |
| Barriga, rabo | `#d8ccb0` |
| Dentro da orelha | `#c99a8a` |
| Olho | `#16140f` |
| Contorno | `#16140f` |

**Anatomia** (virado para a direita):

- **corpo** 10×7, de y −7 a 0, com 2 linhas de barriga embaixo e uma mancha clara 2×2 na traseira;
- **cabeça** 6×5 na frente, 3 px acima do corpo, com olho 1×1;
- **rabo** 2×2 claro atrás;
- cada peça tem um retângulo de contorno 1 px maior.

**Orelhas: o sinal do coelho** (`:298-307`):

- **abaixadas**: 6×2, deitadas para trás sobre o pescoço;
- **em pé**: duas orelhas 2×6 e 2×7, uma escura e uma clara com o interior rosado 1×4. Chegam a ≈17 px do chão.

**Estados** (`RabbitBrain.cs:148, 190-219`; números em `assets/data/plains/RabbitTuning.tres`):

| Estado | Orelhas | O que se vê | Tempo / valor |
|---|---|---|---|
| Graze (pastando) | abaixadas | Pulinhos de 0,3 s a cada 1,1–2 s; sobe até 2 px (`|sin(t·14)|·2`) | 66 px/s no pulo; até 44 px da toca |
| Alert (ouviu) | **em pé** | Fica **parado**, virado para o jogador | 0,35 s (`AlertTime`) |
| Dash1 / Dash2 (corrida) | em pé | Corpo **esticado 3 px** (13×7), sem pulinhos | 165 px/s (`DashSpeed`) |
| Pause (entre as corridas) | em pé | Parado | 0,18 s (`DashPause`) |
| Hidden (na toca) | — | **Invisível** (não é desenhado nem pode ser atingido) | 4,5 s (`HideTime`), depois volta a pastar |
| Dead | — | Some; aparece a Carcaça | — |

A distância em que ele ouve não aparece na tela. Ela depende de como o jogador se move: parado 14 px, andando agachado 26 px, andando 95 px, com passo ou corte 130 px (`Rabbit.cs:244-250`).

### 3.9 Carcaças (`Carcass.cs:333-351`)

Ficam sempre viradas para a direita e não mudam quando se tira carne ou pele.

- **Poça de sangue**: 20×3 `#7a3a32` a 55% (`:341`).
- **Javali**: contorno 28×10; pelo `#4a3a2c` 26×8; barriga escura `#2c2219` 26×3; presa `#e6dcc0` 3×2 na ponta; duas patas 3×2.
- **Coelho**: contorno 14×6; pelo 12×4; faixa clara 12×2; orelha 2×3; rabo 2×2.

### 3.10 Pegadas e toca

**Pegadas** (`TrackPrint`, `Landmarks.cs:28, 41-60`): cada pegada aponta na direção da trilha. Elas ficam a 22 px uma da outra (`design/gdd/plains-progression.md:77`).

| Tipo | Desenho | Cores |
|---|---|---|
| Casco (javali) | Duas marcas 2×3 à frente e duas 2×2 atrás, lado a lado (4 px entre elas) | `#2a2219` na frente, `#5e4a34` atrás |
| Pata (coelho) | Duas marcas 2×2 desencontradas e um ponto 1×1 à frente | mesmas cores |
| Brilhando (depois de examinar, ou todas com Rastreamento grau 3) | Tudo em `#d4fbf6` + halo 12×10 pulsando | ciano-branco |

**Toca** (`ChunkGround.cs:100-106`): monte de terra 18×7 + 14×3, buraco 10×5 + 6×1 e três pegadinhas 2×1 entrando.

## 4. Sinais de leitura do combate

### Sem o Modo de assistência

| Figura | "Vai atacar" | "Pode bater agora" | "Não adianta bater" |
|---|---|---|---|
| Discípulo | Lâmina erguida para trás em ocre; contorno piscando ocre; **cunha no chão** que se enche de dentro para fora (`ValeFighter.cs:356-366`) | Recuperação (0,55 s depois do golpe); de lado/atrás quando está **enganado** pelo passo lateral; atordoado | De frente, em guarda: aparece o **arco da guarda** e a palavra "aparado" |
| Sênior | Igual, e na postura de ataque o contorno pulsa (aura); o 3.º golpe tem cunha bem mais larga | Pose com a lâmina acima da cabeça depois do combo (0,8 s) | Guarda fechada de frente; na postura de ataque, os golpes não interrompem |
| Javali | Cabeça baixa, olho vermelho, casco raspando com poeira; marcas no chão quando a direção trava | Estrelinhas girando sobre a cabeça (1 s) | Faísca na presa (aviso e investida) |
| Coelho | — (não ataca) | Antes das orelhas subirem; aproximar agachado | Orelhas em pé = vai fugir |

**Cunha de aviso** (`ValeFighter.cs:356-366`): 11 raios de pontinhos 2×2 `#c8502a`, a cada 6 px, que começam a 14 px do corpo. Os raios crescem até o alcance real do golpe conforme o aviso avança. A opacidade vai de 18% a 43%. O alcance desenhado é o mesmo testado no acerto (`StrikeArea`, `:117-123`): discípulo 72 px e ±40°, Sênior 78 px e ±40°, 3.º golpe do Sênior 84 px e ±75°.

**Arco da guarda** (`:368-377`): arco de raio 30 na frente, ±65° (`guard_half`), cor `#ece5d0`. Fica fraco (22%) enquanto o inimigo é o **alvo travado**. Acende (até 100%, 2 px) logo depois de um aparo. Depois que o jogador aprende o passo lateral, o resto do círculo aparece **tracejado em ciano** `#6fd8d2` enquanto o inimigo está enganado (na estátua, sempre que está travada).

**Brilho do aparo** (`:390-395`): 9 pontinhos `#fff3c4` em arco diante do peito (y −16), apagando em 0,7 s.

**Estrelas de atordoamento** (`:396`): 3 pontinhos `#d4fbf6` girando em elipse de 7×2 px na altura y −48. Só aparecem na guarda quebrada (Stagger), não no simples encolher (Flinch).

**Efeitos por cima** (`ValeFx.cs`):

| Efeito | Aparência | Onde é chamado |
|---|---|---|
| "aparado" | texto 9 px `#ece5d0` com sombra, sobe 14 px/s, 0,5 s | `ValeGame.cs:490` |
| "flanco" | texto ciano `#d4fbf6`, 0,8 s, 50 px acima do inimigo | `ValeGame.cs:501, 509` |
| "passo" | texto ciano sobre o jogador ao desviar | `ValeGame.cs:221` |
| Anel | 16 pontinhos 2×2 em elipse; o raio cresce de 0,6× a 1,6× e some | `ValeFx.cs:264-268`; aparo `ValeGame.cs:492` |
| Risco do golpe inimigo | Arco de pontinhos 2×2 com 36 px (52 no golpe largo), curvado, 26 px à frente; `#c8502a` (largo: `#e8834c`); some em 0,18 s | `ValeFx.cs:269-279`, `ValeGame.cs:650-652` |
| Faíscas de acerto | 12 ciano + 6 brancas; ao matar, 24 ciano | `ValeGame.cs:513, 529` |
| Cantos da mira | 4 cantos em "L" de 5 px `#e8834c` em volta do alvo travado, pulsando ±1,5 px | `ValeFx.cs:288-299` |

**Barra no alto da tela** (`PlainsHud.cs:223-228, 388-393`): nome + estado e barra de 120×4 em vermelho-ocre.

- No prólogo: "Discípulo da Guarda Fechada · guarda fechada / recuperando / flanco aberto / guarda quebrada", ou "Espadachim Sênior · postura de ataque" (`ValeGame.cs:298-305`).
- Na Planície: "Javali da Mata · Nv 3 · Territorial" para o animal mais próximo a até 170 px (`PlainsGame.cs:664-673`).

### Com o Modo de assistência (Esc → Modo de assistência)

As cores ficam em `src/Game/Assist.cs:34`: guarda `#ece5d0`, abertura `#6fd8d2`, ameaça `#e0503a`. As áreas usam **os mesmos números** dos testes de acerto.

| Figura | Guarda | Aviso | Golpe ativo | Abertura |
|---|---|---|---|---|
| Discípulo / Sênior (`ValeFighter.cs:299-330`) | Setor ±65° de raio 34, preenchido a 10% com borda de 2 px; o resto do círculo tracejado em ciano (mais forte quando enganado) | Contorno tracejado da área inteira + preenchimento crescendo do corpo | Área cheia a 45%, borda de 3 px | Contorno fino tracejado que **encolhe** |
| Sem guarda (postura de ataque, atordoado) | Círculo inteiro tracejado em ciano | | | |
| Javali (`Boar.cs:146-166`) | — | **Corredor** de 170 × 30 px tracejado + preenchimento crescendo; anel de 16 px cor de guarda ("resvala") | Corredor cheio que encurta com o que já correu; anel de guarda | Anel ciano tracejado pulsando (16 ± 4 px) |

Com a assistência ligada, o arco de guarda normal não é desenhado (`ValeFighter.cs:371`), porque a assistência já mostra a guarda. A estátua não ganha desenho de assistência (`:367`).

## 5. Relação com o protótipo HTML e com a bíblia de arte

**Protótipo** (`prototypes/spiritofmurin/index.html`): `FighterArt` é uma **tradução quase linha a linha** de `LOOK` (linha 1124), `drawFighter` (1132) e `drawDead` (1221). As cores dos três Looks são idênticas. Diferenças:

- O protótipo tinha as poses `cut`, `cast` e `step` e o contorno "fantasma" (`ghost`). O Godot só traz as poses de inimigo; as do jogador ficam em outro arquivo.
- **Respiração**: o protótipo somava `x · 0,37` à fase, e cada inimigo respirava num ritmo próprio. No Godot essa parte saiu (`FighterArt.cs:90`), e todos respiram juntos.
- A sombra do protótipo tinha duas camadas (22×4 + 16×6). No Godot é só um retângulo 22×4 (`ValeFighter.cs:378`).
- Javali, coelho, carcaças e pegadas **não existem no protótipo**. Foram criados para a Planície no mesmo estilo: contorno `#16140f`, retângulos e paleta terrosa.

**Bíblia de arte** (`design/art/art-bible.md`), pontos ligados aos inimigos:

- **Cores com significado (§4.1, §4.3)**: o vermelho-ocre `#c8502a`/`#e8834c` é "intenção hostil", só no inimigo durante o ataque. O código segue isso no aviso, no contorno, na cunha, no risco do golpe e na aura do Sênior. O ciano-branco `#d4fbf6`/`#6fd8d2` é "conhecimento" e aparece no flanco, na página e nas pegadas.
- **Contorno de 1 px `#16140f` em todo ator vivo (§3.2)**: seguido.
- **O que a bíblia descreve e o jogo ainda não usa**: a bíblia revisada (§4.2, §4.3, §4.4 e §5.4) define um discípulo novo, em forma de **ampulheta**. Ele teria aba de 21 px inclinada, lâmina a 45° cruzando o peito, base de 14–16 px com perneiras claras, corpo em ardósia `#555d5e` e palha `#b0a382`. Também define um Sênior de **48–50 px** com coque alto, e uma estátua **sem contorno de ator**, com pés esculpidos em passo lateral. O jogo continua com a **aparência original do protótipo**, que o criador escolheu depois do experimento com sprites (relatório `2026-11-25-ciclo-mil-caminhos.md`). Por isso o código tem túnica até os pés, chapéu reto, Sênior de cabelo solto e estátua com contorno.

## 6. Como alterar

### Dados (sem mexer em código C#)

| O que | Arquivo | Efeito visual |
|---|---|---|
| Tempo do aviso, alcance e ângulo do golpe, guarda, recuperação do discípulo e do Sênior | `assets/data/vale_combat.json` (`dis_windup`, `dis_reach`, `strike_half`, `strike_half_big`, `guard_half`, `dis_recover`, `sen_*`) | Muda o tamanho da cunha, do arco da guarda e das áreas da assistência, e quanto tempo cada pose fica na tela |
| Aviso, investida, recomposição do javali | `assets/data/plains/BoarTuning.tres` (`WarnTime`, `TrackFraction`, `ChargeDistance`, `ChargeHitRadius`, `RecoverTime`) | Duração da cabeça baixa e do olho vermelho, momento em que surgem as marcas no chão, tamanho do corredor da assistência, tempo das estrelinhas |
| Congelamento, corrida e esconderijo do coelho | `assets/data/plains/RabbitTuning.tres` (`AlertTime`, `DashSpeed`, `DashPause`, `HideTime`) | Quanto tempo as orelhas ficam em pé paradas, quanto tempo ele fica invisível |
| Nome e temperamento na barra do alto | `assets/data/plains/plains_progression.json:66-68` | Texto da barra |

`BodyRadius` (javali 10, coelho 6) só define a área que a lâmina acerta. **Não muda o tamanho do desenho.**

### Código (precisa recompilar)

| O que | Onde |
|---|---|
| Cores do discípulo, do Sênior e da estátua | `src/Combat/FighterArt.cs:18, 20, 22` (ordem: túnica, sombra, gola, faixa, pele, cabelo, calça, lâmina, cabo, H, Wide) |
| Cores do chapéu | `FighterArt.cs:106` |
| Cores de ameaça e aura do contorno; frequência da piscada | `FighterArt.cs:25, 111-112` |
| Altura e largura do Sênior | `FighterArt.cs:20` (os números `5, 2`) |
| Posição da espada em cada pose | `FighterArt.cs:39-61` |
| Corpo, túnica, cabeça, acessórios | `FighterArt.cs:77-108` |
| Corpo caído | `FighterArt.cs:117-132` |
| Qual pose cada estado usa | `src/Vale/ValeFighter.cs:379-388` |
| Cunha de aviso (cor, densidade) | `ValeFighter.cs:356-366` |
| Arco da guarda e flanco | `ValeFighter.cs:368-377` |
| Brilho do aparo, estrelas | `ValeFighter.cs:390-396` |
| Aura e chão queimado | `ValeFighter.cs:339, 343-350` |
| Tempo do encolher (0,32 s) e do clarão (0,12 s) | `ValeFighter.cs:272, 290` (fixos no código) |
| Avanço de cada golpe (46 px; 40/40/62 no combo) e ritmo do combo | `ValeFighter.cs:241, 257` (fixos no código) |
| Etapas do fim do prólogo (tempos, cores das partículas, anéis) | `src/Vale/SeniorFinale.cs:95-185` |
| Palavras, anéis, risco, cantos da mira | `src/Vale/ValeFx.cs:261-299`; quem chama: `ValeGame.cs:480-529, 650-652` |
| Javali: cores | `src/Creatures/Boar.cs:86-88` (olho vermelho em `:126`) |
| Javali: corpo, cabeça, patas | `Boar.cs:106-127` |
| Javali: poeira, marcas, estrelinhas, faísca | `Boar.cs:99-100, 128-141` |
| Coelho: cores | `src/Creatures/Rabbit.cs:274-275` |
| Coelho: corpo, orelhas, pulo, esticada | `Rabbit.cs:283-310` |
| Carcaças | `src/Creatures/Carcass.cs:333-351` |
| Pegadas | `src/Plains/Landmarks.cs:28, 41-60` |
| Toca | `src/Plains/ChunkGround.cs:100-106` |
| Cores e desenho do Modo de assistência | `src/Game/Assist.cs:34-75`; por figura: `ValeFighter.cs:299-330`, `Boar.cs:146-166` |
| Barra do alvo | `src/Plains/PlainsHud.cs:223-228` |

## 7. Limitações conhecidas e observações

A aparência está aprovada. Os itens abaixo são só observações do que o código faz hoje, não propostas de mudança.

1. **Todos os inimigos respiram juntos.** O protótipo dava um ritmo diferente a cada um, e essa parte não veio no porte (`FighterArt.cs:90`).
2. **O javali não tem animação de fuçar.** "Rooting" é andar devagar com pausas. A volta para casa também não tem sinal próprio.
3. **O clarão de dano do coelho quase nunca aparece.** O coelho tem 1 de vida em todos os níveis (`health_per_level: 0`), então morre no primeiro golpe. O clarão também não cobre a parte esticada do corpo durante a corrida (`Rabbit.cs:310`).
4. **As orelhas em pé do coelho não espelham.** Ficam sempre na mesma posição relativa à cabeça, mesmo virado para a esquerda (`Rabbit.cs:300-301`). As orelhas abaixadas espelham.
5. **As carcaças não mudam** quando se tira a carne ou a pele, e sempre ficam viradas para a direita (`Carcass.cs:339-351`).
6. **As estrelas da guarda quebrada e as faíscas de acerto usam ciano-branco** (`ValeFighter.cs:396`, `ValeGame.cs:513`). A bíblia (§4.3) reserva essa cor para "o que se aprende". É uma decisão herdada do protótipo e fica só registrada aqui.
7. **Encolher (Flinch) e guarda quebrada usam a mesma pose.** Só as estrelas distinguem os dois.
8. **Vários tempos de animação estão fixos no código**, não no JSON: o encolher de 0,32 s, o clarão de 0,12 s, o brilho do aparo de 0,7 s, os avanços dos golpes e as frequências das piscadas. Mudá-los exige recompilar.
9. **O desenho do código não segue as medidas novas da bíblia** (discípulo em ampulheta, Sênior de 48–50 px, estátua sem contorno). Isso é esperado, porque o criador manteve a aparência original. A bíblia descreve uma direção futura possível.
10. **Os animais não têm vista de frente nem de costas**, só esquerda e direita. Andando na vertical, eles continuam de perfil.
