# Art bible: trabalho pendente (retomar na próxima sessão)

> Este arquivo guarda rascunhos AINDA NÃO APROVADOS. O art bible oficial é `design/art/art-bible.md`.
> Estado em 2026-11-25: Seções 1 e 2 aprovadas e gravadas. Seção 3 em revisão (o criador pediu
> "Ajustar personagens" e ainda não disse o quê). Seção 4 rascunhada, não apresentada. Seções 5–9 por fazer.
> Para retomar: `/art-bible` (modo retomada preenche só as seções vazias), e trazer este rascunho.

## Pedido original do criador (para as seções 3, 5 e 8)
Guia de pixel art para câmera 3/4, tiles 32×32, quadros de personagem 64×64. Começar pelo espadachim
cultivador e pelo Discípulo da Guarda Fechada: distinguir pela silhueta; reconhecer repouso, guarda e
início de corte pelo corpo, pelos pés e pela posição da espada; o rastro da lâmina repete o arco gravado
na pedra do túmulo da mestra; animações em 4 direções, ponto de apoio dos pés e alinhamento entre quadros;
só ferramentas gratuitas (LibreSprite, Pixelorama, Krita; imagens podem vir do ChatGPT e ser limpas à mão);
indicar como validar a leitura dos dois personagens no tamanho do jogo.

## Decisões já tomadas
- Corte na pedra e rastro: arco geométrico de raio constante, 1–2 px, borda limpa (gravado na Seção 1; `game-concept.md` atualizado).
- Rastro na cor do qi do jogador; acende em ciano-branco só ao coincidir com uma marca conhecida.
- Escopo: guia completo (9 seções). Referências: só as do conceito.

---

## RASCUNHO Seção 3: Shape Language (em revisão: "ajustar personagens", detalhe pendente)

Cada princípio nasce da regra visual "Tudo o que importa se lê pela linha antes da cor".

**Princípio de forma 1: três famílias, três significados.**
- Orgânico irregular (rocha, terreno, árvores, água, névoa): contorno sem trechos retos > 3 px, espessura variável, grupos ímpares. "Isto não ensina nada."
- Geométrico reto (arquitetura, cantaria, degraus, pilares): mão humana, permanência, memória.
- Arco de raio constante / arco-linha (marcas gravadas, rastros, ícones de técnica): 1–2 px, borda limpa, degraus espelhados. "Isto se aprende."
- Pilares 3 e 1. Teste: se um elemento de cenário pode ser confundido com uma marca a 1× em escala de cinza, muda de família. Descartado: cenário uniforme e anguloso (low-poly facetado).

**Princípio 2: o arco-linha é a forma-herói e é reservado.** Só em marcas gravadas, rastros e ícones de técnica. Qualquer outra curva se diferencia por curvatura variável (ponta de telhado), massa preenchida ≥ 4 px (ponte, fornalha) ou traço partido/espessura variável (água, pincel da UI). Teste: "este traço poderia ser idêntico a um rastro?" Se sim e não é técnica, muda.

**Princípio 3: personagens em escala de jogo — três linhas, massas grandes.** ~42–48 px em quadros 64×64, 640×360 nativo, escala inteira, 4 direções (3 + 1 espelhada).
- Proporção ~4 cabeças (cabeça ~10–11 px).
- Silhueta: entre repouso/guarda/início de corte o contorno muda ≥ 4 px em ao menos uma direção.
- Pés: largura da base varia ≥ 4 px entre estados; espaço entre pés ≥ 2 px quando abertos; nenhum arquétipo esconde os pés.
- Espada: 1–2 px de espessura, ≥ 18 px de comprimento, contraste de valor com a veste atrás.
- Contorno escuro completo de 1 px (`#16140f`) em todos os atores vivos; ambiente sem contorno.
- O traço de arquétipo sobrevive ao espelhamento e à vista de costas.
- Teste: a 1×, em escala de cinza, com qi ativo, reconhecer arquétipo e estado em 1–2 quadros; extra: reduzido a 50% ainda separa os arquétipos.
- Descartado: proporções realistas de 6–7 cabeças (pés e lâmina viram 1 px).

