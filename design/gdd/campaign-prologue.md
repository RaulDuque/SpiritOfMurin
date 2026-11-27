# GDD: Campanha — prólogo no Vale de Bambus e passagem para a Planície

*Criado: 2026-11-26 · Status: Implementado (entrega 1 da campanha)*
*Fontes: o protótipo HTML (`prototypes/spiritofmurin/index.html` e `config.js`), `user-docs/futuro-do game/` e o pedido em `user-docs/relatorios/prompts/10-campanha-inicial.md`*
*Pilares: 1 "Aprender é a recompensa", 3 "O mundo se lembra, e você também", 4 "Maestria nas mãos"*

## 1. Overview

O **Novo jogo** começa no prólogo, o Vale de Bambus do protótipo, portado para o Godot. O percurso é este:
1. a trilha e o discípulo da Guarda Fechada na ponte;
2. o manual partido num corpo no bambuzal;
3. o selo do túmulo e a estátua da mestra, onde as pegadas ensinam o passo lateral (e o passo longo);
4. a inscrição além da fenda e a Lâmina Silenciosa;
5. o portão e o acampamento;
6. o Espadachim Sênior.

Vencê-lo de verdade abre o fim do prólogo: o Sênior revive, fala, se explode e lança o espadachim para fora do quadro, por cima das montanhas. Vêm a escuridão e o silêncio. Depois, o despertar deitado na Planície Verdejante, de mãos vazias, com o Livro de Técnicas selado.

**Continuar** retoma onde a jornada foi gravada:
- no Vale;
- no despertar, se o jogo foi fechado durante o fim;
- na Planície (incluindo os saves de antes da campanha).

## 2. Player Fantasy

"Eu era forte, venci, e perdi tudo." O jogador sente o espadachim completo no Vale: a guarda que não cai de frente, o passo da mestra que a contorna, a combinação descoberta com as próprias mãos. A vitória sobre o Sênior é real e cobrada. A explosão a desfaz: luz, silêncio, coração. Acordar deitado na relva, sem espada, com o Livro lembrando o que ele sabia, prepara a Planície como recomeço.

## 3. Detailed Rules

### 3.1 O Vale (porte do protótipo)
- **Mapa:** 60×40 tiles de 32 px, tile a tile como no `buildMap` do protótipo.
  - Rocha, rio com ponte, fenda (chasm), trilha, túmulo com porta selada e parede cortada, pedras gêmeas, tendas, estela, bambus, pinheiros.
  - Pinheiro, pedra solta e bambu colidem só na base.
  - A fenda é parede para quem anda; o passo lateral e o passo longo atravessam.
- **Combate:** valores do `config.js` em `assets/data/vale_combat.json`. A vida e o dano estão em pontos (corte da espada = 2), o dobro do protótipo.
  - **Travar (Espaço):** o alvo vivo mais próximo a até 240 px, com colchetes ocre; o corpo fica de frente para ele.
  - **Corte (J ou clique esquerdo):** cadeia de 3; o 3º corte tem o dobro de dano.
  - **Estocada:** frente + J com o alvo travado.
  - **Disparo de Ki (K):** 30 de Ki; Ki de 100, que volta a 22/s.
  - **Passo curto (L):** sempre disponível.
  - **Passo lateral:** L + direção lateral ao alvo travado; contorna 95° e tem 0,14 s de invulnerabilidade ("passo"). Depois da estátua.
  - **Passo longo:** L + direção, sem trava; 112 px, atravessa a fenda. Depois da estátua.
  - **Passo da Lâmina Silenciosa:** passo lateral → corte → Ki dentro da janela. Quebra a guarda: dano 6 e 1,5 s de guarda quebrada. É descoberta fazendo.
- **Guarda Fechada:** apara tudo que vem a até ±65° da frente ("aparado", recuo de 34 px). Depois de um passo lateral, o inimigo perde o alvo por 0,38 s (o Sênior, 0,26 s); golpe no flanco = "flanco".
  - **Discípulo:** aviso de 0,5 s (cunha ocre no chão), alcance de 64 px, recuperação de 0,55 s.
  - **Sênior:** guarda fechada por 3,4 s, depois postura de ataque: avança, combo de 3 golpes com hiper-armadura e 0,8 s de abertura.
- **Lugares:**
  - corpo com o manual (página que brilha);
  - selo do túmulo (o manual o completa);
  - marca na pedra (acende as 3 pegadas): pisar nelas na ordem ensina o passo lateral, enche a Vida e o Ki e mostra a revelação por 3,2 s;
  - estátua (alvo de treino: apara de frente, "responde" no flanco);
  - inscrição além da fenda;
  - baú além da fenda;
  - esteira junto à pedra gêmea oeste: meditar grava a jornada.
- **Itens:**
  - bálsamo: +4 de Vida, H ou inventário;
  - pedra de Ki: enche o Ki;
  - erva espiritual, prata, faixa e pingente: sem uso.
  - Os potes quebram com o corte ou o Ki. As ervas e os itens que caem são pegos andando por cima.
