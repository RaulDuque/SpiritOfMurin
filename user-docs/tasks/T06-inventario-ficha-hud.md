# T06 — Inventário, ficha, HUD e escolha de interação (Entrega 6 da campanha)

**Estado:** planejada · **Frente:** A · **Depende de:** T05 · **Origem:** pedido 10 (Entrega 6, `user-docs/relatorios/prompts/10-campanha-inicial.md`), pedido 13 item 10 (`prompts/13-mira-tecnicas-bolsa-moedas.md`), pedido 14 item 6.A e decisões de abertura (`prompts/14-organizacao-criacao-conteudo.md`)

## Objetivo do jogador

Abrir a bolsa e responder em poucos segundos: "O que equipei? O que posso usar? O que consigo fabricar com o que tenho?". Subir de nível e gastar pontos com o mouse ou o teclado, vendo o efeito antes. Jogar sem tirar os olhos do personagem: os ganhos aparecem perto dele, o rodapé mostra só o que serve agora e, quando uma erva, uma pegada e uma carcaça estão juntas, saber antes do E qual delas será usada.

## O que já existe hoje

- **Bolsa com figuras** (pedido 13, entrega 4; `src/Game/BagPanel.cs`, `src/Game/ItemIcons.cs`; relatório `user-docs/relatorios/2026-11-26-bolsa-visual.md`):
  - figura, pilha e marca "✓ em uso" por item;
  - segunda fileira "Fabricar" com a figura do resultado e um "+";
  - painel do item com descrição, efeito e botão da ação possível (Equipar, Usar, Comer, Fabricar) ou o motivo em vermelho ("Falta: 1 Sílex", "Só junto à fogueira");
  - mouse (passar seleciona, clicar age) e teclado (setas/WASD, E/Enter, I/Esc).
- **Moedas** no alto da bolsa (`src/Items/Wallet.cs`, relatório `2026-11-26-moedas.md`).
- **Ficha (C)** com 7 atributos — FOR, VIT, INT, CON, AGI, SOR, KI — W/S escolhe, E gasta, prévia calculada numa cópia da ficha (`design/gdd/plains-progression.md` §3.2).
- **Pedido 14.0** (relatório `2026-11-27-atributos-ki.md`): FOR (+6% corpo a corpo), CON também reduz dano recebido (−3%/ponto, teto 45%), SOR também dá crítico (5% + 1%/ponto, teto 40%, ×1,5); barra do inimigo focado com "atual/máximo"; números de dano no Modo de assistência (`src/Game/CombatText.cs`).
- **HUD** (`src/Plains/PlainsHud.cs`): Vida em porcentagem com a cura pendente em verde; Vigor e Ki com atual/máximo; "Nv N · XP x/y"; rodapé com faixa escura de fundo (já atende "fundo legível").
- **H** (ação `heal`, `src/Game/Bindings.cs`) usa a melhor erva sem abrir a bolsa quando há Bolsa Simples (`plains-progression.md` §3.7).
- **Controles configuráveis** em Esc → Controles (`src/Game/ControlsPanel.cs`), com duas entradas por ação.
- **K contextual**: perto de carcaça esfola com Ki; longe, dispara (`design/gdd/campaign-prologue.md` §3.5).

## O que falta

1. **Decisão do FOR registrada na tarefa:** a linha "sem FOR" do item 6.A foi substituída no pedido 14 (FOR entra). Conferir que a ficha, a prévia e o Livro tratam os 7 atributos e que KI aparece como "reserva de energia" (Ki máximo = base 100 + 15 por ponto).
2. **Categorias na bolsa:** Equipamentos, Ferramentas, Consumíveis, Materiais (e "Tudo"). A categoria vem do catálogo de itens (`assets/data/plains/plains_items.json`), não do código da tela.
3. **Aba de fabricação** separada da grade de itens:
   - cada receita com ingredientes no formato **tenho / necessário** ("Sílex 2/1"), resultado e efeito;
   - motivo quando depende de fogueira, ferramenta ou proficiência;
   - quantidades atualizadas na hora, sem fechar a bolsa;
   - receitas da Entrega 5 (T05: machado/ferramenta de corte e de mineração) incluídas.
