# Painel de Testes

O `PainelDeTestes.exe`, na raiz do projeto, abre uma janela com botões:

| Botão | O que faz |
|---|---|
| **▶ Jogar** | Abre o jogo com a sua jornada normal |
| **Rodar todos os testes (com janela)** | Roda todos os testes automáticos; você assiste o jogo se jogar sozinho no monitor escolhido |
| **Rodar prólogo + recuperação (com janela)** | Só o Vale, a recuperação do protótipo e a unidade (mais curto) |
| **Rodar todos sem janela (rápido)** | Os mesmos testes, sem abrir a janela do jogo; pula os poucos que só funcionam com janela |
| **Parar** | Interrompe a rodada |
| **Abrir último relatório** | Abre o relatório mais recente no navegador |
| **Escolher o Godot…** | Só se o Godot não estiver em `C:\Users\<você>\Tools\Godot\4.7.2-stable-mono\…` |

**Opções:**
- monitor onde a janela abre;
- semente (20261126, 777 ou as duas);
- "Lento" (para acompanhar com calma).

**Relatório:** no fim de cada rodada, ele é escrito em `production/qa/execucoes/<data>/relatorio.html` (e `relatorio.md`) e abre no navegador. O relatório traz:
- cada execução com o resultado (OK ou FALHA e quantas verificações passaram), o que ela verifica e o tempo;
- os detalhes de cada verificação e as medidas;
- um log por execução.

**Seus dados:** os testes usam saves e preferências próprios (`user://teste_*.json`). A sua jornada e as suas teclas não são tocadas.

**Linha de comando:** `PainelDeTestes.exe --auto [--sem-janela] [--rapido]` roda sozinho, escreve o relatório e fecha.

**Gerar o .exe de novo** (depois de mudar o código em `tools/painel-testes/`):

    dotnet publish tools/painel-testes -c Release -o tools/painel-testes/publicado
    copy tools\painel-testes\publicado\PainelDeTestes.exe PainelDeTestes.exe