- **Missões:** "A guarda fechada", "O manual dividido", "O Espadachim Sênior", "A lâmina que segue o passo".
- **Derrota:** com a Vida em 0, depois de 1,5 s, desperta no início da trilha, ou na entrada do acampamento depois que o portão abre. O que aprendeu fica. Os inimigos vivos voltam ao lugar, com a vida cheia.
- **Livro de Técnicas (B):** técnicas, combinações e fragmentos, com comando, origem e estado (Plena, Selada, Enfraquecida).

### 3.2 O fim do prólogo (`SeniorFinale`)

| Etapa | O que acontece | Duração |
|---|---|---|
| Vitória | Faixa "Vitória", som de vitória, o corpo no chão. **A vitória é gravada** (fase `senior_won`) | 2,0 s |
| Revive | Ele se levanta e muda de postura (ataque). A aura cresce (anéis ocre); poeira gira aos pés, folhas são lançadas para fora, tiras de tecido vermelho e dourado tremulam; ronco grave; a tela treme | 2,6 s |
| Fala | Ele encara o espadachim e diz "Você acha que pode me derrotar? Pois vou garantir que não." **E** avança | até 6 s |
| Carga | A energia é puxada para o corpo dele; a aura pisca; o tremor aumenta | 2,0 s |
| Explosão | Clarão, dois anéis de choque, estilhaços. A câmera fica parada no lugar da explosão, e o espadachim **é lançado girando para fora do quadro**: 560 px e subindo até 260 px de altura, por cima das montanhas. Por isso faz sentido ele cair na Planície (pedido do criador durante a sessão) | 2,0 s |
| Escuridão | Tela preta e **silêncio** (o volume geral vai a zero); duas batidas fracas de coração | 3,4 s |
| Despertar | A campanha troca de região: claridade pálida que se desfaz em ~4 s, os sons do vento e dos pássaros voltam, o espadachim **deitado** na relva. Mover-se (WASD) ou E o levanta (0,9 s) | — |

- **Pular:** depois de ver o fim uma vez (preferência `finale_seen` em `user://preferencias.json`), Esc ou Enter pulam direto para o despertar.

### 3.3 A passagem é persistente
- A **vitória** grava a jornada com a região `vale` e a fase `senior_won`. Fechar o jogo em qualquer ponto do fim e escolher Continuar leva ao **despertar**, o ponto seguro.
- O **despertar** aplica a perda uma única vez:
  - inventário vazio e mãos vazias;
  - o Livro copiado com todas as técnicas do prólogo marcadas como **Selada**;
  - `loss_applied = true`.
  - Em seguida, grava a jornada como **Planície**. Daí em diante, Continuar está na Planície.
- **Saves da Planície anteriores** (formato 1) continuam na Planície com os itens, os atributos, a experiência e as proficiências. Não recebem perda nem Livro.

### 3.4 Selo (o resto da regra está na entrega 2)
- Na Planície, as técnicas seladas não saem por nenhuma entrada:
  - K longe de carcaça avisa "Disparo de Ki está selada…" e não gasta Ki;
  - L lateral com um animal focado vira o passo comum, com o aviso.
- Andar, caçar, cortar com a faca equipada e o passo comum continuam.

### 3.5 Retorno enfraquecido no nível 3 (entrega 2)
- **Quando volta:** ao chegar ao nível `unlock_level` (3) da Planície, as técnicas seladas voltam como **Enfraquecida**.
  - Faixa "As técnicas do Vale voltam", uma mensagem curta e o Livro atualizado com comando, custo e efeito em vigor.
  - Acontece uma vez por jornada (`techniques_returned` vai no save).
  - Um save que já está no nível 3 ou acima, ainda selado, recebe o retorno uma vez ao carregar.
- **Versões enfraquecidas** (`combat_techniques.json`, campos `weak`):

| Técnica | O que muda |
|---|---|
| Disparo de Ki | 45 Ki + 10 vigor (a plena: 30 Ki), 105 px (a plena: 210), dano 1 (a plena: 2) |
| Passo Lateral | 15 Ki + 12 vigor (a plena: grátis), 55° em volta do alvo (a plena: 95°), raio até 70 px, 0,08 s de invulnerabilidade |
| Passo longo | 10 Ki + 14 vigor, 60 px (a plena: 112). Na Planície pede **Shift + direção + L**, para que L + direção continue sendo o passo comum |
| Estocada | 8 de vigor, alcance de 40 px, dano 1 |
| Passo da Lâmina Silenciosa | dano 3 (o pleno: 6), 126 px, 0,8 s de abertura |

- **Recusa:** sem Ki ou sem vigor suficiente, a técnica é recusada com a falta dita ("Disparo de Ki: falta Ki (10/45)") e nenhum dos dois recursos é gasto.
- **Selada:** a recusa diz que está selada e não se repete a cada toque (2,5 s).
- **K conforme o contexto:** perto de uma carcaça, K esfola com Ki; longe, é o disparo. A faixa de atalhos mostra qual ("K esfolar com Ki" ou "K disparo de Ki (selado)"). Um toque faz uma ação só.

