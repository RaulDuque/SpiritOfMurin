# Art Bible: SpiritOfMurin

> **Status**: Draft
> **Owned By**: art-director
> **Last Updated**: 2026-11-25
> **Art Director Sign-Off (AD-ART-BIBLE)**: REVISED 2026-11-25 — CONCERNS em Seções 1, 2 e 4.6/4.8 corrigidos (ver `user-docs/relatorios/2026-11-25-ad-art-bible-revisao.md`)

`/art-bible` writes these nine sections in this order and replaces each
section's `[To be designed]` line as that section is approved. A section still
holding its placeholder is incomplete; a later run fills only those.

## 1. Visual Identity Statement

Direção aprovada: **"Todo Golpe Deixa Marca"** (âncora do `game-concept.md`).

**Regra visual:** **Tudo o que importa se lê pela linha antes da cor: a pedra ensina o golpe, e o golpe repete a pedra.**

*Como usar:* diante de qualquer dúvida visual, perguntar primeiro "qual linha carrega esta informação?" (uma silhueta, o ângulo de uma lâmina, a posição dos pés, um corte gravado). Se a resposta for "nenhuma, só a cor ou o efeito", o elemento ainda não está resolvido.

*Por que:* a frase une a âncora aprovada e "silhueta primeiro, cor depois". Com ela, distinguir o espadachim do Discípulo da Guarda Fechada, e ler repouso, guarda e início de corte, vira uma pergunta sobre linhas (contorno, base dos pés, eixo da espada), não sobre paleta.
*Alternativa descartada:* só "todo movimento que se pode aprender já está escrito no mundo". Resolve as pistas, mas não decide nada no combate.

### Princípio 1: A pedra e o rastro são a mesma linha
*Serve ao Pilar 3 (O mundo se lembra, e você também) e ao Pilar 1 (Aprender é a recompensa).*

O corte gravado no túmulo da mestra e o rastro da lâmina do jogador têm o mesmo traçado, a mesma espessura e o mesmo acabamento de borda. Quando o jogador executa o golpe, o rastro coincide com a marca da pedra, e o aprendizado se vê sem texto. Uma marca que antes era decoração vira um golpe reconhecível: as ruínas valem a volta.

**Forma decidida: arco geométrico.** Corte e rastro são uma **curva de raio constante, borda limpa, espessura uniforme de 1–2 px**. O que torna o corte "feito por mão humana" no meio da rocha deixa de ser a retidão e passa a ser a **regularidade geométrica**: a rocha tem contorno irregular e espessura variável; o corte é um arco perfeito. A curvatura, o sentido e o ponto de partida diferenciam técnicas (arco ascendente, descendente, meia-lua), dando vocabulário às pistas sem cor nem texto. Arcos em pixel art usam um padrão de degraus consistente e espelhado (por exemplo 1-1-2-2-3 | 3-2-2-1-1), desenhado à mão, nunca serrilhado.
*Consequência registrada:* no `game-concept.md`, "cortes na pedra são retos e limpos" passa a valer como "geométricos e limpos (retas ou arcos de raio constante)"; o corte do túmulo no protótipo muda de reta para arco numa próxima iteração.

*Teste de design:* quando a forma de um rastro ou de uma marca gravada estiver ambígua, escolher a forma que poderia ser **idêntica nas duas**. Um detalhe que a pedra não poderia ter (partículas, brilho difuso) vai para uma camada secundária, nunca para a linha principal.
*Alternativas descartadas:* corte reto (mais barato, mas lê como laser e todos os golpes ficam iguais, mudando só o ângulo); híbrido reto/curvo (duas gramáticas para aprender); rastros livres e espetaculares com a pedra como pictograma (quebra a ligação entre ver e fazer).

### Princípio 2: O corpo diz antes da cor
*Serve ao Pilar 4 (Maestria nas mãos) e à falha legível do Pilar 5 (Todo caminho é um Dao).*

Os estados que o jogador precisa reconhecer para agir (repouso, guarda, início de corte) se leem por **três linhas do corpo**, nesta ordem: (1) a **silhueta** total (massa do tronco, abertura dos braços); (2) os **pés** (largura da base, peso adiantado ou recuado); (3) a **espada** (ângulo e altura da lâmina em relação ao corpo). Cor, brilho e qi só **confirmam**. O qi do jogador é desenhado atrás do contorno do inimigo em guarda, ou translúcido sobre ele; esse contorno fica sempre por cima. Na alquimia, a falha se lê primeiro pela **forma** da chama, depois pela cor.

- O espadachim-cultivador e o Discípulo da Guarda Fechada se distinguem pela silhueta **em escala de cinza**. Se só a paleta os separa, o desenho não está pronto.
- Repouso, guarda e início de corte diferem em **pelo menos duas das três linhas** (ex.: repouso com pé à frente, peso atrás e lâmina baixa a ~27°, fora da silhueta da perna; guarda com base aberta, joelhos flexionados e lâmina erguida diante do tronco; início de corte com o peso passando ao pé da frente e a lâmina saindo do eixo do corpo). Pés juntos não são repouso de combatente: ficam reservados ao NPC neutro (Seção 5.2). Medidas por estado na Seção 5.4.

*Teste de design:* quando dois estados ou dois personagens estiverem ambíguos, **mudar a pose (pés, tronco, lâmina), nunca só a cor**. Aprovação: reconhecer o estado em escala de cinza, em 1–2 quadros, com o qi do jogador ativo na tela.
*Alternativa descartada:* estados de combate indicados por cor ou brilho. Mais rápido de produzir, mas falha para daltônicos, some sob o qi e troca reconhecimento de postura por reflexo de cor.

### Princípio 3: Ciano-branco é só para o que se aprende
*Serve ao Pilar 1 (Aprender é a recompensa), com o limite do Pilar 2 (O estilo é seu).*

O **ciano-branco** é exclusivo do conhecimento que se pode aprender: cortes gravados, pegadas, inscrições, e o instante em que o rastro do jogador coincide com uma marca. Nada decorativo, de interface comum ou de recompensa comum usa esse tom: ver ciano-branco significa sempre "aqui há algo para aprender". Limite do Pilar 2: a pista mostra **um fragmento** (um trecho do corte, uma pegada, uma postura), nunca a sequência completa.

**Cor do rastro decidida:** o rastro da lâmina usa a **cor do qi do jogador** (que muda com o caminho dele) e mantém a **forma** da pedra. Ele só acende em **ciano-branco no instante em que coincide com uma marca conhecida** (no reconhecimento ou na primeira execução de uma técnica aprendida). A forma liga o rastro à pedra; a cor liga o rastro ao estilo do jogador; o ciano-branco fica guardado para o momento de aprender.

*Teste de design:* um elemento só pode usar ciano-branco se o jogador puder aprender algo com ele; se for decoração, sai. Se a pista mostrar a combinação inteira, reduzir a um fragmento.
*Alternativa descartada:* um realce genérico de interação igual para baús, portas e pistas. Dilui o significado: o jogador passaria a caçar brilho em vez de ler o mundo.

*Cobertura dos pilares:* Pilar 1 nos Princípios 1 e 3; Pilar 2 no limite do Princípio 3 e na cor do rastro; Pilar 3 no Princípio 1; Pilar 4 no Princípio 2; Pilar 5 no Princípio 2.

## 2. Mood & Atmosphere

### Regras globais de atmosfera

- **Luz-chave fixa no alto à esquerda** para o sombreamento de todos os sprites, em todas as áreas. A hora do dia muda o comprimento e a cor das sombras projetadas, nunca o lado da luz. *Por que:* sprites desenhados uma vez servem a todas as áreas, e a leitura de pés e lâmina (Princípio 2) não muda. *Descartado:* reiluminar por área ou luz dinâmica (multiplica o custo de arte e muda o volume aparente das poses).
- **Hora do dia fixa por área, sem ciclo dia/noite.** A hora identifica a região ("a ponte é a da tarde"): serve ao Pilar 3 e impede que um pôr do sol invada o âmbar e o vermelho-ocre. *Descartado:* ciclo dinâmico (caro para um projeto solo; faz a paleta semântica colidir com a luz várias vezes por sessão).
- **Ambiente sempre em contraste de valor baixo a médio** (L\* ~20–60). Contraste alto pertence a atores, pistas e efeitos.
- **Partículas ambientes** (névoa, poeira, folhas, faíscas) nunca usam ciano-branco, vermelho-ocre, âmbar nem a cor do qi, e nunca passam por cima da camada de pistas nem do contorno de um inimigo em guarda.
- **Escala de energia** de 1 (parado) a 5 (máximo): mede movimento e ritmo na tela, não intensidade emocional (a revelação tem energia 1 e intensidade máxima).

### Quadro-resumo

| Estado | Alvo emocional | Luz | Energia | Elemento que carrega o humor |
|---|---|---|---|---|
| Exploração: trilha de montanha | Solidão serena, amplidão | Manhã cedo, fria e pálida, contraste baixo | 2 | Faixas de névoa e pinheiros escuros na moldura |
| Exploração: vale de bambu | Atenção calma, abrigo | Meio da manhã, filtrada em verde, contraste local médio | 2 | Ritmo vertical dos bambus e manchas de luz no chão |
| Exploração: ponte do rio | Limiar, passagem | Fim de tarde, morna e lateral, sombras longas | 2 | Reflexo partido da ponte na água |
| Túmulo / ruína | Reverência, suspeita, foco | Escuro frio com um único feixe neutro-morno | 1–2 | Feixe de luz com poeira suspensa |
| Revelação | Compreensão súbita ("ah!") | O mundo escurece; só a pista e o jogador ficam acesos | 1 | Arco e pegadas acendendo na ordem do golpe |
| Duelo: discípulo | Pressão, frustração justa, depois curiosidade | Luz da área, fundo um passo mais apagado | 4 | O vento para; lampejo ocre só no contra-ataque |
| Duelo: espadachim sênior | Solenidade, prova | Luz da área mais fria, vinheta mais forte | 5 | Uma camada de folhas pálidas atravessando a arena |
| Derrota | Reflexão, sem castigo | Dessaturação para nanquim | 1 | A guarda que venceu o jogador, congelada em escala de cinza |
| Vitória | Alívio contido, dignidade | A luz da área volta e esquenta um passo | 2→3 | O inimigo baixa a guarda; o selo carimba o nome da arte |
| Fornalha (futuro) | Concentração artesanal, intimidade | Interior noturno, âmbar como fonte única | 3 | Forma da chama |
| Menus em pergaminho | Estudo, posse, calma | Sem luz de mundo: papel neutro-morno | 1 | Arcos em nanquim no livro de técnicas |
| Título | Contemplação, convite | Alvorada fria com névoa | 1 | Estátua da mestra e um arco gravado |

### Exploração: região aberta

Princípio comum: **o mundo espera ser lido**. Nada pisca nem chama atenção; a curiosidade nasce de encontrar uma linha geométrica onde só deveria haver rocha (Pilar 3; anti-pilar dos marcadores). Energia 2: vento leve, elementos animados em ciclos lentos de 2 a 4 quadros.

**Trilha de montanha.** *Alvo:* solidão serena, estar acima do mundo no começo de um caminho de cultivo. *Luz:* manhã cedo, chave pálida levemente morna, sombras frias azul-acinzentadas, contraste baixo, sombras curtas. *Descritores:* rarefeito, silencioso, aberto, enevoado, frio. *Elemento:* faixas horizontais de névoa passando devagar nas bordas, pinheiros escuros nos cantos; a névoa achata o fundo e põe o jogador no primeiro plano. *Por que:* a frieza calma prepara o contraste com o calor do primeiro duelo. *Descartado:* montanha dramática de vento forte e contraste alto (gasta o épico na abertura).

