# Como o espadachim é desenhado (visual do personagem no código)

> Data: 2026-11-26 · Tipo: documentação (nenhum código foi alterado)
> Fonte: leitura do código em `src/Player/`, `src/Combat/FighterArt.cs`, `src/Game/Assist.cs`, das cenas e de `design/art/art-bible.md`.
> A aparência descrita aqui é a **aprovada pelo criador** (decisão de 2026-11-25, `design/art/art-bible.md:770`). Este relatório só explica como ela é feita; as ideias da seção 10 são opcionais.

Todas as referências estão no formato `arquivo:linha`. As medidas são em **pixels nativos** da tela de 640×360.

---

## 1. Resumo

O espadachim **não tem nenhuma imagem** (PNG, sprite ou folha de animação). A cada quadro, o Godot chama a função `_Draw` do personagem (`src/Player/Player.cs:598`), e ela monta a figura com **retângulos coloridos**, um por um, como se pintasse pixel art com um pincel quadrado.

Três ideias sustentam tudo:

- **Retângulos inteiros.** Toda forma é um retângulo com posição arredondada para o pixel inteiro (`src/Player/PixelPainter.cs:16-17`). Linhas (braço, lâmina, fita) viram uma fila de quadradinhos pelo algoritmo de Bresenham (`src/Player/PixelPainter.cs:20-33`).
- **Escala de pixel.** O jogo desenha numa tela interna de 640×360 e a amplia por um fator **inteiro**, sem suavizar (`project.godot:24-30`, filtro "nearest" em `project.godot:134`). Por isso 1 retângulo de 1×1 vira um "pixel grande" nítido no monitor.
- **Contorno.** Antes das cores, cada retângulo é pintado 1 px maior em todas as direções na cor de tinta `#16140f`; depois as cores vão por cima. Sobra só a borda externa escura: o contorno da silhueta (`src/Player/PixelPainter.cs:36-41`, detalhes na seção 7).

O desenho é uma tradução direta da função `drawFighter` do protótipo HTML (comentário em `src/Player/Player.cs:671` e `src/Player/Player.cs:254`).

---

## 2. O pipeline de desenho

### 2.1 Sistema de coordenadas

- A origem **(0, 0) fica nos pés**, no centro da base (`src/Player/PixelPainter.cs:8-9`). É também a posição do nó `Player` no mundo.
- O eixo **y cresce para baixo** (padrão do Godot). Por isso tudo que fica acima do chão tem y negativo: a cabeça está perto de y = −38.
- O círculo de colisão dos pés tem raio 7 px (`src/Player/Player.tscn`, `CircleShape2D_feet`). A câmera mira 16 px acima dos pés (`Camera`, `position = Vector2(0, -16)` no mesmo arquivo).

### 2.2 Ordem das camadas em `_Draw` (de trás para a frente)

| # | Camada | Onde |
|---|---|---|
| 1 | Rastros (afterimages) dos passos | `src/Player/Player.cs:600-605` |
| 2 | Sombra no chão | `src/Player/Player.cs:607-608` |
| 3 | Área do golpe do modo de assistência (se ligado) | `src/Player/Player.cs:609-616` |
| 4a | Caído (deitado), e encerra aqui | `src/Player/Player.cs:617` |
| 4b | Levantando (ajoelhado), e encerra aqui | `src/Player/Player.cs:618` |
| 4c | Lançado no ar (rodopio), e encerra aqui | `src/Player/Player.cs:619-627` |
| 4d | A figura normal (piscando se invulnerável) | `src/Player/Player.cs:628` |
| 5 | Clarão vermelho de ferimento | `src/Player/Player.cs:629` |
| 6 | Risco do corte (slash) | `src/Player/Player.cs:630-637` |

Dentro da figura normal (`DrawFighter`, `src/Player/Player.cs:672-749`) a ordem é:

