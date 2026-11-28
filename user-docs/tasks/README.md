# Tarefas do SpiritOfMurin — ponto de retomada

*Atualizado em 2026-11-28, no fim da T18. Leia isto primeiro ao voltar ao projeto, de qualquer sessão ou ferramenta.*

**Em 1 minuto:**
- **Em andamento:** nenhuma. A T18 terminou (2026-11-28).
- **Próximo prompt a mandar:** o da **[T08](T08-fauna-expandida-chefes.md)** (fauna nova, elites e chefes), na seção "Prompt pronto para executar". Copie e cole numa sessão nova.
- **O que você pode testar agora:** a seção "Pronto" abaixo. Tudo abre pelo **PainelDeTestes.exe → Jogar** (ou **Inspeção** para ver os elementos).
- **O que depende de você:** a seção "Decisões abertas".

Cada tarefa tem um id estável (T01, T02…). As que têm trabalho suficiente ganham um arquivo próprio, com objetivo, estado, o que falta, dependências, decisões, critérios, roteiro de teste e um prompt pronto. As decisões já tomadas ficam nos documentos citados, sem cópia aqui: `design/gdd/`, `docs/elementos/` e os relatórios.

## Ordem das próximas entregas

| # | Tarefa | Por quê nessa ordem |
|---|---|---|
| ✓ | ~~[T06](T06-inventario-ficha-hud.md) — bolsa, ficha, HUD, escolha de interação~~ | pronta em 2026-11-27 |
| ✓ | ~~[T07](T07-combate-rastreamento-fauna.md) — combate, rastreamento, javali e coelho~~ | pronta em 2026-11-28 |
| ✓ | ~~[T18](T18-equipamento-visual.md) — armas, roupas, capacetes~~ | pronta em 2026-11-28 |
| 4 | **[T08](T08-fauna-expandida-chefes.md) — fauna nova, elites e chefes** (sub-entregas T08a–T08j) | Depende do vocabulário de combate da T07 |
| 5 | **[T09](T09-clima-e-agua.md) — clima, lagos, peixes e patos** | Depende da T07 (avisos legíveis na chuva) |
| 6 | **[T10](T10-escavacao-cavernas-dungeons.md) → [T11](T11-primeira-caverna.md) → [T12](T12-primeira-dungeon.md) — pá, caverna, dungeon** | Usa as ferramentas e o estado persistente da T05 |
| 7 | **[T13](T13-reinos-cultivo-nivel10.md) — nível 10, reinos e missão de avanço** | Precisa da ficha da T06 |
| 8 | **[T14](T14-npcs-biomas-assentamentos.md) → [T15](T15-primeiro-vilarejo.md) → [T16](T16-cidade-e-leilao.md) — NPCs, vilarejo, cidade** | O vilarejo usa as moedas e os materiais |
| — | **[T17](T17-organizar-elementos-antigos.md) — levar os elementos antigos ao padrão** | Por partes, junto da entrega que mexer em cada família |

## Pronto (dá para testar agora)

| Id | O quê | Relatório | Como testar |
|---|---|---|---|
| T01 | Prólogo do Vale até o Sênior e o despertar na Planície | [campanha-prologo](../relatorios/2026-11-26-campanha-prologo.md) | Painel → Jogar → Novo jogo |
| T02 | Técnicas seladas e de volta enfraquecidas no nível 3 | [tecnicas-seladas](../relatorios/2026-11-26-tecnicas-seladas.md) | suba ao nível 3 na Planície |
| T03 | Tela cheia (F11), mapa (M), minimapa **com ícones e legenda** (pedido 14.2) | [tela-cheia-mapa](../relatorios/2026-11-26-tela-cheia-mapa.md), [pedido 14](../relatorios/2026-11-27-pedido14-organizacao-e-entrega5.md) | M abre o mapa; legenda no caderno |
| T04 | Ambientação: dia e noite, vento, pássaros, terreno, água | [ambientacao](../relatorios/2026-11-27-ambientacao.md) | espere a noite perto da fogueira |
| — | Pedido 13: mira pelo mouse, Disparo e Reforço de Ki, bolsa com figuras, moedas | [pedido13-final](../relatorios/2026-11-27-pedido13-final.md) | roteiro no relatório |
| 14.0 | **Ki** no lugar de Qi; **FOR**; defesa de CON; crítico; vida do inimigo em números; números de dano na assistência | [atributos-ki](../relatorios/2026-11-27-atributos-ki.md) | ficha (C), Espaço num javali, Esc → Modo de assistência |
| 14.1 | Brilho azul na espada com o Reforço de Ki; rastro de corte maior | [pedido 14](../relatorios/2026-11-27-pedido14-organizacao-e-entrega5.md) | Shift + J com 75 de Ki |
| 14.3 | Andar sobre a água funda com Shift (10 Ki/s) ou nadar devagar (só Planície) | idem | entre no riacho fundo |
| **T05** | **Machado e picareta; cortar pinheiro e salgueiro, quebrar rocha (quartzo, ferro), bambu cedendo, arbustos que se abrem** | idem, [T05](T05-recursos-arvores-pedras-bambus.md) | roteiro "Painel → Jogar" no relatório |
| **T06** | **Bolsa com abas, Fabricação com "tenho X de Y", comparação em golpes, 8 atalhos (1-6, Q, R) + H, ficha com "+" e prévia da arma, "Vida 60% · 6/10" ao passar o mouse, alvo do E com anel e lista (roda troca), rodapé contextual, ganhos sobre a cabeça** | [t06-bolsa-ficha-hud](../relatorios/2026-11-27-t06-bolsa-ficha-hud.md) | roteiro curto no relatório |
| **T07** | **Javali: aparo, investida e contra-ataque distintos; faixa da investida (Esc → Modo de assistência: completa/curta/desligada); barra "atual/máximo" junto ao animal; Lâmina atravessa o couro com metade do dano. Coelho: agacha antes de fugir, mergulha na toca. Pegadas frescas pelo caminho real (90 s); exame mais preciso com Rastreamento; javali no mapa só depois de visto ou rastreado; Disparo de Ki se desfaz no fim do alcance** | [t07-combate-fauna](../relatorios/2026-11-28-t07-combate-fauna.md) | roteiro curto no relatório |
| **T18** | **Equipamento trocável: espada, faca e sabre com números próprios; túnica de viajante e chapéu de bambu (defesa e peso no Vigor); trouxa de viajante sob o pinheiro; bolsa compara e marca o que está em uso; técnica Meia-Lua descoberta com o sabre; Inspeção troca as peças e gira nas 8 direções; `/create arma`, `roupa`, `capacete` jogáveis** | [t18-equipamento](../relatorios/2026-11-28-t18-equipamento.md) | Painel → Jogar → trouxa ao norte da clareira; Painel → Inspeção |
| — | Padrão de elementos, cena de Inspeção e comando `/create` | [como criar e editar](../relatorios/2026-11-27-como-criar-e-editar-elementos.md) | Painel → Inspeção; `/create arvore "…"` |