**Vale de bambu.** *Alvo:* atenção calma, abrigo; um lugar onde se para e se olha. *Luz:* meio da manhã, filtrada e esverdeada; manchas mornas no chão sobre sombra verde-oliva; contraste local médio, global baixo. *Descritores:* abrigado, rítmico, sussurrante, vertical, sombreado. *Elemento:* ritmo vertical dos colmos e manchas de luz que oscilam em 2–3 quadros quando o vento passa. *Por que:* entre linhas verticais orgânicas, um corte geométrico salta à vista (Seção 3). *Descartado:* floresta densa e sombria (mistura exploração com clima de túmulo).

**Ponte do rio.** *Alvo:* limiar; perceber que se chegou longe. *Luz:* fim de tarde, chave morna mais baixa (ainda do alto à esquerda), sombras longas para baixo e à direita, contraste baixo a médio. *Descritores:* morno, fluido, horizontal, transitório, refletido. *Elemento:* reflexo partido da ponte em faixas horizontais curtas e sombras longas dos pilares. *Regra de cor:* a luz morna vem da família terra-caminho, **nunca do âmbar `#e2a640`**; o brilho da água é cinza-azulado dessaturado, **nunca ciano**. *Por que:* é a única área aberta morna, então as três se distinguem por temperatura além da geometria. *Descartado:* pôr do sol saturado laranja e rosa (colide com âmbar e ocre).

### Túmulo / ruína (investigação)

*Alvo:* reverência e suspeita, "alguém esteve aqui antes de mim"; o foco se estreita. *Luz:* interior escuro e frio, base cinza-violeta (`#3a3a3f`); **um único feixe** neutro-morno, cor de pergaminho clara e translúcida, entrando por uma fresta. Contraste global baixo e escuro; contraste local alto só dentro do feixe. Borda do feixe pontilhada em 2 valores, sem gradiente suave. *Descritores:* poeirento, imóvel, frio, cerimonial, abafado. *Energia:* 1–2 (só a poeira se move, dentro do feixe). *Elemento:* o feixe com poeira ilumina o chão **perto** da pista, nunca a própria pista: guia de composição diegético, linha de luz que leva o olho sem apontar como um marcador. *Pista em repouso:* o arco gravado se lê **só pela linha** (sulco mais escuro que a pedra, 1 px de borda de luz na cor da pedra), com um ciano mínimo e lento como respiração; o ciano-branco pleno fica para a revelação. *Por que:* o feixe é a única fonte morna e o ciano a única luz fria e clara; não competem. *Descartado:* tochas laranja (encostam no âmbar e no ocre; a luz tremulante faz ruído na linha da pista).

### Revelação: o momento de aprender

*Alvo:* compreensão súbita, um instante de reverência que **não interrompe** o jogo (anti-pilar dos tutoriais longos). *Luz:* o mundo escurece e dessatura (valor ~40%, saturação ~30%); **só três coisas ficam acesas**: as pegadas, o arco gravado (ciano-branco `#d4fbf6`, halo `#6fd8d2`) e o jogador em cor normal. Contraste máximo: o único momento do jogo com tela quase preta e uma linha branca. *Descritores:* suspenso, silencioso, luminoso, preciso, íntimo. *Energia:* 1. *Elemento:* **o arco se desenha a partir do ponto de partida, no sentido do golpe**, e **as pegadas acendem na ordem em que a mestra pisava**. Só se revela o fragmento (Pilar 2). *Duração sugerida:* 1 a 1,5 s (o tempo exato é decisão de design de jogo). *Descartado:* tela de "Técnica aprendida!" com painel de texto.

### Duelos

Regras comuns: no duelo, **o mundo prende a respiração**. Vento e partículas param ou ficam bem mais lentos; o fundo cai um passo de valor e saturação; uma vinheta leve fecha as bordas; os atores ficam inalterados, então a separação figura-fundo cresce sem mudar os sprites. O vermelho-ocre aparece **só** como telegrafia, em lampejos curtos ligados à pose. O qi do jogador fica atrás do contorno do inimigo.

**Discípulo da Guarda Fechada.** *Alvo:* pressão e frustração **justa**, que vira curiosidade ("como se passa por essa guarda?"). *Luz:* a da área, fundo um passo mais apagado, contraste médio. *Descritores:* tenso, fechado, repetitivo, próximo, abafado. *Energia:* 4. *Elemento:* o vento que para quando o duelo começa e o lampejo ocre que corre a borda da lâmina só quando ele prepara o contra-ataque (a lâmina deixa a diagonal da guarda e fica parada ≥ 2 quadros); fechar a guarda nunca acende ocre.

**Espadachim sênior.** *Alvo:* solenidade e prova: o jogador mostra o que entendeu. *Luz:* a da área, um passo mais fria, vinheta mais forte; contraste médio-alto entre atores e fundo. *Descritores:* cerimonial, pesado, frio, espaçoso, decisivo. *Energia:* 5, mas contida (o pico vem da velocidade dos golpes, não de tremor ou efeitos). *Elemento:* uma única camada de folhas pálidas (família pergaminho e palha, **nunca ocre nem vermelho**) atravessando a arena devagar.
*Descartado para os duelos:* trocar a paleta da área no combate (gastaria o vermelho-ocre como clima).

### Derrota / retorno ao início da área

*Alvo:* reflexão sem castigo ("o que foi que eu não vi?"), a falha legível do Pilar 5 levada ao combate. *Luz:* a cena dessatura rápido até **nanquim** (cinza levemente morno), congela por um instante, funde para o tom do pergaminho e volta ao início da área. *Descritores:* quieto, monocromático, lúcido, breve, gentil. *Energia:* 1. *Elemento:* **a guarda que derrotou o jogador, congelada em escala de cinza**: a última imagem é a silhueta da postura a ler. *Descartado:* vinheta vermelha com texto grande de morte.

### Vitória

*Alvo:* alívio contido e dignidade marcial. *Luz:* o fundo volta ao valor normal e esquenta um passo por um instante; vento e partículas voltam. *Descritores:* sereno, morno, aberto, respeitoso, breve. *Energia:* sobe a 3 no golpe final (hitstop) e assenta em 2. *Elemento:* **o inimigo baixa a guarda** (a linha da lâmina cai); quando o jogo reconhece uma combinação nomeada, o nome aparece em pincel sobre pergaminho e o selo vermelho o carimba (Pilar 3). *Opcional:* uma arte nomeada deixa um arco-cicatriz temporário no chão, na forma da Seção 1 e na cor do qi. *Descartado:* chuva de partículas douradas e números de dano grandes.

### Fornalha de alquimia (futuro)

*Alvo:* concentração artesanal e intimidade (Pilar 5). *Luz:* interior noturno, **âmbar como fonte única e dominante**: a única área em que o âmbar tinge o ambiente. Sombras marrom morno profundo, nunca preto; contraste alto em torno da fornalha. *Descritores:* morno, crepitante, fechado, atento, enfumaçado. *Energia:* 3, com foco (a chama se move, o resto fica parado). *Elemento:* **a forma da chama**: saudável = gota estável; excessiva = alta com pontas irregulares; fraca = baixa e intermitente. *Descartado:* alquimia em menu.

### Menus em pergaminho (livro de técnicas, inventário, mapa)

*Alvo:* estudo e posse calma ("isto é o que eu sei"); o caderno do cultivador. *Luz:* sem luz de mundo; papel `#d8c7a0` com tinta `#2a2118`, luz plana, sombreamento de 1–2 valores só nas dobras e bordas do rolo. Contraste alto entre tinta e papel; baixo no resto. *Descritores:* quieto, tátil, manuscrito, ordenado, morno. *Energia:* 1. *Elemento:* as técnicas aparecem no livro como **o próprio arco desenhado a nanquim** (mesma curvatura, sentido e ponto de partida da pedra): o livro repete a pedra. O mapa é desenhado à mão, sem marcar destinos. *Descartado:* painéis escuros e translúcidos.

### Título

*Alvo:* contemplação e convite, promessa de caminho longo. *Luz:* alvorada fria, névoa baixa, luz pálida do alto à esquerda; contraste baixo; a peça mais clara é a pedra da estátua. *Descritores:* imóvel, enevoado, antigo, silencioso, aberto. *Energia:* 1. *Elemento:* **a estátua de pedra da mestra** com **um arco gravado** na base; o logotipo usa um traço de pincel que termina no mesmo arco, retraçado na cor base do qi ao começar o jogo. *Descartado:* protagonista em pose heroica com efeitos de qi.

## 3. Shape Language

Cada princípio desta seção nasce da regra visual da Seção 1: **"Tudo o que importa se lê pela linha antes da cor."** Medidas valem para 640×360 nativo, escala inteira, tiles 32×32 e quadros de personagem 64×64; "px" é sempre pixel nativo.

### Princípio 1: três famílias de forma, três significados

*Serve aos Pilares 3 e 1.*

- **Orgânico irregular** (rocha, terreno, árvores, água, névoa): contorno sem trechos retos maiores que 3 px, espessura variável, grupos ímpares. Diz: "isto não ensina nada".
- **Geométrico reto** (arquitetura, cantaria, degraus, pilares): mão humana, permanência, memória.
- **Arco de raio constante, o "arco-linha"** (marcas gravadas, rastros, ícones de técnica): 1–2 px, borda limpa, degraus espelhados. Diz: "isto se aprende".

*Teste:* se um elemento de cenário pode ser confundido com uma marca a 1×, em escala de cinza, ele muda de família.
*Descartado:* cenário uniformemente anguloso (low-poly facetado) — a rocha teria a geometria do corte, e o corte perderia o destaque.

### Princípio 2: o arco-linha é a forma-herói, e é reservado

Só aparece em marcas gravadas, rastros e ícones de técnica. Qualquer outra curva se diferencia por **curvatura variável** (ponta de telhado), **massa preenchida ≥ 4 px** (ponte, fornalha, a lâmina de pedra da estátua) ou **traço partido / espessura variável** (água, pincel da UI).

*Teste:* "este traço poderia ser idêntico a um rastro?" Se sim e não é técnica, muda.
*Descartado:* liberar arcos limpos na decoração "porque ficam bonitos" — o jogador passaria a ler pistas onde não há.

### Princípio 3: personagens em escala de jogo, com corpo de artista marcial

*Serve aos Pilares 4 e 2.* A referência de presença é o Murim de *Contos de Cultivação de um Regressor*: postura, peso e atitude de quem treinou a vida inteira. Os personagens são originais; da referência vem a presença, nunca trajes ou rostos.

#### 3.1 Proporção: alongada, ~4,6 cabeças

O protótipo, com ~4 cabeças, transforma a túnica num bloco: não sobra altura para pernas, cintura e braços separados.

| | A: compacta (protótipo) | **B: alongada (adotada)** |
|---|---|---|
| Altura (sola ao topo da cabeça) | 44 px | **46 px** (+ coque ≤ 3 px; máx. 49) |
| Cabeça | 11 px | **10 px** (piso: abaixo disso olhos e cabelo se fundem) |
| Proporção | 4,0 cabeças | **~4,6 cabeças** |
| Pernas (sola à virilha) | ~16 px | **19–20 px** |
| Tronco (ombro à faixa) | 12 px | 12–13 px |
| Pescoço | 0–1 px | 1–2 px |