1. **Braço e arma**, só quando ele está de costas (vista "up"), para ficarem atrás do corpo (`src/Player/Player.cs:713`).
2. **Pernas** (`src/Player/Player.cs:715-717`).
3. **Túnica**, linha por linha, com sombra lateral, gola e barra (`src/Player/Player.cs:719-726`).
4. **Faixa** da cintura (`src/Player/Player.cs:727`) e **frente da túnica** (`src/Player/Player.cs:728`).
5. **Cabeça**: pele, cabelo, olhos (`src/Player/Player.cs:730-743`).
6. **Coque** (`src/Player/Player.cs:744`) e **fita** (`src/Player/Player.cs:745-746`).
7. **Braço e arma** na frente do corpo, em todas as vistas menos de costas (`src/Player/Player.cs:747`).
8. **Flush**: contorno de tudo, depois as cores (`src/Player/Player.cs:748`).

### 2.3 Direção (para onde ele olha)

- `Facing` é um vetor de comprimento 1 com a última direção andada (`src/Player/Player.cs:221`, atualizado em `src/Player/Player.cs:344`). Com um alvo travado, ele passa a olhar para o alvo (`src/Player/Player.cs:351-355`).
- O ângulo vira dois números: `fx` (quanto olha para a direita) e `fy` (quanto olha para baixo) (`src/Player/Player.cs:675`).
- **Vista** (`src/Player/Player.cs:676`): se o componente vertical vence o horizontal (com folga de 0,8), a vista é **"down"** (de frente) ou **"up"** (de costas); senão é **"side"** (de lado). Na prática, as diagonais aparecem de frente ou de costas; só perto da horizontal (até ~51° do eixo horizontal) ele fica de perfil.
- **Espelhamento:** não há imagem espelhada. O código guarda o lado em `s` (+1 olhando à direita, −1 à esquerda, `src/Player/Player.cs:677`) e calcula cada peça já do lado certo: a faixa de sombra da túnica troca de lado (`src/Player/Player.cs:724`), o cabelo vai para a nuca e o nariz para a frente (`src/Player/Player.cs:741-742`).
- `nx = -fy` é a direção "de lado" em relação ao olhar; serve para pôr a mão da espada ao lado do corpo (`src/Player/Player.cs:678`, `src/Player/Player.cs:701`).

### 2.4 Quem fica na frente de quem (y-sort)

As cenas ligam a ordenação por altura (`y_sort_enabled = true` em `src/Main/Main.tscn:15`, `src/Plains/Plains.tscn:20`, `src/Vale/Vale.tscn:14`). Como a origem do personagem são os pés, quem tem os pés mais abaixo na tela é desenhado por cima. É isso que faz o espadachim passar na frente de uma árvore quando está abaixo dela e atrás quando está acima.

---

## 3. Paleta

As cores fixas do jogador estão em `src/Player/Player.cs:255-257` (copiadas do `LOOK.player` do protótipo, `prototypes/spiritofmurin/index.html:1125`).

| Nome no código | Hex | Onde aparece |
|---|---|---|
| `Robe` (túnica) | `#dce5d4` | túnica, mangas dos braços, braço no peito ao deitar |
| `Shade` (sombra da túnica) | `#9db3a0` | faixa de 3 px num lado da túnica, linha da barra, topo da túnica deitada |
| `Trim` (debrum) | `#2f7f6c` | gola (linha 3 da túnica), frente da túnica, fita do cabelo, borda inferior quando deitado |
| `Sash` (faixa) | `#2f7f6c` | faixa da cintura (mesma cor do debrum) |
| `Skin` (pele) | `#e2b58e` | rosto, mãos, nariz de perfil |
| `Hair` (cabelo) | `#1b191e` | cabelo, costeletas, coque |
| `Pants` (calça) | `#3d4a44` | pernas |
| `Blade` (lâmina) | `#eef3f2` | lâmina da espada |
| `Flint` (pedra) | `#8c8a80` | lâmina da faca de pedra |
| `Hilt` (cabo) | `#7a5a3a` | cabo/guarda, sob a mão |
| `Outline` (tinta) | `#16140f` | contorno de tudo e os olhos |
| `Learn` (ciano-branco) | `#d4fbf6` | centro do risco do corte |
| `LearnGlow` (ciano) | `#6fd8d2` | pontas do risco, rastros do passo, palma do Qi, "respiração" ao deitar |

