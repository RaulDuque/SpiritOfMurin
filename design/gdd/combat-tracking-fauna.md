# Combate, rastreamento e leitura da fauna (T07)

*Criado em 2026-11-28 com a T07. Decisões na prancha `user-docs/relatorios/2026-11-27-t07-prancha-javali.html` e em `user-docs/tasks/T07-combate-rastreamento-fauna.md`. Os valores estão em `assets/data/plains/plains_fauna.json`.*

## 1. Overview

O javali e o coelho da Planície passam a ser lidos **pela imagem**. O javali mostra três resultados diferentes no contato:
- golpe aparado no couro;
- investida que acerta;
- contra-ataque na recuperação.

Ele também desenha no chão uma faixa com a trajetória já fixada. O coelho mostra quando vai fugir e mergulha na toca. Os animais deixam pegadas novas pelo caminho que percorreram de verdade. Examinar uma pegada aponta onde o animal está, com uma precisão que cresce com Rastreamento. As técnicas enfraquecidas mostram onde terminam.

## 2. Player Fantasy

"Eu li o bicho." O espadachim espera o casco raspar, vê a faixa de terra, sai da linha e acerta no momento em que o javali se recompõe. Ele ouve o golpe resvalar quando ataca cedo demais. Para caçar o coelho, ele chega quieto ou segue as pegadas frescas até a toca.

## 3. Detailed Rules

### 3.1 Javali: resultado do golpe

| Estado do javali | Golpe do espadachim | Resultado |
|---|---|---|
| Aviso, investida | corte, Disparo de Ki comum | **aparado**: faísca e risco branco do lado de quem bateu, som `parry`, o espadachim recua `parry_nudge_px`; a Vida do javali não muda; aviso de texto no máximo a cada 4 s |
| Aviso, investida | Passo da Lâmina Silenciosa (enfraquecido ou pleno) | **atravessa** (decisão B) com `lamina_through_factor` do dano; o javali continua |
| Aviso, investida | Disparo de Ki Concentrado, Reforço que rompe | rompe a defesa (regra anterior, sem mudança) |
| Fuçando, recuperação, voltando | qualquer golpe | **acerto**: clarão, recuo do corpo (`boar_flinch_seconds`, `boar_flinch_px`) no mesmo quadro em que a Vida cai, som `hit`; na recuperação, soma `HitFlinch` |
| Investida alcança o espadachim | — | **investida acerta**: dano, empurrão e som `hurt` no mesmo quadro |

### 3.2 Faixa da investida (decisão A)

- A faixa aparece quando a direção trava, no aviso e durante a investida. Na investida ela mostra **o que falta** da corrida.
- Ela tem as bordas tracejadas cor de terra e um preenchimento fraco. O comprimento é `ChargeDistance` e a largura é `2 × ChargeHitRadius`.
- Fica em Esc → Modo de assistência → "Faixa da investida do javali": **completa** (padrão), **curta** (60 px) ou **desligada**. A escolha fica salva nas preferências (`ChargeStrip`).

### 3.3 Barra junto ao animal (decisões A e 4)

- Mostra "atual/máximo" sobre a cabeça do animal focado (Espaço) ou do javali mais próximo que avisa, investe ou se recupera a até `combat_radius`.
- Continua visível `near_bar_seconds` depois que a luta acaba.
- A barra do alto continua. A barra mostra só Vida (a postura fica para a T08).

### 3.4 Coelho

- As orelhas ficam erguidas do lado de trás da cabeça, conforme a direção para onde ele olha.
- No estado Alerta o corpo se agacha (mais baixo e mais largo): é o quadro de "vai fugir".
- Ao entrar na toca, o corpo encolhe para dentro do buraco em `rabbit_dive_seconds`, com poeira.
- Ele ouve conforme o modo de andar: andando, `HearWalk` (95 px na base); com Shift, `HearSneak` (26 px). Os dois sobem com o nível.
- O arbusto se curva quando o animal passa (regra da T05).
- Continua com 1 de Vida (decisão 6).

### 3.5 Pegadas frescas (decisão 7)

