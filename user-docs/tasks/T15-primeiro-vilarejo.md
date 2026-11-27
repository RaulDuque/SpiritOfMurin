# T15 — Primeiro vilarejo

**Estado:** planejada · **Frente:** F · **Depende de:** T06 · **Origem:** pedido 14 item 6.F (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`), pedido 13 item 11 (Moedas, `prompts/13-mira-tecnicas-bolsa-moedas.md`), `user-docs/futuro-do game/gpt.md` (Faisão Cauda-de-Fogo "para vender na primeira vila"); visão geral em `user-docs/tasks/T14-npcs-biomas-assentamentos.md`

## Objetivo do jogador

Depois de caçar e coletar, seguir o riacho (ou o caminho decidido) e chegar a um vilarejo pequeno. Entrar, conversar com duas ou três pessoas, vender peles, carne ou ervas a um comerciante, ver as Moedas subirem e comprar alguma coisa útil com elas. Voltar outro dia e encontrar o vilarejo igual, com o que comprou e vendeu lembrado.

## O que já existe hoje

- **Moedas** (`src/Items/Wallet.cs`): `Receive(amount)`, `Spend(amount, out reason)` com recusa por saldo insuficiente, evento `Changed`; saldo no alto da bolsa; salvo na jornada; jornadas antigas com 0 (relatório `user-docs/relatorios/2026-11-26-moedas.md`). Bolsinha de +25 Moedas dois passos à direita da esteira de meditação.
- **Bolsa com figuras** e (após T06) categorias, comparação e fabricação.
- **Materiais vendáveis:** Pele Rasgada/Comum/Fina, Couro Rasgado/Comum/Grosso, Carne Macia/Rija (crua e assada), Presas, Tendão, Tira de Amarração, Grama Sangue-de-Coelho, Raiz Amarga, Sílex, Galho Rijo; madeira e pedra da T05 (catálogo em `assets/data/plains/plains_items.json`).
- **Encaixe de lugares feitos à mão:** `PlainsGenerator.AddStamp`.
- **Fala com avançar/pular** no prólogo (`design/gdd/campaign-prologue.md` §3.2), reaproveitável para diálogo.
- **Ciclo de dia e noite** (`DayNight`), se a loja fechar à noite.

## O que falta

1. **Lugar do vilarejo** encaixado pela semente conforme a decisão da T14 (proposta: beira do riacho, ao sul, 4–6 blocos), com caminho ou pista visível desde a clareira; aparece no mapa depois de visto, com nome.
2. **Vilarejo feito à mão** (estampa): 4–6 construções baixas, cerca, poço ou cais no riacho, uma barraca de comerciante, entrada clara. Visual procedural no estilo aprovado (sem sprites; art bible §8.12).
3. **NPCs (2–3):** o comerciante; um morador que comenta a Planície (boato que vira pista, sem marcador); opcional um caçador que compra/pede pele. Cada um com nome, fala curta, avançar/pular.
4. **Diálogo:** E perto do NPC abre a fala; opções curtas ("Comprar", "Vender", "Perguntar", "Sair"); a fala não bloqueia o jogo por muito tempo.
5. **Loja — vender:** escolhe item da bolsa, mostra o preço unitário e o total, confirma; as Moedas sobem pela `Wallet.Receive`; o item sai da bolsa.
6. **Loja — comprar:** estoque pequeno em dados (proposta: erva, carne assada, Faca de Pedra, Bolsa Simples ou uma ferramenta simples); mostra preço, saldo e "falta N Moedas"; compra passa por `Wallet.Spend`, com recusa sem gastar nada.
7. **Preços** por item e qualidade em dados (Pele Fina vale mais que Pele Rasgada); o comerciante pode pagar menos que o preço de venda (margem em dados).
8. **Memória:** estoque comprado, itens vendidos (se o estoque do comerciante muda), conversas já ouvidas, vilarejo descoberto: tudo no save.
9. **Segurança:** animais não entram no vilarejo; javali não investe dentro; Disparo de Ki e corte não acertam NPCs (ou a regra decidida); abrir a loja não dispara ação no mundo (regra da T06).
10. Organização conforme `docs/elementos/PADRAO.md` (vilarejo, construções, NPC, fala, loja, tabela de preços).

## Dependências

- **T06** (bolsa com categorias, Moedas na interface, regra de "abrir tela não age no mundo").
- **T14** (decisões de lugar e de moeda).
- **T05** (madeira e pedra como itens vendáveis), não obrigatória.

## Decisões abertas (do criador)

1. **Lugar** (decisão 1 da T14): beira do riacho ao sul (recomendado), saída para o Vale, ou sorteado numa faixa.
2. **Nome do vilarejo** e dos NPCs (propostas para escolher, por exemplo "Vila do Vau", "Aldeia Junco Alto").
3. **O que o comerciante vende** no começo? A) consumíveis e ferramentas básicas (recomendado); B) também uma arma simples (pode antecipar a espada do posto de vigia: cuidado com a progressão); C) só consumíveis.
4. **Faixa de preços:** quanto vale uma Pele Fina em relação às 25 Moedas da bolsinha? Proposta: Pele Rasgada 2, Pele Comum 4, Pele Fina 8, Couro Grosso 12, erva comprada 6.
5. **Loja fecha à noite?** A) sim, o comerciante dorme (recomendado: usa o dia e noite); B) não.
6. **Atacar um NPC:** A) o golpe não sai perto de NPCs (recomendado nesta etapa); B) sai e o vilarejo reage.

## Critérios de aceite (jogáveis e testáveis)

1. Nas sementes 20261126 e 777, o vilarejo existe na faixa decidida, não tem objetos gerados nem animais dentro e aparece no mapa depois de visto.
2. **Entrar:** andar até a entrada e passar por ela não troca de cena (ou troca, se decidido) e o jogador fica dentro dos limites do vilarejo.
3. **Conversar:** E perto do comerciante abre a fala com nome e opções; avançar e sair funcionam por teclado e mouse; abrir/fechar a fala não executa corte, passo, Ki ou consumo.
4. **Vender material:** com 1 Pele Fina e 0 Moedas, vender deixa 0 Pele Fina e Moedas = preço em dados; a bolsa e o saldo atualizam na hora.
5. **Comprar:** com Moedas suficientes, comprar uma erva diminui as Moedas pelo preço e põe a erva na bolsa; com Moedas insuficientes, aparece "falta N Moedas" e nada muda.
6. **Ciclo completo de jornada:** acordar, recolher a bolsinha de +25, caçar um coelho, vender a pele, comprar algo — tudo pelo fluxo normal.
7. Meditar, fechar e continuar preserva Moedas, itens comprados, estoque e o vilarejo descoberto.
8. Jornada antiga (fixture `tests/fixtures/saves/jornada_antes_do_ki.json`) carrega, encontra o vilarejo intacto e consegue vender e comprar.
9. Com a loja fechada à noite (se decidido), a fala diz isso e a loja não abre; de dia abre.

## Roteiro de teste

1. Unidade: preço por item e qualidade; venda e compra via `Wallet` (saldo exato, recusa sem efeito colateral); estoque; serialização do vilarejo.
2. Integração `VilarejoCheck` (fases `lugar`, `entrar`, `conversar`, `vender`, `comprar`, `noite`, `save`, `oldsave`) nas duas sementes.
3. Evidências do viewport: vilarejo visto de fora, mapa com o vilarejo, fala do comerciante, tela de venda antes/depois, compra recusada e aceita.
4. Jogar pelo Painel → Jogar: da clareira ao vilarejo, vender e comprar.

## Prompt pronto para executar

```text
Tarefa T15 do SpiritOfMurin — Primeiro vilarejo.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md, user-docs/tasks/T14-npcs-biomas-assentamentos.md e user-docs/tasks/T15-primeiro-vilarejo.md. Leia user-docs/relatorios/2026-11-26-moedas.md, src/Items/Wallet.cs, design/gdd/plains-first-stretch.md §3.1 (AddStamp) e design/art/art-bible.md (visual procedural; sprites foram rejeitados em §8.12). Confirme no README de tasks que a T06 está pronta.

Quero um primeiro vilarejo pequeno onde eu consiga entrar, conversar com 2 ou 3 pessoas, vender material (peles, carne, ervas) e comprar algo usando as Moedas da jornada. O vilarejo é feito à mão e encaixado pela semente, aparece no mapa depois de visto, fica protegido de geração e de animais, e lembra tudo no save. Toda compra e venda passa por Wallet.Receive / Wallet.Spend, com recusa sem efeito colateral.

Antes de implementar, pergunte-me as "Decisões abertas" da T15 (e as da T14 que ainda estiverem abertas: lugar e moeda), uma por vez, com opções e recomendação. Mostre o vilarejo, os NPCs e a tela da loja numa prancha HTML em user-docs/relatorios/ antes de pedir aprovação.

Regras:
- Organize vilarejo, construções, NPCs, falas, loja e preços conforme docs/elementos/PADRAO.md (pode usar /create).
- Preços, margem, estoque, falas e horário da loja em assets/data/.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_primeiro-vilarejo/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice (com um roteiro curto pelo Painel → Jogar dizendo onde fica o vilarejo), commits atômicos, push para origin. Atualize a T15 em user-docs/tasks/README.md com links.
```
