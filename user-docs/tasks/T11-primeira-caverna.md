# T11 — Primeira caverna jogável

**Estado:** planejada · **Frente:** D · **Depende de:** T10, T07 (e uma espécie da T08 se for usada como habitante) · **Origem:** pedido 14 item 6.D (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`); regras comuns em `user-docs/tasks/T10-escavacao-cavernas-dungeons.md`

## Objetivo do jogador

Na Planície, ver uma boca de caverna numa encosta de pedra, entrar, perceber que está em outro lugar (escuro, eco, gotejar), avançar com a luz que tem, minerar um veio, enfrentar o que vive ali, achar uma passagem fechada que leva à dungeon (T12) e sair no mesmo ponto da superfície por onde entrou. Voltar depois e ver o veio esgotado e o inimigo morto continuarem assim.

## O que já existe hoje

- Regras comuns, modelo de dados e de save de cavernas preparados pela **T10** (verificar o relatório dela).
- Troca de cena com etapas persistentes (`src/Campaign/Campaign.tscn`).
- Mineração de rocha da **T05** (rachaduras, materiais, SOR).
- Linguagem de combate da **T07**; luz do personagem e da fogueira (`src/Plains/Ambient.cs`, `SoftLight`).
- `PlainsGenerator.AddStamp` para encaixar a entrada.

## O que falta

1. **Uma entrada** garantida pela semente, a uma distância média da clareira (proposta: 3–5 blocos), em encosta de pedra; protegida contra escavação e objetos; aparece no mapa depois de vista, com um nome.
2. **Cena da caverna** feita à mão (ou conforme a decisão 1 da T10), pequena: 4–6 câmaras ligadas, uma bifurcação, um beco com recompensa, um veio de minério, uma câmara com o habitante principal e a passagem fechada para a dungeon.
3. **Atmosfera:** escuro com a luz do personagem; som de gotas e eco; chão de pedra com poças rasas; sem ciclo de dia (o relógio para ou continua? decisão aberta).
4. **Habitantes:** pelo menos uma criatura de caverna com sinal → ação → abertura (proposta: morcegos que revoam quando há barulho, e um bicho territorial como guardião).
5. **Recursos:** veio de minério minerável (mesmo sistema da T05, tabela própria); materiais de caverna (proposta: Pedra Úmida, Cristal Opaco).
6. **Passagem para a dungeon:** visível, fechada, com uma pista de como abrir (abre na T12).
7. **Saída:** a boca leva de volta ao tile da entrada com a direção de saída; uma segunda saída opcional pode levar a outro ponto (decisão aberta).
8. **Memória:** câmaras visitadas, veio esgotado, criaturas mortas, recompensa do beco: tudo no save.
9. **Mapa da caverna** em M, revelado por câmara visitada (reaproveitando o mapa por blocos).
10. Organização conforme `docs/elementos/PADRAO.md`.

## Dependências

- **T10** (regras, modelo de dados, proteção de tiles).
- **T07** (combate legível), **T05** (mineração).
- **T08** se o habitante for uma espécie da Planície.

## Decisões abertas (do criador)

1. **Quem vive na primeira caverna?** A) morcegos + um guardião animal (proposta: texugo de pedra ou aranha grande) — recomendado; B) bandidos escondidos (humanos); C) só fauna passiva e o perigo vem do terreno.
2. **O relógio dentro da caverna:** A) continua correndo (recomendado: coerência ao sair); B) para.
3. **Uma ou duas saídas?** A) uma (recomendado para a primeira); B) duas, a segunda em outro ponto da Planície.
4. **Luz:** A) a luz do personagem basta (recomendado); B) tocha fabricável que ilumina mais e dura um tempo.
5. **Nome da caverna** e do lugar na superfície (proposta para escolher: "Gruta do Eco", "Boca da Pedra Úmida").

## Critérios de aceite (jogáveis e testáveis)

1. Nas sementes 20261126 e 777, a entrada existe na faixa de distância decidida, não cai em lugar protegido e aparece no mapa depois de vista.
2. E na boca troca para a cena da caverna; sair volta ao tile da boca, virado para fora, a menos de 8 px da posição de saída definida.
3. Dentro, a luz do personagem permite ler os avisos do habitante a 60 px (captura do viewport).
4. O habitante principal mostra o sinal antes do ataque e tem a abertura prevista; derrotado, continua derrotado depois de sair, meditar, fechar e continuar.
5. O veio de minério rende os materiais da tabela uma única vez e fica esgotado.
6. A passagem para a dungeon está visível e fechada, com a pista legível.
7. O mapa da caverna revela só as câmaras visitadas e persiste.
8. Desmaiar dentro segue a regra decidida na T10 e não duplica recompensas.
9. Jornada antiga (fixture) carrega e a caverna está intocada.

## Roteiro de teste

1. Unidade: posição da entrada por semente; proteção; serialização do estado da caverna.
2. Integração `CavernaCheck` (fases `entrada`, `ida-volta`, `habitante`, `veio`, `mapa`, `save`).
3. Evidências do viewport: entrada na Planície, primeira câmara, habitante avisando, veio esgotado, passagem fechada, mapa da caverna.
4. Jogar pelo Painel → Jogar: da clareira até a caverna e de volta.

## Prompt pronto para executar

```text
Tarefa T11 do SpiritOfMurin — Primeira caverna jogável.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md, user-docs/tasks/T10-escavacao-cavernas-dungeons.md e user-docs/tasks/T11-primeira-caverna.md. Leia o relatório da T10 (o modelo de dados e de save de cavernas) e o da T05 (mineração). Confirme no README de tasks que T10 e T07 estão prontas.

Quero uma primeira caverna pequena e jogável: entrada na Planície garantida pela semente e protegida, cena separada com 4 a 6 câmaras, atmosfera escura com eco e gotas, um habitante com sinal, ataque e abertura, um veio de minério, um beco com recompensa, a passagem fechada para a dungeon (T12) com uma pista, mapa da caverna por câmara e retorno à mesma posição da superfície. Tudo o que for explorado, aberto e recolhido fica no save.

Antes de implementar, pergunte-me as "Decisões abertas" da T11 (e as da T10 que ainda estiverem abertas), uma por vez, com opções e recomendação. Mostre o desenho da entrada, das câmaras e do habitante numa prancha HTML em user-docs/relatorios/ antes de pedir aprovação.

Regras:
- Organize entrada, câmaras, habitante, veio e passagem conforme docs/elementos/PADRAO.md (pode usar /create).
- Distância da entrada, tabelas, vida e tempos em assets/data/.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_primeira-caverna/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a T11 em user-docs/tasks/README.md com links.
```