- Um animal visível em movimento deixa uma pegada a cada `track_spacing_rabbit` ou `track_spacing_boar` px de caminho.
- A pegada aponta para o **movimento recente** do animal (as últimas 6 atualizações), por isso acompanha as curvas.
- Um salto maior que 120 px (recarga de bloco) recomeça o rastro sem deixar pegada.
- Cada pegada dura `track_seconds` (90 s) e vai sumindo. Acima de `track_max`, as mais antigas saem primeiro.
- As trilhas geradas pela semente não mudam e não somem.

### 3.6 Examinar uma pegada

A indicação aponta **onde o animal está agora** (ou onde ficou a carcaça), a partir da pegada examinada.

| Grau de Rastreamento | Largura do setor (`track_sector_degrees`) | Texto |
|---|---|---|
| 0 | 120° | "mais ou menos para o norte" (4 direções) |
| 1 (Iniciante) | 75° | 8 direções; distância em passos; as próximas 3 pegadas brilham |
| 2 | 40° | o mesmo, mais o nível do animal |
| 3 (Mestre) | 20° | "(com precisão)"; a trilha inteira brilha |

- Uma pegada fresca também diz a idade.
- Um cone fraco aparece no chão por `track_hint_seconds`.

### 3.7 Javalis no mapa (decisão 5)

- O ícone do javali só aparece depois que o animal chega a até `seen_radius` do espadachim ou depois que uma trilha dele é examinada.
- A lista fica salva em `SeenAnimals`. Uma jornada antiga começa vazia, e as trilhas que ela já examinou contam.

### 3.8 Técnica enfraquecida legível

- Quando o Disparo de Ki chega ao fim do alcance sem acertar nada, ele se desfaz no ponto final: o crescente se parte, sobem partículas e fica uma marca no chão por 1 s. Isso vale para a Planície e para o Vale.
- Pleno: 210 px. Enfraquecido: 105 px.
- O passo longo vai 112 px pleno e 60 px enfraquecido.

## 4. Formulas

- **Dano da Lâmina no couro:** `dano_aplicado = dano_resolvido × lamina_through_factor`, com `lamina_through_factor = 0,5`. O dano resolvido inclui o crítico. Exemplo: a Lâmina enfraquecida tem dano 3, então tira 1,5.
- **Comprimento da faixa:** no aviso travado, `L = ChargeDistance`; na investida, `L = ChargeDistance − Travel`. No modo curto, `L = min(L, 60)`. Desligada ou fora do aviso e da investida, `L = 0`. A ponta fica em `posição + Facing × L`, que é fixa no mundo enquanto o javali corre.
- **Setor do exame:**
  - meia largura `h = track_sector_degrees[grau] / 2`;
  - centro `c = ângulo_real + k × 0,6 × h`, com `k ∈ [−1, 1]` fixo por pegada (hash do id);
  - como `|c − ângulo_real| ≤ 0,6 h < h`, o setor sempre contém a direção real.
- **Opacidade da pegada fresca:** `α = clamp(1,5 × (1 − idade / track_seconds), 0, 1)`. Ela fica inteira até um terço da vida e depois some aos poucos.
- **Recuo do javali:** `deslocamento = −lado × round(boar_flinch_px × t / boar_flinch_seconds)`, com `t` igual ao tempo que resta do recuo.

## 5. Edge Cases

- Golpe aparado várias vezes seguidas: o som e o recuo saem a cada golpe, mas o texto aparece no máximo a cada 4 s.
- Lâmina no javali já em recuperação: dano cheio, como qualquer golpe.
- Coelho escondido: não deixa pegada e não conta como visto.
- Animal levado de bloco em bloco (salto maior que 120 px): o rastro recomeça sem uma pegada falsa entre os dois pontos.
- Examinar a pegada de um animal já caçado: aponta para a carcaça e diz "(O animal já foi caçado.)".
- Barra junto ao animal: some quando ele morre ou é removido.
- Saves antigos sem `seen_animals` ou `charge_strip`: lista vazia e faixa completa.
- Disparo que acerta algo ou é parado: não se desfaz no fim do alcance (só some no ponto do acerto).

## 6. Dependencies

