---
name: subir-git
description: "Fecha uma entrega do SpiritOfMurin: compila, roda os testes, atualiza relatório e índice, faz commits atômicos, envia ao GitHub e confirma no remoto. Use ao terminar qualquer tarefa que mudou arquivos, ou quando o criador pedir 'sobe para o git'."
argument-hint: "[resumo da entrega]"
user-invocable: true
---

# Subir para o git (fechamento de entrega)

Aplica a regra permanente do `CLAUDE.md` ("Fluxo de trabalho: fechamento de cada
entrega") e a regra `.claude/rules/commits-atomicos.md`. **O commit e o push estão
autorizados pelo criador**: não peça confirmação para eles. Pergunte só quando um
arquivo pode ser de outra tarefa.

Se nenhum arquivo mudou (a sessão foi só conversa), responda que não há o que
subir. Nunca crie commit vazio.

## 1. Compilar e testar

Use o Godot .NET de `engine.path` no `project.yaml`, chamado aqui de `$GODOT`.
Rode, sem janela:

| Comando | Quando |
|---|---|
| `dotnet build` (0 erros) | sempre que houver C# |
| `$GODOT --headless --path . res://tests/unit/UnitRunner.tscn` | sempre que houver C# |
| `res://tests/integration/ControlsCheck.tscn` | sempre que houver C# |
| `res://tests/integration/PlainsCheck.tscn -- --seed=20261126` e `--seed=777` | se a mudança toca a Planície |
| `res://tests/integration/JourneyCheck.tscn -- --phase=new --seed=N`, depois `--phase=resume` | se a mudança toca progressão, inventário ou salvamento |

**Teste de interface:** rode também com janela no monitor 2, com
`--windowed --resolution 1280x720 --screen 1`, ou com
`tools/godot/testes-janela.ps1`.

**Antes de abrir janelas,** confira se há um Godot do criador aberto
(`tasklist`). As janelas roubam o foco do teclado. Se houver, pergunte se pode
rodar agora ou se deve esperar.

**Para cada teste, anote um destes resultados:** `PASSOU (n/n)`, `FALHOU (o quê)` ou
`NÃO RODOU (por quê)`.
- Um teste que não rodou nunca entra como passou.
- Se uma falha foi causada pela entrega, corrija e rode de novo.
- Se a falha já existia, não esconda: registre no relatório.

## 2. Relatório e índice

- Atualize ou crie `user-docs/relatorios/<data>-<tema>.md`, com:
  - o que ficou pronto;
  - os testes e seus resultados;
  - o que foi observado jogando × o que foi verificado com testes controlados;
  - as limitações.
- Guarde o pedido do criador em `user-docs/relatorios/prompts/NN-<tema>.md`, com o texto integral.
- Acrescente as linhas no índice `user-docs/relatorios/README.md`.

## 3. Revisar e separar em commits atômicos

1. Rode `git status` e `git diff`. Liste os arquivos mudados e diga de que mudança lógica cada um é.
2. **Arquivos de outras tarefas** (alterações locais que você não fez nesta
   entrega) ficam fora. Na dúvida, pergunte.
3. **Agrupe em commits atômicos:**
   - uma mudança lógica por commit;
   - dependências juntas;
   - gerados junto com a origem;
   - evidências junto com o relatório que as cita;
   - a ordem dos commits deve deixar o projeto compilando a cada passo. Quando
     não for óbvio, rode `dotnet build` com só os commits até ali.
4. Para cada grupo: `git add <arquivos>`, nunca `git add .` nem `-A`. Depois,
   `git diff --staged --stat` para conferir que só está o que a mensagem diz.
5. A mensagem segue `.claude/rules/commits-atomicos.md`:
   - formato `<tipo>(<escopo>): <descrição em português>`;
   - o corpo diz o que mudou e os testes, e cita `Story:` ou o documento de design;
   - a última linha é a coautoria pedida pelo sistema.

## 4. Enviar e confirmar

1. Envie com `git push origin <branch>` (hoje `main`).
2. Confirme com `git fetch origin` e `git rev-parse HEAD origin/<branch>`. Os dois hashes precisam ser iguais.
3. **Se o push falhar:** não force (nada de `--force`). Informe o motivo e o
   hash do commit local para retomar depois.

## 5. Resposta final ao criador (em PT-BR)

- **Branch:** e se o remoto confirmou.
- **Commits:** link de cada um (`https://github.com/RaulDuque/SpiritOfMurin/commit/<hash>`), com uma linha do que cada um contém.
- **Relatório:** link (`https://github.com/RaulDuque/SpiritOfMurin/blob/<branch>/user-docs/relatorios/<arquivo>`).
- **Testes:** cada um com PASSOU, FALHOU ou NÃO RODOU, e o número de verificações.
- **O que ficou de fora do commit, e por quê.**
