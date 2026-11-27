# Pedido 14 — organização, padrão de elementos, /create e Entrega 5 — 2026-11-27

> O pedido está em [prompts/14-organizacao-criacao-conteudo.md](prompts/14-organizacao-criacao-conteudo.md). As entregas, na ordem em que foram feitas:
> - **14.0** — Ki no lugar de Qi, FOR, defesa de CON, crítico, vida do inimigo em números e números de dano. Relatório próprio: [2026-11-27-atributos-ki.md](2026-11-27-atributos-ki.md).
> - **14.1, 14.2, 14.3** — pedidos que você mandou durante o trabalho: brilho azul e corte maior; ícones no minimapa; andar sobre a água.
> - **Pedido 14, itens 1 a 8** — tarefas, padrão de elementos, guia, `/create`, Entrega 5, prompts futuros, pendências e testes.
>
> Você pediu para terminar tudo primeiro e testar no fim. Entre as partes rodei só a compilação, os testes de unidade e o teste novo de cada parte. A bateria completa rodou uma vez, no fim (seção 9).

## 1. Pasta de tarefas — o ponto de retomada

**[user-docs/tasks/README.md](../tasks/README.md)** responde, em um minuto:
- o que está pronto e como testar;
- o que está parcial e o que falta;
- o que está em andamento;
- as decisões que dependem de você;
- a conferência manual;
- a ordem das próximas entregas;
- qual prompt mandar agora.

**Tarefas com arquivo próprio**, cada uma com objetivo, estado, o que falta, dependências, decisões abertas, critérios, roteiro de teste e prompt pronto:
- T05 (pronta: Entrega 5);
- T06 (bolsa, ficha e HUD);
- T07 (combate e rastreamento);
- T08 (fauna, elites e chefes);
- T09 (clima e água);
- T10, T11 e T12 (escavação, primeira caverna, primeira dungeon);
- T13 (reinos de cultivo);
- T14, T15 e T16 (NPCs, vilarejo, cidade);
- T17 (levar os elementos antigos ao padrão);
- T18 (armas, roupas e capacetes).

As ideias de `user-docs/futuro-do game/` foram copiadas para as tarefas como **ideias, não decisões**: espécies, variantes raras, materiais, profissões. Todas as decisões ficam abertas para você.

**Conciliação dos pedidos anteriores:** [user-docs/tasks/conciliacao-2026-11-27.md](../tasks/conciliacao-2026-11-27.md). O levantamento cruzou cada pedido do prompt 10 (entregas 4 a 8) e do prompt 13 com o código e com os testes que rodam o jogo. Os principais resultados:
- **Já existem e são testados:** a fuga do coelho (duas arrancadas, orelhas, toca) e o aviso do javali (casco, cabeça baixa, olho, poeira). Os relatos estavam certos.
- **Existem, mas falta deixar visível:** a entrada na toca (o coelho só some), o som e o impacto dos golpes na Planície, a faixa da investida (a opção existe e nada a usa) e a diferença do Shift (não há sinal na tela).
- **Faltam de verdade:** aves com comportamento próprio, clima, categorias e atalhos na bolsa, e pegadas deixadas pelo movimento real.

## 2. Padrão de elementos

- **Regra curta no `CLAUDE.md`**, seção "Elementos do jogo".
- **Padrão completo:** [docs/elementos/PADRAO.md](../../docs/elementos/PADRAO.md). Cada elemento tem um **id estável** e sete coisas em lugares previsíveis:
  1. o **desenho**, numa classe `<Nome>Art`;
  2. os **dados**, num JSON por elemento;
  3. o **comportamento**, na classe da família;
  4. o **ícone**;
  5. **onde nasce** (os `habitats`);
  6. o **id** usado no save;
  7. a **prévia**.
- **Índice de tudo o que existe hoje:** [docs/elementos/INDICE.md](../../docs/elementos/INDICE.md). Para cada família, diz se está no padrão, parcial ou antiga, e qual tarefa a organiza.

**Migrado nesta rodada** (os elementos da Entrega 5), com caminhos reais:

