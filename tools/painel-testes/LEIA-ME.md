# Painel de Testes

O `PainelDeTestes.exe`, na raiz do projeto, abre uma janela com botões:

| Botão | O que faz |
|---|---|
| **▶ Jogar** | **Compila o código** e abre o jogo com a sua jornada normal |
| **Vitrine (personagem e mobs)** | Compila e abre a vitrine: o protagonista nas 8 direções, com faca, mãos vazias, ajoelhado e deitado; um boneco que você controla (andar, corte, passo, Qi); discípulo, Sênior e estátua em todas as poses e caídos; javali e coelho. Comandos: roda dá zoom, botão direito arrasta a vista, R volta o boneco, Esc sai |
| **Rodar todos os testes (com janela)** | Roda todos os testes automáticos; você assiste o jogo se jogar sozinho no monitor escolhido |
| **Rodar prólogo + recuperação (com janela)** | Só o Vale, a recuperação do protótipo e a unidade (mais curto) |
| **Rodar todos sem janela (rápido)** | Os mesmos testes, sem abrir a janela do jogo; pula os poucos que só funcionam com janela |
| **Parar** | Interrompe a rodada |
| **Abrir último relatório** | Abre o relatório mais recente no navegador |
| **Escolher o Godot…** | Só se o Godot não estiver em `C:\Users\<você>\Tools\Godot\4.7.2-stable-mono\…` |

**Modo ao vivo** (marcado por padrão): com o jogo ou a vitrine aberta pelo painel, cada vez que você salva um `.cs` em `src/`, o painel:
- recompila;
- se compilou, fecha e reabre a mesma tela com o código novo (alguns segundos);
- se deu erro, mostra o arquivo, a linha e a mensagem, e a tela aberta continua com a versão anterior.

O Godot C# não troca o código com o jogo rodando. Por isso o painel reabre em vez de atualizar na hora.

**Por que uma mudança "não aparece":** o jogo usa o código **compilado**. O F5 do editor compila sozinho; abrir o Godot direto, não. O painel agora compila sempre antes de abrir.

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
