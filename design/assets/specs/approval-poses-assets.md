# Asset Specs — Character: poses de aprovação (protagonista e Discípulo da Guarda Fechada)

> **Source**: `design/art/art-bible.md` — Seções 3 (3.1, 3.2, 4.1, 4.2, 4.5), 4.4, 5 (5.4–5.7), 8, 9.9. Não há ficha de personagem em `design/narrative/`; o art bible é a fonte.
> **Art Bible**: design/art/art-bible.md
> **Referência do criador**: `user-docs/assets/protagonista/image.png` (só traje e cores)
> **Generated**: 2026-11-25 · revisão completa: `art-director` + `technical-artist`
> **Status**: 2 assets specced / 2 produzidos / **rejeitados pelo criador depois de jogar (2026-11-25)**: fica a aparência original do protótipo. PNGs guardados em `assets/art/historico/2026-11-25-experimento-sprites/`.
> **Prancha das escolhas**: `user-docs/relatorios/2026-11-25-asset-spec-poses.html`

## Decisões do criador nesta spec (2026-11-25)

- **Espada do protagonista em repouso:** opção A, mão na altura da faixa. As medidas antigas (cabo a 20–22 px, lâmina de 22 px a 1:2, ponta 3–5 px acima do chão) não cabiam juntas.
- **Arma do discípulo:** espada de **uma mão**, com o braço livre deitado no peito, atrás da lâmina.
- **Cores do discípulo:** corpo `#555d5e` / `#3f4647`, palha `#b0a382` / `#c4b893` / `#8f8466`.
- **Guarda das espadas:** bronze escuro e opaco, nunca latão dourado (âmbar, art bible 4.3).

## Padrões técnicos adotados (propostos pelo technical-artist, sem objeção do criador)

- **Sombra:** o sprite leva só **1 px de contato escuro sob cada pé**, da paleta. A elipse translúcida é desenhada pelo jogo em tempo de execução (art bible 4.8: translucidez só em execução).
- **Linha 59 = contorno da sola** do pé de apoio. Coincide com o `y` que o protótipo passa a `drawFighter`.
- **Nome:** sufixo de quadro `_00` em quadros soltos.
- **Exportação:** PNG sem perda, alpha binário (0 ou 255), de preferência indexado com `pal_master_64`; RGBA de 32 bits aceito com alpha binário. Importação indexada no motor: **não confirmado — rever no /setup-engine**.
- **Paleta:** `assets/palettes/pal_master_64.gpl` (61 cores + 3 de reserva, aprovada em 2026-11-25). Carregar no LibreSprite, Pixelorama ou Krita antes da limpeza.

---

## ASSET-001 — Espadachim-cultivador (protagonista), repouso, direção S

| Field | Value |
|-------|-------|
| Category | Sprite / 2D Art |
| Dimensions | 64×64 px, 1 quadro, exportado a 1×; figura de 46 px (+ coque ≤ 3 px, máx. 49), linhas 11–59; linhas 60–63 livres para a sombra de contato; nada transborda (sem camada `fx_`) |
| Format | PNG sem perda, alpha binário, indexado (`pal_master_64`) ou RGBA |
| Naming | `char_swordsman_idle_s_00.png` · fonte `assets/source/char_swordsman_idle_s.ase` (ou `.pxo` / `.kra`) com guias na linha 59 e na coluna 32 |
| Pivot | (32, 59): contorno da sola do pé de trás; o pé de apoio fica sobre a coluna 32 (±1 px). No protótipo, desenhar em `(round(x) − 32, round(y) − 59)` |
| Polycount | — (2D) |
| Texture Res | 64×64 (art bible §8.4) |

**Visual Description:**
Figura esguia de 46 px, cabeça de 10 px, tronco girado 20–30° para a direita da tela com o ombro de perto 1 px mais baixo. O pé de trás fica sob o quadril, com a cabeça sobre ele; o pé da frente vai 3 px para o lado da espada e 2–3 px mais abaixo, joelho flexionado 1 px, base de 9–10 px. Túnica marfim de gola cruzada (diagonal de 1–2 px), faixa de 3 px em jade gasto, barra dividida mostrando a calça carvão. A mão da espada fica na altura da faixa, com o cotovelo 1–2 px para fora do tronco; a lâmina de 22 px desce para baixo e para fora a 1:2 (~27°), com a ponta 5 px acima do chão e um triângulo de espaço vazio de ≥ 3 px até a perna. A mão livre repousa na faixa.

