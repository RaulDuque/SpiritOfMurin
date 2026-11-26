# GDD: Planície Verdejante — primeiro trecho jogável

*Criado: 2026-11-26 · Status: Implementado (primeira passada, números provisórios)*
*Fontes: `user-docs/futuro-do game/minahvisao.md`, `gpt.md` (fauna, ervas, habitats), `opiniao-claude.md` (mundo por semente + lugares feitos à mão)*
*Pilares servidos: 3 "O mundo se lembra, e você também", 4 "Maestria nas mãos", 1 "Aprender é a recompensa"*

## 1. Overview

O primeiro trecho da Planície Verdejante é um terreno gerado por semente, montado em blocos (chunks) ao redor do personagem conforme ele anda. A mesma semente sempre produz o mesmo terreno. O personagem acorda numa clareira, sem nada, ao lado de um riacho. Há três habitats ligados entre si:
- **margem do riacho:** sílex e galhos;
- **campo de arbustos:** tocas de coelho;
- **lamaçal:** javalis e terra revirada com Raiz Amarga.

Com E o jogador recolhe o que está no chão. No inventário (I), fabrica e equipa a faca de pedra. Com a faca, caça coelhos e enfrenta javalis, e se cura com a Grama Sangue-de-Coelho. Na campanha, esse despertar vem depois da derrota no prólogo. Nesta entrega, o F5 abre direto nele.

## 2. Player Fantasy

"Não tenho nada, mas sei olhar." O jogador lê o ambiente:
- pedras claras na margem: ali há sílex;
- um arbusto com tocas: ali há coelhos;
- terra revirada: ali passou um javali, e ali cresce a raiz.

O primeiro combate é uma lição de leitura. O javali avisa antes de investir, e quem viu o aviso sai da linha e ganha a abertura. Voltar a um lugar e reconhecê-lo (a curva do riacho, o campo de tocas, o lamaçal) faz o mundo parecer real, não sorteado.

## 3. Detailed Rules

### 3.1 Mundo por semente
- O tile tem 32 px. O bloco tem `chunk_tiles` × `chunk_tiles` tiles (16 × 16 = 512 px).
- Ficam montados os blocos a até `load_radius` (2) do bloco do jogador. São liberados os que passam de `unload_radius` (3), pela distância de Chebyshev.
- **Pureza:**
  - Todo tile e todo objeto são função apenas de (semente, coordenada do tile). Nunca dependem da ordem de visita.
  - Cada objeto pertence a um único tile e, por isso, a um único bloco.
  - O id de cada objeto é estável: `tipo@tx,ty`.
- **Semente:**
  - Vem do campo `Seed` da cena `src/Plains/Plains.tscn` ou de `--seed=N` na linha de comando.
  - 0 sorteia uma semente nova a cada execução.
  - A semente aparece no canto superior direito.
- **Riacho:**
  - Corre de norte a sul, a `creek_offset` tiles a leste do despertar.
  - A linha central é a soma de dois senos com fase tirada da semente. O meandro fica mais calmo perto do despertar.
  - A água tem meia largura `creek_half_width` e margem de `bank_width`.
  - A cada `ford_spacing` linhas há `ford_length` linhas de vau (raso, atravessável). Fora do vau, a água tem colisão.
- **Habitats**, em ordem de precedência:
  1. água e margem;
  2. a clareira (raio `clearing_radius` + 2);
  3. o lamaçal garantido;
  4. o campo de arbustos garantido;
  5. longe do início, ruído de valor: lamaçal acima de `mud_threshold`, arbustos acima de `bush_threshold`;
  6. relva.
- **Garantias do início:**
  - A clareira não tem objetos.
  - Na margem: 2 sílex, 2 galhos, 1 pinheiro e 1 pedra com Grama Sangue-de-Coelho.
  - O campo de arbustos fica a `anchor_distance_min..max` tiles, no ângulo `bush_anchor_angle` ± `anchor_angle_jitter`. Tem 2 tocas com coelho, arbustos e uma pedra com erva.
  - O lamaçal segue a mesma regra, no ângulo `mud_anchor_angle`. Tem 1 javali e 3 terras reviradas, 2 delas com Raiz Amarga.
- **Objetos aleatórios:** a chance por tile em cada habitat está em `per_habitat`. Cada tile tem no máximo um objeto principal.
  - Pedra → erva junto (`herb_near_boulder`).
  - Terra revirada → raiz (`root_in_rooted`).
  - Toca → coelho.