Quase todo o ganho vai para as pernas, e é isso que permite a barra dividida, o pé adiantado e a leitura do peso. O pescoço de 1–2 px solta os ombros da cabeça para que girem. Cabeça de 10 px ainda comporta rosto de 8 px de largura, olhos de 1 px com 2 px entre eles (vista 3/4), coque 3×3 e fita de 2–3 px. A figura ocupa as linhas 11–59 do quadro; a linha de base dos pés é a 59 e as linhas 60–63 ficam para a sombra.

*Descartado:* 5 cabeças ou mais (cabeça de 9 px) — ombros abaixo de 12 px, braços de 2 px que voltam a se fundir ao tronco, lâmina sem contraste com um corpo estreito.
*Consequência para a Seção 8:* no início de corte com a lâmina erguida, a arte pode passar da linha 0 do quadro (quadro de ataque maior ou camada de transbordamento).

#### 3.2 Regras para todo ator vivo

- **Contorno externo** completo de 1 px em `#16140f`. O ambiente não tem contorno.
- **Linhas internas** (braço sobre tronco, gola, painéis da barra) usam o tom mais escuro da própria veste, nunca `#16140f` — contorno preto por dentro vira ruído a 1×.
- **Silhueta por estado:** entre repouso, guarda e início de corte, o contorno muda ≥ 4 px em ao menos uma direção.
- **Pés:** a largura da base varia ≥ 4 px entre estados; pés abertos têm ≥ 2 px de espaço entre si; nenhum arquétipo esconde os pés (a barra termina ≥ 6 px acima da sola na frente das pernas).
- **Sombra:** elipse clara geral **mais 1 px escuro de contato sob cada pé**, separados. Uma mancha única apagaria a base, que é o que o jogador precisa ler.
- **Lâmina:** 1 px de núcleo + 1 px de fio claro (2 px). Só **inclinações limpas**: vertical, 1:3 (~18°), 1:2 (~27°), 1:1 (45°), 2:1, horizontal. Ângulos quebrados fazem o degrau tremer entre quadros.
- **Lâmina contra o cenário:** em repouso, **nunca vertical** — o bambu do vale é vertical, 2–4 px e sem contorno, e a lâmina viraria um colmo. A lâmina fica 2 passos de rampa mais clara que o bambu e tem contorno.
- **Direção legível:** as duas pontas da lâmina são diferentes. Ponta: pixel mais claro e afinamento de 1 px. Cabo: guarda transversal de 4 px e empunhadura escura.
- O traço do arquétipo sobrevive ao espelhamento e à vista de costas.

*Teste (todos os atores):* a 1×, em escala de cinza, com qi ativo na tela, reconhecer arquétipo e estado em 1–2 quadros. Repetir com **preenchimento chapado** (todo o ator em preto): o arquétipo continua reconhecível. Reduzido a 50%, os arquétipos ainda se separam.

### Princípio 4: aberto contra fechado — o espaço negativo diz quem é quem

#### 4.1 Espadachim-cultivador (jogador): vertical, aberto, assimétrico

*Comunica liberdade e potencial (Pilar 2). Mesmo parado, já é um espadachim.*

- **Ombros:** 14 px de frente; em repouso, girados ~20–30°, ficam 12 px visíveis, e o ombro de perto fica 1 px mais baixo.
- **Túnica clara de gola cruzada:** a gola é uma **diagonal de 1–2 px** do ombro de perto até a faixa, do lado oposto. É a linha que tira o tronco do bloco e mostra para onde ele gira.
- **Faixa** de 3 px na cintura; separa tronco de pernas pelo valor (cor na Seção 4).
- **Barra dividida:** a túnica desce até o joelho e termina 8–10 px acima da sola. As aberturas laterais começam no máximo 4 px abaixo da faixa; entre os painéis aparece a calça (≥ 2 px). Os painéis terminam em alturas diferentes (1–2 px) e acompanham o passo. A barra nunca fica mais larga que os ombros (máx. 13–14 px) — se ficar, vira o trapézio do discípulo.
- **Calça escura**, para as pernas se lerem contra a túnica clara (valor na Seção 4).
- **Braços:** manga de 3–4 px de largura, braço de 13–14 px do ombro ao punho, mão 3×2 px.
  - Braço da espada: cotovelo 1–2 px para fora do tronco; entre braço e tronco, espaço negativo ≥ 1 px de largura por ≥ 5 px de altura.
  - A outra mão pode encostar no corpo (faixa ou coxa). Só um braço toca o tronco, e ele ganha 1 px de separação no tom escuro da túnica.
  - Em pelo menos 3 das 4 direções, um braço inteiro se lê fora do tronco.
- **Espada:** lâmina 22 px, guarda 4 px, cabo 5–6 px (~30 px no total).
- **Coque e fita:** fita solta de 2–3 px que acompanha o movimento.

**Pose de repouso 3/4 (pose de aprovação):** pé à frente, peso atrás, ombros girados, lâmina ao lado do corpo.

| Elemento | Direção S (frente 3/4) | Direção E/W (perfil 3/4) | Direção N (costas) |
|---|---|---|---|
| Pé de trás (apoio) | sob o centro do quadril | sob o quadril | sob o quadril |
| Pé da frente | 3 px para o lado da espada, 2–3 px para baixo | 6–7 px à frente do pé de trás | 3 px para o lado, 2–3 px para cima |
| Espaço entre os pés | ≥ 2 px | ≥ 3 px | ≥ 2 px |
| Quadril | 1 px para a perna de trás | 1 px para trás | 1 px para a perna de trás |
| Cabeça | sobre o pé de trás (±1 px) | sobre o pé de trás (±1 px) | sobre o pé de trás (±1 px) |
| Joelho da frente | 1 px de flexão | 1 px | 1 px |
| Largura da base | 9–10 px | — | 9–10 px |

**Espada em repouso:** na mão da espada, com a **mão na altura da faixa** e a guarda ~25–26 px acima da sola (decidido no `/asset-spec` em 2026-11-25: com o cabo entre quadril e coxa, a 20–22 px, uma lâmina de 22 px a 1:2 tocaria o chão). Guarda transversal em **bronze escuro e opaco**, nunca latão dourado, que cairia no âmbar (4.3). A lâmina aponta para baixo e para a frente a **1:2 (~27°) da vertical**, com a ponta 3–5 px acima do chão — nunca no chão, ou lê como bengala. A lâmina fica **fora da silhueta da perna**, com um triângulo de espaço negativo de ≥ 3 px na parte mais larga. De costas, ≥ 8 px da lâmina, com a ponta, aparecem fora do corpo.

O peso atrás e o pé à frente dizem "pronto, sem pressa". A guarda (base ~14 px, +4 px) e o início de corte saem desta pose sem trocar de silhueta-mãe.

*Descartados:* espada embainhada na cintura (a 2 px a bainha lê como bastão e a lâmina, 3ª linha de leitura, some); espada nas costas (some na vista S, a mais vista); pés juntos e frontais, como no protótipo (postura de civil; a túnica vira coluna).

#### 4.2 Discípulo da Guarda Fechada: ampulheta cortada pela diagonal

*Comunica parede e anonimato. A guarda fechada é o traço mais forte dele.*

Três faixas: **chapéu largo em cima, cintura estreita, base larga embaixo** — um carretel cortado por uma lâmina na diagonal. Em preto chapado, não há como confundi-lo com o jogador (vertical, lâmina fora do corpo).

- **Altura:** 42–43 px com chapéu, joelhos flexionados (−2 px). Cabeça de 10 px, a mesma escala humana do jogador.
- **Chapéu de palha:** aba de **21 px** (≥ 7 px além dos ombros), cone baixo de 5 px, aba de 1–2 px de espessura. **Aba inclinada:** 2 px mais baixa do lado para onde ele encara. Cobre os olhos e deixa 2–3 px de queixo.
- **Cotovelos colados:** ombros de 14 px; o contorno dos braços fica a ±1 px do contorno do tronco. Aqui o braço "fundido" ao corpo é intencional: a cintura vira um bloco de ~14 px. O que separa braço e tronco por dentro é o antebraço, que **cruza o peito acompanhando a lâmina**.
- **Base:** **14–16 px de fora a fora** na direção S (o jogador em repouso tem 9–10). Joelhos abertos para fora, **≥ 5 px entre os pés** (triângulo de chão visível entre as pernas). Túnica curta, até o meio da coxa, e **perneiras claras** nas canelas. Pés 4×2 px escuros, cada um com seu pixel de contato. As perneiras claras sobre o chão médio-escuro mostram onde ele finca os pés.
- **Arma:** espada reta de **uma mão** (decidido 2026-11-25). O antebraço livre fica deitado sobre o peito, atrás da lâmina e paralelo a ela, com o cotovelo colado.
- **Lâmina na diagonal:** cabo no quadril do lado da espada, ponta acima do ombro oposto, **1:1 (45°)**, lâmina de 20 px (~14×14 px). A ponta sai 3–4 px para fora do contorno do ombro, abaixo da aba (aba ~5 px mais alta); guarda e pomo saem 2 px para fora do quadril do outro lado. A lâmina corta a silhueta dos dois lados: é a linha mais clara do personagem atravessando a parte mais escura.
- **Costas:** a lâmina é mais longa que o tronco e aparece dos dois lados. **E/W:** a diagonal vira lâmina diante do peito apontando para cima e para a frente; pés a 10–12 px um do outro, de frente para trás.

*Descartados:* guarda vertical diante do rosto (repete o bambu e esconde o chapéu); lâmina horizontal (lê como cinto ou bastão e não diz para onde aponta); túnica até os pés, como no protótipo (esconde a base, metade da leitura da guarda).

#### 4.3 Tabela de arquétipos

| Arquétipo | Forma | Traço único | Medidas | Comunica |
|---|---|---|---|---|
| Espadachim-cultivador (jogador) | Vertical, aberta, assimétrica | Lâmina baixa fora do eixo com triângulo de espaço negativo até a perna; gola cruzada em diagonal; barra dividida mostrando as pernas; pé à frente, peso atrás | 46 px (+ coque ≤ 3); cabeça 10; ~4,6 cabeças; ombros 14 (12 girados); lâmina 22 × 2; base de repouso 9–10 | Liberdade, potencial (Pilar 2) |
| Discípulo da Guarda Fechada | Ampulheta: aba larga, cintura estreita, base larga | Chapéu de palha com aba caída 2 px; lâmina a 45° cruzando o peito e saindo dos dois lados; cotovelos colados; perneiras claras | 42–43 px com chapéu; aba 21; ombros 14; lâmina 20 × 2; base 14–16; pés ≥ 5 px | Parede, anonimato |
| Espadachim sênior | Triângulo invertido, massa no alto | Ombros largos de manto pesado; coque alto, sem chapéu; lâmina longa; manto aberto na frente ou até a canela, pés visíveis | 48–50 px; ~4,8–5 cabeças; ombros 18; lâmina 26 × 2 | Autoridade, alcance |
| Estátua da mestra (a mesma do túmulo) | Monólito de pedra sobre pedestal; só pés e lâmina se destacam | Regras de ambiente (sem contorno de ator, contraste baixo); **pés esculpidos em passo lateral**; o arco gravado parte da ponta da lâmina de pedra | Peça 64×96 (2×3 tiles); figura ~66–68 px (~1,5× humano); pedestal 18–20 px com plano superior de 6–8 px | Permanência, memória (Pilar 3) |

#### 4.4 Estátua da mestra: pista legível sem virar ator

A estátua é **a mesma mestra do túmulo**. Segue as regras do ambiente, mas carrega duas pistas: os pés (o passo lateral) e a lâmina (a trajetória do corte).

