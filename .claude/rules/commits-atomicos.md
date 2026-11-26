# Commits atômicos

Regra do criador (2026-11-26). Vale para todo commit no SpiritOfMurin. O passo a
passo de cada entrega está em `/subir-git` (`.claude/skills/subir-git/SKILL.md`).

Cada commit deve conter **uma única mudança lógica**, completa e coerente por si só.

- **Uma mudança por commit:** uma correção, uma funcionalidade ou um refactor.
  Não misture um `fix` com um `refactor` ou com ajustes de estilo sem relação.
- **Mudanças independentes vão em commits distintos.** Use `git add <arquivo>`
  (ou `git add -p`) para selecionar só o que pertence a cada commit, nunca
  `git add .` nem `git add -A`.
- **Cada commit deixa o projeto funcionando.** `dotnet build` precisa passar com
  só aquele commit aplicado, e as cenas e os testes que ele toca precisam abrir.
  Nada de código pela metade.
- **O que depende entre si fica junto.** Exemplo: os dados em `assets/data/`, a
  classe C# que os lê e a cena `.tscn` que usa essa classe, todos da mesma
  funcionalidade, vão no mesmo commit.
- **O que é gerado vai com o que o gerou.**
  - O `project.godot` reescrito por `tools/godot/setup_input.gd` vai no commit
    que muda o script.
  - Os `.uid` e `.import` que o Godot cria vão no commit do arquivo de origem.
  - As evidências de QA (`production/qa/evidence/<data>_<tema>/`) vão com o
    relatório que as cita.
- **Antes de cada commit, revise `git diff --staged`** e confirme que só há o
  que a mensagem descreve.
- **Mensagem no padrão do projeto** (Conventional Commits, `coding-standards.md`):
  - formato `<tipo>(<escopo>): <descrição em português>`;
  - tipos: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`;
  - escopos já usados: `plains`, `player`, `engine`, `prototype`, `claude`;
  - a mensagem descreve exatamente aquela mudança;
  - o corpo cita o documento de design ou o pedido (`Story: …`) e termina com a
    linha de coautoria;
  - evite mensagens genéricas como `teste` ou `ajustes`.
- **Alterações locais de outras tarefas ficam fora do commit.** Se não estiver
  claro de que tarefa um arquivo é, pergunte em vez de incluir.
