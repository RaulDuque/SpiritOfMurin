# T07 — Combate, rastreamento e leitura da fauna — 2026-11-28

Nesta entrega o javali e o coelho passam a ser lidos **pela imagem**. Também entrou o ajuste da **diagonal** que você pediu: em diagonal cada eixo anda 0,8 da velocidade, cerca de 13% mais distância que em linha reta. Antes eram 0,707 por eixo, a mesma distância de reto, e por isso a diagonal parecia lenta na tela. O valor fica em `DiagonalAxisSpeed`, no `assets/data/PlayerTuning.tres`.

As regras estão no GDD `design/gdd/combat-tracking-fauna.md`. A tarefa, com o mapa "onde mexer", é [T07](../tasks/T07-combate-rastreamento-fauna.md).

## Decisões (suas, uma por vez, sobre a [prancha](2026-11-27-t07-prancha-javali.html))

| # | Pergunta | Escolha |
|---|---|---|
| 1 | Passo da Lâmina Silenciosa enfraquecido contra o javali | **B**: atravessa o couro no aviso e na investida com **metade do dano** (1,5), e o javali continua correndo |
| 2 | Faixa da investida | **A**: bordas tracejadas cor de terra, ligada por padrão; Esc → Modo de assistência deixa curta ou desligada |
| 3 | Barra junto ao animal | **A**: "atual/máximo" sobre o animal; a barra do alto continua |
| 4 | Postura | só Vida, por enquanto |
| 5 | Javalis no mapa | só depois de vistos ou rastreados |
| 6 | Coelho | continua com 1 de Vida |
| 7 | Pegadas novas | ficam 90 s |

## O que você consegue fazer agora

**Duelo com o javali** — os três resultados são diferentes e cada um acontece no quadro do contato:
- **Golpe cedo demais** (no aviso ou na investida): a lâmina resvala, com faísca e um risco branco do seu lado. Toca um som seco, o espadachim recua 2 px e a Vida do javali não muda. O aviso de texto aparece no máximo a cada 4 s.
- **Investida que acerta:** você perde Vida e é empurrado no mesmo quadro, com o som pesado.
- **Contra-ataque na recomposição:** o corpo do javali dá um tranco para trás no quadro em que a Vida cai, e a recomposição fica 0,15 s mais longa.
- **Faixa da investida:** aparece quando a direção trava e acompanha o que falta da corrida até a ponta real (diferença medida: 0,4 px). Em Esc → Modo de assistência → "Faixa da investida do javali" você escolhe completa, curta ou desligada.
- **Barra junto ao animal:** fica sobre a cabeça do animal focado (Espaço) ou do javali que avisa, investe ou se recompõe perto de você, e continua 3 s depois da luta.
- **Passo da Lâmina Silenciosa:** atravessa o couro com metade do dano.

**Coelho:**
- Ergue as orelhas, **agacha** antes de disparar, corre e **mergulha** na toca (encolhe para dentro do buraco, com poeira).
- O arbusto se curva quando ele passa.
- Andando, ele ouve de longe; com Shift, só bem perto. Os valores sobem com o nível: no Nv 2, 106 px andando e 29 px com Shift (95 e 26 na base).

**Pegadas e rastreamento:**
- Coelhos e javalis em movimento deixam **pegadas frescas** pelo caminho real, que acompanham as curvas. Elas somem aos poucos em 90 s.
- Examinar uma pegada (E) mostra um **cone no chão** apontando para onde o animal está agora:
  - Rastreamento 0: largo, "mais ou menos para o leste";
  - Iniciante: 8 direções e a distância;
  - grau 2: mais o nível do animal;
  - Mestre: estreito, "(com precisão)".
- A pegada fresca diz se é recente.
- As trilhas da clareira continuam as mesmas.
- **Mapa:** o javali só aparece depois que você o viu por perto (180 px) ou examinou uma pegada da trilha dele. Isso fica salvo na jornada.

**Técnicas:** o Disparo de Ki que chega ao fim do alcance **se desfaz ali**: o crescente se parte, sobem partículas e fica uma marca no chão por 1 s. O pleno chega a 210 px e o enfraquecido a 105 px. O passo longo vai 114 px pleno e 62 px enfraquecido (medido).

## Onde mexer (padrão de elementos)

| Para mudar | Arquivo |
|---|---|
| **Orelhas do coelho** | `src/Elements/Fauna/CoelhoArt.cs` → `Ears` |
| Agachado, corrida, mergulho | `CoelhoArt.Draw` e `CoelhoArt.Dive` (duração: `rabbit_dive_seconds`) |
| **Poeira do javali** | `src/Elements/Fauna/JavaliArt.cs` → `Dust` |
| **Faixa da investida** | `JavaliArt.Strip` (desenho) e `JavaliArt.StripLength` (comprimento); cores `StripEdge`, `StripFill` |
| Resvalo, recuo, corpo | `JavaliArt.Glance`, `JavaliArt.Body` |
| **Duração de uma pegada** | `assets/data/plains/plains_fauna.json` → `track_seconds` |
| Desenho da pegada | `src/Elements/Fauna/PegadaArt.cs` |
| Onde o Ki se desfaz | `src/Combat/KiFizzle.cs` |
| Barra junto ao animal | `src/Game/NearBar.cs` |
| Cone do exame e precisão por grau | `src/Game/TrackHint.cs`; `track_sector_degrees` no JSON |
| Todos os números da T07 | `assets/data/plains/plains_fauna.json` (o bloco `_doc` explica cada chave) |

