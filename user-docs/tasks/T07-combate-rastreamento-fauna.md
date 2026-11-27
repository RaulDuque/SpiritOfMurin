# T07 — Combate, rastreamento e leitura da fauna real (Entrega 7 da campanha)

**Estado:** planejada · **Frente:** B (parte 1) · **Depende de:** T05, T06 (escolha de interação) · **Origem:** pedido 10 (Entrega 7, `user-docs/relatorios/prompts/10-campanha-inicial.md`), pedido 14 itens 1, 6.B e 7 (`prompts/14-organizacao-criacao-conteudo.md`), `user-docs/futuro-do game/gpt.md` (padrões do coelho e do javali, Rastreamento)

## Objetivo do jogador

Ler o javali e o coelho **olhando para eles**, sem depender de texto: saber quando o javali vai investir, para onde, se o golpe entrou ou resvalou e quando é a hora do contra-ataque. Ver o coelho erguer as orelhas, disparar e sumir na toca, e perceber que andar quieto (Shift) muda isso. Seguir pegadas que apontam para onde o animal realmente foi, com mais detalhe conforme o Rastreamento sobe. Sentir, numa luta, que a técnica enfraquecida vai menos longe que a do prólogo.

## O que já existe hoje

Fonte: `user-docs/relatorios/2026-11-26-visual-mobs.md` §3.7–3.10 e §4, `design/gdd/plains-first-stretch.md` §3.3–3.4, `design/gdd/plains-progression.md` §3.4–3.6.

- **Javali** (`src/Creatures/Boar.cs`, `BoarBrain.cs`, dados em `assets/data/plains/BoarTuning.tres`):
  - aviso de 0,8 s com cabeça baixa, olho vermelho, casco raspando e poeira; a cabeça segue o jogador por 55% do aviso e depois trava com 4 marcas no chão;
  - investida reta (240 px/s, até 170 px) com poeira; golpes resvalam com faísca em cruz;
  - recomposição de 1 s com estrelinhas (abertura), +0,15 s por golpe;
  - com Modo de assistência: corredor tracejado, anel de guarda e anel ciano da abertura.
- **Coelho** (`Rabbit.cs`, `RabbitBrain.cs`, `RabbitTuning.tres`): audição por modo de andar (parado 14 px, quieto 26, andando 95, passo/corte 130), orelhas em pé + congela 0,35 s, duas arrancadas com pausa, some na toca por 4,5 s.
- **Barra do animal focado** (Espaço, até 170 px) no alto da tela, com "Javali da Mata · Nv 4 · Territorial" e "atual/máximo" (pedido 14.0).
- **Números de dano** no Modo de assistência (`src/Game/CombatText.cs`).
- **Pegadas** (`src/Plains/Landmarks.cs`): trilhas geradas pela semente, a 22 px, apontando para o animal; E examina; Rastreamento muda o detalhe (direção → distância e 3 pegadas brilhando → nível → trilha inteira).
- **Técnicas enfraquecidas** no nível 3 com valores em `assets/data/combat_techniques.json` (campos `weak`); Disparo de Ki 105 px (pleno 210), passo lateral 55° (pleno 95°).
- **Vento** em rajadas, capim alto e folhas (`src/Plains/Ambient.cs`, relatório `2026-11-27-ambientacao.md`).

**Limitações conhecidas** (visual-mobs §7): orelhas em pé do coelho não espelham; clarão de dano do coelho quase não aparece (1 de vida); carcaças não mudam ao tirar partes; javali sem animação de fuçar e sem sinal ao voltar para casa; animais só de perfil.

## O que falta

1. **Três resultados distintos no javali**, cada um com brilho breve, reação do corpo, som e impacto moderado, coincidindo com o quadro do contato:
   - **golpe aparado no couro** (aviso/investida): faísca + som seco + o jogador recua 1–2 px, sem dano;
   - **investida que acerta**: o personagem é empurrado, clarão vermelho curto, som pesado;
   - **contra-ataque na recomposição**: clarão no corpo do javali, ele encolhe, som de carne, número de dano (se ligado).
