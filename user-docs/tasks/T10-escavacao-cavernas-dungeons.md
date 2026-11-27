# T10 — Escavação, cavernas e dungeons de tesouro (visão geral)

**Estado:** planejada · **Frente:** D · **Depende de:** T05, T06 · **Origem:** pedido 14 item 6.D (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`), `user-docs/futuro-do game/opiniao-claude.md` (terreno gerado + lugares feitos à mão), `design/gdd/game-concept.md` (Pilar 3 "O mundo se lembra")

Esta tarefa define as regras comuns. A primeira caverna jogável é a **T11** e a primeira dungeon é a **T12**; cada uma tem seu prompt.

## Objetivo do jogador

Notar um sinal no chão, cavar com a pá e achar algo; distinguir três atividades: **escavar terra** (pá, solo mole), **minerar rocha exposta** (picareta da T05) e **explorar uma caverna** (descer para outro lugar). Encontrar a boca de uma caverna, entrar, enfrentar o que vive lá, achar a passagem para uma dungeon de tesouro, ler as armadilhas, voltar pelo caminho certo e sair no mesmo ponto da superfície. Voltar dias depois e encontrar tudo como deixou.

## O que já existe hoje

- **Mundo por semente em blocos** (`design/gdd/plains-first-stretch.md` §3.1): tiles e objetos são função da semente e da coordenada; id estável `tipo@tx,ty`; `PlainsGenerator.AddStamp` reserva tiles para lugares feitos à mão.
- **Memória do mundo** (`WorldState`) e jornada (`user://jornada_planicie.json`): objetos recolhidos, carcaças, partes tiradas, trilhas examinadas (`plains-progression.md` §3.8).
- **Ferramentas e recursos** da Entrega 5 (T05): ferramenta de corte e de mineração, rachaduras em rocha, materiais em dados, estado por objeto persistente.
- **Troca de cena** já usada pela campanha (`src/Campaign/Campaign.tscn`: prólogo → Planície), com etapas persistentes.
- **Moedas** (`src/Items/Wallet.cs`) e Livro de Técnicas (descobertas registradas).

## O que falta

### Escavação (pá)
1. **Pá** como ferramenta nova (receita e dados no padrão), separada da picareta.
2. **Solos apropriados:** só cava em terra, lama, areia de margem; nunca em água, rocha, clareira inicial, entradas protegidas, pontos de missão nem lugares carimbados.
3. **Pistas no terreno:** terra fofa, pedrinhas brilhando, montículo, planta que só cresce sobre minério, marcas de animal cavando (javali fuçando já é pista de raiz). Rastreamento/Erboristeria podem melhorar a leitura.
4. **Resultado:** buraco persistente (estados: intacto → cavado → esgotado), com tabela de achados em dados (terra, pedra, sílex, minério, raiz, raramente moedas antigas), SOR no bônus como na T05.
5. **Diferença clara:** escavar (pá, chão) × minerar (picareta, rocha exposta, T05) × caverna (entrada, outra cena).

### Cavernas
6. **Entradas geradas** pela semente em habitats elegíveis (encosta, pedreira) ou carimbadas à mão; marcadas no mapa depois de vistas.
7. **Cena separada** por caverna, com **entrada e retorno à mesma posição da superfície** (tile da boca + direção de saída), sem gerar a superfície de novo de outro jeito.
8. **Conteúdo:** fauna de caverna, inimigos, chefe opcional, recursos (veios de minério, cristais), e uma passagem para uma dungeon.
9. **Interior:** feito à mão (primeiro) ou gerado a partir da semente da entrada (decisão aberta).

### Dungeons de tesouro
10. **Salas** encadeadas com propósito (entrada, armadilha, quebra-cabeça leve, guarda, tesouro), **armadilhas legíveis** (o aviso vem antes do dano, como no combate), **rotas de retorno** (atalho que se abre por dentro).
11. **Recompensas:** materiais, moedas, ouro (decisão: ouro é moeda separada ou material?), técnicas (fragmentos de manual, Pilar 2: pistas, nunca receitas).

### Memória
12. **Semente + save lembram** o que foi explorado, aberto e recolhido: salas visitadas, baús abertos, armadilhas desarmadas, atalhos abertos, buracos cavados, chefe derrotado. Uma recompensa sai uma única vez.
13. **Compatibilidade com blocos:** escavações e entradas pertencem a um tile, e portanto a um bloco; sair do bloco e voltar mantém o estado; liberar o bloco não perde nada.
14. Organização conforme `docs/elementos/PADRAO.md` (pá, solo, buraco, entrada, sala, armadilha, baú).

## Dependências

- **T05:** sistema de ferramenta, golpe, alcance, progresso visível, estado por objeto, tabela de materiais com SOR.
- **T06:** bolsa com categorias e fabricação (pá, recompensas).
- **T07/T08:** linguagem de combate e fauna para os habitantes das cavernas.
- **T13** (opcional): técnicas e pistas de cultivo como recompensa de dungeon.

## Decisões abertas (do criador)

1. **Interior das cavernas:** A) feito à mão e encaixado pela semente (recomendado para a primeira); B) gerado pela semente da entrada; C) mistura (salas feitas à mão ligadas por corredores gerados).
2. **Ouro:** A) moeda separada das Moedas (mais valiosa, para leilão); B) material que se vende/troca por Moedas (recomendado: uma moeda só por enquanto); C) só nome de pilha de Moedas grandes.
3. **Buracos se fecham com o tempo?** A) nunca (recomendado: o mundo se lembra); B) depois de N dias de jogo; C) só os sem achado.
4. **Morrer dentro da caverna:** A) desperta na última esteira de meditação na superfície (regra atual de desmaio, recomendado); B) desperta na entrada da caverna; C) há um ponto de meditação dentro.
5. **Técnicas em dungeon:** fragmento de manual que só vira técnica depois de experimentar (A, recomendado, Pilar 2), ou técnica completa (B)?

