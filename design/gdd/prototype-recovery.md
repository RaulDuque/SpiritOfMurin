# GDD: Recuperação do protótipo (ponte, guarda, pistas, minimapa, livro, controles, assistência)

*Criado: 2026-11-26 · Status: Implementado (pedido 11)*
*Referência: `prototypes/spiritofmurin/index.html` e `production/qa/evidence/2026-11-25_meditacao/regressao-ciclo/`*
*Pilares: 1 "A técnica é a chave" (a guarda que só cai pelo flanco), 3 "O mundo se lembra" (pistas e mapa)*

## 1. Overview

Esta entrega devolve ao Godot o que o protótipo HTML tinha e o porte tinha perdido ou enfraquecido:
- a ponte desenhada;
- a leitura da Guarda Fechada;
- o mapa com missões e pistas;
- o Livro em duas colunas.

Ela também acrescenta quatro recursos pedidos:
- minimapa de canto;
- controles configuráveis;
- modo de assistência;
- rodapé legível.

## 2. Player Fantasy

"Eu reconheço a ponte, vejo a guarda fechar e sei por que meu corte não passou. Quando recuo, o mapa me lembra a última pista, e o livro me diz exatamente o que cada técnica custa hoje."

## 3. Detailed Rules

### Ponte e guarda
- **Ponte:** as tábuas são desenhadas pelo chão. A camada da água pulava as tábuas, mas ainda pintava água por cima delas; isso foi corrigido.
- **Arco da guarda (como no protótipo):**
  - quando o discípulo é o alvo travado, ou logo depois de uma aparada, um arco de ±`guard_half` (65°) aparece na frente dele;
  - o arco fica mais claro e mais grosso na aparada;
  - depois do Passo Lateral, enquanto ele está enganado (`dis_fooled`), o resto do corpo aparece como um arco tracejado em jade: o flanco aberto.
- **Aviso do golpe inimigo:** a cunha no chão usa o mesmo meio-ângulo e o mesmo alcance do teste de acerto (`strike_half` 40°, `dis_reach` + `strike_slack`). Antes, ela mostrava ±30° enquanto o golpe acertava em ±40°.
- **Alvo travado no alto:** nome, barra de vida e estado da guarda (guarda fechada, flanco aberto, recuperando, guarda quebrada, postura de ataque).

### Pistas e mapa (M)
- **Missões:** guardam onde foram recebidas (`vale.quest_at`). No mapa, ficam como um selo vermelho, com um traço que aponta a direção, nunca o lugar.
- **Pistas do Vale:** vêm dos eventos reais da campanha e ficam salvas em `vale.clues`:
  - recuo na ponte;
  - três aparadas;
  - manual;
  - laje;
  - inscrição;
  - Passo Lateral;
  - portão.
- **Pistas da Planície:** ficam salvas em `clues`:
  - despertar;
  - cada rastro examinado;
  - volta das técnicas.
- **Coluna do mapa:** responde "Onde estou?" (área, margem ou ponte, direção do olhar; na Planície, distância e direção até a clareira e o terreno sob os pés), "Última pista" e lista as missões.
- **Visual:** papel de pergaminho com rolos, cores de mapa do protótipo e o jogador como quadrado de tinta com centro jade piscando, mais um traço para onde olha.

### Minimapa
- **Onde aparece:** no canto superior direito, sob a semente, enquanto se explora, no Vale e na Planície.
- **Quando some:** atrás de painéis, do título e do escurecimento.
- **O que mostra:** os mesmos sinais do mapa, centrados no jogador.
- **Planície:** só os blocos explorados. A exploração fica salva com a jornada.
- **Tamanho:** 18% × 20% da tela, entre 104×66 e 170×110 pixels de jogo.

### Livro de Técnicas
- **Disposição:** em painel largo (≥ 420 px de jogo), lista à esquerda (técnicas, combinações, fragmentos) e página à direita. Em painel estreito, lista em cima e página embaixo, as duas com rolagem.
- **Página:**
  - nome, tipo e selo de estado (plena, selada, enfraquecida);
  - o comando como teclas, com os controles atuais;
  - como usar;
  - os números reais de `combat_techniques.json` (versão em uso e a outra, para comparar);
  - a origem;
  - o selo "feita por você" nas combinações;
  - a "Página rasgada" enquanto a combinação não foi descoberta.
- **Comandos:** clique ou passagem do mouse folheia, a roda rola a página, as teclas de andar folheiam, Livro ou Esc fecha.

