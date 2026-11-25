# Game Brief: SpiritOfMurin

## In your words

O projeto se chama SpiritOfMurin. Quero criar um RPG de ação 2D de mundo aberto para PC, com temática Murim. O protagonista é um espadachim cultivador que combina artes da espada e feitiçaria. Seu objetivo de longo prazo é alcançar o Dao da Espada.

Quero que a exploração seja uma forma de aprender. O jogador pode encontrar técnicas em tesouros antigos, recuperar manuais nos corpos de inimigos derrotados, investigar túmulos de mestres e entrar em masmorras que pertenceram a cultivadores de outras eras. Cada grande descoberta deve mudar alguma coisa que o jogador consegue fazer com a espada ou com sua energia espiritual.

Uma cena que representa o jogo: o espadachim encontra metade de um manual no corpo de um discípulo. Uma anotação na margem indica o túmulo de sua mestra. Lá dentro, marcas de espada na pedra revelam o movimento que falta. Ao sair do túmulo, o jogador já consegue usar esse golpe em combate.

A primeira versão jogável deve demonstrar, em uma pequena região, o ciclo de explorar, lutar, descobrir uma técnica e usá-la com as próprias mãos.

Imagino enfrentar um discípulo que fecha a guarda toda vez que avanço. Tento vencê-lo de frente, ele apara meus golpes e sou obrigado a recuar. Mais tarde, no túmulo de uma mestra, encontro marcas de pés ao lado de um corte na pedra. A metade do manual que carrego descreve o golpe, mas só ali entendo onde ela pisava para executá-lo. No duelo seguinte, outro espadachim assume aquela mesma guarda. Dessa vez eu reconheço a postura, dou o passo de lado que aprendi no túmulo e acerto o flanco.

Vamos começar com uma região pequena que eu possa jogar no meu PC. Meu primeiro marco é entrar em um duelo, encontrar o manual dividido, explorar o túmulo da mestra e voltar para testar o golpe que aprendi. Quero sentir se é divertido encadear o passo, o corte e o qi com minhas próprias mãos.

1. Defesa: opção 3 — inicialmente, apenas movimentação e recuo para evitar ataques. Não quero uma esquiva lateral ou um bloqueio perfeito que já consiga superar a guarda inimiga. O passo lateral aprendido no túmulo deve representar uma descoberta importante, mudando realmente as possibilidades de combate.
2. Sensação do duelo: opção 1 — duelista, com poucos inimigos, ataques bem definidos, posturas reconhecíveis e aberturas importantes. Entretanto, quero que o sistema já permita encadear técnicas e cancelamentos, pois futuramente teremos estilos de combate rápidos e combos longos.
3. Derrota: opção 1 — retornar ao início da área com a vida cheia, preservando conhecimentos e descobertas. Quero poder repetir os duelos rapidamente para aprender os padrões.
4. Teclas: utilizar WASD para movimentação, J para corte, K para qi, L para passo/esquiva e Espaço para travar no alvo. O passo lateral especial em L será desbloqueado ao aprender a técnica. Os comandos direcionais devem continuar relativos ao inimigo travado.
5. Inimigos: por enquanto, não adicionar um terceiro tipo. Prefiro concentrar o protótipo nos dois encontros principais e melhorar a qualidade do combate, das posturas e da inteligência dos inimigos. Podemos adicionar o discípulo que lança qi na próxima etapa.
6. Combinação nomeada: aprovo Passo da Lâmina Silenciosa, formada pela combinação de passo lateral + corte + qi. Quero que a descoberta tenha feedback visual e sonoro e que o jogo reconheça e registre a técnica quando o jogador conseguir executá-la.

Diretriz principal: o jogador precisa perceber uma diferença clara entre lutar antes e depois de aprender o passo lateral. A descoberta deve mudar o que ele consegue fazer, não apenas aumentar seu dano. Mantenha o protótipo pequeno, mas realmente jogável. Priorize a qualidade dos controles, o impacto dos golpes, a inteligência das posturas e a liberdade de experimentar sequências.

**Pitch:** Um espadachim cultivador perde para uma guarda que não sabe vencer, segue meio manual até o túmulo da mestra, lê nas pegadas da pedra o passo que faltava e volta para vencer essa guarda com as próprias mãos.

**Genre:** aventura de ação em visão de cima + ação de personagem (duelo Murim) *(inferred (strong))*