| Elemento | Desenho | Dados |
|---|---|---|
| Pinheiro | `src/Elements/Flora/PinheiroArt.cs` | `assets/data/elements/arvores/pinheiro.json` |
| Salgueiro (`/create`) | `src/Elements/Flora/SalgueiroArt.cs` | `arvores/salgueiro.json` |
| Pedra e rochas | `src/Elements/Rocks/RochaArt.cs` | `rochas/pedra.json`, `rocha_quartzo.json`, `rocha_ferro.json` |
| Bambu | `src/Elements/Flora/BambuArt.cs` | `bambus/bambu_verde.json` |
| Arbusto | `src/Elements/Flora/ArbustoArt.cs` | `arbustos/arbusto.json` |
| Ferramentas | `src/Elements/Tools/ToolArt.cs` | `plains_items.json` (`tool`) |

- **O Vale continua igual:** o `Obstacle` desenha pelas mesmas artes, sem dados, e nesse caso elas fazem o desenho original.
- **O protagonista não foi mexido:** o `Player.cs` é a sua referência aprovada. Só ganhou a ferramenta na mão, o brilho do Reforço e a água, desenhados à parte.
- **Separar as camadas do personagem** com prova pixel por pixel fica na T18.

## 3. Guia para editar à mão

**[Como criar e editar elementos do SpiritOfMurin](2026-11-27-como-criar-e-editar-elementos.md)** traz:
- a tabela "quero mudar → abro → edito → testo", com cerca de 30 linhas reais;
- passo a passo para árvore e bambu, coelho, ferramenta, item e interface;
- o que já dá para fazer hoje e o que depende de uma tarefa futura (arma, roupa, capacete, inimigo, vendedores).

**Cena de inspeção:** Painel → **Inspeção (elementos)** (`src/Dev/Inspecao.tscn`). Mostra cada elemento no tamanho do jogo, em 5 estados, com rajada, um corpo atravessando e o personagem com cada ferramenta. O modo ao vivo do Painel reabre a cena ao salvar um `.cs` ou um `.json`.

## 4. O comando /create

**Sintaxe:** `/create <tipo> "<descrição>"`. A skill fica em `.claude/skills/create/SKILL.md`. **Nesta sessão o Claude Code a descobriu sozinho, sem reiniciar.**

| Tipo | Hoje |
|---|---|
| `arvore`, `rocha`, `bambu`, `arbusto`, `item`, `ferramenta` (machado, picareta, faca) | **jogável** |
| `arma`, `mob`, `inimigo`, `roupa`, `capacete` | **planejamento**: gera uma ficha e liga à T08 ou à T18 |

**Os dois exemplos testados** (detalhes, arquivos e onde mexer no guia, seção 3):
1. `/create arvore "Salgueiro de margem, com galhos baixos que balançam com o vento"`.
   - Nasce na margem.
   - Leva 5 golpes de machado.
   - Dá 1-2 toras e às vezes Casca de Salgueiro.
   - É jogável no mundo nas duas sementes.
2. `/create rocha "Rocha enferrujada com veio de ferro, que solta minério de ferro"`.
   - Reaproveita a arte da rocha, com veio e manchas de ferrugem.
   - Leva 7 golpes de picareta e dá minério de ferro.
   - Foi testada na cena isolada do `ElementosCheck`.

**A trava dos elementos.** O `tests/integration/ElementosCheck.tscn` percorre **todos** os arquivos de `assets/data/elements` sozinho. Um elemento novo do `/create` é testado sem ninguém editar o teste.
- Pasta vazia ou exceção contam como falha ("NOT ASSESSED").
- Vi essa trava falhar com uma arte inexistente (32/33) antes de confiar nela.

## 5. Entrega 5 — árvores, pedras, bambus e arbustos (jogável)

**Design:** [design/gdd/world-resources.md](../../design/gdd/world-resources.md).

### O que você pode jogar