- **Lugares feitos à mão:** `PlainsGenerator.AddStamp(tile, chão, objetos)` reserva tiles antes das regras aleatórias. É o encaixe previsto para cidades e túmulos.
- **Memória da partida (`WorldState`, só na sessão):**
  - objetos recolhidos;
  - animais mortos, com a posição da carcaça;
  - partes de carcaça já tiradas.
  - Um bloco montado de novo consulta essa memória: o que foi recolhido não volta, e o animal morto volta como carcaça.

### 3.2 Começo e ferramenta
- O personagem acorda com Vida cheia (10 pontos) e as mãos vazias. O HUD mostra a Vida numa barra com porcentagem, no estilo da barra de Qi do protótipo. O trecho que a erva ainda vai devolver aparece em verde na barra. Com as mãos vazias, J ou o clique esquerdo não cortam: aparece um aviso.
- E recolhe o objeto mais próximo a até 26 px. A mensagem mostra o nome e a quantidade: "+1 Sílex (tem 1)". Cada objeto é recolhido uma única vez.
- **Inventário (I ou Tab):**
  - Lista os itens com a quantidade e, em seguida, as receitas.
  - W/S escolhe; E usa, equipa ou fabrica; I, Tab ou Esc fecha.
  - Com o inventário aberto, o corpo não se move.
- **Faca de Pedra:**
  - Custa 1 Sílex + 1 Galho Rijo, consumidos uma vez.
  - Sem material, a fabricação não muda nada e avisa "Falta: …".
  - E sobre a faca equipa ou desequipa.
- **Corte com a faca:**
  - Curto e rápido: 0,05 s de preparo, 0,07 s ativo, 0,15 s de recuperação.
  - Alcance de 22 px, dano 1, encadeia no máximo 2.
  - O traço do golpe é menor que o da espada.
- **Espada:** continua igual (cadeia de 3, alcance de 34 px, dano 2) no campo de teste `src/Main/Main.tscn`.
- **Dano por contato real:**
  - Enquanto a lâmina está ativa, o golpe acerta o animal cujo pé está a até (alcance + raio do corpo) e dentro de ±75° da frente.
  - Cada golpe acerta cada animal uma vez.
- O passo (L ou clique direito) e o andar não mudaram. Shift faz andar quieto: 45% da velocidade.

### 3.3 Coelho de Campo
- Vive junto à toca, no campo de arbustos, e dá pulos a até `GrazeRadius` da toca.
- **Audição:** o raio depende de como o jogador se move:
  - parado: `HearStill`;
  - andando quieto: `HearSneak`;
  - andando: `HearWalk`;
  - passo ou corte: `HearLoud`.
- **Fuga:**
  - Ao ouvir, ergue as orelhas e congela por `AlertTime`.
  - Corre até a toca em duas arrancadas, com uma pausa de `DashPause` entre elas, e some na toca.
  - Volta a sair depois de `HideTime`.
- Dentro da toca não pode ser atingido. Um golpe o abate (`Health` 1).
- **Carcaça:** com uma lâmina na mão, E tira Carne Macia e depois Pele Fina, uma vez cada. A carcaça some quando fica vazia.

### 3.4 Javali da Mata
- Vive no lamaçal e anda devagar entre pontos ao redor de casa, fuçando, com pausas. Pegadas no chão marcam o lugar.
- **Aviso** (`WarnTime`): começa quando o jogador chega a até `AggroRadius` e está a até `LeashRadius` da casa do javali.
  - O javali raspa o casco (poeira), abaixa a cabeça e o olho fica vermelho.
  - Durante `TrackFraction` do aviso, a cabeça segue o jogador. Depois a direção fica fixa, e marcas no chão mostram a linha.
- **Investida:**
  - Reta, a `ChargeSpeed`, por até `ChargeDistance`, ou até bater em algo sólido.
  - Se o corpo chegar a até `ChargeHitRadius` do jogador, causa `ContactDamage` uma vez por investida.
  - Golpes durante o aviso e a investida são desviados pelo couro grosso.
- **Recomposição** (`RecoverTime`): o javali fica tonto (pontos girando) e é a abertura.
  - Os golpes acertam.
  - Cada golpe acrescenta `HitFlinch` à recomposição.
- Depois de `Cooldown`, volta a avisar se o jogador ainda estiver perto. Se estiver longe de casa, volta andando.
- Golpe com o javali parado também acerta, mas ele começa o aviso na hora.
- **Morte:** acontece em `Health` acertos. E tira Carne Rija e depois Couro Grosso.