Cores que não estão na lista acima:

| Uso | Valor | Onde |
|---|---|---|
| Sombra no chão | preto esverdeado `rgba(8,10,8)` a 38% | `src/Player/Player.cs:607` |
| Clarão de ferimento | vermelho ~`#bf3326` com transparência | `src/Player/Player.cs:629` |
| Área do golpe (assistência) | jade pálido `#a6f2e2` | `src/Game/Assist.cs:34` |

---

## 4. Anatomia peça por peça (figura em pé)

Medidas a partir dos pés (y = 0). "x" é horizontal, centro do corpo em x = 0.

| Peça | Medidas e posição | Onde |
|---|---|---|
| **Pernas** | duas colunas de 4×7 px: esquerda de x −5 a −2, direita de x 1 a 4; y −7 a 0. Entre elas fica 2 px de vão | `src/Player/Player.cs:716-717` |
| **Pés** | não há peça separada: o pé é a ponta de baixo da perna | — |
| **Túnica** | 21 linhas de 1 px, de y −27 a y −7. Alarga para baixo em degraus: 12 px (linhas 0–5), 14 (6–10), 16 (11–15), 18 (16–20) | `src/Player/Player.cs:719-726` |
| Sombra da túnica | 3 px de largura em todas as linhas, no lado esquerdo quando olha à direita e vice-versa | `src/Player/Player.cs:724` |
| Gola | linha 3 da túnica (y −25), em `Trim`, com 3 px de margem de cada lado | `src/Player/Player.cs:725` |
| Barra | última linha (y −7) em `Shade`, com 3 px de margem | `src/Player/Player.cs:725` |
| **Faixa** | 12×2 px em y −18 e −17 | `src/Player/Player.cs:727` |
| Frente da túnica | coluna de 2×10 px de y −27 a −18, no centro (deslocada 2 px para o lado do olhar de perfil); some de costas | `src/Player/Player.cs:728` |
| **Linha dos ombros** | y −25 (`sy`) | `src/Player/Player.cs:678` |
| **Cabeça** | 10 px de largura por 11 de altura (y −38 a −28), com os cantos cortados (linhas de cima e de baixo têm 8 px) | `src/Player/Player.cs:731-732` |
| Cabelo de frente | 10×4 no alto da cabeça + costeletas de 1×4 dos dois lados | `src/Player/Player.cs:735` |
| Olhos de frente | dois pixels de 1×1 em tinta, em x −3 e x 2, na linha y −32 | `src/Player/Player.cs:736` |
| Cabelo de costas | 10×10 cobrindo toda a cabeça | `src/Player/Player.cs:738` |
| Perfil | cabelo 7×7 na nuca + 10×3 no alto; um olho 1×1; nariz de pele 1×2 que sai 1 px da cabeça | `src/Player/Player.cs:741-742` |
| **Coque** | 4×3 px no topo, y −41 a −39 | `src/Player/Player.cs:744` |
| **Fita** | linha de 1 px em `Trim`, do coque para trás (~7 px, oposta ao olhar), balançando | `src/Player/Player.cs:745-746` |
| **Braço** | linha de 3 px de espessura em `Robe`, do ombro até a mão | `src/Player/Player.cs:707-708` |
| **Mão** | 2×2 px de pele | `src/Player/Player.cs:711` |
| **Cabo** | 3×3 px em `Hilt`, sob a mão | `src/Player/Player.cs:709-710` |
| **Espada** | linha de 1 px em `Blade`. Em repouso vai da mão até (14, 9) px adiante (de frente, ~13 px apontando para baixo; de perfil, ~17 px para a frente e para baixo) | `src/Player/Player.cs:701-702`, `709` |
| **Faca de pedra** | linha de 2 px em `Flint`, com 40% do comprimento da espada | `src/Player/Player.cs:680`, `710` |
| **Mãos vazias** | só braço e mão, sem lâmina nem cabo | `src/Player/Player.cs:709-711` |