## Critérios de aceite (jogáveis e testáveis)

1. Com a pá equipada, E/golpe numa pista de terra fofa cava; na rocha exposta, a pá avisa "use a picareta"; na água, na clareira e numa entrada protegida, nada acontece e aparece o motivo.
2. Um buraco cavado continua cavado ao sair do bloco, voltar, meditar, fechar e continuar; o achado sai uma única vez.
3. A tabela de achados vem de `assets/data/` e a influência de SOR é verificável com rolagem fixa.
4. Nenhum buraco, entrada de caverna ou objeto gerado cai sobre a clareira, a ponte, as trilhas garantidas ou um lugar carimbado (teste varre blocos nas duas sementes).
5. Entrar numa caverna troca de cena; sair volta ao tile da boca com a direção de saída, nas duas sementes.
6. O que foi aberto, desarmado e recolhido dentro de cavernas e dungeons persiste no save e não volta.
7. Jornada antiga (fixture) carrega sem cavernas visitadas e sem erro.

## Roteiro de teste

1. Unidade: regra de solo válido; estados do buraco; tabela de achados com SOR; proteção de tiles (clareira, carimbos, entradas); serialização do estado de caverna/dungeon.
2. Integração: `EscavacaoCheck` (fases `cavar`, `proibido`, `persistencia`); as cenas de T11 e T12 têm checagens próprias.
3. Evidências do viewport: pista, buraco cavado, recusa na rocha, entrada vista no mapa.

## Prompt pronto para executar

```text
Tarefa T10 do SpiritOfMurin — Escavação com pá (regras comuns de escavação, cavernas e dungeons).

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T10-escavacao-cavernas-dungeons.md. Leia design/gdd/plains-first-stretch.md §3.1 e design/gdd/plains-progression.md §3.8, e o relatório da Entrega 5 (T05) para reaproveitar ferramenta, golpe, estado por objeto e tabela de materiais. Confira no README de tasks se T05 e T06 estão prontas.

Nesta tarefa implemente a ESCAVAÇÃO (itens 1 a 5 e 12 a 14 de "O que falta"): a pá, solos apropriados, pistas no terreno, buraco persistente, tabela de achados com SOR, proteção da clareira, das entradas e dos pontos de missão, e a diferença clara entre escavar, minerar e explorar. Prepare (sem conteúdo) o modelo de dados e de save que a primeira caverna (T11) e a primeira dungeon (T12) vão usar, e registre as regras num GDD em design/gdd/.

Antes de implementar, pergunte-me as "Decisões abertas" da T10, uma por vez, com opções e recomendação. Escolhas visuais (a pá, o buraco, as pistas) vêm com prancha HTML em user-docs/relatorios/.

Regras:
- Organize pá, solo, buraco e pistas conforme docs/elementos/PADRAO.md.
- Tabelas, chances e receitas em assets/data/. A geração continua pura (semente + coordenada).
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_escavacao/. Nunca capture a área de trabalho.
- Termine todas as partes primeiro e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a T10 em user-docs/tasks/README.md com links.
```