### 3.5 Ervas
- **Grama Sangue-de-Coelho:** cresce junto às pedras. Usar pelo inventário recupera `regen_amount` (4) de Vida aos poucos, em `regen_seconds` (6 s). Não pode ser usada com a Vida cheia.
- **Raiz Amarga:** cresce na terra revirada pelo javali. Usar recupera `heal_now` (1) de Vida na hora. O uso como antídoto e pasta fica para quando existirem veneno e preparo.
- **Desmaio:** com a Vida em 0, o personagem desmaia e volta a si na clareira com a Vida cheia. O inventário e a memória do mundo continuam.

## 4. Formulas

| Fórmula | Definição |
|---|---|
| Linha do riacho | `cx(ty) = creek_offset + calm(ty) · [m1·(sin(ty·f1 + φ1) − sin φ1) + m2·(sin(ty·f2 + φ2) − sin φ2)]`, com `calm(ty) = 0,35 + 0,65·clamp(|ty|/30, 0, 1)`. φ1 e φ2 vêm da semente, e cx(0) = creek_offset |
| Água / margem | `d = |tx + 0,5 − cx(ty)|`: água se d < creek_half_width; margem se d < creek_half_width + bank_width |
| Vau | `((ty − fase) mod ford_spacing) < ford_length` |
| Borda dos habitats garantidos | dentro se `dist(tile, âncora) < anchor_radius · (0,8 + 0,4·ruído(tx·0,35, ty·0,35))` |
| Habitats distantes | `far = clamp((dist_início − anchor_distance_max − anchor_radius)/6, 0, 1)`; lamaçal se `ruído·far > mud_threshold·far + (1 − far)` |
| Objeto do tile | `r = hash01(semente, tx, ty)`; percorre (sílex, galho, pinheiro, pedra, arbusto, toca, terra revirada, javali) subtraindo as chances até r < chance |
| Acerto da lâmina | `|alvo − pé| ≤ alcance + raio` e `ângulo(frente, alvo − pé) ≤ HitArcDegrees`, ou `|alvo − pé| < raio + 4` |
| Cura gradual | por quadro: `min(pendente, (regen_amount / regen_seconds) · dt)` |
| Duração da abertura do javali | `RecoverTime + n · HitFlinch` (n = golpes recebidos nela) |

**Exemplo de travessia:** o personagem anda a 105 px/s. Na investida de 240 px/s com 0,8 s de aviso, sair 15 px da linha leva cerca de 0,15 s. Há folga para quem viu o casco raspar.

## 5. Edge Cases

- **Mesmo bloco montado de novo:** sai idêntico. O que foi recolhido não reaparece e o animal morto vira carcaça. Coberto por teste.
- **Objeto desenhado além da borda do seu tile** (a erva ao lado da pedra): pertence ao tile da pedra e nunca é duplicado no vizinho.
- **Dois coelhos no mesmo corte:** os dois são atingidos, porque cada golpe acerta cada animal uma vez. Cada carcaça é independente.
- **A investida bate em algo** (pedra, pinheiro, arbusto, água): termina ali e começa a recomposição.
- **O jogador está invulnerável** depois de um acerto (0,7 s): a investida não causa dano de novo.
- **Fugir para longe do javali:** além de `LeashRadius` da casa, ele não avisa. Se terminar a investida longe de casa, volta andando.
- **Coelho escondido:** não pode ser atingido nem ouvir. Ao sair, se o jogador estiver fazendo barulho por perto, volta a fugir.
- **Fabricar sem material:** não muda nada.
- **Usar erva com a Vida cheia:** recusado, para não desperdiçar.
- **Inventário aberto:** o corpo para e J/L não agem.
- **Semente inválida ou 0:** sorteia uma nova.
- **Sair muito longe:** o mundo continua sendo gerado (é infinito). Fora da área inicial, os habitats seguem o ruído.

## 6. Dependencies

- **Personagem (`src/Player/`):** andar, corte, passo, Vida, armas e andar quieto. Os valores estão em `assets/data/PlayerTuning.tres`.
- **Cenário (`src/World/Obstacle.cs`):** pinheiro, pedra e arbusto, com colisão só na base (art bible 6.5).
- **Dados:**
  - `assets/data/plains/plains_worldgen.json`;
  - `plains_items.json`;
  - `RabbitTuning.tres`;
  - `BoarTuning.tres`.
