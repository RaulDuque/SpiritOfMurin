# T16 — Cidade com bairros, serviços especializados e leilão

**Estado:** planejada · **Frente:** F · **Depende de:** T15, T14a (caminhos gerados) · **Origem:** pedido 14 item 6.F (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`); visão geral em `user-docs/tasks/T14-npcs-biomas-assentamentos.md`; `design/gdd/game-concept.md` (Pilares 3 e 5; Vertical Slice: "primeira fornalha" e "um mestre que reconhece seu estilo")

## Objetivo do jogador

Chegar, depois do vilarejo, a uma cidade grande: entrar por um portão, se orientar por bairros com identidade (mercado, artesãos, escolas, porto ou ruas de taverna), achar serviços que o vilarejo não tem — ferreiro que trabalha minério e presas, alquimista, cozinheiro, taverna com boatos, escola marcial — e um leilão onde os materiais raros que caçou valem de verdade. Sentir que caça, coleta, fabricação, cultivo e descoberta de técnicas convergem ali.

## O que já existe hoje

- Depois da **T15**: vilarejo encaixado pela semente, NPCs com fala, loja com compra e venda via `src/Items/Wallet.cs`, preços em dados, memória no save.
- Depois da **T14a**: caminhos gerados com NPCs viajantes.
- **Materiais raros planejados** (T08): Chifre de Cristal, Couro Rúnico, Presa Sangrenta, Pelo de Prata, Presa de Aço, Penas do Cauda-de-Fogo, Pluma Leve, Casco Espiritual, Glândula da Víbora Jade.
- **Livro de Técnicas** e técnicas descobertas experimentando (base para a escola marcial reconhecer o estilo, Pilar 3).
- **Proficiências** Esfolar, Preparo de Caça, Erboristeria, Rastreamento (`design/gdd/plains-progression.md` §3.4).

## O que falta

1. **Lugar da cidade** encaixado pela semente, mais longe que o vilarejo e ligado a ele por caminho; ou numa região seguinte (decisão aberta).
2. **Cidade feita à mão** com 3–5 bairros, portão e muralha, ruas legíveis, pontos de referência para orientação; mapa próprio por bairro visitado.
3. **Serviços especializados** (dados de cada um em `assets/data/`):
   - **Ferreiro:** fabrica e melhora armas e ferramentas com minério (T05/T11) e presas; mostra tenho/necessário como a fabricação da T06;
   - **Alquimista:** compra glândulas e ervas, ensina preparos; porta de entrada para a fornalha operada à mão (Pilar 5, falha legível) — a fornalha em si é outra tarefa;
   - **Cozinheiro:** pratos que recuperam mais que a fogueira, usando carnes por qualidade;
   - **Taverna:** boatos que viram pistas (variantes raras, cavernas, missão de avanço da T13) sem marcador no mapa; missões curtas;
   - **Escola marcial:** treino e duelos; o mestre reconhece técnicas descobertas pelo nome (Pilar 3) e pode ensinar uma forma básica;
   - **Comerciantes por bairro** com estoques diferentes.
4. **Leilão:**
   - o jogador põe um item raro, define preço mínimo e o leilão fecha depois de um tempo de jogo (ou ao meditar), pagando em Moedas;
   - lotes à venda para o jogador com lances de NPCs simulados;
   - regras e valores em dados; resultado determinístico por semente + dia para poder testar.
5. **Rotina por horário:** lojas abrem e fecham, taverna mais cheia à noite (usa `DayNight`).
6. **Memória:** bairros descobertos, serviços usados, lotes do leilão, encomendas ao ferreiro, conversas: tudo no save.
7. **Desempenho:** a cidade carrega sem travar a Planície (cena separada ou carregamento por bairro: decisão técnica registrada).
8. Organização conforme `docs/elementos/PADRAO.md` (cidade, bairro, serviço, NPC, lote do leilão).

## Dependências

- **T15** (loja, fala, preços, Moedas), **T14a** (caminhos), **T08** (materiais raros), **T05/T11** (minério), **T13** (pistas de cultivo na taverna/escola).

## Decisões abertas (do criador)

1. **Onde fica a cidade?** A) ainda na Planície, longe da clareira (recomendado para testar cedo); B) na próxima região, depois do Vale de Bambus; C) a cidade é a capital de uma seita.
2. **Quais bairros?** Proposta para escolher 3–5: Mercado, Rua dos Artesãos (ferreiro, curtidor), Pátio das Escolas, Porto do Rio, Beco das Tavernas, Bairro Alto (leilão).
3. **Como funciona o leilão?** A) assíncrono: o lote fecha depois de X tempo de jogo ou na próxima meditação (recomendado); B) leilão ao vivo com lances em tempo real contra NPCs; C) só preço fixo de consignação.
4. **A escola marcial ensina técnicas?** A) só reconhece e treina o que o jogador descobriu (recomendado: Pilar 2 "o jogo dá nome ao que você descobriu"); B) ensina formas básicas pagas; C) ambos, com técnicas ensinadas sempre menores que as descobertas.
5. **Nome da cidade e da moeda de alto valor** (se a T14 decidir por mais de uma moeda).
6. **Cena separada ou parte do mundo em blocos?** A) cena separada com portão (recomendado: mais simples e mais leve); B) parte do mundo contínuo.

## Critérios de aceite (jogáveis e testáveis)

1. Nas sementes 20261126 e 777, a cidade existe no lugar decidido, ligada ao vilarejo por caminho, e aparece no mapa depois de vista.
2. Entrar pelo portão e sair volta à mesma posição da Planície (se for cena separada).
3. Cada bairro tem nome mostrado ao entrar e aparece no mapa da cidade só depois de visitado.
4. **Ferreiro:** com os materiais, fabrica a ferramenta/arma da receita; sem eles, mostra tenho/necessário e não cobra Moedas.
5. **Alquimista e cozinheiro:** compram e vendem pelos preços em dados; um preparo/prato tem o efeito descrito.
6. **Taverna:** um boato aponta (em texto) para um lugar real gerado pela semente; nenhum marcador aparece no mapa sozinho.
7. **Escola marcial:** o mestre cita pelo nome uma técnica que o jogador já descobriu e não cita uma que ele não descobriu.
8. **Leilão:** pôr um Chifre de Cristal com mínimo X; depois do tempo de fechamento, as Moedas sobem conforme o resultado determinístico; o item sai da bolsa uma única vez; lote não vendido volta.
9. Lojas fecham e abrem conforme o horário em dados.
10. Salvar dentro da cidade, fechar e continuar volta à cidade com leilão, encomendas e saldo preservados; jornada antiga (fixture) carrega sem erro.
11. A entrada na cidade não causa travada perceptível (tempo de carregamento medido abaixo do limite em dados).

## Roteiro de teste

1. Unidade: regras do leilão (fechamento, determinismo, devolução), receitas do ferreiro, preços por bairro, citação de técnica descoberta.
2. Integração `CidadeCheck` (fases `lugar`, `portao`, `bairros`, `ferreiro`, `servicos`, `taverna`, `escola`, `leilao`, `horario`, `save`).
3. Evidências do viewport: portão, cada bairro, ferreiro com tenho/necessário, leilão antes/depois, mestre citando a técnica.
4. Jogar pelo Painel → Jogar: vilarejo → cidade, vender um raro no leilão, encomendar ao ferreiro.

## Prompt pronto para executar

```text
Tarefa T16 do SpiritOfMurin — Cidade com bairros, serviços especializados e leilão.

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md, user-docs/tasks/T14-npcs-biomas-assentamentos.md, user-docs/tasks/T15-primeiro-vilarejo.md e user-docs/tasks/T16-cidade-e-leilao.md, e os relatórios da T15 e da T14a. Leia design/gdd/game-concept.md (Pilares 2, 3 e 5, anti-pilares). Confirme no README de tasks que T15 e T14a estão prontas. Não leia arquivos *-SOLUCAO.md.

Quero uma cidade feita à mão, encaixada pela semente e ligada ao vilarejo por caminho, com 3 a 5 bairros, ferreiro, alquimista, cozinheiro, taverna, escola marcial, comerciantes por bairro e um leilão para os materiais raros. Lojas seguem o horário. Tudo é lembrado no save. Toda transação passa por src/Items/Wallet.cs.

Antes de implementar, pergunte-me as "Decisões abertas" da T16, uma por vez, com opções e recomendação. Mostre a planta da cidade, os bairros e as telas do leilão e do ferreiro numa prancha HTML em user-docs/relatorios/ antes de pedir aprovação. Registre as regras num GDD em design/gdd/.

Regras:
- Organize cidade, bairros, serviços, NPCs e leilão conforme docs/elementos/PADRAO.md (pode usar /create).
- Preços, receitas, horários, regras do leilão e falas em assets/data/. Resultado do leilão determinístico por semente e dia, para ser testável.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_cidade-leilao/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a T16 em user-docs/tasks/README.md com links.
```