4. **Comparação com o equipado:** ao selecionar uma arma ou ferramenta, mostrar a diferença para o que está na mão (dano, alcance, tipo de ferramenta), por exemplo "Dano 1 → 2 (+1)".
5. **Atalhos configuráveis para itens:** alguns espaços rápidos ligados a itens escolhidos na bolsa; as teclas ficam em Esc → Controles. **H continua sendo a erva.** Abrir ou fechar qualquer tela não pode executar ataque, passo ou consumo no mundo.
6. **Botões "+" clicáveis na ficha**, com navegação por teclado, prévia do próximo valor ao passar o mouse e indicação dos pontos disponíveis.
7. **Vida completa ao inspecionar:** o HUD continua com "Vida 50%"; ao passar o mouse na barra (e na ficha) aparece "Vida 50% · 6/12". Vigor e Ki continuam atual/máximo.
8. **Rodapé contextual:** mostra só as ações úteis no momento (perto de carcaça, de erva, de fogueira, em combate); a lista completa fica em Controles.
9. **Ganhos e XP perto do personagem**, agrupando repetições ("+3 Galho Rijo" em vez de três linhas); mensagens importantes ficam tempo suficiente para leitura.
10. **Escolha de interação:** quando erva, pegada e carcaça (ou outro alvo de E) estão ao alcance, mostrar antes do E qual será o alvo (marca no mundo + nome no rodapé) e oferecer um jeito simples de trocar. Testar também fora da clareira, porque a semente pode gerar a mesma proximidade em outro bloco.
11. Organizar os componentes de interface por responsabilidade conforme `docs/elementos/PADRAO.md` (bolsa, fabricação, ficha, HUD, rodapé, atalhos).

## Dependências

- **T05** (Entrega 5): ferramentas de corte e de mineração, madeira e pedra com uso, e as receitas novas que a aba de fabricação precisa listar.
- `docs/elementos/PADRAO.md` (padrão de organização dos elementos, escrito na sessão principal).
- Não depende de T07; a T07 usa a escolha de interação (examinar pegada perto de carcaça).

## Decisões abertas (do criador)

1. **Quantos atalhos de item e em que teclas?**
   - A) 4 espaços em 1–4 (recomendado: cabe no rodapé e não conflita com WASD/J/K/L);
   - B) 2 espaços em Q e R;
   - C) 6 espaços em 1–6.
2. **Como trocar o alvo quando vários competem pelo E?**
   - A) o alvo é o mais próximo do **cursor do mouse**; com teclado, Tab alterna (recomendado: coerente com a mira livre do pedido 13);
   - B) sempre o mais próximo do personagem, e Tab alterna;
   - C) roda pequena de escolha ao segurar E.
3. **Onde mostrar "Vida 50% · 6/12"?**
   - A) ao passar o mouse na barra e sempre na ficha (recomendado);
   - B) só na ficha;
   - C) sempre no HUD, ao lado da porcentagem (contraria o pedido de manter a porcentagem limpa).
4. **H sem Bolsa Simples:** continua exigindo a Bolsa Simples (regra atual, recomendado: a bolsa tem valor de progressão) ou passa a funcionar desde o início?
5. **Categorias:** as quatro propostas bastam, ou "Comida" fica separada de "Consumíveis"? (recomendado: quatro + "Tudo").

## Critérios de aceite (jogáveis e testáveis)