| Ação | Como |
|---|---|
| **Fabricar** | Machado de Pedra (2 sílex + 2 galhos) e Picareta de Pedra (3 sílex + 1 galho). Na bolsa (I), clique na ferramenta para equipar |
| **Pinheiro** | Machado e J: entalhes no tronco e uma barrinha de progresso. No 6º golpe ele **cai** para o lado oposto ao seu e fica um **toco** (base pequena: dá para passar perto) |
| **Rocha com veio** | Veio claro com brilhos. A picareta abre **rachaduras** que crescem, e no 5º golpe ela vira **cascalho** (não colide) |
| **Pedra sem veio** | Avisa que não tem veio |
| **Rocha enferrujada** | 7 golpes de picareta |
| **Bambu** | Faca ou machado: cada golpe **corta uma haste, que cede e tomba**. Ficam os tocos em bisel |
| **Arbustos** | Agora **deixam passar**. Você, o coelho e o javali, ao atravessar, **inclinam as folhas para onde vão**, e elas voltam. O capim alto também se abre para os animais |
| **Ferramenta errada** | Não trabalha e diz o porquê (no máximo uma vez a cada 1,5 s) |
| **Materiais** | Tora, Resina (bônus), Pedra Bruta, Quartzo (bônus), Minério de Ferro, Vara de Bambu, Broto de Bambu (bônus, dá vigor), Casca de Salgueiro (bônus, alivia a dor) |
| **Usos testáveis** | **Machado Lapidado** (tora + 2 pedras + tira) derruba um pinheiro em 3 golpes. **Picareta Lapidada** (vara de bambu + 2 pedras + tira) |
| **SOR** | Aumenta a chance dos bônus. A rolagem é fixa por semente e lugar: recarregar não muda o resultado, e mais SOR nunca tira um bônus |
| **Persistência** | O trabalho de cada árvore, rocha ou bambu fica guardado pelo id do lugar. Sair do bloco, meditar, fechar e continuar mantém o meio-cortado e o esgotado, e os materiais saem uma vez só |
| **Preservado** | A ponte do Vale, a clareira inicial (os pinheiros e as pedras forçados continuam decorativos), as pegadas e as sementes antigas (o bambu tem sorteio próprio, só nos tiles vazios) |

### Roteiro — Painel → Jogar

1. Continue a jornada ou faça uma nova.
2. Junte 5 sílex e 3 galhos na margem do riacho, a leste da clareira.
3. I → Fabricar → Machado de Pedra e Picareta de Pedra. Clique no machado para equipar.
4. Vá até um pinheiro fora da clareira. O aviso diz "J · cortar pinheiro (6 golpes)". Corte até ele cair.
5. Troque pela picareta. Procure uma rocha **com o veio claro** e quebre-a. Uma pedra lisa avisa que não tem veio.
6. Equipe a faca. Corte uma touceira de bambu na margem.
7. Atravesse um arbusto. Espere um coelho fugir por entre as moitas.
8. Saia para longe, volte e medite: tocos e cascalho continuam.

## 6. Pedidos feitos durante a rodada

| # | Pedido | O que foi feito |
|---|---|---|
| 14.1 | "o shift atk que reforça coloca um brilho azul ao redor da espada" e "efeitos visuais do corte maiores" | Halo azul com faíscas em volta da lâmina enquanto houver golpes reforçados; o golpe reforçado deixa rastro azul. Rastros de corte maiores, em meia-lua (espada 52 px, terceiro golpe 66, faca 26). Só aparência: a área que acerta não mudou |
| 14.2 | "no minimapa podia ter indicadores visuais do que cada coisa" | Figuras próprias no minimapa e no mapa (fogueira, esteira, toca, javali, erva, moedas, marcador, pista, você). A legenda do mapa (M) desenha as mesmas figuras com nome. Ervas e moedas recolhidas somem do mapa |
| 14.3 | "andar sobre água ao apertar shift… consome Ki por segundo; ao zerar tem que nadar e fica lento" | **Só na Planície** (sua decisão). Shift sobre a água funda anda por cima a 10 Ki/s, com um anel de Ki sob os pés. Sem Shift, ou com o Ki zerado, você nada a 45% da velocidade, sem atacar, com o corpo meio submerso. Os animais continuam bloqueados pela água |

## 7. Pendências e conferência manual

- **Lista de conferência manual e decisões abertas:** no [README de tarefas](../tasks/README.md). Inclui a Lâmina enfraquecida contra o javali, os animais no mapa, os tempos da cena final do Sênior, F5 e F11 fora do editor, o arco da guarda, a assistência no seu monitor e a dificuldade do Sênior.
- **PainelDeTestes.exe:** estava livre, então foi **substituído pela versão nova**. Ele lista os testes do pedido 14 (Atributos e Ki, Recursos da Entrega 5, Elementos) e tem o botão **Inspeção (elementos)**.
- **/asset-spec:** ligado às fichas visuais futuras (T08 e T18) pelo README de tarefas.

## 8. Evidências (imagens e vídeo só do jogo)