## Parcial (existe, mas falta algo): detalhes em [conciliacao-2026-11-27.md](conciliacao-2026-11-27.md)

| Assunto | O que já há | O que falta | Tarefa |
|---|---|---|---|
| Leitura na chuva | a fase `vento` do `FaunaCheck` mostra a faixa legível na rajada | repetir com chuva | T09 |
| Pássaros | bandos com sombra | aves que pousam e fogem de você | T08 |
| Clima | vento e rajadas | chuva, trovoada, raios | T09 |
| Rajada | capim, copa e folhas reagem | a ordem capim → copa → folhas não é encenada | T09 |

## Decisões abertas (dependem de você)

As da próxima tarefa ficam em destaque. Cada prompt pergunta as suas, uma por vez, antes de implementar.

- **T08 (próxima):** as decisões estão no arquivo da tarefa.
- **Da T18:** quando entram o Saque do Viajante e o Corte Fluido (técnicas futuras do sabre); se a regra dos chapéus (aba nivelada, ≤ 16 px, olhos à vista) entra na bíblia de arte; separar os itens comuns e os ícones em arquivos próprios (T17).
- **Pendências registradas no pedido 14 (item 7):**
  - **Tempos da cena final do Sênior:** pausa antes de reviver, duração da fala e da explosão. Hoje estão em `src/Vale/SeniorFinale.cs`. Ajustar depois de você jogar sem reposição de vida. Vai para a T08 (chefes).
- **T08, T09, T10–T16, T17:** a lista de cada uma está no arquivo da tarefa.

## Conferência manual (o que só você consegue ver no seu PC)

- [ ] **F5 e F11 fora do editor:** desmarque "Integrar Jogo na Próxima Execução" (relatório [recuperacao-prototipo](../relatorios/2026-11-26-recuperacao-prototipo.md) §11) e confira a tela cheia.
- [ ] **Aparência do arco da guarda** do discípulo na ponte.
- [ ] **Modo de assistência no seu monitor:** áreas e os números de dano novos (tamanho e leitura).
- [ ] **Dificuldade real do Sênior** numa luta sem reposição artificial de vida.
- [ ] **Brilho azul do Reforço e rastro maior do corte:** estão do jeito que você queria?
- [ ] **Andar sobre a água e nadar:** o custo de 10 Ki/s está bom?
- [ ] **T07 no seu monitor:** a faixa da investida (cor e tracejado), a barra junto ao javali, o efeito de onde o Ki se desfaz e o cone do exame da pegada estão legíveis?
- [ ] **Diagonal** (0,8 da velocidade em cada eixo): está natural agora?
- [ ] **T18 no seu monitor:** o chapéu de bambu, a túnica de viajante e o sabre no jogo; o custo de Vigor com a túnica (+10%) está bom?
- [ ] **Salgueiro e rocha de ferro**, criados pelo `/create`: aparência aprovada? Se não, diga o que mudar.

## Fichas visuais

Para elementos que precisam de aprovação visual antes do código (fauna nova, chefes, roupas), use **`/asset-spec`**. Ele gera a ficha e os prompts de imagem em `design/assets/specs/`. Ligue a ficha à tarefa (T08). As fichas da T18 estão em `design/assets/specs/t18-equipamento-assets.md`. As fichas de planejamento geradas pelo `/create` ficam em `user-docs/tasks/fichas/`.

## Histórico das tarefas

| Id | Fechada em | Commits |
|---|---|---|
| T05 + pedido 14 | 2026-11-27 | ver o relatório do pedido 14 |
| T06 | 2026-11-27 | ver o relatório da T06 |
| T07 | 2026-11-28 | ver o relatório da T07 |
| T18 | 2026-11-28 | ver o relatório da T18 |
