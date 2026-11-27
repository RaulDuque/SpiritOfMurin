# GDD: Tela cheia, imagem e mapa do mundo

*Criado: 2026-11-26 · Status: Implementado (campanha, entrega 3; F5 e minimapa no pedido 11 — `prototype-recovery.md`)*
*Pilar: 3 "O mundo se lembra, e você também" (o mapa guarda o que você explorou)*

## 1. Overview

O jogo preenche a janela ou a tela inteira com pixels inteiros e nítidos, sem bordas pretas. **F11** e o **menu de pausa** (Esc ou P) ligam e desligam a tela cheia, e a preferência fica salva. O **mapa (M)**, também no menu de pausa, mostra:
- no Vale, o que já foi visto;
- na Planície, os blocos explorados, gerados pela semente.

O mapa tem posição e direção do personagem, lugares conhecidos, zoom, legenda, recentralizar e marcadores manuais salvos com a jornada.

## 2. Player Fantasy

"Em tela cheia, o mundo é meu monitor inteiro." E o mapa é um caderno de viagem: só tem o que você percorreu, com as suas marcas.

## 3. Detailed Rules

### Imagem
- **Escala:** o maior número inteiro `k` com `janela ≥ 640·k × 360·k`.
- **Resolução do jogo:** `janela ÷ k`, arredondada para baixo. O jogo mostra mais mundo quando a janela é maior, nunca bordas: no máximo sobra menos de 1 pixel de jogo.
- **Exemplos:** 1280×720 → 2×, 640×360; 1600×900 → 2×, 800×450; 1920×1080 → 3×, 640×360; 2560×1440 → 4×, 640×360.
- **HUD:** acompanha o tamanho real. A faixa de atalhos fica no rodapé, a semente à direita e os painéis centralizados.
- **Mouse:** a linha de painel sob o cursor é a selecionada, e o clique age nela, em qualquer tamanho. O Godot converte o clique pela escala da janela.
- **Fechar um painel com o clique** não vira corte, passo nem uso no mundo: o corpo ignora a tecla ou o botão até soltá-lo.

### Tela cheia
- **F11** (em qualquer tela) ou o menu de pausa ("Tela cheia: ligada/desligada").
- A tela cheia é a do Windows sem borda, no monitor onde a janela está.
- A preferência `fullscreen` fica em `user://preferencias.json` e vale ao abrir o jogo. `--windowed` na linha de comando força janela (testes).

### Menu de pausa (Esc ou P)
Voltar ao jogo · Mapa (M) · Tela cheia (F11) · Controles (a lista completa) · Sair do jogo. Navegação por teclado (W/S, E/Enter, Esc) e por mouse.

### Mapa (M)
- **WASD:** move o mapa. **Roda do mouse ou + / −:** zoom. **C:** recentraliza no personagem. **X ou clique esquerdo:** põe ou tira um marcador. **M ou Esc:** fecha.
- **Vale:** os tiles já vistos (raio de 8×6 tiles ao redor do caminho percorrido), os nomes dos lugares vistos, a esteira de meditação e o ponto de missões em andamento.
- **Planície:** os blocos (16×16 tiles) em que o personagem entrou, coloridos pelo terreno da semente (relva, margem, água, vau, lamaçal, arbustos), mais a fogueira, a esteira, as tocas e os javalis vivos nesses blocos, e a "clareira".
- **Salvos com a jornada:** os blocos explorados e os marcadores (`explored`, `markers`; no Vale, `vale.seen` e `vale.markers`).

## 4. Formulas
`k = max(1, min(⌊L/640⌋, ⌊A/360⌋))`, `resolução = (⌊L/k⌋, ⌊A/k⌋)`, sobra = `(L mod k, A mod k)` < k pixels de tela.

## 5. Edge Cases
- Janela menor que 640×360: escala 1 e resolução = janela, com piso de 320×180.
- Janela maximizada com barra de tarefas (1600×877): 2×, 800×438, sobra de 1 px.
- Desligar a tela cheia volta à janela; se ela ficar pequena demais, volta a 1280×720.
- `--estica-antiga`: a regra antiga (640×360 fixo, bordas), só para comparação.
- **F5 no editor (Godot 4.7):** o jogo roda incorporado à aba Jogo. Em "Tamanho Fixo", a janela fica em 1280×720 e o editor desenha margens em volta dela quando a janela flutuante é maximizada. Incorporado, a tela cheia não existe (`Engine.IsEmbeddedInEditor`): o F11 avisa e o menu mostra "indisponível dentro do editor". Para tela cheia pelo F5, desmarque "Integrar Jogo na Próxima Execução" no menu ⋮ da aba Jogo (pedido 11; relatório `2026-11-26-recuperacao-prototipo.md`).
- **Diagnóstico:** a cada início e redimensionamento, o jogo escreve em `user://diagnostico_tela.txt` o modo (incorporado ou janela própria), a janela, o monitor, a área desenhada e as margens.

## 6. Dependencies
`src/Game/DisplayManager.cs` (autoload "Display"), `src/Game/GamePanels.cs`, `src/Game/MapView.cs`, `PlainsHud` (layout, linhas clicáveis), `PlainsGame` e `ValeGame` (`IMapSource`), `SaveData` (`Explored`, `Markers`), `Prefs`.

## 7. Tuning Knobs
Resolução base (`DisplayManager.Base` = 640×360, o mínimo de mundo visível), zoom inicial do mapa (Planície 0,12; Vale 0,15), tamanho do bloco revelado (o bloco do gerador).

## 8. Acceptance Criteria
1. Em tela cheia (1600×900, monitor 2) e na janela maximizada, a área do jogo é a janela inteira (sobra < 1 pixel de jogo), com escala inteira.
2. Em 1920×1080, escala 3×, mundo de 640×360.
3. F11 e o menu ligam e desligam a tela cheia, e a preferência é salva.
4. Em tela cheia, passar o mouse sobre uma linha a seleciona e o clique age nela; fechar o menu com o clique não corta.
5. M abre o mapa; X marca; explorar outro bloco o revela; meditar e carregar trazem os blocos e o marcador de volta. O mapa do Vale mostra só o que foi visto e guarda o marcador.