- **Mapa de entradas:** `tools/godot/setup_input.gd` acrescenta `inventory` (I, Tab) e `sneak` (Shift).
- **Ampliado por** `design/gdd/plains-progression.md` (2026-11-26): nível, atributos, proficiências, trilhas, partes da carcaça por grau de Esfolar, fogueira, bolsa, Meditar e Continuar. O que ele mudou aqui: a erva passa a ter qualidade pela Erboristeria, a pele depende do Esfolar e o HUD ganha vigor, Qi e nível.
- **Próximas dependências:** salvamento (Meditar e Continuar), espada simples no posto de vigia, ligação com o Vale de Bambus, carimbos de lugares feitos à mão.

## 7. Tuning Knobs

| Onde | Valores |
|---|---|
| `plains_worldgen.json` | tamanho do bloco, raios de carga, clareira, riacho (posição, largura, meandros, vaus), limites dos habitats, âncoras do início, chances por habitat, erva junto à pedra, raiz na terra revirada |
| `plains_items.json` | nome, descrição, arma, cura gradual e imediata de cada item; receitas |
| `BoarTuning.tres` | aviso 0,8 s, fração de rastreio 0,55, investida 240 px/s por 170 px, recomposição 1,0 s, intervalo 0,35 s, alerta 110 px, coleira 260 px, contato 15 px, dano 2, vida 5, abertura extra 0,15 s |
| `RabbitTuning.tres` | audição 14 / 26 / 95 / 130 px, alerta 0,35 s, corrida 165 px/s, pausa 0,18 s, esconderijo 4,5 s, pasto 44 px, vida 1 |
| `PlayerTuning.tres` | tempos e alcance da faca e da espada, dano, arco do golpe, fator de andar quieto, Vida máxima, invulnerabilidade, empurrão |

**Faixas seguras (primeira estimativa)**

**Javali:**
- `WarnTime` de 0,5 a 1,2 s. Abaixo de 0,5 s, não dá para reagir só andando.
- `ChargeSpeed` de 180 a 300 px/s.
- `RecoverTime` de 0,6 a 1,5 s. Abaixo de 0,6 s, a faca não alcança o javali andando.
- `ChargeDistance` de 120 a 220 px.

**Coelho:**
- `HearSneak` menor que `KnifeReach` + raio do coelho (28 px). Senão, a caça quieta fica impossível.
- `HearWalk` de 70 a 130 px.
- `HideTime` de 2 a 8 s.

**Mundo:**
- `clearing_radius` de 2,5 a 5.
- `anchor_distance_max` até 20. Acima disso, os habitats saem da área que o teste de alcance cobre.

**Validação:** depois de qualquer mudança, o teste `Start_IsWalkableWithFirstMaterialsAndAllHabitatsReachable` e o PlainsCheck precisam continuar passando.

## 8. Acceptance Criteria

1. O F5 abre o despertar na Planície, com as mãos vazias e a semente visível.
2. A mesma semente e as mesmas coordenadas dão o mesmo bloco, em qualquer ordem de visita. Duas sementes dão mundos diferentes. As bordas dos blocos batem com os vizinhos.
3. Em 60 sementes, a clareira é livre, e são alcançáveis a pé: sílex e galho a até 10 tiles, erva a até 10, toca a até 22, javali a até 22 e raiz a até 24.
4. Sílex e galho recolhidos com E aparecem no inventário, com nome e quantidade. Sair do bloco e voltar não os devolve ao chão nem os duplica.
5. A faca de pedra é fabricada uma única vez com 1 sílex e 1 galho. Equipada, J e o clique esquerdo cortam curto (cadeia de 2). O passo continua com L e o clique direito.
6. O coelho ergue as orelhas, foge em duas arrancadas e se esconde na toca. Esperando e chegando quieto, a faca o abate. A carcaça dá carne e pele, uma vez cada.
7. O javali avisa por cerca de 0,8 s, investe em linha reta e se recompõe. Andar para o lado evita o dano. O golpe durante a recomposição acerta; durante o aviso e a investida, é desviado. Ficar na linha causa 2 de dano no contato.
8. A Grama Sangue-de-Coelho recupera a Vida aos poucos. A Raiz Amarga aparece na terra revirada perto do javali.
9. Os testes de controles da espada continuam passando no campo de teste.

## Esquema dos arquivos de dados

O `plains_worldgen.json` e o `plains_items.json` usam chaves em snake_case. A classe C# correspondente (`PlainsGenSettings`, `ItemCatalog`) é a fronteira do formato e lê os arquivos com `JsonNamingPolicy.SnakeCaseLower`. O bloco `_doc` de cada arquivo explica cada chave e é ignorado na leitura. Uma chave ausente usa o valor padrão da classe.