2. **Faixa curta e discreta no chão** com a trajetória fixada (além das 4 marcas), com opção em Esc → Modo de assistência para reduzir ou desligar.
3. **Barra compacta junto ao animal focado** enquanto houver combate (vida e, se decidido, postura), além ou no lugar da barra no alto.
4. **Técnica enfraquecida legível:** o Disparo de Ki enfraquecido termina visivelmente mais perto (marca/esfarelar no fim do alcance) e o passo cobre menos chão (rastro curto); a diferença precisa ser perceptível numa luta, e medida contra o prólogo.
5. **Coelho legível:** orelhas em pé espelhando com a direção; um quadro claro de "vai fugir" (corpo encolhido antes da arrancada); entrada na toca visível (mergulho + poeirinha); diferença observável ao se aproximar com Shift.
6. **Pegadas que seguem o deslocamento real:** animais que se movem deixam pegadas novas orientadas pelo sentido real do movimento (pastar, fugir, investir), que somem com o tempo; examinar mostra uma indicação breve na direção da toca ou do animal.
7. **Precisão crescente com Rastreamento:** a indicação começa vaga (setor largo, "para o noroeste") e fica mais precisa por grau (setor estreito, distância, idade da pegada, nível).
8. **Leitura com vento e chuva:** os avisos do javali e as orelhas do coelho continuam legíveis numa rajada; o mesmo critério vale para a chuva quando a T09 existir (deixar o teste preparado).
9. Organizar javali, coelho, pegadas e efeitos de combate conforme `docs/elementos/PADRAO.md`, com tempos de animação hoje fixos no código (encolher 0,32 s, clarão 0,12 s etc.) passando para dados.
10. Os testes finais que dependem das Entregas 6 e 7 ficam na tarefa da Entrega 8 (ver `user-docs/tasks/README.md`).

## Dependências

- **T05** (padrão aplicado aos elementos e reação dos arbustos à passagem de animais).
- **T06** (escolha de interação: examinar pegada perto de carcaça ou erva; rodapé contextual).
- **T09** só para a parte "legível na chuva" (o critério de chuva fica pendente até a T09).

## Decisões abertas (do criador)

1. **Lâmina enfraquecida contra o javali** (pendência do pedido 14 item 7): o Passo da Lâmina Silenciosa enfraquecido (dano 3) pode atravessar o couro durante a investida?
   - A) não, só acerta na recomposição como os outros golpes (recomendado: mantém a lição do javali);
   - B) sim, com dano reduzido;
   - C) sim, e interrompe a investida.
2. **Faixa de trajetória:** ligada por padrão e redutível (recomendado), desligada por padrão, ou só no Modo de assistência?
3. **Barra junto ao animal:** substitui a barra do alto (A), soma-se a ela (B, recomendado durante combate), ou só aparece com o Modo de assistência (C)?
4. **Postura do javali:** a barra mostra só Vida (recomendado agora) ou também uma barra de postura que, cheia, prolonga a recomposição?
5. **Descoberta dos animais no mapa** (pendência do pedido 14 item 7): javalis vivos aparecem no mapa dos blocos visitados (como hoje), só depois de examinar uma trilha deles, ou nunca?
6. **Coelho com 1 de vida:** manter morte num golpe (recomendado) ou dar 2 de vida para o clarão de dano aparecer?
7. **Duração das pegadas dinâmicas:** quanto tempo uma pegada nova fica no chão (proposta: 90 s de jogo, valor em dados)?

## Critérios de aceite (jogáveis e testáveis)