**Princípio 4: aberto contra fechado (espaço negativo).**
| Arquétipo | Forma | Traço único | Medidas | Comunica |
|---|---|---|---|---|
| Espadachim-cultivador (jogador) | Vertical, aberta, assimétrica | Lâmina fora do eixo, espaço negativo visível entre lâmina e tronco; fita/coque solto de 2–3 px que acompanha o movimento | ~46 px; ombros ~14 px; lâmina ~20 px | Liberdade, potencial (Pilar 2) |
| Discípulo da Guarda Fechada | Compacta, base larga (trapézio), topo achatado | Chapéu de palha em cone largo e baixo, mais largo que os ombros; guarda com a lâmina atravessada na frente do corpo; de costas a lâmina é mais longa que o tronco (aparece dos dois lados) | ~42–44 px com chapéu; chapéu ~18–20 px; lâmina ~18–20 px | Parede, anonimato |
| Espadachim sênior | Triângulo invertido, massa no alto | Ombros largos de manto pesado; lâmina longa; coque alto, sem chapéu; manto aberto na frente ou até a canela (pés visíveis) | ~48 px; ombros ~18 px; lâmina ~26 px | Autoridade, alcance |
| Estátua da mestra | Monólito fundido ao pedestal, facetado | Sem espaço sob os pés; regras de ambiente (sem contorno de ator); arco gravado na base carrega o ciano | peça de cenário ~2×3 tiles (64×96) | Permanência, memória (Pilar 3) |
Questão aberta: a estátua é a mesma mestra do túmulo (rascunho supõe que sim; o protótipo já a chama "Estátua da Mestra").

**Princípio 5: arquitetura pesada e retilínea; natureza vertical e em camadas.** Arquitetura horizontal e pesada, pilares ≥ 4 px, telhados como massas grossas, só pontas de telhado curvas (curvatura variável e preenchidas). Bambu vertical 2–4 px, nós a cada 6–10 px em intervalos irregulares, inclinação ±1 px/16 px. Pinheiros em camadas serrilhadas nas bordas (moldura). Rocha irregular. Ponte em arco = volume de pedra preenchido ≥ 4 px. Água em traços horizontais curtos e quebrados.

**Princípio 6: interface — pergaminho, pincel, arco a nanquim, tecla, selo.** Rolo de pergaminho (painéis), traço de pincel de espessura variável (divisórias, seleção, títulos), arco a nanquim (ícone de técnica: o livro repete a pedra), tecla desenhada (sequências de comando; a única forma "moderna", porque precisa de leitura precisa — conflito arte × legibilidade decidido pela legibilidade, levar à Seção 7), selo vermelho (reconhecimento).

**Princípio 7: hierarquia de leitura.** 1 arco-linha; 2 silhuetas de atores em guarda; 3 lâminas; 4 objetos interativos (forma fechada, contorno parcial); 5 ambiente (sem contorno, contraste baixo). O contorno separa o que age do que é cenário.

---

## RASCUNHO Seção 4: Color System (não apresentado ao criador ainda)

**Colisões encontradas no protótipo:** (1) qi jade `#3fae9e`/`#a6f2e2` está na mesma faixa de matiz do ciano-branco (171–177°) → o flash do rastro não se vê; (2) água `#2c5a5e` ~185° é família do ciano; (3) veste do jogador `#dce5d4` L\* ~90, vizinha do ciano-branco → proposta `#d6d2bf` (L\* ~84); (4) veste do sênior `#6e3027` é família do vermelho-ocre; (5) veste do discípulo `#7f846c` tem o valor da rocha `#8a877a` → contorno + chapéu claro (L\* 65–70).

**4.1 Sete famílias:** terra-musgo (o mundo: chão `#4c5a3e`, caminho `#86704f`, rocha `#8a877a`, água, túmulo `#3a3a3f`); ciano-branco `#d4fbf6`/`#6fd8d2` (só conhecimento); vermelho-ocre `#c8502a`/`#e8834c` (intenção hostil, só lampejos ligados à pose); âmbar `#e2a640`/`#ffd27a` (alquimia e tesouro, objetos parados); jade do qi (proposta `#24603f`/`#3f9e6a`/`#9be3b0`, muda com o caminho); tinta (`#16140f` contorno, `#2a2118` UI); pergaminho `#d8c7a0` e selo `#a8382a` (só UI). Descartado: paleta por elemento (fogo vermelho etc.).

**4.2 Faixas:** mundo L\* ~20–60, saturação ≤ 45%; sinal L\* ~50–88, saturação ≥ 60%; conhecimento L\* ≥ 90 frio (só ciano-branco). Ninguém além do ciano-branco passa de L\* ~88.

**4.3 Exclusividade:** ciano só em marcas/pegadas/inscrições e no instante do rastro coincidente (proibido em água, gelo, luar, UI comum, brilhos, partículas); ocre só no inimigo durante a intenção (nunca cenário, veste, barra de vida); âmbar em objetos parados, tinge o ambiente só na fornalha; qi só no que o jogador emite; selo só na UI, em formas grandes (contraste ~3,8:1).

**4.4 Qi:** base jade verde ~140–150° (rampa de 3: `#24603f` L\*36, `#3f9e6a` L\*58, `#9be3b0` L\*85). Zonas proibidas: ciano 160–200°, qualquer L\* > 88, ocre 0–30°/345–360°, âmbar 30–60°. Faixas permitidas por caminho: jade 100–150°, azul-tempestade 210–240° (claro < L\* 75), violeta/ameixa 260–320°. Muda só o matiz; os valores ficam fixos; a forma do rastro nunca muda; elementos (ex.: fogo) se expressam pela forma, nunca em ocre; a rampa ocupa 3 índices fixos da paleta (troca de paleta, não sprites novos).