1. A bolsa abre com a categoria "Tudo"; clicar em "Ferramentas" mostra só ferramentas (faca e as da T05); o mesmo pelo teclado.
2. Na aba Fabricar, a Faca de Pedra mostra "Sílex x/1 · Galho Rijo y/1" com os números reais; fabricar diminui x e y na hora e a faca aparece na bolsa.
3. Uma receita "na fogueira" longe da fogueira mostra o motivo e não fabrica; perto dela, fabrica.
4. Selecionar uma ferramenta não equipada mostra a comparação com a da mão; os números batem com os dados em `assets/data/`.
5. Ligar a erva a um atalho em Controles e usá-lo no mundo cura; a mesma tecla com a bolsa aberta não consome nada no mundo.
6. H usa a melhor erva (com a regra decidida); abrir/fechar bolsa, ficha, Livro e mapa nunca dispara corte, passo, Ki ou consumo.
7. Na ficha, clicar no "+" de VIT com 1 ponto gasta o ponto, a Vida máxima sobe 2 e o "+" some/desativa com 0 pontos; a prévia ao passar o mouse mostra "Vida máxima: 10 → 12". O mesmo pelo teclado.
8. Com 6/12 de Vida, o HUD diz "Vida 50%"; ao inspecionar (regra decidida) aparece "Vida 50% · 6/12".
9. Perto da carcaça, o rodapé mostra "E pegar carne · K esfolar com Ki"; longe de tudo, não mostra esses atalhos.
10. Recolher 3 galhos seguidos mostra um único "+3 Galho Rijo" perto do personagem; subir de nível mostra a mensagem por pelo menos o tempo definido em dados.
11. Com uma erva e uma pegada ao alcance, uma marca indica o alvo antes do E; trocar o alvo (regra decidida) muda a marca; E age só no alvo marcado. Passa nas duas sementes, dentro e fora da clareira.
12. Uma jornada salva antes da mudança (`tests/fixtures/saves/jornada_antes_do_ki.json`) carrega com os mesmos itens, equipamento, atributos e moedas.
13. Bolsa e ficha legíveis e clicáveis em janela e em tela cheia (o item sob o cursor é o selecionado).

## Roteiro de teste

1. Unidade: categorias vindas do catálogo; "tenho/necessário" de cada receita; comparação de equipamento; agrupamento de ganhos; escolha do alvo entre candidatos (distância, cursor, alternância).
2. Integração (nova `InventarioCheck` ou fases novas em `NovidadesCheck`): itens 1–11 acima com entrada simulada (lembrar do `await ProcessFrame` depois de cada `ParseInputEvent`).
3. Busca por semente: um teste procura nos blocos das sementes 20261126 e 777 um lugar fora da clareira com erva e pegada a menos de 26 px e confere a escolha.
4. Save antigo (fixture) e save novo com atalhos ligados: salvar, fechar, continuar e conferir.
5. Evidências (viewport do jogo): bolsa por categoria, aba Fabricar, comparação, ficha com "+", "Vida 50% · 6/12", rodapé em dois contextos, ganhos agrupados, marca de alvo entre erva e pegada.

## Prompt pronto para executar

```text
Tarefa T06 do SpiritOfMurin — Inventário, ficha, HUD e escolha de interação (Entrega 6 da campanha).

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T06-inventario-ficha-hud.md. Leia também user-docs/relatorios/2026-11-26-bolsa-visual.md, user-docs/relatorios/2026-11-27-atributos-ki.md e design/gdd/plains-progression.md §3.2 e §3.7. Confira o estado da T05 no README de tasks: as receitas e ferramentas dela precisam aparecer na aba de fabricação.

Atenção: a ficha TEM FOR (decisão do pedido 14, que substitui o "sem FOR" do item 6.A). Os atributos são FOR, VIT, INT, CON, AGI, SOR e KI.

Implemente o que está em "O que falta" da T06: categorias na bolsa, aba de fabricação com tenho/necessário e motivos, comparação com o equipado, atalhos de item configuráveis em Controles (H continua sendo a erva), botões "+" clicáveis na ficha com prévia, "Vida 50% · 6/12" ao inspecionar mantendo a porcentagem no HUD, rodapé contextual, ganhos e XP agrupados perto do personagem e escolha clara quando erva, pegada e carcaça disputam o E. Abrir ou fechar telas nunca executa ação no mundo.

Antes de implementar, pergunte-me as "Decisões abertas" da T06, uma por vez, com as opções e a recomendação. Para escolhas visuais, mostre uma prancha HTML em user-docs/relatorios/ antes de perguntar.

Regras:
- Organize cada componente de interface conforme docs/elementos/PADRAO.md.
- Valores de jogo e tempos de mensagem em assets/data/, nunca fixos no código.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2. Inclua os testes novos no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só da imagem do viewport do jogo, em production/qa/evidence/<data>_inventario-ficha-hud/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice README.md, commits atômicos (git add por arquivo), push para origin. Atualize o estado da T06 em user-docs/tasks/README.md com os links do relatório, das evidências e dos commits.
```