O cérebro e o corpo continuam em `src/Creatures/` (`BoarBrain`, `Boar`, `RabbitBrain`, `Rabbit`). As pegadas frescas estão em `src/Creatures/FreshTracks.cs`. O índice está em `docs/elementos/INDICE.md`.

## Testes

**Bateria completa pelo `PainelDeTestes.exe --auto`, com janela no monitor 2, nas sementes 20261126 e 777** (11 min 42 s). O relatório do Painel está em `production/qa/evidence/2026-11-28_t07-fauna/painel/relatorio.md`.

| Rodada | Resultado | O que aconteceu |
|---|---|---|
| Painel, bateria completa (rodada direta) | **871/871 verificações**, 1 execução marcada como falha | **Recuperação · controls2**: as 4 verificações passaram, mas o processo do Godot saiu com código diferente de zero ao fechar. Essa execução troca as teclas, não tem relação com a T07 |
| Repetições | controls → controls2 **3 vezes, 4/4 e código 0 nas três** | não se reproduz |
| **Total** | **871/871** | as 22 execuções do `FaunaCheck` (11 fases × 2 sementes) passaram na bateria, assim como `JourneyCheck` com o texto novo e o `AtributosCheck` da diagonal |

| Teste | Resultado (isolado, antes da bateria) |
|---|---|
| Unidade | **57/57**. Novos em `TrackingTests`: o setor sempre contém a direção real e fica mais estreito com o grau; as pegadas acompanham uma curva e um salto não deixa pegada |
| `FaunaCheck` (novo), semente 20261126 | duelo 8/8 · faixa 5/5 · barra 6/6 · lamina 1/1 · coelho 4/4 · pegadas 7/7 · tecnicas 3/3 · mapa 4/4 · vento 1/1 · oldsave 3/3 · trilhas 2/2 |
| `FaunaCheck`, semente 777 | os mesmos números |
| Testes antigos | `JourneyCheck` passou a esperar o texto novo do exame ("Levam mais ou menos para o…") |

**Correções feitas durante os testes:**
- A pegada logo depois de uma curva apontava para a média do trecho, com 25° de desvio na semente 777. Agora ela usa o movimento recente do animal: 0°.
- O efeito do fim do Disparo era pequeno demais na captura. Ficou maior e dura 1 s.
- A Lâmina às vezes não saía porque o terreno de um lado tinha obstáculo. O teste agora tenta pelos dois lados.

**Legível na rajada:** com o vento no pico (0,98), a faixa muda 4 860 px da imagem, e o capim balançando não a esconde. **A chuva fica para a T09:** a fase `vento` serve de modelo para a futura fase `chuva`.

A jornada antiga (`tests/fixtures/saves/jornada_antes_do_ki.json`) carrega sem javalis vistos e com a faixa completa. Todos os testes usam `user://teste_fauna.json` e `user://teste_fauna_prefs.json`: sua jornada e suas teclas não foram tocadas.

## Evidências (só a imagem do jogo)

Pasta `production/qa/evidence/2026-11-28_t07-fauna/`:

| Arquivo | O que mostra |
|---|---|
| `video-duelo.gif` | aparo, investida e contra-ataque |
| `video-coelho-ate-a-toca.gif` | aproximação andando e com Shift, a fuga e o mergulho |
| `duelo-1-aparo`, `duelo-2-investida`, `duelo-3-contra-ataque` | os três resultados |
| `faixa-sem-assistencia`, `faixa-com-assistencia` | a faixa com a direção travada |
| `barra-junto-focado` | "6/6" sobre o javali |
| `lamina-atravessa` | a Lâmina no aviso |
| `coelho-1-orelhas-e-agachado`, `coelho-2-correndo`, `coelho-3-mergulho`, `coelho-4-arbusto` | o coelho |
| `pegadas-1-fuga`, `pegadas-2-exame-novato`, `pegadas-2-exame-mestre`, `pegadas-3-roda-escolhe-a-pegada` | as pegadas e o exame |
| `tecnica-disparo-plena`, `tecnica-disparo-enfraquecida`, `tecnica-passo-plena`, `tecnica-passo-enfraquecida` | pleno e enfraquecido, na mesma escala de câmera |
| `mapa-1-antes`, `mapa-2-depois-de-visto` | o javali no mapa |
| `vento-faixa-na-rajada` | a faixa no pico da rajada |

## Roteiro curto — PainelDeTestes.exe → Jogar

1. Numa jornada já na Planície, siga uma trilha de casco até o lamaçal. Veja o javali aparecer no mapa (**M**).
2. Chegue perto, aperte **Espaço** e veja a barra "6/6" sobre ele. Quando ele raspar o casco, corte (**J**): a lâmina resvala com um som seco e a Vida não muda.
3. Olhe a faixa de terra no chão, saia da linha e corte enquanto ele se recompõe (estrelinhas). O corpo dá um tranco e a barra baixa.
4. **Esc → Modo de assistência → Faixa da investida do javali:** troque para curta e para desligada.
5. Ande até um coelho e depois tente de novo com **Shift**. Veja o agachado, a fuga e o mergulho na toca.
6. Examine (**E**) uma pegada fresca da fuga e veja o cone apontando para a toca. Com mais Rastreamento, o cone fica mais estreito.
7. Dispare o Ki (**K**) num campo aberto e veja onde ele se desfaz.

## Pendências

- **Chuva:** repetir a leitura com chuva quando a T09 existir.
- **Postura do javali:** fica para a T08 (chefes e inimigos humanos).
- A conferência no seu monitor (cores da faixa, tamanho da barra, diagonal) está no [README de tarefas](../tasks/README.md).
