# Game Concept: SpiritOfMurin

*Created: 2026-11-24*
*Status: Draft*
*Conceito escolhido no /brainstorm: "Mil Caminhos do Dao"*
*Terminologia (2026-11-27, pedido 14): a energia espiritual se chama **Ki**. Documentos e relatórios anteriores a essa data dizem "Qi"; é a mesma coisa.*

---

## Elevator Pitch

> SpiritOfMurin é um RPG de ação 2D de mundo aberto em que você, um espadachim
> cultivador, encontra fragmentos de técnicas em túmulos, manuais e marcas na
> pedra, e os combina com as próprias mãos em artes que ninguém lhe ensinou, em
> busca do Dao da Espada.

**Cena de referência (o jogo em uma cena):** o espadachim enfrenta um discípulo
que fecha a guarda toda vez que ele avança e é obrigado a recuar. Mais tarde,
encontra metade de um manual no corpo de um discípulo; uma anotação na margem
aponta para o túmulo da mestra. Lá dentro, marcas de pés ao lado de um corte na
pedra revelam onde ela pisava. No duelo seguinte, outro espadachim assume a
mesma guarda; o jogador reconhece a postura, dá o passo lateral aprendido no
túmulo e acerta o flanco. O personagem aprendeu uma arte; o jogador entendeu
quando e por que usá-la.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | RPG de ação 2D de mundo aberto; exploração com portões de conhecimento (metroidvania em visão de cima); combate encadeado por sequência de comandos |
| **Platform** | PC (teclado; movimento em WASD) |
| **Target Audience** | Exploradores e mestres que aprendem padrões (ver Player Motivation Profile) |
| **Player Count** | Single-player |
| **Session Length** | 30–120 min, com objetivo escolhido pelo jogador |
| **Monetization** | Nenhuma: projeto pessoal (hobby), sem objetivo comercial |
| **Estimated Scope** | Large (visão completa: vários anos, solo com desenvolvimento assistido pelo Claude; sem prazo fixo, desenvolvido em etapas jogáveis) |
| **Comparable Titles** | Castlevania: Symphony of the Night, Hollow Knight, Magicka, Noita (vendas: not researched) |

---

## Core Fantasy

Ser o cultivador que **entende** o que outros apenas copiam e, a partir disso,
**cria o próprio estilo marcial e é lembrado por ele**. Cada túmulo, manual e
inscrição é uma conversa com mestres de outras eras; cada combinação que você
descobre é sua, e o mundo passa a chamá-la pelo nome.

*Por que esta fantasia:* une as três experiências que o criador quer na mesma
jornada (desafio e maestria, descoberta, expressão). *Alternativa descartada:*
"dominar o combate e vencer o impossível" (soulslike puro), que perderia a
descoberta e a expressão.

---

## Unique Hook

É como **Symphony of the Night**, **e também** cada técnica encontrada é uma
peça (forma de espada, elemento de ki, passo) que você combina **com as próprias
mãos, por sequência de comandos**, em artes que o jogo reconhece e nomeia —
e as passagens que se abrem não são só portas, são também **posturas inimigas**
que você aprendeu a ler.

*Posicionamento:* a referência mais próxima do "combinar por sequência" é
**Magicka** (elementos combinados por teclas). O diferencial de SpiritOfMurin é
a camada forma + ki + passo, o reconhecimento nomeado de combinações lendárias,
e o vínculo direto entre descoberta no mundo (túmulos, marcas na pedra) e o que
o jogador consegue fazer nas mãos.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Expression** (self-expression, creativity) | 1 | Combinações sistêmicas de forma + ki + passo; estilo próprio reconhecido e nomeado pelo mundo; cor do ki muda conforme o caminho |
| **Discovery** (exploration, secrets) | 2 | Técnicas em túmulos, manuais, corpos e masmorras; portões de conhecimento; combinações lendárias escondidas por pistas fragmentadas |
| **Challenge** (obstacle course, mastery) | 3 | Inimigos e chefes com posturas legíveis; padrões aprendidos por repetição; execução de sequências |
| **Fantasy** (make-believe, role-playing) | 4 | Ser um cultivador Murim que avança por reinos rumo ao Dao da Espada |
| **Sensation** (sensory pleasure) | 5 | Game feel do encadeamento (hitstop, impacto, cancelamentos); rastro do golpe igual ao corte na pedra |
| **Narrative** (drama, story arc) | 6 | História contada pelo ambiente (legado de mestres mortos); NPCs que reagem às suas artes |
| **Submission** (relaxation, comfort zone) | 7 | Sessões de coleta de ervas e fornalha (visão completa) |
| **Fellowship** (social connection) | N/A | Single-player |