- **Corpo:** veste e corpo fundidos num monólito facetado, sem contorno, no L\* do ambiente. **A barra da veste se abre na frente** e mostra os dois pés sobre o plano superior do pedestal.
- **Pés:** 6 px de comprimento cada; **sulco de ≥ 3 px** entre eles, 2 passos mais escuro que a pedra (na estátua o espaço negativo é sombra esculpida, nunca fundo vazado); **deslocamento lateral ≥ 6 px** no sentido do passo; o pé que dá o passo pode girar até 90°. Medidas maiores que as dos atores porque a pedra tem contraste baixo e não tem contorno.
- **Pegadas gravadas:** em volta de cada pé, um sulco de 1 px no pedestal. É a pegada, não o pé, que recebe o ciano mínimo em repouso e acende na revelação (Seção 2). O corpo da estátua nunca fica ciano.
- **Lâmina de pedra:** massa preenchida de ≥ 3 px — nem a linha de 1–2 px de uma lâmina de ator, nem o arco-linha (Princípio 2). Aponta para o **ponto de partida do arco**.
- **O corte:** o arco-linha (1–2 px, raio constante) fica gravado na face frontal do pedestal ou no chão diante dele, começando na direção da ponta da lâmina e seguindo o sentido do golpe. A lâmina diz "daqui"; o corte diz "por aqui".
- **Identidade com o túmulo:** estátua e figura gravada no túmulo têm **o mesmo traço de silhueta** (coque alto com grampo atravessado na diagonal) e **a mesma base de pés**, para que quem viu um reconheça o outro.

*Teste:* em escala de cinza, a 1×, (a) a estátua não pode ser confundida com um ator, mas (b) dá para dizer **para que lado ela dá o passo** e **de onde sai o corte**. Se (b) falhar, cresce o sulco entre os pés ou o deslocamento lateral — o contraste nunca sobe.
*Descartado:* estátua com contorno de ator para destacar os pés — competiria com os inimigos na hierarquia (Princípio 7).

#### 4.5 Aprovação visual: duas poses antes de qualquer animação

O criador aprova **uma pose de cada** — o espadachim em **repouso** e o discípulo em **guarda**, ambos na direção S — por comparação **antes/depois no bambuzal do protótipo**, com a mesma câmera e o mesmo zoom (escala inteira, 1× nativo). Os prompts de imagem são feitos no `/asset-spec`. Nenhuma animação é produzida antes dessa escolha.

| Espadachim (repouso) deve mostrar | Discípulo (guarda) deve mostrar |
|---|---|
| A lâmina num relance, ponta sem tocar o chão | Para onde a lâmina aponta (ponta clara contra cabo com guarda) |
| Dois braços distintos, um fora do tronco | Cotovelos colados e antebraço sobre o peito |
| As pernas pela barra dividida | Onde cada pé está fincado (dois contatos separados) |
| Pé à frente, peso atrás (cabeça sobre o pé de trás) | Aba inclinada e mais larga que os ombros |
| A gola cruzada girando o tronco | Base mais larga que o tronco |

**Como julgar** (passa se todos passarem):
1. Cor normal, depois **escala de cinza**.
2. **Preenchimento chapado** dos dois lado a lado: ainda se distinguem.
3. **Um colmo de bambu atrás** de cada lâmina: a lâmina não se funde com ele.
4. Redução a **50%**: ainda se distinguem.
5. **Teste do relance** (3 s, de preferência com alguém de fora do projeto): "qual mão segura a espada? qual pé está à frente? para onde aponta a lâmina do discípulo?"

Item reprovado se corrige **na pose** (pés, tronco, lâmina), nunca na cor.

### Princípio 5: arquitetura pesada e retilínea; natureza vertical e em camadas

- **Arquitetura:** horizontal e pesada; pilares ≥ 4 px; telhados como massas grossas; só as pontas de telhado são curvas (curvatura variável, preenchidas).
- **Bambu:** vertical, 2–4 px de espessura, nós a cada 6–10 px em intervalos irregulares, inclinação de ±1 px a cada 16 px.
- **Pinheiros:** camadas serrilhadas nas bordas da tela, como moldura.
- **Rocha:** irregular.
- **Ponte em arco:** volume de pedra preenchido ≥ 4 px.
- **Água:** traços horizontais curtos e quebrados.

*Consequência:* entre verticais orgânicas, as únicas linhas que destoam são as lâminas dos atores (inclinadas, com contorno, mais claras) e os arcos gravados (curvos e limpos). Por isso a lâmina em repouso nunca é vertical (3.2).

### Princípio 6: interface — pergaminho, pincel, arco a nanquim, tecla, selo

- **Rolo de pergaminho:** painéis.
- **Traço de pincel de espessura variável:** divisórias, seleção, títulos.
- **Arco a nanquim:** ícone de técnica — o livro repete a pedra.
- **Tecla desenhada:** sequências de comando. É a única forma "moderna", porque exige leitura precisa; no conflito entre arte e legibilidade, vence a legibilidade (detalhado na Seção 7).
- **Selo vermelho:** reconhecimento.

### Princípio 7: hierarquia de leitura

1. Arco-linha
2. Silhuetas de atores em guarda
3. Lâminas
4. Objetos interativos (forma fechada, contorno parcial)
5. Ambiente (sem contorno, contraste baixo — a estátua fica aqui, mesmo carregando pista)

O contorno separa o que age do que é cenário. Uma pista dentro do cenário sobe de nível só pela linha gravada e, na revelação, pelo ciano.

### Decisões desta seção

- **Traço da mestra:** coque alto com grampo atravessado na diagonal, igual na estátua e na figura gravada no túmulo.
- **Espelhamento:** 3 direções desenhadas + 1 espelhada. Numa das laterais o espadachim segura a espada com a outra mão; aceito para conter o custo de arte (4 direções desenhadas custariam ~+33%).

## 4. Color System

A cor vem **depois** da linha (Seção 1): ela confirma o que a forma já disse, nunca carrega sozinha uma informação. Cada família de cor tem **um único significado**, e a exclusividade é o que dá força a ela.

### Colisões encontradas no protótipo (motivo desta seção)

1. O qi jade-ciano `#3fae9e` / `#a6f2e2` está na mesma faixa de matiz (171–177°) do ciano-branco do conhecimento, então o rastro que acende ao coincidir com uma marca não se destaca.
2. A água `#2c5a5e` (~185°) é da família do ciano.
3. A veste do jogador `#dce5d4` (L\* ~90) é vizinha do ciano-branco.
4. A veste do sênior `#6e3027` é da família do vermelho-ocre da intenção hostil.
5. A veste do discípulo `#7f846c` tem o mesmo valor da rocha `#8a877a`.

Todas são corrigidas abaixo.

### 4.1 Sete famílias de cor

| Família | Valores | Significado | Uso |
|---|---|---|---|
| **Terra-musgo** | chão `#4c5a3e`, caminho `#86704f`, rocha `#8a877a`, água (rampa dessaturada, ver 4.5), túmulo `#3a3a3f` | o mundo, que "não ensina nada" | todo o cenário |
| **Ciano-branco** | `#d4fbf6` / `#6fd8d2` | conhecimento | marcas, pegadas e inscrições, e o instante do rastro coincidente |
| **Vermelho-ocre** | `#c8502a` / `#e8834c` | intenção hostil | só lampejos de inimigo ligados à pose de ataque |
| **Âmbar** | `#e2a640` / `#ffd27a` | alquimia e tesouro | objetos parados |
| **Jade do qi** | `#24603f` / `#3f9e6a` / `#9be3b0` | o que o jogador emite | rastros, auras e técnicas do jogador |
| **Tinta** | `#16140f` (contorno de ator), `#2a2118` (texto e traço da UI) | — | contorno e escrita |
| **Pergaminho e selo** | `#d8c7a0`, selo `#a8382a` | reconhecimento (selo) | só na UI |

*Descartado:* paleta por elemento (fogo vermelho, gelo azul, raio amarelo). Ela gastaria o vermelho, o ciano e o âmbar, que já têm significado.

### 4.2 Faixas de valor e saturação

- **Mundo:** L\* ~20–60, saturação ≤ 45%.
- **Sinal** (ocre, âmbar, qi): L\* ~50–88, saturação ≥ 60%.
- **Conhecimento:** L\* ≥ 90, frio — só o ciano-branco.
- Nenhuma cor além do ciano-branco passa de L\* ~88.

*Teste:* em escala de cinza, os sinais se separam do mundo pelo valor; o ciano-branco é o ponto mais claro de qualquer tela de jogo.

### 4.3 Regras de exclusividade

- **Ciano-branco:** só em marcas, pegadas e inscrições, e no instante do rastro coincidente. Proibido em água, gelo, luar, brilhos, partículas e em **qualquer menu** (decidido; ver 4.6).
- **Vermelho-ocre:** só no inimigo durante a intenção. Nunca em cenário, veste ou barra de vida.
- **Âmbar:** em objetos parados. Só tinge o ambiente na fornalha.
- **Jade do qi:** só no que o jogador emite. Vestes e cenário usam o **jade gasto** (4.4), nunca a rampa viva.
- **Selo:** só na UI, em formas grandes (contraste ~3,8:1 com o pergaminho).

### 4.4 Qi e personagens

**Qi (decidido: jade verde ~145°).** Rampa de 3 passos: `#24603f` (L\* 36), `#3f9e6a` (L\* 58), `#9be3b0` (L\* 85).

- **Zonas proibidas:** ciano (160–200°), qualquer L\* > 88, ocre (0–30° e 345–360°), âmbar (30–60°).
- **Matizes permitidos por caminho:** jade (100–150°), azul-tempestade (210–240°, com o tom claro abaixo de L\* 75), violeta/ameixa (260–320°).
- Mudar de caminho troca **só o matiz**. Os valores ficam fixos e a forma do rastro nunca muda. Elementos (fogo etc.) se expressam pela **forma**, nunca em ocre.
- A rampa ocupa **3 índices fixos** da paleta: trocar de caminho é trocar a paleta, não redesenhar sprites.
- **Inimigos não têm qi de cor própria.** O qi deles usa a cor de intenção (ocre, no ataque) ou um neutro escuro.

**Protagonista:**
- **Túnica clara:** brilho `#d6d2bf` (L\* ~84), tom `#b3ad97` (~71), sombra `#8a846f` (~55). A túnica fica abaixo do ciano-branco (L\* ≥ 90) e não colide com ele.
- **Faixa em jade gasto (decidido):** sombra `#3a5a4b`, tom `#4f7a64` (matiz ~150°, saturação ~21%, L\* 40–50). Ela se separa do qi pela saturação e pelo valor, e do musgo do chão pelo matiz. Futuro opcional: cada caminho ganha sua versão gasta (2 índices trocados junto com o qi), sem redesenhar sprites.
- **Calça escura:** L\* ~24–28, pelo menos 10 abaixo do chão (`#4c5a3e`, L\* ~36), para as pernas da barra dividida não sumirem.
- **Lâmina contra a túnica:** o aço (L\* ~80) tem o valor da túnica. Quando a lâmina passa na frente do corpo (guarda, início de corte), ela leva contorno `#16140f` dos dois lados. Em repouso não há conflito, porque a lâmina fica fora do corpo.

