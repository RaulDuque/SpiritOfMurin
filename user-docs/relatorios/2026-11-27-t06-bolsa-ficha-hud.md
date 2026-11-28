# T06 — Bolsa, fabricação, ficha, HUD e escolha de interação — 2026-11-27

> Tarefa: [user-docs/tasks/T06-inventario-ficha-hud.md](../tasks/T06-inventario-ficha-hud.md) · Design: [design/gdd/inventory-sheet-hud.md](../../design/gdd/inventory-sheet-hud.md) · Prancha das decisões: [2026-11-27-t06-prancha-bolsa.html](2026-11-27-t06-prancha-bolsa.html)
>
> Partiu da `main` em `6b09426`, que inclui o pedido 14. O visual do protagonista que você editou à mão não foi tocado.

## Decisões (suas, uma por vez, sobre a prancha)

| # | Pergunta | Escolha |
|---|---|---|
| 1 | Organização da bolsa | **A — abas no alto** (Tudo, Equipamentos, Ferramentas, Consumíveis, Materiais, Fabricação) |
| 2 | Botões da ficha | **A — "+" no fim de cada linha**, prévia no quadro ao lado |
| 3 | Mostrar o alvo do E | **C — anel no chão + lista curta** ao lado do personagem |
| 4 | Trocar o alvo | **Roda do mouse**; o Tab continua abrindo a bolsa |
| 5 | Ganhos | **A — sobre a cabeça**, somando ("+3 Galho Rijo") |
| 6 | Atalhos de item | **1 a 6, Q e R** (8 atalhos), mais o H da erva; o E continua sendo pegar |
| 7 | H sem Bolsa Simples | **Continua exigindo a Bolsa** (os outros atalhos valem desde o início) |
| 8 | Categorias | Tudo, Equipamentos, Ferramentas, Consumíveis, Materiais |
| — | "Vida 50% · 6/12" | Ao passar o mouse na barra e sempre na ficha (você escreveu "ao inspecionar") |

## O que você consegue fazer agora

1. **Bolsa (I).**
   - Abre em **Tudo**, com as figuras, as quantidades, o ✓ no que está na mão e as **moedas** no alto.
   - **Tab** (ou clique) troca de aba.
   - Ao selecionar uma ferramenta ou a faca, aparece a diferença para a da mão **em golpes nas mesmas árvores e rochas**. Exemplos: a faca contra o machado dá "Pinheiro: 6 golpes → não corta"; o Machado Lapidado dá "Pinheiro: 6 golpes → 3 golpes · Salgueiro: 5 → 3 · Bambu: 3 → 2 · Alcance 22 → 24 px".
2. **Aba Fabricação.**
   - Todas as receitas da T05 aparecem.
   - Cada ingrediente diz "✓/✗ Sílex: tenho 2 de 3".
   - Quando você recolhe o último ingrediente, a mensagem **"Agora dá para fabricar: …"** aparece (e fica mais tempo).
   - Fabricar atualiza os materiais, o resultado e o botão na mesma tela.
3. **Atalhos.**
   - Na bolsa, selecione um item e aperte **1–6, Q ou R** para ligar o item àquela tecla (de novo: desliga). Isso não usa nada.
   - No mundo, a tecla usa ou equipa o item. Machado no 1 e picareta no 2 trocam a ferramenta na hora.
   - A fileira no canto de baixo mostra o H e os atalhos com a quantidade. Quando acaba, ela mostra **0** e a tecla avisa "acabou".
   - As teclas mudam em **Esc → Controles** ("Atalho de item 1…8").
   - Os atalhos continuam depois de fechar e abrir o jogo.
4. **Ficha (C).**
   - Os 7 atributos têm **"+"** no fim da linha. Mouse ou W/S selecionam; clique no "+" ou E gasta.
   - **FOR** mostra o dano da arma na mão: "Machado de Pedra (na mão): dano em animais 1,06 → 1,12; crítico 1,59 → 1,68". Também avisa que árvores e rochas dependem da ferramenta, não do FOR.
   - **CON** mostra "ainda reduz 39 pontos (13 pontos de CON)" até o teto.
   - O alto da ficha e a barra, com o mouse em cima, mostram **"Vida 60% · 6/10"**. O HUD continua só com a porcentagem.
5. **Alvo do E.**
   - Com dois ou mais alvos ao alcance (erva, pegada, carcaça…) aparecem um **anel** sob o escolhido e uma lista ("▸ erva (E) / pegada / roda do mouse: trocar").
   - A **roda** troca o alvo, e o E age só no escolhido.
   - Com um único alvo, o E é imediato, como antes.
   - Duas pegadas da mesma trilha contam como uma.
6. **Rodapé e ganhos.**
   - O rodapé mostra o que serve agora: o aviso do alvo, "roda: trocar alvo", "J cortar" diante de árvore com machado, Shift na água, Espaço com bicho perto, H quando falta Vida. Depois vêm o K, a bolsa, a ficha, o mapa e "Esc comandos".
   - Os ganhos sobem da cabeça e somam ("+3 Galho Rijo", "+3 XP").
   - Abrir, confirmar ou fechar a bolsa, a ficha ou a pausa nunca corta, nunca dispara Ki e nunca coleta.