**Art Bible Anchors:**
- §3 Shape Language: 3.1 (proporção ~4,6 cabeças), 3.2 (contorno `#16140f` só por fora, linhas internas no tom da veste, lâmina de 2 px em inclinação limpa, nunca vertical em repouso, ponta e cabo diferentes), 4.1 (corpo, roupa, braços, espada em repouso com a mão na faixa), 4.5 (testes de aprovação)
- §4 Color System: 4.2 (faixas de valor), 4.4 (túnica `#d6d2bf` / `#b3ad97` / `#8a846f`, faixa `#4f7a64` / `#3a5a4b`, calça com L\* 24–28, aço com L\* ~80), 4.3 (sem âmbar: guarda em bronze escuro)
- §5: 5.4 (estado de repouso), 5.5 (origem e pé de apoio travado), 5.6 (detalhe), 5.7 (arma de uma mão: mão livre visível)
- §8.2 e §9.9: a imagem gerada é só referência; traduções obrigatórias (base estreita, peso atrás, ~27°, jade gasto)
- §9.1: o prompt descreve a postura em palavras e não nomeia nenhuma obra

**Generation Prompt** (ChatGPT, em português; revisão do prompt do criador em `user-docs/assets/protagonista/promp.md`; anexar a imagem de referência ao gerar):

