# T14 — NPCs, biomas, vilarejos e grandes cidades (visão geral)

**Estado:** planejada · **Frente:** F · **Depende de:** T06 (bolsa e Moedas na interface), T08 (materiais para vender) · **Origem:** pedido 14 item 6.F (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`), pedido 13 item 11 (Moedas), `user-docs/futuro-do game/opiniao-claude.md` (terreno gerado + lugares feitos à mão, modelo Terraria), `gpt.md` (primeira vila, Vale de Bambus, espada no posto de vigia), `design/gdd/game-concept.md` (Pilares 3 e 5)

Esta tarefa organiza a frente inteira. As entregas jogáveis são a **T15** (primeiro vilarejo) e a **T16** (cidade com bairros e leilão).

## Objetivo do jogador

Encontrar gente pelo mundo e lugares feitos à mão que se encaixam no terreno gerado: um eremita numa trilha, um caçador perto do lamaçal, um vilarejo na beira do riacho, mais tarde uma cidade grande. Conversar, receber missões curtas, vender o que caçou e colheu, comprar com as Moedas da jornada, fabricar com um ferreiro, aprender com um alquimista ou uma escola marcial. Sentir que caça, coleta, fabricação, cultivo e descoberta de técnicas têm para onde levar.

## O que já existe hoje

- **Mundo por semente** com `PlainsGenerator.AddStamp(tile, chão, objetos)` para reservar tiles de lugares feitos à mão antes das regras aleatórias (`design/gdd/plains-first-stretch.md` §3.1).
- **Moedas:** saldo inteiro, receber, gastar com recusa por saldo insuficiente, bolsinha de +25 perto da esteira, salvas na jornada (`src/Items/Wallet.cs`; relatório `user-docs/relatorios/2026-11-26-moedas.md`). "Lojas, preços e comerciantes ficam para a etapa das vilas" (pedido 13).
- **Materiais** da caça e da coleta (peles, couros, carnes, presas, tendão, ervas, sílex, galho; mais madeira e pedra da T05).
- **Diálogo** só no prólogo (fala do Sênior com avançar/pular, `design/gdd/campaign-prologue.md` §3.2).
- **Não existe:** NPC, caminho gerado, assentamento, loja, preço, missão.

## O que falta (visão da frente)

### NPCs pelo mundo
1. **Papéis:** viajante, caçador, eremita, mercador ambulante, discípulo de seita, rival. Cada papel tem lugar típico, horário, falas e o que oferece (informação, troca, missão, ensino).
2. **Diálogo curto** (anti-pilar: nada de texto longo obrigatório), com avançar/pular como no prólogo e a opção de perguntar mais.
3. **Missões curtas** ligadas aos sistemas existentes: trazer materiais, mostrar uma pista, caçar um animal que ameaça o caminho, entregar algo no vilarejo.
4. **Memória:** NPCs lembram do que foi feito e, mais tarde, citam as artes do jogador pelo nome (Pilar 3).
5. **Caminhos gerados:** trilhas pela semente ligando a clareira, o vilarejo e saídas da região (Vale de Bambus); NPCs viajantes andam por elas; bandidos e caçadores (T08) usam os mesmos caminhos.

### Biomas com identidade
6. A Planície Verdejante tem lugares nomeados na visão do `gpt.md`: **Clareira do Despertar**, **Campos das Tocas**, **Lamaçal das Raízes**, **Pedras do Riacho**, **Bosque do Orvalho**, **Espinhaço dos Lobos**, e a saída para o **Vale de Bambus** (posto de vigia com uma espada simples).
7. Cada bioma novo define: chão e paleta, flora, fauna (fichas da T08), clima típico (T09), recursos, lugares feitos à mão e música/sons. Os nomes e a ordem dos biomas depois da Planície são decisão do criador.

### Assentamentos feitos à mão no mundo gerado
8. **Escala:** acampamento → vilarejo → vila → cidade grande com bairros.
9. **Encaixe por semente:** o assentamento é uma cena/estampa feita à mão; a semente decide **onde** (dentro de regras: perto de água, longe da clareira, ligado a um caminho) mas nunca muda o interior.
10. **Proteção:** escavação, geração de objetos e fauna não invadem o carimbo (regra comum com a T10).

### Serviços previstos
| Serviço | O que faz | Liga a |
|---|---|---|
| Comerciante | Compra materiais, vende básicos (erva, comida, ferramentas) | caça, coleta, Moedas |
| Ferreiro | Fabrica/conserta armas e ferramentas com minério, presas (Presa de Aço do Javali Casca-de-Ferro) | mineração (T05), cavernas (T11) |
| Alquimista | Compra glândulas e ervas, ensina preparos, depois a fornalha | Erboristeria, Pilar 5 "Todo caminho é um Dao" |
| Cozinheiro | Prepara carnes melhores que a fogueira | Preparo de caça |
| Taverna | Boatos (pistas sem marcador), descanso, missões | rastreamento, variantes raras |
| Escola marcial | Treino, duelos, reconhecimento de técnicas descobertas | Livro de Técnicas, cultivo (T13) |
| Leilão (cidade) | Vende raros (Chifre de Cristal, Couro Rúnico, penas do Cauda-de-Fogo) | variantes raras (T08), dungeons (T12) |

## Sub-entregas

| Id | Conteúdo | Depende de |
|---|---|---|
| T15 | Primeiro vilarejo: entrar, conversar, vender material, comprar com Moedas | T06 |
| T14a | Caminhos gerados + NPCs viajantes nas trilhas | T15 |
| T14b | Missões curtas e memória dos NPCs | T15 |
| T14c | Segundo bioma (nome e identidade a decidir) | T08, T09 |
| T16 | Cidade com bairros, serviços especializados e leilão | T15, T14a |

## Dependências

- **T06** (bolsa com categorias, Moedas visíveis), **T08** (materiais), **T09** (clima por bioma), **T10** (proteção de tiles), **T13** (escola marcial e pistas de cultivo).

## Decisões abertas (do criador)

1. **Onde fica o primeiro vilarejo?** A) na beira do riacho, ao sul da clareira, a 4–6 blocos (recomendado: o riacho já guia o jogador); B) na saída para o Vale de Bambus, junto ao posto de vigia; C) em lugar sorteado pela semente dentro de uma faixa.
2. **Moeda única ou várias?** A) só "Moedas" por enquanto (recomendado); B) cobre, prata e ouro; C) Moedas + pedras espirituais para itens de cultivo.
3. **Nome definitivo da moeda** (hoje provisório "Moedas").
4. **Segundo bioma:** A) Vale de Bambus ampliado (já existe no prólogo); B) floresta/pântano; C) montanha com seita. Recomendação: A.
5. **NPCs com rotina por horário** (dormem à noite, loja fecha) desde o primeiro vilarejo, ou só depois? Recomendado: loja fecha à noite já na T15 (usa o `DayNight` que existe).

## Critérios de aceite (jogáveis e testáveis)

São os da T15 e da T16. Para a frente inteira:
1. Todo assentamento é uma cena/estampa feita à mão e aparece no mesmo lugar para a mesma semente, nas sementes 20261126 e 777.
2. Nenhum objeto gerado, buraco ou animal nasce dentro de um assentamento.
3. Todo preço, estoque e fala está em `assets/data/` (nada fixo no código).
4. Toda compra e venda passa por `Wallet.Receive`/`Wallet.Spend`, com recusa por saldo insuficiente.
5. NPCs, serviços e assentamentos seguem `docs/elementos/PADRAO.md`.

## Roteiro de teste

Ver T15 e T16. Teste comum: varrer blocos nas duas sementes e conferir posição dos assentamentos, proteção do carimbo e ligação a um caminho.

## Prompt pronto para executar

```text
Tarefa T14 do SpiritOfMurin — NPCs, biomas e assentamentos: planejamento detalhado e sub-entrega <T14a|T14b|T14c>.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md, user-docs/tasks/T14-npcs-biomas-assentamentos.md e, se já estiver pronta, o relatório da T15. Leia user-docs/futuro-do game/ (gpt.md e opiniao-claude.md) e design/gdd/game-concept.md (Pilares 3 e 5, anti-pilares).

Execute a sub-entrega indicada (caminhos gerados com NPCs viajantes, missões curtas com memória dos NPCs, ou o segundo bioma). Registre as regras num GDD em design/gdd/ (fonte única) e deixe a T14 apontar para ele.

Antes de implementar, pergunte-me as "Decisões abertas" da T14 que afetam esta sub-entrega, uma por vez, com opções e recomendação. NPCs, lugares e biomas vêm com prancha HTML em user-docs/relatorios/ antes de pedir aprovação.

Regras:
- Organize NPCs, falas, caminhos e biomas conforme docs/elementos/PADRAO.md (pode usar /create).
- Falas, preços, rotinas e regras de posição em assets/data/. Assentamentos e caminhos são encaixados pela semente sem mudar o interior feito à mão (PlainsGenerator.AddStamp).
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_<tema>/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a sub-entrega em user-docs/tasks/README.md com links.
```