### Controles (Esc → Controles)
- **Linha de cada ação:** nome em português, entrada principal e segunda (tecla ou botão do mouse), cada uma com "Alterar".
- **Troca:** "Alterar" espera a próxima tecla ou botão e aplica na hora. Esc cancela.
- **Entrada já usada:** as duas ações trocam de lugar e uma linha avisa. A troca é recusada se deixaria a outra ação sem entrada.
- **Botões:** "Restaurar padrões" e "Lista completa de comandos".
- **Salvamento:** em `preferencias.json` (`bindings`) a cada troca e ao fechar o jogo.
- **Textos que acompanham:** todo texto que nomeia uma tecla usa marcadores (`[cut]`, `[cut+]`, `[walk]`) e é reescrito na hora (rodapé, mapa, livro, avisos de combate, prompts).

### Modo de assistência (Esc → Modo de assistência → Mostrar áreas de combate)
- **Preferência:** fica salva em `assist_areas`.
- **O que desenha:** o que o código de combate testa, com os mesmos números:
  - **guarda:** setor de ±`guard_half`, contorno cheio claro;
  - **flanco:** o resto do círculo, tracejado em jade, mais forte enquanto o inimigo está enganado;
  - **aviso:** contorno tracejado da área do golpe, preenchendo do corpo para fora;
  - **golpe ativo:** área cheia e borda grossa;
  - **recuperação:** contorno fino e oco que encolhe;
  - **sua lâmina:** as mesmas três fases, com `cut_slack` e `thrust_slack`;
  - **javali:** corredor da investida (`charge_distance` × `charge_hit_radius`), anel de "desvia" no aviso e na investida, anel aberto na recuperação.
- **Desligada:** ficam as pistas normais (postura, arco breve da aparada, poeira do casco, reação ao acerto).

## 4. Formulas
- **Aparar:** o golpe é aparado quando `|ângulo(frente) − ângulo(de onde veio)| ≤ guard_half`, com o inimigo em guarda. É o mesmo teste que `WouldParry` usa para desenhar.
- **Golpe inimigo:** acerta quando `distância ≤ alcance + strike_slack` e `|ângulo − frente| ≤ strike_half` (ou `strike_half_big` no terceiro golpe do Sênior).
- **Sua lâmina:** acerta quando `distância ≤ alcance + slack + raio do alvo` e `|ângulo| ≤ hit_arc` (`InBladeArc`).

## 5. Edge Cases
- **Saves antigos sem `quest_at`, `clues` ou `bindings`:** o selo vai para o lugar padrão da missão, a lista de pistas começa vazia e os controles ficam nos padrões.
- **Pistas repetidas:** uma por id. Na Planície, um rastro examinado de novo vai para o fim com o texto novo.
- **Testes:** usam sempre um arquivo de preferências próprio. Uma cena de teste sem `--prefs` usa `teste_padrao_prefs.json` e nunca as preferências do jogador.
- **Jogo incorporado ao editor (F5):** F11 avisa que não funciona ali e explica a opção do editor.

## 6. Dependencies
- **Novos:** `src/Game/Bindings.cs`, `Assist.cs`, `ControlsPanel.cs`, `BookPanel.cs`.
- **Alterados:** `MapView.cs` (mapa e minimapa), `GamePanels.cs`, `PlainsHud.cs`, `ValeFighter.cs`, `Boar.cs`, `Player.cs` (`BladeArea`), `ValeGame.cs`, `PlainsGame.cs`, `ValeState.cs` (`Clue`, `QuestAt`), `SaveGame.cs` (`Clues`), `Prefs.cs`.

## 7. Tuning Knobs
- `vale_combat.json`: `strike_half`, `strike_half_big`, `strike_big_extra`, `strike_slack`, `strike_time`, `strike_live_at`, `sen_reach`, `cut_slack`, `thrust_slack`.
- Código: zoom do minimapa (Vale 0,085; Planície 0,06) e largura mínima do livro lado a lado (420 px de jogo).

## 8. Acceptance Criteria
1. Ao chegar à ponte, as tábuas aparecem sobre o rio.
2. Um corte de frente é aparado e o resultado bate com o arco desenhado; de fora do arco, o corte entra pelo flanco.
3. O recuo grava a pista "Uma trilha desce para o vale, a leste." e o mapa a mostra como última pista. Depois do manual, a missão e a última pista apontam para as pedras gêmeas. Tudo continua depois de meditar e Continuar.
4. O minimapa aparece ao explorar, some atrás dos painéis e fica sob a semente em 1280×720, 1600×900 e 1920×1080.
5. Trocar o corte para U por mouse funciona na hora, o rodapé diz U, os cliques que fecham os menus não cortam, U continua valendo ao reabrir o jogo e "Restaurar padrões" devolve J.
6. A assistência liga e desliga durante a partida e fica salva.
7. O livro mostra selada (com os números das duas versões) e enfraquecida no nível 3 (custo 45 Qi e 10 vigor, alcance 105 px), lado a lado em tela larga e empilhado em painel estreito.