**4.5 Temperatura por área:** trilha (pálida morna, sombras azul-acinzentadas, névoa L\* < 70); vale (verde-morno filtrado, sombras verde-oliva, nunca verde-azulado); ponte (morna lateral, teto morno = caminho claro, nunca `#e2a640`; brilho da água cinza-azulado ≤ 30% sat.); túmulo (`#3a3a3f`, sombras quase-pretas mornas, feixe pergaminho translúcido); duelos herdam a área com fundo −1 passo (sênior −1 de temperatura); fornalha âmbar dominante. Sombras nunca preto puro: deslocam para o frio ao ar livre e para violeta-cinza no túmulo; brilhos deslocam para o morno.

**4.6 UI:** pergaminho `#d8c7a0`, claro ~`#e6d9b8`, sombra ~`#b5a078`, tinta `#2a2118` (~9,5:1), aguada ~`#6b5a45` (~4,6:1), selo `#a8382a`. A UI é morna e clara, o mundo verde-terroso médio-escuro. Sem ciano e sem ocre nos menus. HUD: barra de qi na rampa do qi; barra de vida em tinta/pergaminho ou neutro, nunca ocre.

**4.7 Daltonismo (par → reforço):** ocre × qi (proto/deuter) → telegrafia é mudança de pose, qi atrás do contorno, lampejo percorre a borda da lâmina, som de "aço que se prepara"; âmbar × ocre → âmbar em objetos parados com brilho em losango, ocre em atores como traço de borda, sons distintos; ciano × qi (tritan) → rastro sobrepõe a marca, salto de valor 85→96 e espessura 1→2 px, som de reconhecimento; ciano × água → água sem linha limpa, brilho dessaturado L\* < 70; ciano × veste do jogador → veste `#d6d2bf`; ocre × veste do sênior → pose + lampejo 2 passos acima + som; selo × ocre → contextos separados, quadrado carimbado; veste do discípulo × rocha → contorno + chapéu claro. Teste a cada sprint: simulador de daltonismo (Krita ou ferramenta gratuita) e escala de cinza.

**4.8 Orçamento:** paleta-mestra indexada de 48 cores (44 + 4 reserva; distribuição: linha/neutros 4, musgo 5, terra 4, pedra 4, água 3, túmulo 3, pele 3, veste jogador 3, discípulo 2, sênior 2, ciano 2, ocre 2, âmbar 2(+1), qi 3, UI 5 = 47). Rampas de 3–5 passos, ~10–12 L\* entre passos, matiz deslocado (sombras 10–20° para o frio, brilhos 10–15° para o morno), saturação no pico no tom médio; rampas semânticas de 2 passos (qi 3). Pontilhado proibido em personagens; ordenado só em grandes gradientes de ambiente. Translucidez só na camada de efeitos, em tempo de execução. Arquivo `.gpl` (LibreSprite, Pixelorama e Krita leem) + tira PNG de amostras; trocas de qi em `.gpl` de 3 índices (ex.: `pal_master_base_48.gpl`, `pal_qi_jade_3.gpl`).

**Decisões pendentes da Seção 4 (perguntar ao criador):**
1. Cor base do qi: jade verde ~145° (recomendado) / manter jade-ciano do protótipo / azul profundo.
2. Veste do sênior: índigo-ardósia ~`#3d4160` (recomendado) / manter vermelho-escuro `#6e3027`.
3. Ciano nos menus: nenhum (recomendado) / só em fragmento ainda não estudado.
4. Orçamento: 48 cores (recomendado) / 32 / 64.
Questões abertas: inimigos usam qi de cor própria? (rascunho: cor de intenção ou neutro escuro); elementos de qi (fogo etc.)?; duração exata da revelação (1–1,5 s sugerido).

## Depois da Seção 4
- Seções 5–6: rascunhadas por mim sob o guia do diretor de arte (`.claude/skills/art-bible/references/lens-art-director.md`). A Seção 5 traz o detalhamento pedido: repouso / guarda / início de corte do espadachim e do discípulo (corpo, pés, espada), 4 direções, ponto de apoio dos pés, alinhamento entre quadros.
- Seção 7: visão de arte + visão de UX (lens-ux-designer.md).
- Seção 8: visão de arte + visão técnica (lens-technical-artist.md); ferramentas gratuitas; validação da leitura no tamanho do jogo.
- Seção 9: diretor de arte (referências).
- Fase 5: gate AD-ART-BIBLE feito na própria conversa.
