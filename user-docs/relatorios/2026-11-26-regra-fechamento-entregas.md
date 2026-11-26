# Regra permanente: fechamento de cada entrega — 2026-11-26

## O que mudou

O `CLAUDE.md` da raiz ganhou, dentro de **Collaboration Protocol**, a seção **"Fluxo de trabalho: fechamento de cada entrega"**. É a regra permanente do criador, com o texto dele. Ao concluir cada entrega que altere arquivos do projeto:

1. compilar e rodar os testes aplicáveis, corrigindo as falhas causadas pela entrega;
2. atualizar o relatório em `user-docs/relatorios/` e o índice `README.md`;
3. revisar o `git diff` e o `git status`, incluir a tarefa, o relatório e as evidências, e preservar as alterações locais de outras tarefas;
4. fazer o commit e o push para `RaulDuque/SpiritOfMurin`, **sem pedir confirmação de novo**;
5. confirmar que o commit chegou à branch remota e informar a branch, o link do commit, o link do relatório e os testes. Se o push falhar, informar o motivo e o hash local.

A regra vale também para documentação e configuração. Conversa sem mudança de arquivo não gera commit.

A linha "No commits without user instruction" do mesmo protocolo agora aponta essa exceção, para as duas regras não se contradizerem.

## Verificação

- **Compilação** (`dotnet build`): 0 avisos, 0 erros. Esta entrega só muda documentação, então nenhum teste de jogo se aplica. O último resultado dos testes está em [2026-11-26-planicie-primeiro-trecho.md](2026-11-26-planicie-primeiro-trecho.md).
- **Commit:** a regra foi aplicada a esta própria entrega, com commit e push deste relatório, do índice e do `CLAUDE.md`.