| Pasta | O que tem |
|---|---|
| `production/qa/evidence/2026-11-27_recursos/` | antes e depois no mesmo ponto (pinheiro inteiro → toco), entalhes, queda, rocha com veio, rachaduras, cascalho, bambu cedendo, arbusto com o coelho, toco depois de Continuar, nas duas sementes; **vídeo** `recursos-video.gif` |
| `production/qa/evidence/2026-11-27_create-salgueiro/` | o salgueiro na Inspeção (4 estados) e no mundo, nas duas sementes |
| `production/qa/evidence/2026-11-27_create-rocha-ferro/` | a rocha enferrujada na Inspeção |
| `production/qa/evidence/2026-11-27_reforco-corte/` | corte normal maior, brilho azul, corte reforçado azul |
| `production/qa/evidence/2026-11-27_minimapa-icones/` | minimapa e mapa com figuras e legenda |
| `production/qa/evidence/2026-11-27_atributos-ki/` | ficha com FOR, barras com números, crítico, dano recebido, menu de assistência, Vale |

## 9. Testes

**Bateria completa com janela no monitor 2** (`tools/godot/testes-janela.ps1`, uma vez no fim): **46 execuções, 555/556 verificações**.
- **A única falha** foi uma contagem antiga do `AmbientCheck`: ele procurava a variedade de pinheiros só entre os `Obstacle`, e agora os pinheiros da Planície são elementos.
- **Corrigi o teste** (commit `c2ad9ac`) e repeti a execução: dia 19/19 nas duas sementes. **Total corrigido: 556/556.**
- **O código do jogo não mudou desde o início da bateria**; só os testes mudaram. Por isso o resultado das outras 45 execuções continua valendo.
- **A falha do TechCheck da primeira bateria** ("passo lateral enfraquecido: 86° em vez de ~55°") não se repetiu. Aquela bateria rodou com recompilações no meio, o que explica o resultado.

| Teste | Resultado |
|---|---|
| Unidade (`UnitRunner`) | 49/49. Novos: `DamageRulesTests`, `KiMigrationTests` (visto falhar sem a migração), `WaterRulesTests`, `ResourceRulesTests` (SOR em 400 lugares: 12%, 32%, 60%) |
| `AtributosCheck` | plains 16/16, vale 4/4, oldsave 8/8, mapa 2/2, visual 1/1 |
| `RecursosCheck` (Entrega 5) | work 29/29, resume 5/5 e bush 6/6 nas sementes 20261126 e 777; oldsave 3/3; video 1/1 |
| `ElementosCheck` | catalogo 39/39 (visto falhar com uma arte inexistente); mundo 12/12 nas duas sementes |
| `AmbientCheck` dia (com a água funda nova) | 19/19 nas duas sementes |
| As verificações anteriores (prólogo, recuperação, novidades, técnicas, tela, controles, Planície, jornada, ambientação) | todas OK na bateria |

**Saves:** os testes usam `user://teste_*.json`. Sua jornada e suas teclas não foram tocadas. A migração Ki foi conferida numa cópia real de jornada salva antes do renome (`tests/fixtures/saves/jornada_antes_do_ki.json`) e em preferências com a tecla do Qi num botão do mouse, como a sua.

## 10. Commits

Na branch `main`, em ordem:

| Commit | O quê |
|---|---|
| `d63ec58` | Qi → Ki (texto, código, dados, migração dos saves) |
| `d7b5be9` · `319a3ab` | FOR, CON, crítico, barras com números, números de dano · testes |
| `601da7d` | brilho azul do Reforço e corte maior (14.1) |
| `7bdba59` | ícones no minimapa e no mapa (14.2) |
| `c744c5c` | andar sobre a água e nadar (14.3) |
| `8e71ee4` · `cbfcacf` · `cad585f` | artes em `src/Elements` · Entrega 5 · testes |
| `c4e47b7` | correção das pedras listradas |
| `bb6bed3` | cena de inspeção |
| `fe5c988` · `5a792e6` | ElementosCheck · na bateria e no Painel |
| `ace82af` · `59d40c3` | salgueiro e rocha de ferro, criados pelo `/create` |
| `db2b352` | padrão de elementos, índice, regra no CLAUDE.md, skill `/create` |
| `9bb392d` | Painel: botão Inspeção e `.exe` novo |
| `4666e1c` · `7b47185` | pasta de tarefas · GUIA e contexto |
| `c2ad9ac` · `4342f32` | ajustes de teste (variedade, antes/depois, vídeo) |
| (este) | relatórios, índice e evidências |