**Correções que vieram junto:**
- **Clique no KI da ficha:** gastava o ponto em SOR (um limite antigo do índice). Corrigido.
- **Roda do mouse nos Controles:** não era aceita como tecla, e o padrão "roda = próximo alvo" se apagava ao iniciar. Corrigido.

## Onde mexer (padrão de elementos)

Veja a tabela de interface em [docs/elementos/PADRAO.md §5](../../docs/elementos/PADRAO.md#5-interface-organizada-por-responsabilidade). Cada parte tem o seu arquivo:

| Parte | Arquivo |
|---|---|
| Bolsa (desenho) | `src/Game/BagPanel.cs` |
| Abas, "tenho X de Y", comparação | `src/Items/BagRules.cs` |
| Ficha | `src/Game/SheetPanel.cs` |
| Atalhos | `src/Game/QuickBar.cs` |
| Alvo do E | `src/Game/TargetMarker.cs` |
| Ganhos | `src/Game/GainFeed.cs` |
| Tempos das mensagens | `assets/data/plains/plains_hud.json` |
| Categoria de cada item | `assets/data/plains/plains_items.json` (`category`) |

## Testes

**Bateria completa pelo `PainelDeTestes.exe --auto`, com janela no monitor 2, nas sementes 20261126 e 777** (9 min). O relatório do Painel foi copiado para `production/qa/evidence/2026-11-27_inventario-ficha-hud/painel/`.

| Rodada | Resultado | O que aconteceu |
|---|---|---|
| Painel, bateria completa | **775/778** (2 execuções com falha) | **Recuperação · layout** (5/7): "os quatro cantos da janela são os quatro cantos da imagem". Os valores lidos não batem com nenhum canto, o mesmo padrão do mouse real chegando à janela de teste que apareceu no pedido 13. **Bolsa · alvo, semente 777** (4/5): um ramo do teste rodava quando a pegada examinada abria um painel, e com painel aberto o anel some de propósito. O erro era do teste |
| Repetições | layout **7/7 duas vezes**; alvo **5/5 nas duas sementes**, com o teste corrigido | o layout não se reproduz. O alvo passou a esperar o painel |
| **Total corrigido** | **778/778** | |

| Teste | Resultado |
|---|---|
| Unidade | 54/54. Novo `BagRulesTests`: abas, "tenho X de Y", comparação em golpes, ganhos que somam, escolha do alvo |
| `InventarioCheck` (novo) | bolsa 14/14 · atalhos 8/8 · atalhos2 3/3 · ficha 10/10 · entrada 2/2 · oldsave 4/4 · alvo 5/5, rodape 4/4 e percurso 9/9 em cada semente |
| Testes antigos da bolsa | `NovidadesCheck` bag 11/11, `PlainsCheck`, `JourneyCheck` e `PlainsRoute`: os ajudantes passaram a trocar de aba com Tab |

A jornada antiga (`tests/fixtures/saves/jornada_antes_do_ki.json`) carrega com os seus itens, a faca, os atributos e as moedas. Todos os testes usam `user://teste_*.json`: sua jornada e suas teclas não foram tocadas.

## Evidências (só a imagem do jogo)

Pasta `production/qa/evidence/2026-11-27_inventario-ficha-hud/`:

| Arquivo | O que mostra |
|---|---|
| `01-bolsa-tudo` | bolsa com abas |
| `02-bolsa-comparacao-faca` | a faca comparada com o machado na mão |
| `03-fabricacao-falta` | o que falta para o Machado Lapidado |
| `04-fabricado` | depois de fabricar |
| `05-atalhos-erva-acabou` | o atalho da erva em 0 |
| `06-vida-ao-passar-o-mouse` | "Vida 60% · 6/10" |
| `07-ficha-prévia-for` | a prévia do FOR na ficha |
| `08-ficha-sem-pontos` | a ficha sem pontos livres |
| `09/10-alvo-…` | anel e lista, e o alvo trocado, nas duas sementes |
| `11-ganhos-agrupados-…` | "+3 Galho Rijo" sobre a cabeça |
| `12-rodape-carcaca-…` | o rodapé junto à carcaça |
| `13-percurso-falta-silex-…` | o percurso com sílex faltando |

## Roteiro curto — PainelDeTestes.exe → Jogar

1. **I** abre a bolsa. Aperte **Tab** algumas vezes e veja as abas; clique em "Ferramentas".
2. Selecione a faca e leia a comparação com o machado. Vá a **Fabricação** e veja "tenho X de Y".
3. Com a bolsa aberta, selecione o machado e aperte **1**, depois a picareta e aperte **2**. Feche e aperte 2 e 1 no mundo.
4. **C** abre a ficha. Passe o mouse no FOR e no CON, e clique no "+". Passe o mouse na barra de Vida.
5. Pare entre uma erva e uma pegada (ou uma carcaça): **gire a roda** e aperte **E**.
6. Pegue três galhos seguidos e veja "+3 Galho Rijo" sobre a cabeça.

## Pendências

- Nenhuma da T06. A conferência no seu monitor (tamanho das letras, tela cheia) está na lista do [README de tarefas](../tasks/README.md).