- **Javali:** `BoarBrain`/`Boar` (`src/Creatures/`).
- **Coelho:** `RabbitBrain`/`Rabbit`.
- **Artes:** `src/Elements/Fauna/`.
- **Pegadas:** `TrackPrint` (`src/Plains/Landmarks.cs`) e `FreshTracks`.
- **Técnicas:** `TechniqueData` (`combat_techniques.json`) e `KiWave`/`KiFizzle`.
- **Interface:** Modo de assistência (`src/Game/Assist.cs`, `GamePanels`) e preferências (`src/Save/Prefs.cs`).
- **Mapa:** `PlainsGame.DrawMap`/`BoarOnMap`.
- **Arbustos:** reação ao contato da T05.
- **Chuva:** T09 (o critério "legível na chuva" fica pendente).
- **Postura e fauna nova:** T08.

## 7. Tuning Knobs (`assets/data/plains/plains_fauna.json`)

| Chave | Valor | Faixa segura | O que muda |
|---|---|---|---|
| `track_seconds` | 90 | 30–300 | quanto tempo a pegada fresca fica |
| `track_spacing_rabbit` / `_boar` | 14 / 20 | 8–40 | distância entre pegadas |
| `track_max` | 220 | 50–600 | limite de pegadas frescas vivas |
| `track_sector_degrees` | 120, 75, 40, 20 | decrescente | precisão por grau |
| `track_hint_seconds` | 2,5 | 1–6 | tempo do cone no chão |
| `parry_nudge_px` | 2 | 0–4 | recuo do espadachim no aparo |
| `boar_flinch_seconds` / `_px` | 0,25 / 3 | 0,1–0,5 / 1–6 | recuo do javali atingido |
| `rabbit_dive_seconds` | 0,35 | 0,2–0,8 | mergulho na toca |
| `lamina_through_factor` | 0,5 | 0–1 | fração do dano da Lâmina que atravessa o couro |
| `near_bar_seconds` | 3 | 1–8 | quanto a barra fica depois da luta |
| `combat_radius` | 220 | 120–320 | raio em que um javali em luta mostra a barra |
| `seen_radius` | 180 | 100–300 | distância em que um animal conta como visto |

## 8. Acceptance Criteria (`tests/integration/FaunaCheck.cs`, sementes 20261126 e 777)

1. **`duelo`:**
   - corte no aviso: faísca, risco do lado do espadachim, Vida do javali igual;
   - investida: Vida e empurrão no quadro do impacto;
   - contra-ataque: recuo no quadro em que a Vida cai, e a recuperação passa a durar `RecoverTime + HitFlinch`.
2. **`faixa`:**
   - sem faixa antes de travar;
   - ponta da faixa a menos de 4 px do fim real, no aviso e no meio da investida, com a assistência desligada e ligada;
   - modo curto com 60 px, desligado com 0, escolha salva.
3. **`barra`:** "6/6" sobre a cabeça; aparece no aviso sem foco; acompanha a Vida; some 3 s depois.
4. **`lamina`:** tira `dano × 0,5` durante o aviso, e o javali continua.
5. **`coelho`:** ouve no raio dos dados andando e com Shift; agacha antes de fugir; olha para onde corre; mergulha a menos de 8 px da toca; o arbusto se curva.
6. **`pegadas`:**
   - pegadas da fuga dentro de 20° do movimento;
   - curva leste → sul acompanhada;
   - setor do grau 3 menor que um terço do grau 0, sempre contendo o animal;
   - a roda escolhe a pegada fresca em vez da erva;
   - a pegada some ao fim da vida.
7. **`tecnicas`:** Disparo pleno com 210 ± 4 px e enfraquecido com 105 ± 4, os dois se desfazendo; passo de 112 e 60 px.
8. **`mapa`:** fora do mapa antes; no mapa depois de visto e depois de rastreado; fica salvo.
9. **`vento`:** na rajada (vento perto de 1), a faixa muda mais de 150 px da imagem.
10. **`oldsave`:** a fixture carrega, sem javalis vistos e com a faixa completa.
11. **`trilhas`:** as trilhas geradas existem e não somem.
12. **Testes de unidade (`TrackingTests`):** o setor sempre contém a direção real e o passo acompanha curvas.