## 4. Formulas

| Fórmula | Definição |
|---|---|
| Aparo | `|ângulo(frente do inimigo) − ângulo(inimigo → golpe)| ≤ guard_half` (65°), com a guarda erguida e sem Lâmina |
| Lâmina Silenciosa | `hist[-2] = passo lateral`, `hist[-1] ∈ {corte, estocada}`, `t(corte) − t(passo) ≤ time_passo + combo_window` e `agora − t(corte) ≤ combo_window + 0,1` |
| Volta do passo lateral | raio `clamp(dist(alvo), 46, 92)`; ângulo final `a0 − lado·95°`; posição `alvo + (cos, sin)(lerp(a0, a1, 1 − (1 − k)²))·raio` |
| Arremesso | `e = 1 − (1 − f)²`, `pos = início + direção·560·e`, `altura = 260·e²` (sempre subindo), `giro = f·2,5·2π`, com `f = t/1,4 s`; a câmera fica parada no lugar da explosão |
| Dano no prólogo | corte 2, 3º corte 4, estocada 2, Ki 2, Lâmina 6. Discípulo 12, Sênior 20, golpe inimigo 2 de Vida (Vida 10) |

## 5. Edge Cases

- **Fechar durante a vitória, a revivescência, a fala, a carga, a explosão ou a escuridão:** Continuar leva ao despertar (fase `senior_won`) e aplica a perda uma vez.
- **Fechar logo depois do despertar:** a jornada já é da Planície (`loss_applied`). Não há segunda perda, e as conquistas depois do despertar ficam ao meditar.
- **Morrer lutando com o Sênior:** a derrota normal, na entrada do acampamento. Ele volta com a vida cheia e se pode tentar de novo.
- **Passo lateral ou passo longo terminando sobre a fenda:** o corpo é empurrado para o chão firme mais próximo na direção do passo; se não houver, volta ao ponto de despertar.
- **Meditar com um inimigo em guarda por perto:** recusado.
- **Gravação danificada:** aviso no título e jogo novo.
- **Novo jogo com jornada:** pede confirmação ("Não, manter a jornada" vem marcado).
- **Pular o fim antes de vê-lo:** não é possível; a opção aparece só depois de ele ter sido visto uma vez.

## 6. Dependencies

- **Personagem (`src/Player`):** técnicas (`TechniqueSet`), Ki, trava, estocada, passo lateral, passo longo, recuo do aparo, deitado e arremesso.
- **Dados:** `assets/data/vale_combat.json` e `assets/data/combat_techniques.json` (plena e enfraquecida; a enfraquecida é usada na entrega 2).
- **Planície (`plains-first-stretch.md`, `plains-progression.md`):** recebe o despertar, o Livro selado e a perda. O Ki passa a morar no corpo do personagem. K tem uso conforme o contexto.
- **Salvamento:** formato 2 (`Phase`, `Vale`, `ValeInventory`, `Book`, `LossApplied`, `TechniquesReturned`); o formato 1 continua aceito.
- **Som:** `src/Audio/Sfx.cs` (procedural, sem arquivos).

## 7. Tuning Knobs

| Onde | O quê |
|---|---|
| `vale_combat.json` | Vida e Ki do espadachim, aparo, recuo, todos os números do discípulo e do Sênior, dano inimigo, congelamento no golpe, tremor |
| `combat_techniques.json` | custo, alcance, dano, duração e ângulo de cada técnica, plena e enfraquecida; nível de retorno |
| `SeniorFinale.cs` | durações das etapas (2,0 / 2,6 / 6 / 2,0 / 2,0 / 3,4 s), distância e altura do arremesso. Hoje no código: são tempos de encenação, não de balanceamento |

## 8. Acceptance Criteria

1. Novo jogo, pelo título, abre o Vale com a espada e o Livro inicial (Corte, Estocada, Disparo de Ki, Passo curto).
2. A Guarda Fechada apara de frente. O manual, o selo, a marca e as pegadas ensinam o passo lateral e o passo longo. A estátua responde ao flanco. O passo longo atravessa a fenda. A inscrição e a Lâmina Silenciosa entram no Livro.
3. O guardião cai e o portão abre. Meditar grava no Vale, e Continuar volta às pedras gêmeas com tudo.
4. O Sênior chega a 0 de vida pela luta. Vitória → revive com aura → fala (E avança) → carga → explosão que lança o espadachim girando para fora do quadro (mais de 400 px) → escuridão e silêncio.
5. Despertar na Planície: deitado, mãos vazias, sem itens, Livro com as técnicas do Vale "Selada". Mover-se o levanta. A jornada é gravada como Planície uma vez.
6. Fechar no meio do fim e Continuar leva ao despertar. Um save antigo da Planície continua na Planície com o que tinha. Visto uma vez, o fim pode ser pulado com Esc.
7. K na Planície, longe de carcaça, é recusado como selado, sem gastar Ki.