Altura total: dos pés (y 0) ao alto do coque (y −41) são **41 px**, mais 1 px de contorno em cima e embaixo. A mão da espada em repouso fica em y −16, um pouco abaixo da faixa.

---

## 5. Poses e animações

Não há quadros de animação guardados: cada pose é **uma posição de mão e ponta da lâmina calculada na hora** (`src/Player/Player.cs:681-703`), mais pequenos deslocamentos de pernas, barra e cabeça. O corpo (túnica, cabeça) é sempre o mesmo.

| Pose | O que muda | Tempo | Onde |
|---|---|---|---|
| **Parado** | mão ao lado do corpo, lâmina para baixo e à frente. **Respiração:** a cabeça desce 1 px cerca de 31% do tempo | ciclo de ~2,6 s | `src/Player/Player.cs:699-703`, `730-731` |
| **Andar** | as pernas sobem 2 px alternadas; as 5 linhas de baixo da túnica balançam 1 px para os lados (duas vezes por ciclo) | um ciclo completo a cada ~0,9 s (fase avança 7/s) | `src/Player/Player.cs:344`, `715-717`, `720-723` |
| **Andar quieto (Shift)** | mesma pose de andar, só mais lenta (fase avança 4/s → ciclo ~1,6 s); o corpo anda a 45% da velocidade | — | `src/Player/Player.cs:343-344`; `SneakFactor` em `assets/data/PlayerTuning.tres:29` |
| **Corte 1 e 3** | a mão gira num arco de 150° em volta do peito, de −75° a +75° em relação ao olhar; a lâmina (20×14 px de alcance desenhado) acompanha. Antes de o golpe valer, a lâmina fica parada no início do arco (preparação); depois, parada no fim (recuperação) | preparação 0,07 s, golpe 0,08 s, recuperação 0,24 s; o 3º corte ganha +0,03 s em preparação e golpe e +0,1 s de recuperação | `src/Player/Player.cs:681-687`; tempos em `assets/data/PlayerTuning.tres:9-13`, `src/Player/Player.cs:499-505` |
| **Corte 2** | o mesmo arco no sentido contrário (+75° → −75°) | igual ao corte 1 | `src/Player/Player.cs:683` |
| **Corte da faca** | o mesmo arco, com lâmina curta e grossa | 0,05 / 0,07 / 0,15 s; cadeia de 2 | `src/Player/Player.cs:491-496`; `assets/data/PlayerTuning.tres:20-27` |
| **Estocada** | mão estendida 12 px à frente, lâmina reta apontando para o alvo (22×15 px) | 0,09 / 0,1 / 0,28 s | `src/Player/Player.cs:688-692`, `449` |
| **Qi (onda)** | mão da arma desce ao lado do corpo; um quadradinho ciano 3×3 aparece à frente (a palma juntando Qi) | 0,1 s até sair o tiro, total ~0,34 s | `src/Player/Player.cs:693-698`, `430` |
| **Passo** | sem pose própria: figura parada (pernas juntas), deslizando; deixa rastros | 0,16 s, 34 px | `src/Player/Player.cs:513-519`, `592`; `assets/data/PlayerTuning.tres:16-17` |
| **Passo lateral** (em volta do alvo) | igual ao passo, em arco em volta do alvo, com rastros | tempo vem da técnica | `src/Player/Player.cs:561-571` |
| **Passo longo** (dash) | igual ao passo, em linha reta e mais longe, com rastros | tempo vem da técnica | `src/Player/Player.cs:572-579` |
| **Recuo (aparado)** | sem pose própria: figura parada sendo empurrada para trás, freando a cada quadro | 0,24 s | `src/Player/Player.cs:83-86`, `348`; chamado em `src/Vale/ValeGame.cs:494` |
| **Ferido** | clarão vermelho translúcido de 16×40 px sobre o corpo, que some em 0,3 s; empurrão de 26 px | 0,3 s | `src/Player/Player.cs:272-275`, `629` |
| **Invulnerável** após ferimento | a figura pisca: aparece 0,08 s, some 0,04 s | 0,7 s | `src/Player/Player.cs:628`; `assets/data/PlayerTuning.tres:31` |
| **Deitado** (despertar na Planície) | corpo deitado com a cabeça à direita: túnica 24×7, faixa vertical, calças à esquerda, cabeça 9×9, braço sobre o peito, fita mexendo na grama; um pixel ciano pisca no peito (respiração) | — | `src/Player/Player.cs:641-653`; ativado em `src/Plains/PlainsGame.cs:353` |
| **Ajoelhado** (levantando) | um joelho no chão (perna de 6×5), outra perna em pé (4×8), mão apoiada no chão; o tronco sobe 9 px conforme ele se levanta. Sem arma e sem fita | 0,9 s | `src/Player/Player.cs:656-669`; tempo em `src/Plains/PlainsGame.cs:414` |
| **Rodopio da explosão** | a figura inteira gira em volta do meio do corpo (ponto y −20) e sobe; a sombra fica no chão | 1,4 s, 2,5 voltas, até 260 px de altura, 560 px de distância | `src/Player/Player.cs:619-627`; `src/Vale/SeniorFinale.cs:155-165` |