1. Golpe no javali durante o aviso: aparece a faísca, toca o som de couro e a Vida do javali não muda.
2. Investida que acerta: o personagem perde a Vida prevista (com CON aplicada), é empurrado e o clarão sai no mesmo quadro do dano.
3. Golpe na recomposição: a Vida do javali cai no mesmo quadro do clarão no corpo dele; a recomposição aumenta `HitFlinch`.
4. Com a faixa ligada, ela aparece quando a direção trava e coincide com a linha percorrida (desvio menor que 4 px em teste); desligada, não aparece.
5. Em combate, a barra junto ao animal focado mostra "atual/máximo" e some ao sair do combate (regra decidida).
6. No nível 3, o Disparo de Ki enfraquecido termina a 105 px ± 4 px com o sinal de fim de alcance; no prólogo, a 210 px. O passo enfraquecido cobre menos chão que o pleno, medido.
7. Com o coelho virado para a esquerda, as orelhas em pé ficam no lado certo da cabeça.
8. Aproximar-se andando faz o coelho fugir a ~95 px; com Shift, a ~26 px (valores dos dados por nível).
9. O coelho que foge deixa pegadas novas apontando no sentido real da corrida (ângulo dentro de ±20°) e entrando na toca.
10. Examinar uma pegada com Rastreamento 0 dá um setor largo; com grau 2, o setor é mais estreito e mostra o nível; o setor sempre contém a direção real.
11. Numa rajada forte, o aviso do javali (cabeça baixa, olho vermelho, poeira) continua identificável numa captura do viewport; o teste fica preparado para a chuva da T09.
12. Nas sementes 20261126 e 777, o coelho e o javali garantidos do início continuam com trilha começando na clareira.
13. Jornada antiga (fixture) carrega sem pegadas dinâmicas quebradas nem animais duplicados.

## Roteiro de teste

1. Unidade: classificação do resultado do golpe por estado (aparado / acerto / contra-ataque); direção da pegada pela velocidade real; setor de Rastreamento por grau (sempre contém a verdade); fim de alcance do disparo pleno e enfraquecido.
2. Integração (nova `FaunaCheck`, fases `boar`, `rabbit`, `tracks`, `weak`, `wind`): entrada simulada, medir quadros de contato e de efeito, distâncias e ângulos.
3. Evidências do viewport: sequência aviso → investida → recomposição; os três resultados; coelho alerta, arrancada e toca; pegadas de fuga; disparo pleno × enfraquecido lado a lado; aviso numa rajada.
4. Jogar pelo Painel → Jogar: encontrar o javali pela trilha, errar de propósito, desviar e contra-atacar.

## Prompt pronto para executar

```text
Tarefa T07 do SpiritOfMurin — Combate, rastreamento e leitura da fauna real (Entrega 7 da campanha).

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T07-combate-rastreamento-fauna.md. Leia também user-docs/relatorios/2026-11-26-visual-mobs.md (§3.7 a §4 e §7), design/gdd/plains-first-stretch.md §3.3–3.4 e design/gdd/plains-progression.md §3.4–3.6. Confira no README de tasks se T05 e T06 estão prontas.

Trabalhe com o javali e o coelho reais do jogo (src/Creatures/). Implemente o que está em "O que falta" da T07: os três resultados distintos (aparado no couro, investida que acerta, contra-ataque na recomposição) com efeito coincidindo com o contato; faixa de trajetória no chão com opção para reduzir; barra compacta junto ao animal focado; diferença visível das técnicas enfraquecidas; orelhas, fuga e entrada na toca legíveis, com diferença observável ao usar Shift; pegadas que seguem o deslocamento real; indicação ao examinar cuja precisão cresce com Rastreamento; leitura dos avisos durante as rajadas (deixe o teste pronto para a chuva da T09).

Antes de implementar, pergunte-me as "Decisões abertas" da T07, uma por vez, com opções e recomendação (inclui a Lâmina enfraquecida contra o javali e a descoberta dos animais no mapa). Escolhas visuais vêm com prancha HTML em user-docs/relatorios/.

Regras:
- Organize javali, coelho, pegadas e efeitos conforme docs/elementos/PADRAO.md. Preserve a aparência aprovada; mude só o que a tarefa pede.
- Tempos, distâncias e opções em assets/data/ (BoarTuning.tres, RabbitTuning.tres, plains_progression.json ou arquivos novos do padrão).
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2. Inclua os testes no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só da imagem do viewport do jogo, em production/qa/evidence/<data>_combate-fauna/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice README.md, commits atômicos (git add por arquivo), push para origin. Atualize o estado da T07 em user-docs/tasks/README.md com links do relatório, evidências e commits.
```