> Crie um único sprite de corpo inteiro para o protagonista de SpiritOfMurin, um RPG 2D de cultivação de artes marciais com câmera de cima em 3/4. Use a imagem de referência **apenas para o traje e as cores**; a postura deve ser a descrita abaixo, não a da imagem. Entregue uma única pose de repouso, sobre um **fundo chapado cinza-médio neutro (#808080), sem degradê, sem textura e sem sombra no chão**, em formato quadrado, com o corpo inteiro, o coque, a fita e a ponta da espada inteiramente dentro do quadro e com margem em volta.
>
> É um jovem adulto de porte esguio e atlético. O cabelo preto está preso em um coque alto e pequeno, com uma fita estreita que cai curta para trás do ombro, quase parada, sem esvoaçar. Veste túnica marfim de gola cruzada (brilho #d6d2bf, tom #b3ad97, sombra #8a846f), com mangas justas, não largas, e punhos escuros. Na cintura, uma faixa estreita em verde-jade **apagado e acinzentado** (#4f7a64, sombra #3a5a4b), nunca verde vivo. A calça é carvão bem escuro (#3b3a3e) e as botas são simples e escuras. A túnica vai até a altura do joelho e se abre dos lados logo abaixo da faixa, em dois painéis de comprimentos um pouco diferentes, deixando a calça visível entre eles e as duas canelas e os dois pés à mostra. A barra nunca é mais larga que os ombros.
>
> A câmera vê levemente o topo da cabeça e dos ombros. O personagem está de frente para a câmera, com o tronco girado em três quartos para a direita da tela e o ombro mais próximo um pouco mais baixo. **Os pés ficam próximos, numa postura natural e estreita, nada de base aberta de luta.** O pé de trás, que sustenta o peso, fica bem embaixo do centro do quadril, e a cabeça fica alinhada sobre esse pé. O pé da frente fica um pouco para a direita da tela e um pouco mais perto da câmera, com o joelho levemente dobrado. Deve haver um pequeno vão entre os pés. É a postura de quem está pronto, mas sem pressa.
>
> Ele segura com a mão do lado direito da tela uma espada chinesa reta e estreita, com a empunhadura escura e uma guarda transversal pequena em bronze escuro e opaco (sem brilho dourado). A mão fica na altura da faixa, ao lado do quadril, com o cotovelo um pouco afastado do corpo, deixando um vão visível entre o braço e o tronco. A lâmina desce **para baixo e para fora, inclinada cerca de 27 graus em relação à vertical, como o ponteiro de um relógio marcando 5 horas**: nem vertical, nem quase horizontal. A ponta para **acima do chão, sem tocá-lo**, e fica fora da silhueta da perna, com um triângulo de espaço vazio bem visível entre a lâmina e a perna. A lâmina é de aço claro (cerca de #c4c8c6), com um fio mais claro e uma ponta afinada. A mão livre repousa sobre a faixa, do lado esquerdo da tela.
>
> Desenhe para uma grade lógica de 64×64 pixels, como se cada pixel fosse um bloco quadrado uniforme, com o personagem medindo cerca de 46 pixels da sola ao topo da cabeça (até 49 com o coque) e a cabeça medindo cerca de 10 pixels. Use pixel art de bordas duras, com contorno escuro de 1 pixel só por fora da figura, cores chapadas e poucas massas de luz e sombra, com a luz vindo do alto à esquerda. Simplifique primeiro as dobras e o rosto (olhos de um pixel, sem boca detalhada). Preserve a gola cruzada em diagonal, a faixa, os dois pés separados e a linha contínua da espada. A pose precisa ser reconhecível em escala de cinza e em tamanho pequeno.
>
> Evite: anti-aliasing e bordas suavizadas; degradês e pontilhado; brilho, aura ou partículas; linhas de movimento; texto ou moldura; mais de uma pose ou vista; pés cortados; base larga de guarda; lâmina vertical, quase horizontal ou tocando o chão; espada atravessando a perna; mangas largas ou fita longa ao vento; verde saturado; dourado brilhante; fundo xadrez de transparência; sombra projetada; qualquer referência a obras ou personagens existentes.

*O que mudou em relação ao prompt do criador:* referência só para traje e cores; postura estreita com peso atrás; lâmina a ~27° com a ponta acima do chão; mão na altura da faixa; faixa e fita em jade gasto com hex; guarda de latão para bronze escuro; fundo transparente para cinza chapado (o "transparente" do gerador costuma vir com xadrez pintado ou bordas semitransparentes); cabeça de 10 px; mangas justas; fita curta; luz do alto à esquerda; lista "Evite:".

**Limpeza à mão (checklist):**
- [ ] Gerar em quadrado; reduzir por vizinho mais próximo (1024 → 64 = fator 16); redesenhar por cima — não confiar na redução.
- [ ] Guias na linha 59 e na coluna 32; origem (32, 59).
- [ ] Figura nas linhas 14–59 (46 px); coque até a linha 11; cabeça 10 px; pescoço 1–2 px; pernas 19–20 px; tronco 12–13 px.
- [ ] Ombros com 12 px visíveis, o de perto 1 px mais baixo; gola em diagonal de 1–2 px até a faixa; faixa de 3 px.
- [ ] Barra termina 8–10 px acima da sola; aberturas no máximo 4 px abaixo da faixa; calça visível ≥ 2 px; painéis com 1–2 px de diferença; largura máx. 13–14 px.
- [ ] Base de 9–10 px; ≥ 2 px entre os pés; pé da frente 3 px para o lado e 2–3 px abaixo; cabeça sobre o pé de trás (±1 px).
- [ ] Mangas 3–4 px; braço 13–14 px; mão 3×2; vão braço–tronco ≥ 1×5 px; mão livre com 1 px de separação no tom escuro da túnica se tocar o corpo.
- [ ] Mão da espada na faixa (guarda ~25–26 px acima da sola); lâmina de 22 px (núcleo 1 px + fio claro 1 px), degrau 1:2 limpo; guarda 4 px em bronze escuro; cabo 5–6 px escuro; ponta com o pixel mais claro; ponta 3–5 px acima da linha 59; triângulo de espaço ≥ 3 px até a perna.
- [ ] Paleta `pal_master_64.gpl`, sem pontilhado; contorno `#16140f` só por fora; nada acima de L\* 88.
- [ ] Só 1 px de contato escuro sob cada pé, separados (a elipse é do jogo).
- [ ] Remover grupos menores que 2×2 que não carreguem silhueta, pés ou lâmina (§5.6).
- [ ] Registrar procedência: ferramenta, data, prompt (§8.2).
- [ ] Os cinco testes do art bible §3, 4.5: cor e cinza; preto chapado ao lado do discípulo; colmo atrás da lâmina; 50%; relance de 3 s ("qual mão segura a espada? qual pé está à frente?"). Falha corrige a pose, nunca a cor.

**Status:** Needed

---

## ASSET-002 — Discípulo da Guarda Fechada, guarda, direção S

| Field | Value |
|-------|-------|
| Category | Sprite / 2D Art |
| Dimensions | 64×64 px, 1 quadro; figura de 42–43 px com chapéu, linhas ~17–59; aba de 21 px (~x 22–42); nada transborda (sem camada `fx_`) |
| Format | PNG sem perda, alpha binário, indexado (`pal_master_64`) ou RGBA |
| Naming | `char_disciple_guard_s_00.png` · fonte `assets/source/char_disciple_guard_s.ase` (ou `.pxo` / `.kra`) |
| Pivot | (32, 59), no chão entre os dois pés (base 14–16 px, ≥ 5 px entre os pés) |
| Polycount | — (2D) |
| Texture Res | 64×64 (art bible §8.4) |

**Visual Description:**
Silhueta em ampulheta de 42–43 px, joelhos flexionados: chapéu de palha claro de aba larga (21 px), cone baixo, cobrindo os olhos e deixando 2–3 px de queixo, com a aba 2 px mais baixa do lado esquerdo da tela, para onde ele encara. Túnica escura de ardósia até o meio da coxa, cotovelos colados ao tronco, base de 14–16 px com os joelhos abertos, ≥ 5 px de chão entre os pés e perneiras claras nas canelas. Espada reta de uma mão: o cabo fica no quadril do lado direito da tela, com guarda e pomo saindo 2 px do contorno, e a lâmina de 20 px sobe a 45° cruzando o peito, com a ponta 3–4 px além do ombro esquerdo, abaixo da aba; o antebraço livre fica deitado no peito, atrás da lâmina e paralelo a ela.

**Art Bible Anchors:**
- §3 Shape Language: 3.1 (cabeça de 10 px, mesma escala humana), 3.2 (contorno, lâmina de 2 px a 1:1, ponta e cabo diferentes, pés nunca escondidos), 4.2 (todas as medidas, arma de uma mão), 4.5 (testes)
- §4 Color System: 4.4 (corpo `#555d5e` / `#3f4647`; palha `#b0a382` / `#c4b893` / `#8f8466`; esquema de valor inverso ao do protagonista; contorno dos dois lados da lâmina sobre o corpo), 4.3 (sem ocre na guarda: o ocre é só do início de corte)
- §5: 5.4 (estado de guarda), 5.5, 5.6 (chapéu em 2–3 tons, sem textura de palha), 5.7 (arma de uma mão)
- §8.2 e §9.1

**Generation Prompt** (ChatGPT, em português):

> Crie um único sprite de corpo inteiro de um inimigo comum de um RPG 2D de cultivação de artes marciais, com câmera de cima em 3/4: um discípulo anônimo de uma seita, em postura de guarda fechada. Entregue uma única pose, sobre um **fundo chapado cinza-médio neutro (#808080), sem degradê, sem textura e sem sombra no chão**, em formato quadrado, com o corpo inteiro, as duas abas do chapéu, a ponta da espada e o cabo inteiramente dentro do quadro e com margem em volta.
>
> A forma geral é uma ampulheta: largo em cima, estreito na cintura, largo embaixo. Ele usa um chapéu de palha cônico e baixo, de aba muito larga, bem mais larga que os ombros, pintado em dois ou três tons chapados de palha apagada (cerca de #b0a382), sem textura de trançado. A aba cobre os olhos e deixa só o queixo à mostra, e está inclinada, um pouco mais baixa do lado esquerdo da tela, para onde ele olha. O corpo veste uma túnica escura de ardósia acinzentada (cerca de #555d5e, sombra #3f4647), justa, curta, terminando no meio da coxa. Nas canelas, perneiras enfaixadas claras, da mesma cor da palha do chapéu, e pés escuros.
>
> A câmera vê levemente o topo do chapéu e dos ombros. Ele está de frente para a câmera, um pouco girado para a esquerda da tela. **Os pés estão bem afastados, mais largos que o tronco, com os joelhos dobrados e abertos para fora**, cada pé firmemente plantado, com um triângulo de chão visível entre as pernas. O peso está igual nos dois pés e baixo. **Os cotovelos ficam colados ao corpo**, sem nenhum vão entre braço e tronco, formando uma cintura compacta.
>
> Ele empunha uma espada reta de uma mão. A mão da espada segura o cabo junto ao quadril, do lado direito da tela, com a guarda e o pomo saindo um pouco para fora do quadril. **A lâmina sobe em diagonal exata de 45 graus atravessando o peito na frente do corpo**, como o ponteiro de um relógio marcando 10h30, e a ponta sai um pouco para fora do ombro esquerdo da tela, abaixo da aba do chapéu. O antebraço livre fica deitado sobre o peito, paralelo à lâmina e por trás dela, com o cotovelo colado ao corpo. A lâmina é reta, estreita e de aço claro (cerca de #c4c8c6), a linha mais clara do personagem atravessando a parte mais escura, com ponta afinada e clara, guarda transversal pequena e empunhadura escura.
>
> Desenhe para uma grade lógica de 64×64 pixels, como se cada pixel fosse um bloco quadrado uniforme, com o personagem medindo cerca de 42 pixels da sola ao topo do chapéu, a cabeça com cerca de 10 pixels e a aba com cerca de 21 pixels de largura. Use pixel art de bordas duras, com contorno escuro de 1 pixel só por fora da figura, cores chapadas e poucas massas de luz e sombra, com a luz vindo do alto à esquerda. Sem rosto detalhado. A pose precisa ser reconhecível em escala de cinza e em tamanho pequeno.
>
> Evite: anti-aliasing e bordas suavizadas; degradês e pontilhado; brilho, aura ou partículas; linhas de movimento; texto ou moldura; mais de uma pose ou vista; pés cortados; túnica ou manto longo escondendo os pés ou as pernas; pés juntos; espada vertical diante do rosto; espada horizontal; espada atrás do corpo; duas mãos no cabo; braços afastados do tronco; chapéu pequeno ou estreito; olhos visíveis; textura de palha trançada; vermelho ou dourado; fundo xadrez de transparência; sombra projetada; qualquer referência a obras ou personagens existentes.

**Limpeza à mão (checklist):**
- [ ] Redução por vizinho mais próximo (fator inteiro) e redesenho; guias na linha 59 e na coluna 32.
- [ ] Figura nas linhas ~17–59 (42–43 px com chapéu); cabeça 10 px; queixo com 2–3 px à mostra.
- [ ] Aba de 21 px (≥ 7 px além dos ombros, somando os lados); cone de 5 px; aba de 1–2 px de espessura; lado esquerdo da tela 2 px mais baixo; 2–3 tons, sem textura.
- [ ] Ombros de 14 px; contorno dos braços a ±1 px do contorno do tronco; cintura em bloco de ~14 px; antebraço livre no peito no tom escuro da veste, sem `#16140f` por dentro.
- [ ] Base de 14–16 px; ≥ 5 px entre os pés; túnica até o meio da coxa; perneiras claras visíveis; pés escuros 4×2.
- [ ] Lâmina de 20 px a 1:1 exato (~14×14; núcleo 1 px + fio claro 1 px); ponta 3–4 px fora do contorno do ombro e ~5 px abaixo da aba; guarda e pomo 2 px fora do quadril; contorno `#16140f` dos dois lados onde a lâmina passa sobre o corpo; ponta clara e afinada contra guarda de 4 px e cabo escuro.
- [ ] Corpo L\* ~38–42; chapéu e perneiras L\* 65–70; paleta mestra; sem ocre.
- [ ] Só 1 px de contato escuro sob cada pé (a elipse é do jogo).
- [ ] Procedência registrada (§8.2).
- [ ] Os cinco testes do art bible §3, 4.5: cor e cinza; preto chapado ao lado do espadachim; colmo atrás da lâmina; 50%; relance de 3 s ("para onde aponta a lâmina? onde está cada pé?"). Falha corrige a pose, nunca a cor.

**Status:** Needed

---

## Antes/depois no protótipo (aprovação do criador, antes de qualquer animação)

**O que fica idêntico:** o mesmo recorte do bambuzal, a mesma câmera e o mesmo zoom inteiro (1× nativo), cada ator na mesma célula de chão com os pés na mesma linha de base, o mesmo colmo atrás de cada lâmina, o mesmo estado de qi e luz. Jogador à esquerda, discípulo à direita, ambos na direção S, parados (sem `windup`, flash ou alvo travado).

**Diferenças esperadas (não são erro de escala):** o jogador fica +3 a +6 px mais alto que o procedural (43 px → 46–49); o discípulo fica −1 a −2 px (44 → 42–43) e mais largo na base (~12 → 14–16 px). Vêm da proporção decidida no art bible §3.1.

**Plano técnico (proposta do technical-artist, a implementar depois que os sprites existirem):**
1. Interruptor por URL `?sprites=1` (não é valor de jogo, então fica fora do `config.js`; precedente: `?render=cheap`).
2. Um único ponto de troca no topo de `drawFighter`: se o modo sprite estiver ligado, `(kind, pose)` for `('player','idle')` ou `('disciple','guard')`, a vista for `down` e a imagem já tiver carregado, desenhar a imagem em `(round(x) − 32, round(y) − 59)`; caso contrário, procedural. Câmera, mundo, ordem de desenho e HUD não mudam.
3. PNGs copiados para `prototypes/spiritofmurin/assets/`, com entrada em `assets/provenance.yaml` (origem IA registrada); `assets/` da raiz é o original, a cópia do protótipo é derivada e conferida por hash.
4. Captura determinística: mesma seed, mesmo tile, `face = π/2` para os dois, câmera fixada no alvo, mesmo número de passos; capturar o canvas a 640×360. Em `file://`, o canvas com imagem local não exporta por `toDataURL`: usar captura de página ou servidor local.
5. Evidência em `production/qa/evidence/`: `2026-11-25_asset-001-002_bambuzal_antes_1x.png`, `…_depois_1x.png` (e recortes ampliados 4× opcionais), com uma nota de seed, tile, passos e URL.

## Resultado da produção (2026-11-25)

- **Decisão do criador depois de jogar:** preferiu a aparência anterior do protagonista e dos inimigos. O interruptor `?sprites=1` e o ponto `?stage=bambuzal` foram retirados do protótipo; um link antigo com `?sprites=1` mostra a aparência original.
- Originais do ChatGPT intactos em `assets/source/raw/` (mesmo nome dos PNGs finais). Limpeza por `tools/asset-pipeline/clean_chatgpt_sprite.py` (Python + Pillow); máscara, recortes sobre claro e escuro e registro em `assets/source/work/`. Conferência por `tools/asset-pipeline/validate_sprite.py` (`assets/source/work/validation.txt`).
- Os dois PNGs têm 64×64, alpha binário e só cores da paleta mestra; as duas lâminas são uma peça contínua.
- **ASSET-001:** 49 px (linhas 11–59), pé de apoio nas colunas 31–33 sobre a linha 59, pé da frente 2 px abaixo. Lâmina redesenhada a 1:2 com 16 px e ponta 4 px acima da sola. *Desvio:* a spec pede 22 px com a mão mais alta; a mão da imagem gerada é mais baixa.
- **ASSET-002:** 43 px (linhas 17–59), coluna 32 vazia na linha 59 (chão entre os pés). Lâmina redesenhada a 1:1 com 12 px. *Desvios:* aba de ~30 px (spec 21) e base de 29 px (spec 14–16), vindos da imagem gerada. No modo sprite, a sombra do discípulo foi alargada para 34 px.
- Antes/depois: `production/qa/evidence/2026-11-25_asset-001-002_bambuzal_{antes,depois}_1x.png` e recortes 4×. Prancha: `user-docs/relatorios/2026-11-25-antes-depois-bambuzal.html`. No protótipo: `?sprites=1` (troca) e `?stage=bambuzal` (ponto fixo da comparação).

## Pontos em aberto

- O cenário do protótipo ainda não usa a paleta-mestra: o "depois" mostra atores na paleta sobre um mundo fora dela.
- ~~Subpasta dos PNGs exportados~~ resolvido: `assets/art/characters/` (art bible 8.12). Depois da decisão, os dois PNGs foram movidos para `assets/art/historico/2026-11-25-experimento-sprites/`.
- Tudo que depende do motor fica para o `/setup-engine` (art bible §8.10).
