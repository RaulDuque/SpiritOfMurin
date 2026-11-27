# Contexto para assistentes (Claude em outra conta, Cursor e outros)

Este arquivo existe para que **qualquer assistente** continue o SpiritOfMurin sem
perder as decisões. Algumas delas estavam só na memória local do Claude Code
(`C:\Users\Raul\.claude\projects\...\memory\`), e outras ferramentas não leem essa pasta.
Atualizado em 2026-11-27.

> **Se este arquivo contradizer o código, o `CLAUDE.md` ou um documento de design,
> valem os outros.** Os documentos de design ficam em `design/gdd/` (o principal é
> `design/gdd/game-concept.md`).

## Por onde começar cada sessão

1. Leia o `CLAUDE.md` da raiz (protocolo, stack, padrões) e `.claude/rules/commits-atomicos.md`.
2. Leia **`user-docs/tasks/README.md`** (ponto de retomada: pronto, parcial, próximas entregas, decisões abertas, próximo prompt) e `production/session-state/active.md`, se existir. É o ponto de retomada da
   sessão anterior. Ele não vai para o git e pode não existir.
3. Se ele não existir, veja as últimas linhas de `user-docs/relatorios/README.md` e o
   relatório mais recente. Eles dizem o que foi entregue por último e o que ficou pendente.
4. Confira o `git log --oneline -10` e o `git status`.

## Quem é o criador e como trabalhar com ele

- Fale em **português do Brasil**, com **uma decisão por vez**. O ciclo é: pergunta → opções → decisão → rascunho → aprovação.
- Ele é programador backend (PHP, C, C#, C++). Este é o primeiro jogo dele, um projeto hobby sem prazo.
- **Para escolhas visuais (cor, tom, pose), mostre antes de perguntar.** Ponha todas as opções numa prancha HTML em `user-docs/relatorios/`, com cor, escala de cinza e simulação de daltonismo. Nunca peça para ele escolher um código hex que não pode ver.
- Salve todo relatório de sessão e todo pedido feito a agentes (com a resposta) em `user-docs/relatorios/`, e mantenha o `README.md` dessa pasta em dia.
- A solução do túmulo e as técnicas secretas ficam em seção de spoiler ou em arquivo `*-SOLUCAO.md`, nunca nas instruções iniciais.

## Fechamento de cada entrega (regra permanente)

O texto completo está no `CLAUDE.md`, em "Fluxo de trabalho: fechamento de cada entrega". Em resumo:
1. Compile (`dotnet build`) e rode os testes aplicáveis. Corrija o que a entrega quebrou.
2. Atualize o relatório em `user-docs/relatorios/` e o índice `README.md`.
3. Revise o `git diff` e o `git status`. Faça commits atômicos, com `git add` por arquivo (nunca `git add .`), e deixe de fora alterações locais de outras tarefas.
4. Faça o commit (Conventional Commits em português, por exemplo `feat(plains): ...`) e o push para `origin` (`github.com/RaulDuque/SpiritOfMurin`). **O criador já autorizou commit e push no fim de cada entrega.**
   - Nunca faça push para o remoto `upstream` (repositório do framework).
5. Confirme que o commit chegou ao remoto. Responda com a branch, o link do commit, o link do relatório e o resultado dos testes.

## Direção do jogo

- RPG de ação 2D em mundo aberto, tema Murim, câmera top-down 3/4, PC (teclado WASD + mouse).
- Conceito: "Mil Caminhos do Dao". Os pilares estão em `design/gdd/game-concept.md`.
- O mundo é gerado por semente aos poucos (Planície Verdejante, em blocos). Cidades, túmulos e seitas são feitos à mão e encaixados nele.
- O conteúdo atual (Vale, discípulo, túmulo do mestre) é a base do **prólogo**: o jogador se sente forte, perde para um chefe e acorda sem nada na Planície.
- Trabalhe em passos pequenos e testados. Pergunte antes de despejar muito conteúdo de uma vez.
- **Visual:** a aparência procedural do protótipo fica. O criador jogou o experimento com sprites e o rejeitou (art bible, seção 8.12). Não há produção de sprites prevista.

## Elementos do jogo (padrão do pedido 14)

- Todo elemento tem id estável e lugares previsíveis: desenho numa classe `<Nome>Art` (`src/Elements/`), dados num JSON (`assets/data/elements/`), comportamento na classe da família, prévia na cena `src/Dev/Inspecao.tscn`. Detalhes em `docs/elementos/PADRAO.md` e `docs/elementos/INDICE.md`.
- Para criar: `/create <tipo> "<descrição>"` (`.claude/skills/create/SKILL.md`). Fora do Claude Code, siga os passos da skill à mão.
- A energia se chama **Ki** (antes "Qi"). A ficha tem **FOR** (decisão do pedido 14).

## Tecnologia

- **Godot 4.7.2 .NET** em `C:/Users/Raul/Tools/Godot/4.7.2-stable-mono/Godot_v4.7.2-stable_mono_win64/`. A pasta `4.7.2-stable` é a edição sem .NET: não use.
- `project.godot` na raiz. O `SpiritOfMurin.csproj` mira `net10.0` (build offline, com a fonte NuGet local `GodotSharp-4.7.2-offline`). Renderer: Compatibility.
- Código em `src/`, dados de ajuste em `assets/data/` (valores de jogo nunca fixos no código). O mapa de entradas é gerado por `tools/godot/setup_input.gd`.
- **Seu conhecimento de Godot pode estar desatualizado.** Confira `docs/engine-reference/godot/` antes de usar APIs.

## Testes

- Unitários: `tests/unit/UnitRunner.tscn` (runner próprio com atributo `[UnitTest]`). O gdUnit4 **não** está instalado.
- Integração: as cenas `*Check.tscn` em `tests/integration/`, com fases passadas por `-- --phase=...`.
- Bateria completa com janela: `.\tools\godot\testes-janela.ps1` (padrão: monitor 2, isto é, `--screen 1`). Também roda pelo Painel em `tools/painel-testes/`. A última rodada completa deu 556/556 (2026-11-27, pedido 14).
- Entrada simulada precisa de um `await ProcessFrame` depois de cada `ParseInputEvent`, senão os toques se perdem.
- Os testes usam saves próprios (`user://teste_*.json`). Não toque na jornada nem nas teclas do criador.
- **Nunca capture a área de trabalho do Windows** (`DisplayServer.ScreenGetImage`, `CopyFromScreen`). Isso já pegou o navegador e o Discord do criador. Use a imagem do viewport do jogo (`GetViewport().GetTexture().GetImage()`). Se precisar do monitor inteiro, reconstrua com `tools/capture/monitor_compose.py` e marque a imagem como reconstruída. Confira toda imagem antes do commit.
- Mudança visível ao jogador precisa de evidência (captura) em `production/qa/evidence/<data>_<tema>/`.

## Protótipo antigo (navegador)

`prototypes/spiritofmurin/index.html`. Os testes por Chrome/Edge via CDP ficam em `tools/playtest/` (`ciclo-completo.mjs`, `meditar-continuar.mjs`). O save usa `localStorage`, na chave `spiritofmurin.jornada`.