**Discípulo da Guarda Fechada:** corpo e veste escuros (L\* ~38–42); chapéu e perneiras claros (L\* 65–70). Cores aprovadas em 2026-11-25: corpo em **ardósia escura `#555d5e`**, sombra `#3f4647` (frio, para não cair na terra do chão nem no âmbar); chapéu e perneiras em **palha apagada `#b0a382`**, brilho `#c4b893`, sombra `#8f8466`. O esquema de valor é o inverso do protagonista (claro em cima e pernas escuras contra chapéu e canelas claros com corpo escuro), então a escala de cinza separa os dois sozinha, e a lâmina clara se destaca no tronco escuro.

**Espadachim sênior (decidido):** veste **índigo-ardósia** ~`#3d4160`. É pesada e autoritária, e deixa o ocre da intenção limpo.

### 4.5 Temperatura por área

| Área | Luz | Sombras | Proibido |
|---|---|---|---|
| Trilha | pálida, morna | azul-acinzentadas | névoa acima de L\* 70 |
| Vale (bambuzal) | verde-morno filtrado | verde-oliva | verde-azulado |
| Ponte | morna, lateral | mornas | teto em `#e2a640`; brilho da água acima de 30% de saturação (fica cinza-azulado) |
| Túmulo | `#3a3a3f`, feixe translúcido cor de pergaminho | quase pretas, mornas, puxando para o violeta-cinza | — |
| Duelos | herdam a área, fundo −1 passo de valor (sênior: também −1 de temperatura) | — | — |
| Fornalha | âmbar dominante | mornas | — |

- **Sombra nunca é preto puro.** Ao ar livre, puxa para o frio; no túmulo, para o violeta-cinza.
- **Brilhos puxam para o morno.**
- **Água:** sem linha limpa e com brilho dessaturado abaixo de L\* 70, para não lembrar o ciano.

### 4.6 Paleta da interface

A UI é **morna e clara**; o mundo é **verde-terroso médio-escuro**. A divergência é intencional: o jogador sabe sem pensar se está no mundo ou no caderno.

| Papel | Cor | Contraste com o pergaminho |
|---|---|---|
| Pergaminho (base) | `#d8c7a0` | — |
| Pergaminho claro | ~`#e6d9b8` | — |
| Pergaminho sombra | ~`#b5a078` | — |
| Tinta (texto) | `#2a2118` | ~9,5:1 |
| Aguada (texto secundário) | ~`#6b5a45` | ~4,6:1 |
| Selo | `#a8382a` | ~3,8:1 (só formas grandes) |

- **Sem ciano e sem ocre em menus** (decidido). O ciano continua exclusivo do mundo.
- **HUD:** a barra de qi usa a rampa do qi. A barra de vida do **alvo** é neutra (tinta e pergaminho), nunca ocre nem vermelha.
- **Vida do jogador (decidido 2026-11-25):** losangos em **vermelho escuro gasto `#7a3a32`** quando cheios e tinta quando vazios, com contorno de tinta. Saturação baixa e valor baixo (L\* ~32), fora da faixa de sinal do ocre (L\* 50–88, saturação ≥ 60%). Só no HUD, nunca no mundo; a forma de losango carrega a informação sem depender da cor. Escolhido na prancha `user-docs/relatorios/2026-11-25-escolhas-visuais.html`.

### 4.7 Daltonismo: reforço para cada par de risco

| Par que pode falhar | Quem é afetado | Reforço sem cor |
|---|---|---|
| Ocre × qi | protanopia, deuteranopia | o aviso é a mudança de pose; o qi fica atrás do contorno; o lampejo percorre a borda da lâmina; som de "aço que se prepara" |
| Âmbar × ocre | protanopia, deuteranopia | âmbar só em objetos parados, com brilho em losango; ocre só em atores, como traço de borda; sons distintos |
| Ciano × qi | tritanopia | o rastro sobrepõe a marca; salto de valor (85 → 96) e espessura (1 → 2 px); som de reconhecimento |
| Ciano × água | todos | água sem linha limpa, brilho dessaturado abaixo de L\* 70 |
| Ciano × túnica do jogador | todos | túnica em L\* ≤ 84 |
| Ocre × veste do sênior | todos | veste índigo-ardósia; o aviso é pose + lampejo 2 passos acima + som |
| Selo × ocre | protanopia, deuteranopia | contextos separados (UI × mundo); selo é um quadrado carimbado |
| Vida do jogador × ocre | protanopia, deuteranopia | vida só no HUD, em losango e em canto fixo; ocre só em atores no mundo, como traço de borda; os contextos nunca se sobrepõem |
| Veste do discípulo × rocha | todos | contorno de ator + chapéu e perneiras claros |

*Teste a cada sprint:* simulador de daltonismo (Krita ou ferramenta gratuita) e escala de cinza sobre capturas do jogo.

### 4.8 Orçamento e arquivos de paleta

**Paleta-mestra indexada de 64 cores (decidido).** **61 em uso e 3 de reserva** (aprovada em 2026-11-25; arquivo `assets/palettes/pal_master_64.gpl`, amostras em `assets/palettes/pal_master_64_swatches.png`, prancha `user-docs/relatorios/2026-11-25-paleta-mestra.html`).

Distribuição: linha e neutros 4, musgo 5, terra 4, pedra 4, água 3, túmulo 3, pele 3, protagonista 7 (túnica 3, faixa 2, calça 2), discípulo 5 (corpo 2, palha 3), sênior 2, armas 4 (aço 3, bronze 1), ciano 2, ocre 2, âmbar 3, qi 3, interface 7 (pergaminho 3, tinta, aguada, selo, vida do jogador).

A saturação das faixas da Seção 4.2 é medida em HSV. Os brilhos das rampas de sinal (âmbar claro, qi claro) ficam abaixo de 60%, como é próprio do topo de uma rampa.

- **Rampas:** 3–5 passos, ~10–12 de L\* entre passos. O matiz se desloca (sombras 10–20° para o frio, brilhos 10–15° para o morno) e a saturação tem pico no tom médio. Rampas semânticas têm 2 passos (o qi tem 3).
- **Pontilhado:** proibido em personagens; o ordenado é permitido só em grandes gradientes de ambiente.
- **Translucidez:** só na camada de efeitos, em tempo de execução.
- **Arquivos:** `.gpl` (lido pelo LibreSprite, pelo Pixelorama e pelo Krita) mais uma tira PNG de amostras. As trocas de qi ficam em `.gpl` de 3 índices. Exemplos: `pal_master_64.gpl`, `pal_qi_jade_3.gpl`.

### 4.9 Valores de ajuste

- Duração da revelação de uma marca (ciano acendendo): 1–1,5 s, a calibrar jogando.
- Contraste mínimo de texto na UI: 4,5:1 (a aguada está no limite e não deve escurecer menos que isso).

## 5. Character Design Direction

Medidas e silhuetas estão na Seção 3 (Princípios 3 e 4). Esta seção define caráter, categorias de ator, estilo de pose, os três estados de combate, regras de animação e o detalhe que sobrevive na distância de jogo.

### 5.1 Arquétipo do jogador: o cultivador que ainda está se fazendo

*Serve ao Pilar 2.*

- É o único ator **aberto**: lâmina fora do eixo, base estreita em repouso, roupa clara e simples, sem insígnia de seita.
- É o único com **túnica clara e faixa em jade gasto**, e o único que emite qi.
- A identidade cresce pela **lâmina e pelo rastro, nunca pela roupa**. Sem troca de visual por equipamento no MVP: ela mudaria a silhueta que o jogador aprendeu a ler.

*Teste:* uma mudança no visual do jogador altera a silhueta em preto chapado? Se altera, não entra.

### 5.2 Distinguir num relance: a linha da lâmina diz quem pode lutar

*Serve aos Pilares 3 e 4.*

| Categoria | Lâmina | Base dos pés | Cor |
|---|---|---|---|
| **Inimigo** | desembainhada e visível em todos os quadros | aberta, de guarda | ocre só no instante da intenção |
| **NPC neutro** | nenhuma linha de lâmina (embainhada, ausente ou só ferramenta) | pés juntos (base ≤ 6 px) | mãos visíveis; sem ocre e sem qi |
| **Estátua e figuras de mestres** | lâmina de pedra preenchida | pés esculpidos | regras de ambiente (Seção 3, 4.4) |

- **Cada novo arquétipo de inimigo nasce de uma guarda:** uma silhueta de guarda nova, conferida em preto chapado contra a tabela da Seção 3. A guarda é o portão de conhecimento do Pilar 3; se duas guardas se confundem, o jogador não aprende a ler nenhuma.
- **Rivais** que citam as artes do jogador seguem as regras de inimigo quando lutam e as de NPC quando conversam. A lâmina guardada é o sinal de trégua.

*Teste:* em escala de cinza e sem ocre, responder "este ator pode me atacar?" só pela linha da lâmina e pela base.

### 5.3 Pose e expressão: corpo naturalista, guarda exagerada

*Serve ao Pilar 4.*

- **Exagera-se o que se lê:** largura da base, ângulo da lâmina, giro dos ombros. O resto do corpo segue proporções humanas.
- **Expressão pela postura, não pelo rosto.** A cabeça de 10 px tem só olhos de 1 px; a emoção se lê pela inclinação da cabeça (1 px), pela altura dos ombros e pela aba do chapéu. Sem boca nos sprites de jogo.
- **Poses-chave paradas:** o início de corte de um inimigo (a telegrafia) se mantém por **≥ 2 quadros** antes do golpe. Nenhuma telegrafia existe só numa transição.
- **Movimento secundário:** fita, coque, barra e aba seguem o corpo com **1 quadro de atraso**. Sem esticar e achatar o corpo (squash/stretch), que deformaria a silhueta das guardas.

*Descartado:* expressão facial detalhada — não sobrevive a 1× e não carrega informação de jogo.

### 5.4 Os três estados

| Estado | Espadachim (jogador) | Discípulo da Guarda Fechada |
|---|---|---|
| **Repouso** | Pé à frente, peso atrás; base 9–10 px; lâmina baixa a ~27°, fora da perna (Seção 3, 4.1) | Aba nivelada; base 10 px; lâmina baixa e diagonal ao lado da perna, ponta para trás; cotovelos já junto ao corpo |
| **Guarda** | Base 14 px (+4); joelhos flexionados (−2 px de altura); lâmina erguida a 1:1 diante do tronco, ponta para o alvo | Aba inclinada 2 px; base 14–16 px; lâmina a 45° cruzando o peito (Seção 3, 4.2) |
| **Início de corte** | Peso passa ao pé da frente (cabeça avança 2–3 px); lâmina sai do eixo do corpo para trás e para o alto, no ponto de partida do arco | Aba sobe 1 px; pé da frente avança 3 px; lâmina deixa a diagonal para 1:2 à frente; lampejo ocre corre a borda; mantido ≥ 2 quadros |

Cada estado difere do vizinho em **pelo menos duas das três linhas** (silhueta, pés, lâmina), como exige a Seção 1.

### 5.5 Direções, ponto de apoio e alinhamento entre quadros

- **Direções:** S, N e E desenhadas; W espelhada a partir de E (Seção 3). Elementos assimétricos (gola cruzada, fita) são conferidos depois do espelhamento.
- **Ponto de apoio:** todo quadro tem origem no **centro da base, (x 32, linha 59)** do quadro 64×64. Os pés pousam na linha 59; as linhas 60–63 são da sombra.
- **Pé de apoio travado:** entre quadros, o pé que sustenta o peso não se move nem 1 px; só o pé que dá o passo se desloca. Pé que escorrega sem dar passo é defeito de animação.
- **Alinhamento:** no repouso, a cabeça oscila no máximo 1 px (respiração). As trocas de estado acontecem com os pés travados, para o corpo não "pular" no quadro.
- **Verificação:** camada-guia (onion skin) no LibreSprite ou no Pixelorama, com linhas-guia na linha 59 e na coluna 32 em cada arquivo.

