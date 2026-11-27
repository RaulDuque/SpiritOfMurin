# T13 — Nível 10, reinos de cultivo e missão de avanço

**Estado:** planejada · **Frente:** E · **Depende de:** T06 (ficha) · **Origem:** pedido 14 item 6.E (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`), `design/gdd/game-concept.md` (Core Mechanics 5 "Progressão dupla", Long-Term Progression, Pilar 1), `user-docs/futuro-do game/gpt.md` (quatro informações na ficha)

## Objetivo do jogador

Ao chegar ao nível 10, sentir que o corpo atingiu o limite do reino atual: continuar caminhando, lutando, caçando e coletando, mas entender que o nível 11 só vem depois de uma **missão de avanço** (ruptura). Ver na ficha em que reino está, que o limite foi alcançado e onde procurar a pista da missão. Ao concluir a missão, romper para o próximo reino e ganhar **algo nas mãos**, não só números.

## O que já existe hoje

- **Nível e XP** sem teto: `xp_base · xp_growth^(L−1)` (20, 30, 45, 68…), 2 pontos por nível (`design/gdd/plains-progression.md` §3.1; dados em `assets/data/plains/plains_progression.json`; código em `src/Progression/`).
- **Ficha (C)** com nível, XP, 7 atributos (FOR, VIT, INT, CON, AGI, SOR, KI) e proficiências (Esfolar, Preparo de Caça, Erboristeria, Rastreamento) com graus 0–3 (`plains-progression.md` §3.2 e §3.4).
- **Níveis de criatura** separados (coelho 1–2, javali 3–5), fixos pela semente e pelo id (`plains-progression.md` §3.6).
- **Retorno enfraquecido das técnicas no nível 3**, uma vez por jornada, com `techniques_returned` no save (`design/gdd/campaign-prologue.md` §3.5).
- **Não existe:** reino de cultivo, teto de nível, missão de avanço.
- **Regra de design já registrada** (`game-concept.md`, Pilar 1 e Core Mechanics 5): um avanço de reino **precisa desbloquear algo nas mãos** (um cancelamento, um passo, um elemento de Ki); atributos são bônus, nunca o prêmio principal. A visão do `gpt.md` acrescenta: o reino avança por compreensão e meditação, e "o nível 10 da Planície, por si só, não concede iluminação".

## O que falta

1. **Quatro conceitos separados no modelo, no save e na ficha:**
   - **nível do personagem** (XP, corpo);
   - **reino de cultivo** (avanço conquistado);
   - **proficiência de habilidade** (graus por uso);
   - **nível de criatura** (do animal, revelado ao focar).
2. **Teto do reino:** cada reino define o nível máximo (primeiro reino: 10), em dados.
3. **No nível 10:** aviso único ("Seu corpo chegou ao limite deste reino"); a barra de XP mostra o estado travado; o jogo continua normal (andar, lutar, caçar, coletar, fabricar).
4. **XP durante a trava:** tratada conforme a decisão do criador (abaixo), sempre visível na ficha.
5. **Pista da missão:** a ficha mostra o reino, "Limite: nível 10" e **onde encontrar a pista** (um lugar ou pessoa, sem marcador que aponte o caminho: anti-pilar "marcadores que apontam para tudo").
6. **Missão de avanço (ruptura):** definida pelo criador; conclusão registrada uma única vez; libera o nível 11 e aplica o prêmio "nas mãos".
7. **Liberação única:** concluir a missão de novo (ou recarregar) não dá o prêmio duas vezes; um save que já está acima do teto (se existir) é migrado sem perder nada.
8. **Preservar o retorno enfraquecido das técnicas no nível 3** (não mexer na regra nem no save).
9. Organizar reino, missão e textos da ficha conforme `docs/elementos/PADRAO.md`; nomes, tetos e textos em dados.

## Dependências

- **T06:** ficha com espaço para reino, limite e pista.
- **T14/T15** se a pista ou a missão envolverem um NPC ou um lugar habitado.
- **T10–T12** se a missão envolver caverna ou dungeon.

## Decisões abertas (do criador)

1. **Nomes dos primeiros reinos.** Opções:
   - A) nomes clássicos do gênero: "Refinamento do Corpo" → "Condensação de Ki" → "Fundação";
   - B) nomes próprios do jogo ligados ao Dao da Espada: "Lâmina Bruta" → "Lâmina Desperta" → "Lâmina Silenciosa";
   - C) nomes da natureza da Planície: "Raiz" → "Caule" → "Flor".
   Recomendação: A para o primeiro par, porque o jogador do gênero entende na hora; o criador decide.
2. **A missão de ruptura do primeiro reino.** Opções:
   - A) **meditação de compreensão:** juntar três "compreensões" no mundo (observar o javali, a rajada, a água) e meditar na esteira — recomendado, coerente com "avança por compreensão e meditação" (`gpt.md`);
   - B) **prova de combate:** vencer um rival ou elite (por exemplo o Alfa da alcateia, T08) usando uma técnica;
   - C) **busca:** encontrar e consumir um ingrediente raro (Flor de Ki do Bosque do Orvalho) e resistir à ruptura numa luta interna.
3. **O que acontece com a XP recebida durante a trava.** Opções:
   - A) **acumula** e é aplicada depois da ruptura, até um limite (por exemplo, o suficiente para 1 nível) — recomendado: o tempo jogado não se perde, mas não pula vários níveis;
   - B) é **descartada** (a barra fica cheia);
   - C) é **convertida** em outra coisa (prática de proficiência ou Ki excedente).
4. **O prêmio "nas mãos" do primeiro avanço:** A) uma técnica volta da forma enfraquecida para a plena; B) um novo cancelamento ou passo; C) um elemento de Ki. Recomendação: A, porque amarra o selo do prólogo à progressão.
5. **Onde fica a pista:** A) inscrição numa pedra perto da esteira; B) um NPC (eremita) na primeira vila (T15); C) um fragmento numa dungeon (T12).

## Critérios de aceite (jogáveis e testáveis)

1. **9 → 10:** com XP suficiente no nível 9, o personagem sobe para 10, ganha os pontos normais, e aparece uma única vez o aviso de limite.
2. **XP travada:** no nível 10 sem missão concluída, caçar um javali não sobe para 11; a XP segue a regra decidida (acumulada até o limite, descartada ou convertida) e a ficha mostra o estado.
3. A ficha mostra separadamente: nível, reino ("Reino: <nome> · Limite: nível 10"), proficiências e, ao focar um animal, o nível dele.
4. A ficha mostra onde procurar a pista; nenhum marcador no mapa aponta para a missão.
5. **Conclusão da missão:** ao cumprir a condição decidida, o reino muda, o teto sobe (próximo teto em dados), o prêmio nas mãos aparece no Livro/ficha e a XP acumulada (se for o caso) é aplicada.
6. **Liberação única:** repetir a condição, recarregar ou meditar não aplica o prêmio nem a XP de novo.
7. **Save e retomada:** salvar no nível 10 travado, fechar e continuar mantém a trava e a XP; salvar depois da ruptura mantém o novo reino.
8. O retorno enfraquecido no nível 3 continua acontecendo uma vez, igual a hoje.
9. Jornada antiga (fixture `tests/fixtures/saves/jornada_antes_do_ki.json`) carrega no primeiro reino sem perder nível, XP ou pontos.

## Roteiro de teste

1. Unidade: teto por reino; XP travada (cada opção da decisão como caso); liberação única; migração de save sem reino.
2. Integração `ReinoCheck` com saves de teste preparados em nível 9 e 10 (fases `9para10`, `travado`, `missao`, `unico`, `save`, `nivel3`).
3. Evidências do viewport: aviso de limite, ficha com reino/limite/pista, ficha depois da ruptura, Livro com o prêmio.

## Prompt pronto para executar

```text
Tarefa T13 do SpiritOfMurin — Nível 10, reinos de cultivo e missão de avanço.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T13-reinos-cultivo-nivel10.md. Leia design/gdd/game-concept.md (Pilar 1, Core Mechanics 5 e Long-Term Progression), design/gdd/plains-progression.md §3.1–3.4 e design/gdd/campaign-prologue.md §3.5. Confirme no README de tasks que a T06 (ficha) está pronta.

Implemente: nível do personagem, reino de cultivo, proficiência e nível de criatura como conceitos separados; teto do primeiro reino no nível 10; aviso único ao chegar ao limite; o jogo continua normal; XP durante a trava conforme a minha decisão; a ficha mostrando reino, limite e onde está a pista; a missão de avanço; a liberação única do nível 11 e do prêmio nas mãos. Preserve o retorno enfraquecido das técnicas no nível 3.

Antes de implementar, pergunte-me, uma por vez e com opções e recomendação: os nomes dos primeiros reinos, a missão de ruptura, o destino da XP durante a trava, o prêmio nas mãos e onde fica a pista. Registre as decisões num GDD em design/gdd/ (fonte única). Se a missão for um segredo, a solução vai num arquivo *-SOLUCAO.md separado.

Regras:
- Organize reino, missão e textos conforme docs/elementos/PADRAO.md. Nomes, tetos, regra da XP e textos em assets/data/.
- Testes com saves isolados (user://teste_*.json) preparados nos níveis 9 e 10, sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2. Cubra: 9→10, XP travada, conclusão da missão, liberação única, save e retomada. Inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_reinos/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a T13 em user-docs/tasks/README.md com links.
```
