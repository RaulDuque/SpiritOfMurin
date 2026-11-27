# T12 — Primeira dungeon de tesouro

**Estado:** planejada · **Frente:** D · **Depende de:** T11 · **Origem:** pedido 14 item 6.D (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`); regras comuns em `user-docs/tasks/T10-escavacao-cavernas-dungeons.md`; Pilares 2 e 3 em `design/gdd/game-concept.md`

## Objetivo do jogador

Abrir a passagem fechada da primeira caverna (T11) usando a pista que viu, descer para uma dungeon antiga, atravessar salas com armadilhas que se deixam ler, escolher entre ir mais fundo ou voltar, abrir um atalho de retorno e sair com um tesouro: materiais, moedas e um fragmento de técnica que pede para ser experimentado. Voltar depois e encontrar baús abertos, armadilhas desarmadas e o atalho ainda aberto.

## O que já existe hoje

- **T11:** caverna com a passagem fechada e a pista; modelo de save de cavernas/dungeons (T10).
- **Livro de Técnicas** com descobertas registradas e técnicas secretas que se descobrem experimentando (pedido 13, relatórios `2026-11-26-disparo-concentrado.md` e `2026-11-26-reforco-qi.md` — têm spoiler no fim; não copiar).
- **Moedas** (`src/Items/Wallet.cs`: receber, gastar com recusa).
- Linguagem de aviso → ativo → abertura (T07; `user-docs/relatorios/2026-11-26-visual-mobs.md` §4).

## O que falta

1. **Abrir a passagem** com base na pista da T11 (conhecimento, não item-chave: Pilar 3).
2. **5–7 salas** com papéis distintos:
   - antessala (ensina a primeira armadilha em lugar seguro);
   - sala de armadilhas de piso (placas que afundam e avisam antes de disparar);
   - sala de lâminas/dardos com ritmo legível (aviso → ativo → pausa);
   - sala do guarda (um inimigo humano antigo ou estátua animada; decisão aberta);
   - sala de escolha (ir mais fundo × voltar);
   - sala do tesouro;
   - atalho de retorno que só abre por dentro, ligando o tesouro à entrada.
3. **Armadilhas legíveis:** todo dano é precedido de um sinal visível e sonoro com tempo em dados; com o Modo de assistência, a área de efeito aparece como as áreas de ataque.
4. **Recompensas** em dados: materiais (minério, cristal), Moedas, ouro (conforme a decisão da T10) e **um fragmento de técnica** registrado no Livro como pista; a técnica só é reconhecida quando o jogador a executar (Pilar 2: pistas, nunca receitas).
5. **Memória:** salas visitadas, armadilhas desarmadas/disparadas, baús abertos, atalho aberto, guarda derrotado; cada recompensa sai uma única vez.
6. **Rotas de retorno:** sair pela entrada, pelo atalho ou desmaiar (regra da T10) sempre devolve à superfície no ponto certo.
7. **Mapa da dungeon** revelado por sala.
8. Organização conforme `docs/elementos/PADRAO.md` (sala, armadilha, baú, guarda, fragmento).

## Dependências

- **T11** (caverna e passagem), **T10** (regras e save), **T07** (combate).
- **T13** opcional: se o fragmento for ligado ao cultivo.

## Decisões abertas (do criador)

1. **Qual técnica (ou fragmento) a primeira dungeon guarda?** A) um fragmento de técnica nova de espada; B) um fragmento de movimento (Qinggong básico, citado em `gpt.md`) — recomendado, abre caminho para a furtividade; C) uma pista ligada à missão de avanço de reino (T13). *Como é segredo de jogo, a solução fica em arquivo `*-SOLUCAO.md`, nunca no relatório comum.*
2. **Guarda da dungeon:** A) estátua animada (eco da Estátua da Mestra); B) espadachim antigo (humano, eco do Sênior); C) criatura grande de caverna. Recomendado: A.
3. **Armadilhas podem matar?** A) causam dano normal e podem levar ao desmaio (recomendado); B) dano limitado que nunca leva a 0.
4. **Ouro e valor do tesouro:** quanto de Moedas/ouro (proposta: suficiente para 1–2 compras na primeira vila, T15)?

## Critérios de aceite (jogáveis e testáveis)

1. A passagem da T11 abre ao fazer o que a pista indica e continua aberta depois de salvar e continuar.
2. Cada armadilha mostra o sinal antes do dano, pelo tempo em dados (medido); passar no ritmo certo não toma dano.
3. Com o Modo de assistência, a área de cada armadilha coincide com a área de dano usada no teste.
4. O guarda tem aviso, ativo e abertura; derrotado, permanece derrotado.
5. A sala do tesouro dá os materiais, as Moedas e o fragmento uma única vez; o saldo de Moedas sobe na hora; o fragmento aparece no Livro como pista, não como técnica pronta.
6. O atalho abre só por dentro e, aberto, leva do tesouro à entrada; continua aberto depois de recarregar.
7. Sair por qualquer rota (entrada, atalho, desmaio) devolve ao lugar certo, sem duplicar recompensas.
8. O mapa da dungeon revela só as salas visitadas e persiste.
9. Jornada antiga (fixture) carrega com a dungeon intocada.

## Roteiro de teste

1. Unidade: temporização das armadilhas; tabela do tesouro; registro único do fragmento; serialização das salas.
2. Integração `DungeonCheck` (fases `abrir`, `armadilhas`, `guarda`, `tesouro`, `atalho`, `save`).
3. Evidências do viewport: cada armadilha no aviso e no ativo, guarda avisando, baú aberto, página do Livro com a pista, atalho aberto.
4. Jogar pelo Painel → Jogar da clareira ao tesouro e de volta pelo atalho.

## Prompt pronto para executar

```text
Tarefa T12 do SpiritOfMurin — Primeira dungeon de tesouro.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md, user-docs/tasks/T10-escavacao-cavernas-dungeons.md, user-docs/tasks/T11-primeira-caverna.md e user-docs/tasks/T12-primeira-dungeon.md. Leia o relatório da T11 (a passagem fechada e a pista). Confirme no README de tasks que a T11 está pronta. Não leia arquivos *-SOLUCAO.md nem seções de spoiler.

Quero a primeira dungeon: abrir a passagem da caverna pela pista, 5 a 7 salas com papéis distintos, armadilhas sempre com aviso antes do dano, um guarda com aviso e abertura, sala de escolha, tesouro com materiais, Moedas e um fragmento de técnica registrado no Livro como pista, atalho de retorno que abre por dentro, mapa por sala e memória completa no save.

Antes de implementar, pergunte-me as "Decisões abertas" da T12, uma por vez, com opções e recomendação. Mostre salas, armadilhas e guarda numa prancha HTML em user-docs/relatorios/ antes de pedir aprovação. A solução do segredo da técnica vai num arquivo *-SOLUCAO.md separado, nunca no relatório comum nem no prompt.

Regras:
- Organize salas, armadilhas, baús, guarda e fragmento conforme docs/elementos/PADRAO.md.
- Tempos de armadilha, dano, recompensas e Moedas em assets/data/. Use src/Items/Wallet.cs para as Moedas.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_primeira-dungeon/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a T12 em user-docs/tasks/README.md com links.
```