---

## 6. Efeitos ligados ao personagem

### Risco do corte (slash)
- Nasce quando o golpe passa a valer (`src/Player/Player.cs:580-591`). Fica **preso no mundo**, não no corpo: se ele andar, o risco fica onde o golpe aconteceu.
- É uma linha reta de quadradinhos 2×2: o terço central em ciano-branco, as pontas em ciano (`src/Player/Player.cs:635-636`). A altura é achatada a 70% para parecer deitada no chão (`src/Player/Player.cs:634`).
- Comprimento: 34 px nos cortes 1 e 2, 44 px no 3º, 16 px na faca, 10 px na estocada (`src/Player/Player.cs:589`). Inclinação: ±55° em relação ao olhar, alternando entre cortes (`src/Player/Player.cs:584`, `588`).
- Dura 0,16 s e começa a apagar no último terço (`src/Player/Player.cs:589`, `634`).

### Rastros do passo
- A cada quadro de passo, passo lateral ou passo longo fica um rastro (`src/Player/Player.cs:568`, `576`, `592`).
- Cada rastro é um retângulo ciano de 12×40 px (do tamanho do corpo), que começa a 25% de opacidade e some em 0,22 s (`src/Player/Player.cs:604`).

### Sombra
- Dois retângulos sobrepostos (22×4 e 16×6) formando uma "elipse" de pixel, preto esverdeado a 38% (`src/Player/Player.cs:607-608`). Fica sempre no chão, inclusive quando ele voa.

### Área do golpe (modo de assistência)
- Só aparece com a opção "Mostrar áreas de combate" ligada (`src/Game/GamePanels.cs:49`, `94`).
- Desenha a fatia de pizza onde o golpe acerta: raio = alcance da arma (34 px espada, 22 px faca) + folga da região; meia abertura de 75° (corte) ou 20° (estocada) (`src/Player/Player.cs:68-77`).
- Três fases (`src/Player/Player.cs:613-615`): **preparação**, borda tracejada e preenchimento crescendo; **golpe valendo**, preenchimento mais forte e borda grossa de 3 px; **recuperação**, borda tracejada encolhendo.
- Ao contrário do resto, esta área usa arcos suaves do Godot, não retângulos (`src/Game/Assist.cs:37-60`).

---

## 7. O contorno (PixelPainter)

`PixelPainter` é um "caderno de encomendas" (`src/Player/PixelPainter.cs:11-42`):

1. Durante o desenho, cada `Rect` e cada `Line` só **anota** a forma e a cor numa lista (`src/Player/PixelPainter.cs:16-33`). Nada vai à tela ainda.
2. No `Flush` (`src/Player/PixelPainter.cs:36-41`), ele percorre a lista **duas vezes**:
   - 1ª passada: desenha cada forma **1 px maior para todos os lados**, na cor do contorno `#16140f`;
   - 2ª passada: desenha cada forma no tamanho certo, na sua cor.