### 5.6 Detalhe na distância de jogo (filosofia de LOD)

- **Uma só escala, sem níveis de detalhe:** o que se desenha é o que se vê a 1×.
- **Sobrevive:** silhueta, pés com sombra de contato, lâmina com ponta e cabo, gola cruzada, faixa, aba do chapéu, perneiras.
- **Sai:** dedos, traços do rosto além dos olhos, bordados, padrões de tecido, textura de palha (o chapéu é massa de 2–3 tons).
- **Regra:** um grupo de pixels menor que 2×2 só existe se carrega uma das três linhas de leitura.

*Teste:* apagar o detalhe muda a resposta ao teste dos 3 segundos (Seção 3, 4.5)? Se não muda, ele sai.

### 5.7 A pegada segue a arma

*Serve aos Pilares 2 e 4.* Origem: o criador quer outras armas no futuro (espada de duas mãos, lança, foice, machado, estrelas de arremesso), 2026-11-25.

- **Arma de uma mão** (espada reta): a mão livre fica visível e separada da lâmina — no repouso, sobre a faixa; na guarda, deitada no peito ou fora do corpo.
- **Arma de duas mãos** (espada longa, lança, foice): as duas mãos no cabo, e a distância entre elas acompanha o comprimento do cabo.
- **Arma de arremesso:** a mão que arremessa fica livre e visível em todos os estados.
- Em qualquer arma, as três linhas de leitura (silhueta, pés, arma) e as regras da lâmina (3.2: inclinações limpas, ponta e cabo diferentes, nunca vertical em repouso) continuam valendo; armas de haste longa podem exigir a camada de transbordamento (8.11).

*Teste:* em preto chapado, dá para dizer com quantas mãos o ator segura a arma. Se não dá, a pose muda.

Quais armas existem é decisão de design (sistema de combate); esta seção só fixa como elas se leem.

## 6. Environment Design Language

### 6.1 Arquitetura e cultura: duas eras se leem pelo material

*Serve ao Pilar 3.* O mundo tem duas camadas de história, e o material diz qual é qual.

| Era | O que é | Material e forma |
|---|---|---|
| **Era dos mestres antigos** | túmulos, estátuas, ruínas | pedra em cantaria pesada; arestas gastas, arredondadas por 1 px de erosão |
| **Era atual** | seitas, postos, pontes | madeira escura e telha, mais leve; pilares ≥ 4 px, telhados em massa (Seção 3, Princípio 5) |

**Regra-chave: as marcas gravadas nunca se desgastam.** A pedra em volta erode; o arco-linha mantém a borda limpa. O ensinamento sobrevive ao tempo, e é isso que faz o corte destoar da ruína.

*Teste:* um elemento antigo com aresta perfeita só existe se for uma marca para aprender.
*Descartado:* palácios ornamentados e dourados — gastariam o âmbar e poluiriam a leitura.

### 6.2 Textura: pixel desenhado à mão, chapado em grupos

- Cada material tem 2–3 tons em grupos chapados. Sem textura fotográfica, sem PBR, sem gradiente suave; pontilhado ordenado só em grandes gradientes de ambiente (Seção 4.8).
- O ambiente não tem contorno (Seção 3).
- Cada tile de chão tem **3–4 variações**; nenhuma linha de textura se repete a cada 32 px, para a grade não aparecer.

*Por que:* o contraste baixo do mundo põe atores e pistas na frente, e o custo cabe num projeto solo com ferramentas gratuitas.
*Descartado:* pintura digital suave sobre os tiles — brigaria com o contorno de 1 px dos atores e exigiria outro pipeline.

### 6.3 Densidade de objetos por área

| Área | Densidade | Regra |
|---|---|---|
| Trilha de montanha | esparsa | rochas e pinheiros nas bordas da tela; centro aberto |
| Vale de bambu | média-alta, em **touceiras** | touceiras de 3–7 colmos com **corredores ≥ 1 tile** entre elas; dentro e atrás das touceiras se passa (6.5) |
| Ponte do rio | esparsa, horizontal | pilares e reflexos; nada vertical alto no tabuleiro |
| Túmulo | esparsa | objetos só ao longo do feixe de luz até a pista |
| Arena de duelo | vazia no centro | **raio ≥ 4 tiles sem objetos**, para base dos pés e lâmina ficarem limpas |

**Regra das pistas:** a menos de 2 tiles de uma pista, nada de cenário com linha parecida com o arco-linha, e nada mais alto que um ator entre a pista e o caminho de chegada.

### 6.4 Narrativa ambiental: rastro, não texto

*Serve ao Pilar 3 e ao anti-pilar do texto longo.*

- A história se conta por **vestígios**: cortes de luta antigos em árvores e bambu (traço **irregular e partido**, para não parecer técnica), objetos deixados para trás (a metade do manual no corpo do discípulo), chão de pedra gasto em volta de pontos de treino, estandartes de seita desbotados.
- **Só o arco-linha é aprendível.** Todo outro vestígio conta história, mas não ensina golpe.

*Teste:* o vestígio se entende sem legenda? Se precisa de texto, vira manual ou inscrição (com ciano), não decoração.

### 6.5 Atores atrás do cenário e travessia de água

Origem: retorno do criador sobre o protótipo (2026-11-25) — o bambuzal bloqueia por inteiro e não se passa por trás nem por dentro dele; a água ainda não pode ser atravessada.

- Cenário mais alto que um ator (bambu, pinheiro, telhado) se divide em **base** e **corpo**:
  - **Base:** ≤ 1 tile por touceira, com contato escuro no chão. É a única parte sólida e mostra onde se bate.
  - **Corpo** (colmos, folhas): o ator passa por trás.
- **A silhueta de um ator nunca fica mais de 50% escondida pelo cenário.** Atrás do cenário, a silhueta aparece através dele; a técnica (contorno desenhado por cima ou cenário translúcido) é decidida na Seção 8.
- O tamanho exato da colisão é decisão de jogabilidade e técnica; fica registrado como dependência para o design do mundo.
- **Água:** a travessia é decisão de design (provavelmente uma técnica de movimento). Regra visual: todo ponto de travessia se lê **pela linha** — pedras com contorno geométrico, uma pegada gravada na margem —, nunca só pela cor.

## 7. UI/HUD Visual Direction

Esta seção tem duas visões, escritas separadamente: a de **arte** e a de **UX**. O conflito entre elas e a decisão do criador estão em 7.12.

**Desvios do protótipo a corrigir** (HUD atual, iteração 1): barra de vida do inimigo em ocre (viola 4.3 e 4.6); seleção dos menus e sublinhado do HUD em ciano (viola 4.3); fonte Courier New (viola 7.3).

### Visão de arte

#### 7.1 Duas camadas de interface

- **Durante o jogo, o HUD é tinta sobre o mundo:** na tela (screen-space), nos cantos, com traços de pincel em tinta `#2a2118` translúcida e texto cor de pergaminho. Sem painel claro — seria a coisa mais clara da tela e brigaria com o ciano-branco.
- **Menus** (livro de técnicas, inventário, mapa, teclas): rolo de pergaminho claro que ocupa a tela (Seções 2 e 4.6).
- **Sem HUD diegético no MVP:** vida e qi no corpo do personagem cobririam a silhueta (Seção 1, Princípio 2).

*Teste:* um elemento de HUD durante o jogo é mais claro que o ciano-branco ou cobre um ator? Se sim, sai.

#### 7.2 Alvo travado

- **Quatro marcas de canto em tinta** em volta do ator travado; nunca um círculo sob os pés, que esconderia a base.
- No alto, ao centro: nome do alvo e barra de vida **neutra** (tinta e pergaminho, nunca ocre).
- Rótulo de estado: ver a decisão em 7.12.

#### 7.3 Tipografia

- Fonte **bitmap pixel de licença livre** (OFL ou equivalente) com os acentos do português (ã, â, ç, é, ê, õ, ú). Sai a Courier New.
- Hierarquia em px nativos (640×360):

| Nível | Tamanho | Uso |
|---|---|---|
| Nome de arte | 16 px | letras de pincel desenhadas como imagem (conjunto limitado) |
| Título de menu | 12 px | títulos de painel e de capítulo do livro |
| Texto | 8–9 px | corpo de texto, descrições |
| Mínimo absoluto | 8 px | dicas de tecla, contadores |

- O pincel dá a textura do traço; as letras continuam legíveis, sem caligrafia cursiva.

#### 7.4 Ícones

- **Técnicas:** o arco a nanquim — mesma curvatura, sentido e ponto de partida da pedra (o livro repete a pedra).
- **Itens:** silhueta preenchida de 16×16, contorno de tinta de 1 px, 2–3 tons.
- **Teclas:** tecla desenhada, a única forma "moderna" (Seção 3, Princípio 6).
- **Reconhecimento:** selo vermelho quadrado.

#### 7.5 Animação da interface

- **Menus:** traço de pincel que se desenha (0,15–0,25 s); rolo que se abre (~0,2 s).
- **Selo:** aparece de uma vez, com 1 quadro de impacto.
- Sem quique ou elasticidade. **No combate, o HUD muda instantaneamente.**

### Visão de UX

#### 7.6 Encaixe com a interação

O combate exige leitura rápida de pose. **Nada de interface persistente nos 60% centrais da tela**: HUD nos cantos, alvo travado no alto ao centro. Avisos rápidos duram no máximo 1,5 s e nunca ficam sobre o jogador ou o alvo.

#### 7.7 Legibilidade

- Janela mínima de 1280×720 (escala 2×): o texto de 8 px vira 16 px na tela — é o piso.
- Opção de **texto maior** (segundo tamanho da fonte bitmap) nas configurações.
- Contraste mínimo de texto: 4,5:1 (Seção 4.9).

#### 7.8 Acessibilidade

- **Cor nunca sozinha.** A barra de qi do protótipo muda só de cor quando há qi para uma técnica; ela precisa de uma **marca no limiar de custo**. A vida em losangos (forma) já está certa.
- **Lampejos** (ocre, revelação) não piscam mais de 3 vezes por segundo.
- **Tremor de tela e congelamento no impacto** reduzíveis nas opções.
- **Controle:** só teclado, com teclas remapeáveis (já no protótipo).

#### 7.9 Retorno

O jogador sempre sabe o que aconteceu e por quê: golpe que acertou, guarda que bloqueou, técnica reconhecida (selo), falta de qi (marca de limiar + som).

#### 7.10 Posição da UX sobre o rótulo de estado

Manter o rótulo de estado do alvo ("guarda fechada", "guarda quebrada"): dá retorno claro, ajuda quem tem dificuldade de ler poses (acessibilidade cognitiva) e ensina o nome da guarda, servindo ao Pilar 3 (o mundo nomeia).

#### 7.11 Posição da arte sobre o rótulo de estado

Sem rótulo: o texto puxa o olho para longe do corpo e faz ler palavras em vez de poses, o que enfraquece o portão de conhecimento (Seção 1, "o corpo diz antes da cor").

### 7.12 Decisão do criador (2026-11-25)

**O rótulo de estado aparece só até o jogador aprender a guarda, com opção para mantê-lo sempre.** Nos primeiros encontros com cada guarda o rótulo aparece; depois some, e a pose fala sozinha. Uma opção de acessibilidade mantém o rótulo sempre visível. O critério exato de "aprendeu" (número de encontros, guarda quebrada uma vez etc.) é decisão de design do sistema de combate.

