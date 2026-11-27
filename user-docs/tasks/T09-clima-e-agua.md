# T09 — Clima e corpos d'água

**Estado:** planejada · **Frente:** C · **Depende de:** T07 (leitura dos avisos) · **Origem:** pedido 14 item 6.C (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`), pedido 10 Entrega 4 (`prompts/10-campanha-inicial.md`), `user-docs/futuro-do game/gpt.md` (Pedras do Riacho, tartarugas, ciclo de horário)

## Objetivo do jogador

Sentir o tempo mudar: uma tarde calma vira vento forte, depois chuva, e às vezes trovoada com raios. Perceber a mudança chegando (céu, som, animais) e decidir se continua caçando ou se abriga. Encontrar lagos que se reconhecem no mapa, com margem, parte rasa e funda, peixes visíveis na água e patos que entram na água quando ele se aproxima.

## O que já existe hoje

Fonte: `user-docs/relatorios/2026-11-27-ambientacao.md`; código em `src/Plains/Ambient.cs`; dados em `assets/data/plains/plains_ambient.json`.

- **Dia e noite:** dia de 8 min reais, paleta por hora (`DayNight`, `Phase`: Madrugada, Amanhecer, Manhã, Tarde, Entardecer, Anoitecer, Noite), luz da fogueira e do personagem, relógio sob o minimapa, hora salva ao meditar.
- **Vento:** brisa constante + rajadas que correm pelo chão como onda (`Wind`: base 0,25, rajada 1, período 7 s, direção 15°); capim alto, copas e arbustos balançam; folhas nas rajadas.
- **Pássaros** de dia, com sombra.
- **Água:** riacho de norte a sul (`PlainsGenerator`, `plains_worldgen.json`), vau raso atravessável a 60% da velocidade com ondinhas, água funda mais escura e bloqueando.
- **Mapa (M):** blocos visitados coloridos pelo terreno, inclusive água e vau (`design/gdd/display-and-map.md`).
- **Não existe:** estado de clima, chuva, trovão, raio, som de chuva, lagos, peixes, patos.

## O que falta

### Clima
1. **Seis estados:** calmaria, vento forte, chuva leve, chuva forte, trovoada, e raios (dentro da trovoada).
2. **Transições** graduais (o céu escurece antes da chuva; a chuva engrossa e afina), nunca troca de um quadro para outro. Encadeamento proposto: calmaria ↔ vento forte ↔ chuva leve ↔ chuva forte ↔ trovoada.
3. **Duração** de cada estado em dados (faixa mínima–máxima em minutos de jogo), sorteada a partir da semente e do dia do mundo, para ser reproduzível.
4. **Sinais visuais:** tinta do céu somada à do horário; gotas na tela e no chão; poças que aparecem e somem; respingos nos pés; clarão de raio que ilumina a cena por um instante.
5. **Sinais sonoros:** vento forte, chuva leve e forte, trovão com atraso proporcional à distância do raio. Respeitar o silêncio e a volta dos sons da natureza já usados na campanha.
6. **Horário:** chuva à noite fica mais escura, mas a luz do personagem e da fogueira continua mantendo o combate legível; a fogueira se apaga na chuva forte? (decisão aberta).
7. **Visibilidade:** a chuva forte reduz o alcance de visão desenhado (névoa leve nas bordas), sem esconder avisos de ataque a curta distância.
8. **Animais:** reações por espécie em dados (coelho fica na toca mais tempo na chuva forte; pássaros somem antes da trovoada; javali indiferente; pegadas no lamaçal ficam mais fundas e as antigas se apagam). A chuva cobre parte do som dos passos (audição do coelho reduzida).
9. **Save:** o estado de clima e o tempo restante vão na jornada; Continuar volta no mesmo clima.
10. **Raios causam dano?** — decisão separada (abaixo). Até ela ser tomada, raio é só visual e sonoro.

### Água
11. **Lagos** gerados pela semente longe da clareira (sem quebrar as garantias do início nem o riacho), com forma orgânica, **reconhecíveis no mapa** (cor própria e contorno), e opcionalmente ligados ao riacho.
12. **Margem** (areia/lama), **rasa** (atravessável, lenta, ondinhas — mesma regra do vau) e **funda** (bloqueia, mais escura).
13. **Peixes:** sombras que nadam na parte rasa e na funda perto da margem, fogem quando o jogador entra na água. Pesca fica para depois (decisão aberta).
14. **Patos:** usam a ficha da T08 (sub-entrega T08h) — andam na margem e entram na água ao ver o jogador.
15. Lagos, peixes e clima organizados conforme `docs/elementos/PADRAO.md`.

## Dependências

- **T07:** critério de legibilidade dos avisos (o teste preparado para chuva).
- **T08h** (patos) pode ser entregue junto com os lagos desta tarefa ou logo depois.
- `PlainsGenerator` e `MapView` (lagos), `Ambient.cs` (clima).

## Decisões abertas (do criador)

1. **Raios causam dano?** *(decisão separada, pedida pelo criador)*
   - A) não: só clarão, som e susto (recomendado para a primeira versão);
   - B) sim, só em árvores isoladas e altas, que pegam fogo ou racham (o jogador perto delas leva dano), com aviso legível antes (estalo, cabelo arrepiado, faísca);
   - C) sim, podem cair perto do jogador em campo aberto durante a trovoada, sempre com aviso legível de ~1 s no chão.
2. **Frequência do clima:** quanto tempo, em média, de tempo bom antes de uma chuva? Proposta: 70% do tempo calmo/vento, 25% chuva, 5% trovoada.
3. **A fogueira apaga na chuva forte?** A) não (recomendado: é o ponto seguro); B) sim, e acende de novo depois; C) só sem abrigo.
4. **Lagos:** quantos por área e a que distância da clareira? Proposta: 1 lago a cada ~6×6 blocos, nenhum a menos de 3 blocos da clareira.
5. **Peixes:** só ambientação agora (A, recomendado) ou já com pesca (B, precisa de vara e proficiência)?
6. **Clima no prólogo (Vale):** fica sempre como está (recomendado) ou também muda?

## Critérios de aceite (jogáveis e testáveis)

1. Forçando cada estado por comando de teste, a cena mostra os sinais visuais e toca o som correspondente; o estado aparece no relatório de teste.
2. Uma transição calmaria → chuva forte leva pelo menos o tempo mínimo em dados, e a densidade de gotas cresce de forma monotônica durante ela.
3. Com a mesma semente e o mesmo dia do mundo, a sequência de climas é idêntica em duas execuções.
4. Na trovoada, cada clarão de raio é seguido de trovão com atraso proporcional à distância (tolerância em dados). Sem a decisão 1, nenhum raio tira Vida.
5. À noite com chuva forte, o aviso do javali a 60 px continua identificável numa captura do viewport (mesmo critério da T07).
6. Na chuva forte, o coelho ouve um jogador andando a uma distância menor que no tempo seco (valor em dados) e fica mais tempo na toca.
7. Meditar durante a chuva, fechar e continuar volta na chuva, com o tempo restante aproximado.
8. Nas sementes 20261126 e 777, existe pelo menos um lago dentro do raio de busca definido; ele aparece no mapa com cor própria depois de visitado.
9. Entrar na parte rasa do lago reduz a velocidade a `shallow_speed` e gera ondinhas; a funda bloqueia.
10. **Teste do criador:** a partir de um ponto de observação fixo na margem de um lago (seed 20261126), o jogador parado vê **um peixe nadando** dentro do lago em até 20 s, e, ao andar em direção a um **pato** na margem, vê o pato **sair da margem e entrar na água** (posição do pato passa de tile de margem para tile de água). Captura antes/depois do viewport.
11. Peixes nunca aparecem fora da água; patos nunca entram em água funda andando (nadam).
12. A jornada antiga (fixture) carrega com clima calmo e sem erro.

## Roteiro de teste

1. Unidade: máquina de estados do clima (transições permitidas, duração, determinismo por semente/dia); geração de lagos (pureza: função de semente e coordenada, igual dos dois lados da divisa entre blocos); regra rasa/funda.
2. Integração (nova `ClimaCheck`, fases `estados`, `transicao`, `noite`, `animais`, `save`; nova `LagoCheck`, fases `mapa`, `agua`, `peixe-pato`).
3. Evidências do viewport: cada estado de clima na mesma paisagem; o mesmo ponto de dia e de noite com chuva; lago no mapa; peixe; pato antes/depois de entrar na água.
4. Jogar pelo Painel → Jogar e esperar uma mudança de clima natural (anotar o tempo).

## Prompt pronto para executar

```text
Tarefa T09 do SpiritOfMurin — Clima e corpos d'água.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T09-clima-e-agua.md. Leia user-docs/relatorios/2026-11-27-ambientacao.md, design/gdd/plains-first-stretch.md §3.1 (mundo por semente) e design/gdd/display-and-map.md. Confira no README de tasks se a T07 está pronta (o critério de legibilidade dos avisos).

Implemente o que está em "O que falta" da T09: calmaria, vento forte, chuva leve, chuva forte, trovoada e raios, com transições, duração, sinais visuais e sonoros, interação com o horário, a visibilidade e os animais, e o clima no save. Depois, lagos gerados pela semente e reconhecíveis no mapa, com margem, rasa e funda, peixes visíveis e patos (ficha da T08, sub-entrega T08h).

Antes de implementar, pergunte-me as "Decisões abertas" da T09, uma por vez, com opções e recomendação. "Raios causam dano?" é uma decisão separada: até eu responder, raio é só visual e sonoro. Escolhas visuais (cor da chuva, do lago no mapa) vêm com prancha HTML em user-docs/relatorios/.

Regras:
- Organize clima, lagos, peixes e patos conforme docs/elementos/PADRAO.md.
- Estados, durações, probabilidades, cores e reações dos animais em assets/data/ (plains_ambient.json, plains_worldgen.json ou arquivos novos do padrão). A geração de lagos é pura: função da semente e da coordenada.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1. Inclua o teste do peixe e do pato saindo da margem para a água.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_clima-agua/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a T09 em user-docs/tasks/README.md com links.
```