3. Resultado: onde duas formas se encostam, a cor da segunda passada cobre o contorno da primeira; na beira da figura, sobra 1 px escuro. A figura ganha **um contorno único da silhueta inteira**, sem linhas pretas por dentro.

Detalhes úteis:
- O `Flush` aceita uma cor de tingimento que pinta tudo de uma cor só (usado pelos inimigos para o clarão branco, `src/Combat/FighterArt.cs:113`). O jogador não usa: o ferimento dele é o retângulo vermelho por cima (`src/Player/Player.cs:629`).
- O mesmo `PixelPainter` desenha o jogador, o deitado e o ajoelhado (`src/Player/Player.cs:651`, `668`, `748`), e as figuras do `FighterArt`.

### Relação com `FighterArt`
`src/Combat/FighterArt.cs` é a **mesma função do protótipo** em versão compartilhada, para o discípulo, o Sênior e a estátua (`src/Combat/FighterArt.cs:6-9`). A estrutura de pernas, túnica, cabeça e braços é idêntica à do jogador (compare `src/Combat/FighterArt.cs:77-109` com `src/Player/Player.cs:715-747`), com extras: altura e largura a mais, chapéu, cabelo longo, ombreiras, coque duplo, contorno piscando em vermelho no ataque. O jogador **não usa** o `FighterArt`: tem sua cópia própria em `DrawFighter`, com as poses dele (corte em arco, estocada, Qi, faca).

---

## 8. Relação com o protótipo HTML e com a bíblia de arte

### Protótipo (`prototypes/spiritofmurin/index.html`)

**Mantido igual:** cores (`index.html:1125`), contorno em duas passadas (`index.html:1118-1122`), sombra (`index.html:1131`), vistas frente/costas/perfil, túnica em degraus, cabeça, coque, fita, respiração, arco do corte de ±75° e o risco reto (`index.html:1132-1220`, `1604-1606`).

**Diferente no Godot:**

| Ponto | Protótipo | Godot |
|---|---|---|
| Rastro do passo | a figura inteira repetida, tingida de ciano (`index.html:1561`) | um retângulo ciano 12×40 (`src/Player/Player.cs:604`) |
| Ferimento | figura pisca toda em branco (`index.html:1219`) | retângulo vermelho translúcido (`src/Player/Player.cs:629`) |
| Estocada | preparação com a lâmina em vermelho, depois o golpe (`index.html:1568`) | só a pose do golpe (`src/Player/Player.cs:688-692`) |
| Recuo | pose "cambaleando", corpo balançando (`index.html:1571`) | pose de repouso empurrada (`src/Player/Player.cs:699-703`) |
| Qi | segundo braço estendido com a palma (`index.html:1158`) | quadradinho ciano 3×3 (`src/Player/Player.cs:697`) |
| Risco da estocada | linha de 26 px na direção do golpe (`index.html:1605`) | linha de 10 px atravessada (`src/Player/Player.cs:584-589`) |
| Novidades do Godot | — | faca de pedra, mãos vazias, deitado, ajoelhado, rodopio, área de assistência |

### Bíblia de arte (`design/art/art-bible.md`)

A bíblia descreve uma **arte futura** mais detalhada (46 px, ~4,6 cabeças, barra dividida, gola cruzada, pé à frente; `design/art/art-bible.md:157-223`). Essa direção foi testada com sprites e **o criador escolheu ficar com a aparência do protótipo** (`design/art/art-bible.md:770`; `design/assets/specs/approval-poses-assets.md:7`). As regras da bíblia seguem como referência para o futuro.

- **Seguido pelo código:** contorno externo de 1 px em `#16140f` (`design/art/art-bible.md:177`); olhos de 1 px sem boca; escala única, sem níveis de detalhe.
- **Diferenças registradas (mantidas por decisão):** a túnica `#dce5d4` e o debrum `#2f7f6c` são os do protótipo, não os tons da bíblia (`design/art/art-bible.md:327`, `374-376`); o risco do corte e os rastros usam ciano-branco, que a bíblia reserva ao conhecimento (`design/art/art-bible.md:358`, `361`); sombra única em vez de pixels de contato por pé (`design/art/art-bible.md:181`); lâmina de 1 px em vez de 2 px (`design/art/art-bible.md:182`); figura de 41 px com pés juntos.