**Controls:** WASD anda · J corte (encadeia até 3) · K feitiçaria de qi (gasta qi) · L passo: antes do túmulo, um passo curto para frente ou para trás em relação ao alvo, sem invencibilidade *(inferred (strong))*; depois do túmulo, A/D + L dá o passo lateral que contorna o alvo · Espaço trava/destrava no alvo; com a trava, "frente" é em direção ao inimigo · E examina corpos, marcas e inscrições *(guessed)* · Tab abre o Registro de técnicas *(guessed)*

**Question:** Lutar depois de aprender o passo lateral parece claramente diferente de lutar antes, e encadear passo, corte e qi com as mãos é divertido?

**Hooks:** a técnica é aprendida lendo o mundo (pegadas ao lado de um corte na pedra) e é a resposta exata a uma postura que o jogador já viu e não conseguia vencer; a combinação é descoberta com as mãos, e o jogo a reconhece, nomeia e registra.

**Intent:** staged failure *(you said: o primeiro duelo de frente é para ser perdido)*

## Look and references
- Look: pixel art "Todo Golpe Deixa Marca": o rastro do golpe do jogador tem a mesma forma do corte gravado na pedra do túmulo. Mood: descoberta contida, montanha calma quebrada por rajadas curtas e nítidas de combate. Camera and scale: visão de cima 3/4, resolução nativa 640×360, escala inteira, sem suavização, câmera sem subpixel. Shapes: personagens angulosos de pose exagerada; cada guarda com silhueta própria, legível em 1–2 quadros (guarda fechada = lâmina cruzada na frente do corpo); arquitetura retilínea e pesada; cortes na pedra retos e limpos entre rochas orgânicas. Values: jogador e inimigos com o maior contraste; chão e fundo numa faixa estreita. Palette: terra e jade dessaturados no ambiente; ciano-branco = conhecimento que se aprende (pegadas, cortes, inscrições, a anotação do manual); vermelho-ocre = intenção hostil e telegraph; âmbar = tesouro; a cor do qi do jogador é azul-jade; sombras mais frias, luzes mais quentes. Detail: nas silhuetas das guardas, nas marcas do túmulo e no rastro do golpe; nada de detalhe atrás da luta. Rules: uma densidade de pixel, contorno escuro de 1 px nos personagens, sombreamento em 2 tons. Signature: o rastro ciano-branco que o passo lateral e o corte deixam no chão, igual às marcas do túmulo. O qi nunca cobre a silhueta do inimigo em guarda. Every figure drawn, no emoji, no plain circles.
- References: Castlevania: Symphony of the Night (o comando aprendido executado no meio da luta; voltar e "agora eu consigo passar"), Zelda visto de cima (o portão visto antes da chave), Sifu e jogos de duelo marcial (guarda e postura legíveis), manhwa de cultivação (o legado de mestres de outras eras).
- View: 2D · Camera: visão de cima 3/4 que segue o jogador; com a trava, enquadra o jogador e o alvo.

## First minute
O espadachim começa numa trilha de montanha. Andando com WASD, ele passa por uma fenda na rocha larga demais para atravessar; do outro lado brilha algo âmbar (o portão é visto logo). A trilha chega a uma ponte guardada pelo Discípulo da Guarda Fechada. Uma fala curta: "Sua lâmina não passa pela minha guarda." O jogador trava no alvo com Espaço e ataca com J: cada corte frontal é aparado, com faíscas e um empurrão para trás. Tentar dar a volta andando não adianta, porque o discípulo gira e mantém a guarda de frente. Quando o golpe dele brilha em vermelho-ocre, o jogador recua com S ou com o passo curto. Em menos de um minuto ele entende que, de frente, essa guarda não cai, e vê a trilha lateral que desce para o vale.

## Challenge
A primeira dificuldade real chega em menos de 1 minuto: o Discípulo da Guarda Fechada não pode ser vencido de frente. Perder custa pouco: volta ao início da área com a vida cheia, mantendo o manual, o passo aprendido e as técnicas registradas. Depois do túmulo, o passo lateral abre o flanco do discípulo por uma janela curta antes de ele girar. O duelo final é contra o Espadachim Sênior, que alterna a guarda fechada com uma postura de ataque (vermelho-ocre) e gira mais rápido. Ele exige ler qual postura está ativa e escolher entre recuar e contornar. Nunca fácil demais, nunca um muro sem resposta.