## 8. Asset Standards

Esta seção tem duas visões, escritas separadamente: a de **arte** e a **técnica**. Os conflitos entre elas e as decisões do criador estão em 8.11.

> **Motor e orçamento:** nenhum motor escolhido e nenhum orçamento de desempenho definido (`project.yaml` sem `engine` e sem `performance`; `technical-preferences.md` em "[TO BE CONFIGURED]"). A visão técnica não inventa números: o que depende do motor está marcado como **não confirmado** e deve ser revisto no `/setup-engine`.

### Visão de arte

#### 8.1 Ferramentas e arquivos-fonte

- Ferramentas gratuitas: **LibreSprite, Pixelorama, Krita**.
- Arquivos-fonte no formato da ferramenta (`.ase`, `.pxo`, `.kra`) em `assets/source/`; o jogo usa **PNG exportado** em `assets/`.
- Paleta `pal_master_64.gpl` (Seção 4.8) carregada em todas as ferramentas. **Todo pixel exportado pertence à paleta**; translucidez só em tempo de execução, na camada de efeitos.

#### 8.2 Imagens geradas por IA (ChatGPT)

- Servem como **referência ou rascunho**, nunca como asset direto.
- Toda imagem gerada é redesenhada ou limpa à mão na resolução nativa, reduzida à paleta e conferida com os testes da Seção 3, 4.5.
- A origem (ferramenta, data, prompt) fica registrada num arquivo de procedência junto aos assets.

#### 8.3 Nomes

`[categoria]_[nome]_[variante]_[direção]_[quadro].png` — minúsculas, ASCII, sem acento, sem espaço.

- Categorias: `char_`, `tile_`, `prop_`, `fx_`, `ui_`, `icon_`, `pal_`.
- Direção: `s`, `n`, `e` (o `w` é espelhado em tempo de execução, Seção 5.5).
- Exemplos: `char_swordsman_idle_s.png` (tira de quadros), `char_disciple_guard_e.png`, `fx_swordsman_cut_overflow_s.png`, `prop_statue_master.png`, `tile_grove_floor_02.png`, `pal_qi_jade_3.gpl`.

#### 8.4 Tamanhos por categoria

| Categoria | Tamanho |
|---|---|
| Tiles | 32×32 (3–4 variações por tile de chão, Seção 6.2) |
| Personagens | quadros de 64×64 (escolha da comparação de 2026-11-25; pode evoluir, ver 8.12) |
| Objetos grandes | múltiplos de 32 (estátua 64×96) |
| Ícones | 16×16 |
| Efeitos | 32×32 ou 64×64 |
| Painéis de UI | 9 partes (bordas de pincel não deformam ao esticar) |

Sem retratos no MVP.

#### 8.5 Folhas de sprite e origem

- Uma tira horizontal por animação e por direção, com células iguais.
- Origem de personagem no **centro da base, (32, 59)** (Seção 5.5).
- Objetos altos (bambu, pinheiro, estátua, telhados) têm a origem **na base**, para o jogo ordenar quem fica na frente pela posição do pé (y-sort). É o que permite passar por trás do bambu (Seção 6.5).

#### 8.6 Exportação e detalhe

- Exportar a 1× nativo, sem redimensionar, sem suavização.
- Sem níveis de detalhe (Seção 5.6).

#### 8.7 Validação da leitura no tamanho do jogo

1. Ver a 1× e a 2× na própria ferramenta.
2. Escala de cinza (Krita: filtro Dessaturar).
3. Preenchimento chapado: encher o ator de preto numa camada de teste (LibreSprite ou Pixelorama).
4. Simulador de daltonismo gratuito (ex.: Color Oracle).
5. Por último, **captura no protótipo**: bambuzal, mesma câmera e mesmo zoom (Seção 3, 4.5), guardada como evidência.

### Visão técnica

#### 8.8 Regras para qualquer motor 2D com pixel art

- Filtro de textura "mais próximo" (sem suavização), sem mipmaps, escala inteira.
- Câmera e sprites alinhados ao pixel, para não haver tremor de 1 px.
- PNG sem perda; sem compressão de textura com perda (borra o pixel art).

#### 8.9 Estimativa de memória (estimativa, não orçamento)

Um quadro de 64×64 RGBA ocupa ~16 KB. Um personagem com 3 direções e ~40 quadros por direção soma ~2 MB. Folgado em qualquer PC; **não há orçamento de memória de textura definido**.

#### 8.10 Não confirmado (depende do motor)

- Tamanho máximo de atlas — padrão conservador provisório: folhas ≤ 2048×2048.
- Importação de `.gpl` e de paleta indexada.
- Troca de paleta do qi por shader ou por índice (Seção 4.4).
- Técnica da silhueta através do cenário (8.11).

Rever tudo no `/setup-engine`.

### 8.11 Conflitos arte × técnica e decisões do criador (2026-11-25)

**1. Início de corte maior que o quadro.** Com a lâmina erguida, a arte passa da linha 0 do quadro de 64×64 (Seção 3, 3.1).
- *Ideal da arte:* quadros de ataque de 96×96, com a mesma origem nos pés — desenho inteiro num arquivo, mas duas grades de tamanho.
- *Padrão restrito:* todos os quadros em 64×64; o que transborda (ponta da lâmina, rastro) vira sprite de efeito separado.
- **Decisão: 64×64 + camada de efeito.** Grade única e simples; o transbordamento vai em `fx_[ator]_[animação]_overflow_[direção].png`, com a mesma origem (32, 59) do quadro do ator, para alinhar sem ajuste.

**2. Ator atrás do cenário (Seção 6.5).**
- *Ideal da arte:* cenário opaco e a silhueta do ator desenhada por cima — mantém a regra "translucidez só na camada de efeitos" (4.8). Exige shader ou desenhar o ator duas vezes, o que depende do motor.
- *Padrão restrito:* o corpo do cenário fica semitransparente perto do ator — funciona em qualquer motor, mas abre exceção à regra de translucidez.
- **Decisão: silhueta através do cenário.** A forma de implementar é confirmada no `/setup-engine`.

### 8.12 Direção da câmera e escolhas da comparação antes/depois (2026-11-25)

- **Direção do jogo: 2D, vista de cima em 3/4.** A câmera vê o topo da cabeça e dos ombros; os atores ficam de frente, de costas ou de lado sobre o chão. Isso vale para todos os assets.
- **O quadro de 64×64 (8.4) e a paleta atual (`pal_master_64.gpl`, Seção 4.8) são escolhas desta comparação** entre os sprites e as figuras do protótipo. A resolução e a paleta dos próximos assets podem evoluir depois da avaliação do criador jogando. Se mudarem, esta seção e a 8.4 são revistas.
- **Limpeza das imagens do ChatGPT (8.2):** feita por script gratuito (Python + Pillow) em `tools/asset-pipeline/clean_chatgpt_sprite.py`. O script tira o fundo, reduz à grade e à paleta, redesenha a lâmina e aplica as correções de pixel registradas nele. A conferência é feita por `tools/asset-pipeline/validate_sprite.py`. Os originais ficam intactos em `assets/source/raw/`.
- **Evidência:** `production/qa/evidence/2026-11-25_asset-001-002_bambuzal_*`. Prancha: `user-docs/relatorios/2026-11-25-antes-depois-bambuzal.html`.
- **Decisão do criador depois de jogar (2026-11-25): fica a aparência original do protótipo.** O protagonista e os inimigos continuam desenhados por código durante toda a partida, e o cenário do Vale de Bambus não muda. O experimento com os dois sprites foi encerrado: o jogo não carrega mais imagens de personagem, e um endereço antigo com `?sprites=1` mostra a aparência escolhida. Os dois PNGs ficam como histórico em `assets/art/historico/2026-11-25-experimento-sprites/`; a comparação e a prancha continuam onde estão. As regras de personagem desta bíblia (Seções 3 a 5) seguem valendo como direção para uma arte futura, mas não há produção de sprites agendada.

## 9. Reference Direction

As referências mostram **como resolver** um problema que este documento já definiu; não decidem **o que** resolver. Todas vêm do `game-concept.md` (decisão do criador). São cinco, cada uma com uma lição diferente: presença do corpo (*Contos de Cultivação de um Regressor*), separação figura-fundo (*Hollow Knight*), animação em pixel (*Castlevania: Symphony of the Night*), escala dos efeitos (*Noita*) e leitura da sequência de comandos (*Magicka*).

Nenhuma é pixel art de cima em 3/4 com 640×360, tiles 32×32 e quadros 64×64. Por isso cada lição vem com a sua **tradução**: o que muda quando sai do meio original e entra no nosso.

### 9.1 *Contos de Cultivação de um Regressor* (manhwa): o peso de quem treinou a vida inteira

*Apoia: Seção 3, Princípio 3; Seção 5.3; Seção 1, Princípio 2.*

**O que tomar:** a **distribuição de peso** e a **quietude antes do golpe** — o que separa quem luta de quem só segura uma espada:
- peso visivelmente num pé, cabeça sobre o pé de apoio;
- ombros soltos e girados, nunca de frente nem simétricos;
- lâmina parada num ângulo deliberado, já apontando para onde o corte vai sair;
- o momento parado antes do movimento, que dá autoridade ao mestre.

**Como colher:** separar 5–10 painéis em que a **postura** (não o rosto, nem o efeito) passa presença. Reduzir cada um a **silhueta em preto chapado** com ~46 px de altura, ao lado da tabela de repouso da Seção 3, 4.1. Só fica o que sobrevive: base dos pés, giro do tronco, ângulo da lâmina. Se a postura só funciona em cor e em tamanho grande, não serve.

**Tradução de meio:** o manhwa é ilustração colorida, de frente ou de lado, com close e enquadramento de cinema. Nosso personagem tem 46 px, é visto de cima em 3/4 e não tem boca. A presença passa para as **três linhas** (silhueta, pés, lâmina); expressão facial, cabelo ao vento e drama de câmera não passam.

**Evitar / divergir:**
- **Rostos, trajes, penteados, acessórios, armas, insígnias de seita:** nada entra. Os personagens são originais (Seção 3, Princípio 3).
- **Auras coloridas e brilho de qi por cima da figura:** violam "o qi nunca cobre a leitura" (Seção 1, Princípio 2) e a exclusividade de cor (4.3).
- **Linhas de velocidade, desfoque de movimento, manchas de impacto:** em pixel art viram ruído e competem com o arco-linha.
- **Pose heroica frontal e simétrica:** descartada para o título (Seção 2) e contrária ao "aberto e assimétrico" do jogador (Seção 3, 4.1).
- **Prompts de imagem (Seção 8.2)** descrevem a postura em palavras (peso no pé de trás, ombros girados, lâmina a ~27°); nunca levam o nome da obra, de personagens ou "no estilo de".

*Teste:* silhueta chapada do nosso personagem ao lado de um painel de referência. Alguém de fora deve dizer "os dois têm o mesmo peso", e **nunca** "é o mesmo personagem".

### 9.2 *Hollow Knight*: o mundo apagado, o ator nítido

*Apoia: Seção 2 (contraste baixo a médio no ambiente); Seção 4.2; Seção 3, Princípio 7.*

**O que tomar:** a **separação figura-fundo pelo valor**. O cenário fica numa faixa estreita, abafada, pouco saturada; a figura que importa tem o maior salto de valor da tela, concentrado num ponto pequeno. Uma figura pequena se lê no meio de muito cenário porque o cenário abre mão do contraste, não porque a figura é grande ou colorida. Segunda lição: **atmosfera em camadas planas** — névoa e poeira como faixas de um valor só, como a névoa da trilha e o feixe do túmulo (Seção 2).