### Key Dynamics (Emergent player behaviors)

- O jogador **anota mentalmente** passagens que ainda não consegue atravessar e
  volta por conta própria depois de aprender algo ("agora eu consigo passar por ali").
- O jogador **observa posturas** inimigas e procura no mundo a resposta para a
  guarda que o derrotou.
- O jogador **experimenta combinações** fora do roteiro ("e se eu juntar esse
  passo com a feitiçaria de ki que já conheço?") e volta a regiões antigas para
  testá-las.
- O jogador **procura peças específicas** porque elas completam uma combinação
  que ele imagina (efeito Vampire Survivors: o item ignorado vira o desejado).
- O jogador **repete** um chefe ou uma pílula até entender o porquê da falha.

### Core Mechanics (Systems we build)

1. **Combate encadeado por sequência de comandos** — passo, corte, feitiçaria
   de ki e cancelamento; comandos direcionais relativos ao alvo travado (trava
   suave); buffer de input e janelas de cancelamento definidos em dados.
2. **Posturas legíveis** — inimigos com guardas reconhecíveis primeiro pela
   silhueta; cada guarda tem uma resposta aprendível.
3. **Sistema de combinação híbrido** — base sistêmica (forma + elemento de ki +
   passo → resultado, por regras) e combinações lendárias autorais escondidas,
   reconhecidas e nomeadas quando executadas.
4. **Exploração como aprendizado** — fragmentos (páginas de manual, inscrições,
   pegadas e cortes na pedra) concedem peças; portões de conhecimento no mundo.
5. **Progressão dupla** — níveis por experiência (corpo) e reinos de cultivo com
   avanços conquistados que desbloqueiam algo nas mãos (um cancelamento, um passo,
   um elemento de ki). Alquimia (fornalha operada à mão, falha legível) como
   segundo caminho de cultivo, introduzida em etapas.

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** (freedom, meaningful choice) | O jogador escolhe o objetivo de cada sessão, o caminho (espada, alquimia), o quanto se dedica a cada arte e o próprio estilo de combinação | Core |
| **Competence** (mastery, skill growth) | Vencer a guarda que antes o derrotava; executar a sequência difícil; conquistar o avanço de reino; entender por que a pílula falhou | Core |
| **Relatedness** (connection, belonging) | Mestres vivos que respondem às suas dúvidas; mestres mortos cujo legado você lê na pedra; NPCs e rivais que reconhecem suas artes pelo nome | Supporting |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** (goal completion, collection, progression) — How: reinos de cultivo, avanços conquistados, coleção de peças e combinações lendárias, chefes vencidos por maestria
- [x] **Explorers** (discovery, understanding systems, finding secrets) — How: túmulos, manuais, portões de conhecimento, regras sistêmicas de combinação a descobrir (**tipo primário**)
- [ ] **Socializers** (relationships, cooperation, community) — How: N/A (single-player); a relação é com mestres e rivais do mundo
- [ ] **Killers/Competitors** (domination, PvP, leaderboards) — How: N/A

**Secundário (Quantic Foundry):** Criador — inventar um estilo próprio e vê-lo reconhecido.
**Não é para:** quem quer ação imediata sem aprendizado, quem depende de marcadores apontando o caminho, quem busca história linear muito narrada.

### Flow State Design

- **Onboarding curve**: os primeiros minutos ensinam pelo fracasso: o duelo
  contra o discípulo de guarda fechada é perdido de frente. O mestre dá uma pista
  curta; nenhum tutorial longo.
- **Difficulty scaling**: novas posturas e combinações de posturas; inimigos que
  exigem a resposta certa em vez de mais dano. Atributos de nível são bônus, nunca
  o que decide o encontro.
- **Feedback clarity**: o jogo nomeia o que você executou; o rastro do golpe
  repete a forma do corte na pedra; a falha da pílula é legível (cor e forma da
  chama, estado dos ingredientes, som).
- **Recovery from failure**: retorno rápido ao duelo; a falha sempre deixa uma
  pista (pilar 5). Mestres explicam em profundidade quando o jogador volta com uma
  dúvida real.

---

## Core Loop

### Moment-to-Moment (30 seconds)
Mover-se com WASD e encadear passo, corte, feitiçaria de ki e cancelamento por
sequência de comandos, relativos ao inimigo travado. Ler a postura do inimigo e
responder com a sequência certa. A satisfação vem do fluxo do encadeamento, do
impacto (hitstop, feedback de cancelamento) e de reconhecer a guarda.

### Short-Term (5-15 minutes)
Explorar → lutar → encontrar um fragmento (página de manual, inscrição, marcas
na pedra, pílula) → ganhar uma peça nova (forma, ki ou passo) → testá-la no
próximo encontro. Aqui nasce o "e se eu juntar isto com aquilo?".

### Session-Level (30-120 minutes)
O jogador escolhe o foco: um túmulo, um duelo contra um mestre, uma coleta de
ervas para uma pílula. Pontos de parada naturais são locais de meditação
(salvar e consolidar o aprendido). Toda sessão termina com uma pista aberta:
meio manual, uma passagem ainda intransponível, uma pílula que falhou.

### Long-Term Progression
Níveis (experiência) fortalecem o corpo. Reinos de cultivo exigem avanços
conquistados e ampliam o que o ki suporta: mais peças por sequência, novos tipos
de peça. Espada e alquimia alimentam o cultivo; a espada é o caminho inicial, e o
jogador decide quanto se dedica a cada arte. Meta de longo prazo: o **Dao da
Espada**.

### Retention Hooks
- **Curiosity**: passagens lembradas e ainda fechadas; meio manual; combinações lendárias sugeridas por fragmentos
- **Investment**: o estilo próprio construído; artes nomeadas que o mundo reconhece; avanço de reino em preparo
- **Social**: N/A (single-player); substituído por mestres e rivais que reagem ao jogador
- **Mastery**: o chefe que ainda vence; a sequência ainda inconsistente; a pílula que ainda falha

---

## Game Pillars

Os pilares foram revisados pelo creative-director (CD-PILLARS: CONCERNS) e
ajustados conforme o parecer; as revisões estão incorporadas abaixo.

### Pillar 1: Aprender é a recompensa
Toda grande descoberta muda o que você consegue fazer com as mãos (recompensa),
com a espada ou com sua energia espiritual.

*Design test*: Um avanço de reino dá só atributos ou desbloqueia algo nas mãos
(um cancelamento, um passo, um elemento de ki)? → Precisa desbloquear algo nas
mãos. Atributos são bônus, nunca o prêmio principal.

### Pillar 2: O estilo é seu
As combinações nascem do jogador; o jogo reconhece, não prescreve. Regra das
combinações lendárias: as pistas em túmulos e manuais são fragmentos, nunca
receitas. "O jogo dá nome ao que você descobriu; nunca diz o que descobrir."

*Design test*: Entre uma build guiada e deixar uma combinação em aberto → este
pilar escolhe deixar em aberto.

### Pillar 3: O mundo se lembra, e você também
Ruínas, passagens e posturas são legíveis e valem a volta; e o mundo também se
lembra de você: NPCs, rivais e inimigos reconhecem e citam pelo nome as artes que
você criou.

*Design test*: Portão de conhecimento (reconhecer uma guarda, uma pegada) ou
portão de item-chave? → este pilar escolhe o conhecimento.

### Pillar 4: Maestria nas mãos
O domínio vem da execução e do reconhecimento (execução), não de menus. Regra de
desempate com o Pilar 2: executar a sequência pode ser difícil, mas descobri-la
nunca depende de execução perfeita.

*Design test*: Entre ativar uma técnica por menu e por sequência de comandos →
este pilar escolhe a sequência.

### Pillar 5: Todo caminho é um Dao
Espada e alquimia são caminhos de cultivo, cada um com aprendizado e revelações
próprios. A falha é legível (cor e forma da chama, estado dos ingredientes, som)
e o conhecimento vale na próxima tentativa. A alquimia também é feita com as mãos
(ex.: reconhecer o momento de reduzir o fogo). A espada é o caminho inicial; o
jogador decide quanto se dedica a cada arte. Desenvolvido em etapas: o Dao da
Alquimia completo faz parte da visão completa e ganha profundidade à medida que
for testado.

*Design test*: Entre uma falha que só pune e uma falha que se pode ler → este
pilar escolhe a que se pode ler.

**Tensões intencionais:** Pilar 2 (liberdade) × Pilar 4 (execução exigente),
resolvida pela regra de desempate do Pilar 4; Pilar 3 (mundo memorável) × escopo,
contida pelo anti-pilar do mundo vazio e pelas etapas jogáveis.

### Anti-Pillars (What This Game Is NOT)

- **NOT tutoriais longos nem texto obrigatório que interrompe o jogo**: compromete *Aprender é a recompensa*. O mestre dá uma pista curta; o texto completo fica disponível para quem tiver curiosidade ou voltar com uma dúvida.
- **NOT marcadores que apontam o caminho para tudo**: compromete *O mundo se lembra*.
- **NOT recompensas principais apenas numéricas**: compromete *Aprender é a recompensa*.
- **NOT árvore de habilidades por menu que define o estilo**: compromete *O estilo é seu*.
- **NOT alquimia por receita fixa sem aprendizado**: compromete *Todo caminho é um Dao*.
- **NOT mundo aberto vazio**: tamanho nunca substitui densidade de pistas; protege *O mundo se lembra* e o escopo.

---

## Visual Identity Anchor

**Direção:** "Todo Golpe Deixa Marca" (escolhida entre três direções do
art-director no gate AD-CONCEPT-VISUAL; recomendação do art-director).

**Regra visual:** todo movimento que se pode aprender já está escrito no mundo.
O rastro de um golpe do jogador e o corte gravado num túmulo têm a mesma forma: a
pedra ensina o golpe, e o golpe repete a pedra.

**Princípios de apoio:**
1. **Silhueta primeiro, cor depois.** Cada guarda tem silhueta própria (lâmina
   alta, lâmina baixa, postura aberta, postura fechada), legível em 1–2 quadros.
   *Teste:* a guarda é reconhecível em escala de cinza e sob efeitos de ki? Se
   não, redesenhar a silhueta, não a cor.
2. **O ki nunca cobre a leitura.** O ki do jogador é desenhado atrás da silhueta
   do inimigo ou com transparência; o contorno do inimigo em guarda fica sempre
   por cima. *Teste:* entre um efeito mais espetacular e a guarda legível, a
   guarda vence.
3. **Pistas têm destaque exclusivo.** Cortes na pedra são geométricos e limpos (arcos de raio constante, decidido no art bible em 2026-11-25),
   propositalmente "artificiais" entre as rochas orgânicas; o destaque de pista
   nunca se repete em decoração comum. *Teste:* se um elemento decorativo usa o
   destaque de pista, ele sai.

**Filosofia de cor:** ambiente em tons terrosos e jade dessaturados, com baixo
contraste de valor. **Ciano-branco** = conhecimento que se pode aprender
(inscrições, pegadas, cortes). **Vermelho-ocre** = intenção hostil (guarda de
ataque, telegraph). **Âmbar** = alquimia. A **cor do ki muda conforme o caminho
do jogador** (reforça *O estilo é seu*). A cor nunca é o único portador de
informação (acessibilidade para daltônicos).

**Escala:** pixel art; tiles de 32×32; personagens em quadros de 64×64 (~48 px
visíveis); 4 direções (3 desenhadas + 1 espelhada). Leitura escolhida para o
"2D, 64 bits" original: o 64 como escala dos personagens.

**Alternativas consideradas:** "O Vazio Entre os Traços" (nanquim, paleta de 64
cores; identidade mais marcante, custo alto e guardas menos legíveis) e
"Relíquia Esculpida" (3D low-poly em sprites, estética N64; exige Blender).

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| Castlevania: Symphony of the Night | Portões de conhecimento; "será que agora consigo entrar?"; magia por sequência de comandos | Visão de cima 3/4; os portões incluem posturas inimigas; comandos relativos ao alvo travado | Valida que a memória do jogador como mapa e o comando aprendido são satisfatórios |
| Vampire Survivors | Habilidade + item que evoluem juntos; a descoberta muda o que você procura | A evolução é descoberta executando a sequência com as mãos, não por menu ou automaticamente | Valida o prazer de combinações que reorientam a busca |
| Minecraft | Liberdade de escolher os próprios objetivos; investigar um lugar a sessão inteira | Mundo autoral denso de pistas, não procedural; objetivos escolhidos dentro de uma jornada de cultivo | Valida sessões guiadas pelo jogador |
| Magicka | Combinar elementos por sequência de teclas | Camada forma + ki + passo; combinações lendárias nomeadas; vínculo com descoberta no mundo | Referência de posicionamento mais próxima do gancho |
| Hollow Knight / Noita | Conhecimento e retorno (HK); descobrir regras pelo experimento (Noita) | Ambientação Murim; aprendizado vem de mestres e túmulos | Comparáveis para o mesmo tipo de jogador |

**Non-game inspirations**: obras de Murim e cultivação, principalmente *Contos
de Cultivação de um Regressor*; também *Logando no Murim* e *Upando
Infinitamente: Murim*. Delas vêm o universo, os reinos de cultivo, o Dao como
iluminação por compreensão ("tudo pode ser um Dao") e o legado de mestres de
outras eras.

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | Não definido (público principal: o próprio criador) |
| **Gaming experience** | Mid-core a hardcore |
| **Time availability** | Sessões de 30–120 min, com objetivo escolhido |
| **Platform preference** | PC, teclado |
| **Current games they play** | Castlevania: Symphony of the Night, Minecraft, Vampire Survivors |
| **What they're looking for** | Um RPG Murim em que aprender técnicas explorando e dominá-las com as mãos seja a própria recompensa |
| **What would turn them away** | Texto demais de uma vez, explicações longas obrigatórias; progressão só numérica |

O criador gosta de grind, combate difícil e crafting elaborado: farma até o item
perfeito e repete um chefe cem vezes até aprender o padrão.

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | Não definida (criador sem preferência); `/setup-engine` decide. Critérios do technical-director: tilemap 2D maduro com y-sort; controle de animação por quadro dirigido por frame data própria; loop de timestep fixo; importação de Aseprite; recursos legíveis em texto; serialização flexível para saves versionados; curva de aprendizado adequada a um primeiro jogo; câmera pixel-perfect |
| **Key Technical Challenges** | Buffer de input com cancelamentos fluidos; comandos relativos ao alvo em 3/4; sistema de combinação orientado a dados; animação em camadas; banco de fatos da memória do mundo com save versionado; hitbox e y-sort em 3/4 |
| **Art Style** | Pixel art 2D, visão de cima 3/4 |
| **Art Pipeline Complexity** | Medium (2D custom em Aseprite); animação é o maior custo e o principal gargalo, já que o código é assistido pelo Claude |
| **Audio Needs** | Moderate: cada peça e combinação precisa de uma assinatura sonora reconhecível; a nomeação de uma combinação lendária precisa ser audível |
| **Networking** | None |
| **Content Volume** | Em aberto; definido por etapas jogáveis. Etapa 1: uma região pequena, 2 inimigos com guarda, 1 técnica aprendida |
| **Procedural Systems** | Nenhuma geração procedural de mundo; combinações sistêmicas por regras |

**Decisões técnicas já tomadas:**
- **Comandos relativos ao alvo travado** (trava suave): "frente" é em direção ao inimigo travado.
- **Animação em camadas** (recomendação do technical-director): a forma tem sprites do corpo; o ki é VFX/paleta por cima; o passo é modificador de deslocamento e timing. O custo de arte por peça deve ser aditivo, não multiplicativo.
- **Frame data em dados**, não em grafos de animação da engine; simulação de combate em timestep fixo.
- **Memória do mundo como banco de fatos** (IDs de conhecimento, registro de artes do jogador), com número de versão no save desde o primeiro save e IDs estáveis em texto.
- **Sem streaming de mundo** na etapa 1; regiões futuras ligadas por transições de cena ou áreas discretas.

---

## Risks and Open Questions

### Design Risks
- Combinações sistêmicas podem parecer variações do mesmo golpe; o sistema precisa provar variedade com 3–4 peças antes de investir em arte.
- Tensão entre liberdade (Pilar 2) e execução exigente (Pilar 4): sequências difíceis demais podem esconder a descoberta.
- Leitura de posturas em pixel art 3/4: se o jogador não reconhecer a guarda, a revelação do túmulo não acontece.
- Equilíbrio entre grind (que o criador aprecia) e o anti-pilar de recompensas numéricas.
- Profundidade da alquimia como Dao próprio ainda não testada.

### Technical Risks
(Parecer TD-FEASIBILITY: **CONCERNS** — MVP tecnicamente viável; dois riscos altos a tratar na arquitetura.)
- **ALTO:** explosão combinatória de animação (forma × ki × passo) — mitigação: animação em camadas.
- **ALTO:** ambiguidade de comandos direcionais em 3/4 — mitigado pela decisão de comandos relativos ao alvo; atenção a normalização de diagonais e ghosting de teclado (evitar mais de direção + 1 botão simultâneos).
- **MÉDIO:** sensação do buffer de input e dos cancelamentos (exige ferramenta de debug visual desde a primeira semana).
- **MÉDIO:** versionamento e migração de save da memória do mundo.
- **MÉDIO:** nomes de artes gerados por composição mudam de gramática entre idiomas (se houver localização).
- Hitbox no plano do chão, y-sort e oclusão de objetos altos em 3/4.
- Pistas da fornalha não podem depender só da cor (acessibilidade).

### Market Risks
- Projeto pessoal, sem objetivo comercial: riscos de mercado não se aplicam. Mercado not researched.

### Scope Risks
(Parecer PR-SCOPE: **UNREALISTIC** para o nível de lançamento de 2–3 regiões em 1–2 anos, avaliado antes de o criador definir o projeto como hobby sem prazo. Decisão do criador: sem prazo fixo, desenvolvimento em etapas jogáveis, quantidade de regiões em aberto. Estimativas do producer partiram de horas humanas e estão descalibradas para desenvolvimento assistido pelo Claude; confiança baixa até medir o ritmo real.)
- Animação é o principal gargalo: o Claude acelera código, mas não produz animação final de pixel art.
- Primeiro jogo do criador; ritmo limitado pela disponibilidade de tokens.
- Visão de mundo aberto grande: risco de expandir antes de validar o combate.
- Corte gracioso: cada região deve ser desenhada como capítulo fechado (começo, mestre, avanço, fim).

### Open Questions
- O encadeamento de passo, corte e ki é divertido com as mãos? → Protótipo de combate com formas simples (etapa 1).
- 3–4 peças geram combinações visivelmente diferentes? → Segundo protótipo de combinação com animação em camadas.
- Qual engine? → `/setup-engine`.
- Qual é o ritmo real de produção de arte (quadros finalizados por dia)? → Medir nas primeiras semanas de arte.
- Como apresentar o mundo que cita as artes do jogador sem muito texto? → `/design-system` do sistema de memória do mundo.

---

## MVP Definition

**Core hypothesis**: Explorar para encontrar uma técnica e usá-la com as
próprias mãos contra a guarda que antes derrotava o jogador é divertido, e
encadear passo, corte e ki é satisfatório por si só.

**Required for MVP** (etapa 1, o primeiro marco):
1. Uma região pequena: trilha, acampamento de discípulos, túmulo da mestra.
2. Cadeia base de combate: passo, corte, uma feitiçaria de ki, cancelamento, com comandos relativos ao alvo travado.
3. O discípulo de guarda fechada, que vence o jogador de frente.
4. A cadeia de descoberta: metade do manual no corpo de um discípulo → anotação na margem → túmulo da mestra → pegadas ao lado do corte na pedra → técnica do passo lateral.
5. A revanche: outro espadachim com a mesma guarda, vencível com o passo lateral.
6. Uma passagem que só se atravessa depois da técnica (portão de conhecimento).
7. Uma combinação reconhecida e nomeada pelo jogo.

**Explicitly NOT in MVP** (defer to later):
- Fornalha e alquimia (opção para a próxima etapa).
- Avanço de reino de cultivo (opção para uma etapa seguinte).
- XP e níveis completos (placeholder, se necessário).
- Segunda região; streaming de mundo.
- NPCs que citam as artes do jogador.
- Arte final (a etapa 1 pode começar com formas simples).

### Scope Tiers (if budget/time shrinks)

Projeto pessoal sem prazo fixo: os níveis são **etapas jogáveis**, e a próxima
etapa é escolhida pelo criador depois de jogar a anterior.

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **MVP (Etapa 1)** | Uma região pequena (trilha, acampamento, túmulo) | Duelo perdido → manual → túmulo → técnica → revanche; uma combinação nomeada; portão de conhecimento | Sem prazo fixo |
| **Vertical Slice** | A região expandida e completa | Combate refinado, primeira fornalha (1 pílula, 1 decisão com as mãos), primeiro avanço de reino, um mestre que reconhece seu estilo | Sem prazo fixo; conteúdo e ordem escolhidos após a etapa 1 |
| **Alpha** | Regiões como capítulos fechados (quantidade em aberto) | Todos os sistemas: combinações sistêmicas e lendárias, reinos, alquimia, memória do mundo | Sem prazo fixo |
| **Full Vision** | Mundo aberto grande, reinos até o Dao da Espada | Dao da Alquimia completo, dezenas de peças, mundo que cita suas artes | Vários anos |

---

## Next Steps

- [ ] Fill in CLAUDE.md technology stack based on engine choice (`/setup-engine`)
- [ ] **Prototype core idea** (`/prototype [core-mechanic]`) — combate encadeado + descoberta do passo lateral
- [ ] Define the visual identity (`/art-bible`) a partir da âncora "Todo Golpe Deixa Marca"
- [ ] Validate the concept (`/design-review design/gdd/game-concept.md`)
- [ ] If prototype PROCEEDS: Decompose concept into systems (`/map-systems`)
- [ ] Design each system (`/design-system [system-name]`) — use prototype learnings in Tuning Knobs and Formulas sections
- [ ] Build vertical slice in Pre-Production (`/vertical-slice`) — validate full game loop before committing to Production
- [ ] Validate core loop with playtest (`/playtest-report`)
- [ ] Plan first milestone (`/sprint-plan new`)