## Core loop
- Lutar: travar no alvo, ler a postura, encadear passo, corte e qi; o que é frontal bate na guarda.
- Explorar quando a luta não tem saída: examinar corpos, marcas e inscrições, que dão fragmentos de técnica (meio manual, pista para o túmulo).
- Descobrir: no túmulo, pisar nas pegadas ao lado do corte na pedra ensina o passo lateral, e o golpe passa a existir nas mãos.
- Voltar e usar: a mesma guarda agora tem resposta; a fenda vista no começo agora se atravessa; combinações novas são reconhecidas e registradas.
- Leads: a ação de personagem comanda os controles, a trava, os combos, os cancelamentos e as regras do duelo; a aventura de ação comanda o mundo, os portões e a derrota (voltar ao início da área).

## Goal & fail state
- Vitória: vencer o Espadachim Sênior no acampamento, depois de vencer o Discípulo da Guarda Fechada na ponte.
- Derrota: a vida chega a zero; o jogador volta ao início da área com a vida cheia, mantendo conhecimento e descobertas; os inimigos voltam com a vida cheia.

## Features
- [x] Combate encadeado com trava no alvo: corte em cadeia de 3 (J), onda de qi (K, gasta qi), passo (L), cancelamentos entre eles com janelas e buffer; comandos relativos ao alvo; impacto com hitstop, tremor de tela e som
- [x] Guarda fechada legível: golpes frontais são aparados (faísca, empurrão), o flanco e as costas não; o inimigo gira para manter a guarda com velocidade limitada; ataques telegrafados em vermelho-ocre
- [x] 2 tipos de inimigo, cada um atacando de um jeito: o Discípulo da Guarda Fechada (ponte) e o Espadachim Sênior (acampamento), que alterna a guarda fechada com uma postura de ataque
- [x] A cadeia de descoberta: corpo de discípulo com meio manual → anotação na margem → túmulo da mestra → pegadas ciano-brancas ao lado do corte na pedra → aprender o passo lateral, com um momento de revelação (pausa, som, nome)
- [x] Portão de conhecimento: a fenda vista no começo só se atravessa com o passo lateral e leva a um tesouro âmbar ou a um atalho
- [x] Passo da Lâmina Silenciosa: passo lateral + corte + qi dentro da janela é reconhecido, nomeado na tela com som próprio e registrado; o resultado é diferente (o qi segue o corte e quebra a guarda)
- [x] Registro de técnicas (Tab): cada técnica e combinação com o lugar onde foi aprendida ou descoberta; derrota volta ao início da área mantendo tudo

## Your systems
- combate encadeado (passo, corte, qi, cancelamentos) · in the slice
- posturas e guardas dos inimigos · in the slice
- descoberta de técnicas pela exploração (manual, túmulo, marcas) · in the slice
- combinação nomeada e registrada · in the slice
- portão de conhecimento · in the slice *(guessed)*
- níveis e experiência · later
- reinos de cultivo · later
- fornalha e alquimia · later
- discípulo que lança qi · later

## Keep
-

## Avoid
- Esquiva lateral ou bloqueio perfeito antes do túmulo: no começo, só movimentação e recuo
- O passo lateral como simples aumento de dano: ele precisa mudar o que o jogador consegue fazer
- Terceiro tipo de inimigo neste protótipo
- Textos longos que interrompem o jogo; marcadores apontando o caminho
- Emoji ou formas simples no lugar de figuras desenhadas

## Out of scope
- Fornalha, pílula e alquimia: próxima etapa (add it as a feature)
- Discípulo que lança qi à distância: próxima etapa (add it as a feature)
- Níveis e experiência; reinos de cultivo e avanços: grow it
- Um chefe com fases: change it after play
- Vários túmulos e masmorras, mapa do mundo e viagem rápida: grow it
- NPCs que citam as artes do jogador: add it as a feature
- Novas técnicas com o tempo, além do passo lateral: add it as a feature

## Build order
1. A fatia: trilha → Discípulo da Guarda Fechada na ponte (invencível de frente) → corpo com meio manual → túmulo da mestra → pegadas → passo lateral → vencer o discípulo → vencer o Espadachim Sênior. Derrota volta ao início da área com a vida cheia.
2. O Passo da Lâmina Silenciosa reconhecido e o Registro de técnicas.
3. A fenda como portão de conhecimento; ajuste de impacto, janelas de cancelamento e giro dos inimigos.

---
**Kill condition:** se, depois de duas rodadas de ajuste, lutar antes e depois do passo lateral não parecer diferente. *(guessed)*

**Who it's for / what they feel:** o próprio criador, explorador que repete até aprender o padrão; sente "agora eu entendi". *(inferred (strong))*

**Games you love / don't:** Love Castlevania: Symphony of the Night (voltar com algo novo e entrar; magia por comando), Minecraft (escolher os próprios objetivos), Vampire Survivors (combinações que evoluem e mudam o que se procura).