---

## 9. Como alterar

### O que é dado e o que é código

- **Dado (sem programar):** `assets/data/PlayerTuning.tres` guarda **tempos e distâncias**: velocidade, tempos dos cortes, passo, alcance, invulnerabilidade, empurrão. Eles mudam **quanto tempo** uma pose dura e **até onde** ele vai, não o desenho.
- **Código:** cores, tamanhos e poses estão escritos dentro de `src/Player/Player.cs`. Mudar o visual exige editar esse arquivo e recompilar.

### Onde mexer

| Quero mudar... | Onde |
|---|---|
| Qualquer cor do jogador | `src/Player/Player.cs:255-257` |
| Cor ou força da sombra | `src/Player/Player.cs:607-608` |
| Cor do contorno | `Outline` em `src/Player/Player.cs:257` (passado ao `Flush` em `748`) |
| Largura da túnica, altura, degraus | `src/Player/Player.cs:719-726` |
| Faixa da cintura | `src/Player/Player.cs:727` |
| Pernas e o passo das pernas | `src/Player/Player.cs:715-717` |
| Cabeça, cabelo, olhos | `src/Player/Player.cs:730-743` |
| Coque e fita | `src/Player/Player.cs:744-746` |
| Espada parada (posição e comprimento) | `src/Player/Player.cs:699-703` |
| Comprimento da faca | `src/Player/Player.cs:680` |
| Arco do corte (75°) e alcance desenhado | `src/Player/Player.cs:683-686` |
| Pose da estocada | `src/Player/Player.cs:688-692` |
| Pose do Qi | `src/Player/Player.cs:693-698` |
| Velocidade da perna ao andar | `src/Player/Player.cs:344` (7 andando, 4 quieto) |
| Respiração | `src/Player/Player.cs:730` |
| Risco do corte (tamanho, cor, duração) | `src/Player/Player.cs:584-589`, `631-637` |
| Rastros do passo | `src/Player/Player.cs:601-605` (duração 0,22 em `568`, `576`, `592`) |
| Clarão de ferimento e pisca-pisca | `src/Player/Player.cs:628-629` (duração 0,3 em `273`) |
| Deitado / ajoelhado | `src/Player/Player.cs:641-669` |
| Tempos dos golpes e do passo | `assets/data/PlayerTuning.tres:7-32` |
| Cor da área de assistência | `src/Game/Assist.cs:34` |

Atenção: o corpo dos inimigos usa a cópia em `src/Combat/FighterArt.cs`. Mudar a túnica do jogador não muda a deles, e vice-versa.

---

## 10. Limitações conhecidas e ideias

### Limitações
- **Visual preso no código:** as cores e medidas não estão em arquivo de dados; qualquer ajuste visual pede recompilar.
- **Duas cópias da mesma figura:** `Player.cs` e `FighterArt.cs` repetem o desenho; uma melhoria precisa ser feita nos dois.
- **Poses sem desenho próprio:** passo, passo lateral, passo longo e recuo reaproveitam a pose parada; só os rastros e o deslocamento os diferenciam.
- **Rodopio sai da grade de pixels:** girada, a figura deixa de ficar alinhada aos pixels (os retângulos são rotacionados), diferente do resto do jogo (`src/Player/Player.cs:622-623`).
- **Diagonais:** não há vista diagonal; elas usam frente ou costas.
- **A área de assistência** usa linhas suaves, não pixel art (`src/Game/Assist.cs:46-60`).
- **A sombra e o clarão vermelho** usam medidas fixas (não acompanham mudanças no tamanho do corpo).

### Ideias (só se o criador quiser; nada disso foi pedido)
- Levar a paleta do jogador para um arquivo `.tres`, como já é feito com os tempos, para trocar cores sem programar.
- Juntar o desenho do jogador ao `FighterArt` com um "visual do jogador", para manter uma cópia só.
- Trazer do protótipo a pose de cambalear no recuo e a figura-fantasma nos rastros.