**Tradução de meio:** *Hollow Knight* é visto de lado, desenhado à mão em alta resolução, com parallax e desfoque. Não temos parallax nem profundidade de campo: na vista de cima, a "distância" vira **borda da tela** — névoa e pinheiros escuros nas margens (Seção 6.3). A separação pelo valor se aplica tile a tile, nas faixas da Seção 4.2 (mundo L\* 20–60, sinais 50–88, conhecimento ≥ 90).

**Evitar / divergir:**
- **Mundo escuro e melancólico como regra.** Só o túmulo é escuro; trilha, vale e ponte são de manhã e de tarde (Seção 2). A lição é a faixa estreita, não o tom sombrio.
- **Atores como mancha escura sem contorno.** Os nossos têm contorno de 1 px `#16140f` (Seção 3, 3.2).
- **Traço vetorial suave, gradientes, brilhos difusos.** Nosso mundo é chapado em grupos de 2–3 tons (Seção 6.2).
- **Bestiário de insetos e máscara-ícone.** Nenhuma forma de personagem vem daí.

*Teste:* captura em escala de cinza — atores e pista são os pontos de maior contraste e o cenário nunca passa de L\* 60. Se um elemento de cenário chama o olho antes de um ator, ele perde contraste; o ator nunca ganha brilho para compensar.

### 9.3 *Castlevania: Symphony of the Night*: a pose-chave segura e o movimento que segue o corpo

*Apoia: Seção 5.3; Seção 5.5; Seção 8.11, decisão 1.*

**O que tomar:** a **economia da animação desenhada pixel a pixel**:
- poucos quadros, cada pose-chave **legível sozinha** e segurada o bastante para ser lida;
- capa e cabelo que **chegam depois do corpo** — peso sem deformar a silhueta;
- o arco da arma como **traço separado do corpo**, desenhado por cima do sprite.

É a lição de *como animar*; a de *como posar* é a 9.1.

**Tradução de meio:** SotN é visto de lado e o golpe corre no plano da tela. Na nossa vista 3/4, o mesmo corte se lê em **três direções desenhadas** (S, N, E) e uma espelhada. O atraso da capa vira atraso de **fita, coque, barra dividida e aba** (Seção 5.3). O traço da arma por cima do corpo vira `fx_[ator]_[animação]_overflow_[direção].png`, com origem (32, 59) (Seção 8.11).

**Evitar / divergir:**
- **Rastros de imagem fantasma do personagem:** efeito sem a forma da pedra; polui a camada do arco-linha (Seção 1, Princípio 1).
- **Capa ou manto que esconde os pés:** a base é a segunda linha de leitura (Seção 3, 4.3).
- **Densidade gótica e ornamental, roxos e azuis saturados:** nosso ambiente é esparso, terroso e sem contorno (Seções 6.2 e 6.3).
- **Arcos de arma diferentes por arma, em cores livres:** nosso rastro tem **uma só gramática** (arco de raio constante, 1–2 px) na cor do qi (Seção 1, Princípio 3).

*Teste:* passar a animação quadro a quadro. Cada pose-chave, congelada, passa no teste dos 3 segundos (Seção 3, 4.5); os elementos secundários nunca chegam antes do corpo; o pé de apoio não se mexe nem 1 px.

### 9.4 *Noita*: o efeito na mesma grade de pixels do mundo, e com teto

*Apoia: Seção 1, Princípio 2; Seção 2 (partículas ambientes); Seções 8.6 e 8.8.*

**O que tomar:** o efeito é feito do **mesmo pixel que o mundo**. Faíscas, poeira e respingos são pixels únicos na resolução nativa, nunca sprites suavizados de resolução maior por cima da cena. O efeito não parece "colado" e o jogo tem uma só textura de pixel.

**A contra-lição, tão importante quanto:** quando todo pixel pode virar efeito, a tela vira caos, e no caos nenhuma pose se lê. *Noita* mostra **até onde não ir**. Na revelação só três coisas ficam acesas (Seção 2); no duelo o vento para.

**Tradução de meio:** *Noita* é simulação física em vista lateral; aqui nada é simulado. Toma-se só a **regra de grade**: partículas de 1–2 px, presas à grade nativa de 640×360, na paleta-mestra (Seção 4.8), com translucidez só na camada de efeitos.

**Evitar / divergir:**
- **Muitas partículas, explosões, reações em cadeia:** no duelo as partículas param ou ficam bem mais lentas (Seção 2).
- **Cor por material ou elemento:** gastaria ocre, âmbar e ciano (Seções 4.1 e 4.3); elementos se expressam pela **forma** (4.4).
- **Efeito por cima do contorno de um inimigo em guarda:** nunca, em nenhuma camada.

*Teste:* captura do momento de maior efeito. Todo pixel de efeito está na grade nativa, sem meia escala, e a guarda do inimigo continua legível em escala de cinza. Se não continuar, o efeito diminui; a guarda nunca muda.

### 9.5 *Magicka*: a sequência de comandos aparece enquanto é digitada

*Apoia: Seção 3, Princípio 6; Seções 7.4, 7.6 e 7.9.*

**O que tomar:** a **fila de entrada visível**. Cada comando digitado aparece na hora como ícone numa fila curta, e o jogador vê a combinação se formar antes de soltá-la. É a referência mais próxima do nosso gancho (forma + qi + passo); a lição é de **leitura da interface**, não de estética.

**Tradução de meio:** *Magicka* é 3D visto de cima, com elementos como ícones coloridos. Aqui a fila usa **teclas desenhadas** em tinta sobre o mundo (Seção 7.1), sem painel claro, num canto da tela. A fila não usa cor para dizer o que é cada passo: cada passo tem **forma própria** (tecla, sentido do arco, pegada), como exige a Seção 4.7. Quando a combinação é reconhecida, respondem o selo vermelho e o ciano do rastro coincidente; a fila não muda de cor.

**Evitar / divergir:**
- **Uma cor por elemento:** é a paleta descartada na Seção 4.1.
- **Feixes e jatos largos e saturados:** o rastro é arco-linha de 1–2 px (Seção 1, Princípio 1).
- **Fila sobre a cabeça do jogador ou no centro da tela:** cobriria a silhueta e invadiria os 60% centrais (Seções 7.1 e 7.6).
- **Tom de paródia e caos cômico:** nossa vitória é "alívio contido, dignidade marcial" (Seção 2).

*Teste:* em escala de cinza e com simulador de daltonismo, o jogador diz qual sequência digitou só olhando a fila. A fila nunca é o elemento mais claro da tela (Seção 7.1).

### 9.6 Referências do conceito que ficam fora desta seção

| Referência | Por que fica fora |
|---|---|
| *Logando no Murim*, *Upando Infinitamente: Murim* | Apontam na mesma direção de *Contos de Cultivação de um Regressor*. Continuam referências de **universo** no `game-concept.md`; repeti-las aqui só aumentaria o risco de cópia. |
| *Vampire Survivors* | A lição visual (tela inundada de efeitos) é a contra-lição de *Noita* (9.4). A contribuição dele é de design. |
| *Minecraft* | Contribuição de design (objetivos escolhidos). O visual (blocos, textura repetida na grade) contraria a Seção 6.2. |

### 9.7 Como usar as referências

1. **Uma referência nunca derruba uma seção deste documento.** Se discordam, vale o documento. A referência só entra onde o documento deixa espaço.
2. **Toma-se a técnica, não a imagem.** Cada uso se escreve como "tomamos [técnica] de [referência] para cumprir [seção]". Se a frase não fecha, é imitação.
3. **Toda referência passa pela tradução:** escala do jogo (46 px de figura, 32 px de tile, 1× nativo), vista de cima em 3/4, paleta-mestra de 64 cores. Só o que sobrevive vale.
4. **Personagens: presença sim, design nunca.** Nenhum traje, rosto, penteado, arma, insígnia ou nome vem das obras — também nos prompts de imagem (Seção 8.2).
5. **Referências não aparecem no jogo nem em material público** como "no estilo de". Ficam na pasta de trabalho do criador, com a seção que cada imagem apoia.

### 9.8 Teste de design

**Teste da troca de referência:** cobrir o nome da referência e olhar só o asset pronto, no jogo, a 1×.

- **(a)** Dá para dizer qual regra deste documento ele cumpre? Tem de dar.
- **(b)** Quem conhece a obra diria "isto é tirado de [obra]"? Não pode dizer.

Se (a) falha, a referência trouxe estética sem função: a parte emprestada sai. Se (b) falha, a imitação passou da técnica para o design: muda-se traje, acessório ou forma — **nunca** a postura, os pés ou a lâmina que a regra exige.

### 9.9 Referências visuais do projeto (imagens do criador)

Imagens geradas pelo criador (ChatGPT) e salvas no projeto. Seguem a Seção 8.2: são referência ou rascunho, nunca asset direto. Registradas em 2026-11-25.

| Imagem | Prompt | O que tomar |
|---|---|---|
| `user-docs/assets/protagonista/image.png` | `user-docs/assets/protagonista/promp.md` | Protagonista: **túnica marfim de gola cruzada**, **faixa jade** (reduzida ao jade gasto), **fita no cabelo**, **espaço visível entre a espada e o corpo**, calça carvão, barra dividida, botas simples |
| `user-docs/assets/mundo/image.png` (mapa do mundo à esquerda, cenário inicial à direita) | `user-docs/assets/mundo/mundo-promp.md` (**vazio** em 2026-11-25) | **Ambientação e expansão do vale:** trilha que se divide, rio com margem de pedras, ponte de madeira, bambuzal nas bordas, estátua da mestra com espada ao lado da porta do túmulo, abrigo de madeira; no mapa, a seita na montanha, o portão, a aldeia e os arrozais |

Prioridade desta etapa: **os personagens**. O cenário atual do protótipo agrada ao criador; a imagem do mundo orienta a ampliação, não uma troca.

**Traduções obrigatórias (o que não vem das imagens):**

*Protagonista:*
- A pose é adaptada à câmera 3/4 e à escala do protótipo (46 px, Seção 3, 3.1).
- Na imagem, os pés estão abertos como numa **guarda**. O repouso usa base de 9–10 px, com o peso no pé de trás (Seção 3, 4.1).
- Na imagem, a lâmina está quase **horizontal**, apontando para fora. O repouso usa ~27° da vertical, com a ponta 3–5 px acima do chão; o espaço entre a lâmina e a perna é mantido.
- O verde da faixa e da fita é mais vivo que o jade gasto. Reduzir a `#4f7a64` / `#3a5a4b` (Seção 4.4).
- Rosto detalhado, dobras e mangas largas se simplificam a 1× (Seção 5.6).

*Mundo:*
- O rio está em **turquesa saturado**. A água do jogo é dessaturada e fora da família do ciano (Seção 4.5).
- O bambu está amarelo-esverdeado vivo. O mundo fica em saturação ≤ 45% e L\* 20–60 (Seção 4.2).
- O brilho ciano na porta do túmulo é coerente: ciano = conhecimento (Seção 4.3).
- A estátua ganha os pés em passo lateral e a pegada gravada (Seção 3, 4.4).
- O ambiente não tem contorno (Seção 6.2). O bambuzal segue a regra de base sólida e corpo passável (Seção 6.5).
- A paleta laranja das montanhas distantes no mapa não entra no jogo; o mapa do jogo é desenhado à mão em pergaminho (Seção 2).
