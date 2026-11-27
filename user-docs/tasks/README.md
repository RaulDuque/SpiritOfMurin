# Tarefas do SpiritOfMurin — ponto de retomada

*Atualizado em 2026-11-27, no fim do pedido 14. Leia isto primeiro ao voltar ao projeto, de qualquer sessão ou ferramenta.*

**Em 1 minuto:**
- **Em andamento:** nenhuma. O pedido 14 terminou.
- **Próximo prompt a mandar:** o da **[T06](T06-inventario-ficha-hud.md)** (bolsa, ficha e HUD), na seção "Prompt pronto para executar". Copie e cole numa sessão nova.
- **O que você pode testar agora:** a seção "Pronto" abaixo. Tudo abre pelo **PainelDeTestes.exe → Jogar** (ou **Inspeção** para ver os elementos).
- **O que depende de você:** a seção "Decisões abertas".

Cada tarefa tem um id estável (T01, T02…). As que têm trabalho suficiente ganham um arquivo próprio, com objetivo, estado, o que falta, dependências, decisões, critérios, roteiro de teste e um prompt pronto. As decisões já tomadas ficam nos documentos citados, sem cópia aqui: `design/gdd/`, `docs/elementos/` e os relatórios.

## Ordem das próximas entregas

| # | Tarefa | Por quê nessa ordem |
|---|---|---|
| 1 | **[T06](T06-inventario-ficha-hud.md) — bolsa, ficha, HUD, escolha de interação** | Com machado, picareta e 15 materiais novos, a bolsa precisa de categorias e da aba de fabricação |
| 2 | **[T07](T07-combate-rastreamento-fauna.md) — combate, rastreamento, javali e coelho** | Completa a Entrega 7 com os animais que já existem |
| 3 | **[T18](T18-equipamento-visual.md) — armas, roupas, capacetes** | Torna jogáveis os tipos `arma`, `roupa` e `capacete` do `/create` |
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
| — | Padrão de elementos, cena de Inspeção e comando `/create` | [como criar e editar](../relatorios/2026-11-27-como-criar-e-editar-elementos.md) | Painel → Inspeção; `/create arvore "…"` |

## Parcial (existe, mas falta algo): detalhes em [conciliacao-2026-11-27.md](conciliacao-2026-11-27.md)

| Assunto | O que já há | O que falta | Tarefa |
|---|---|---|---|
| Bolsa e ficha | figuras, pilhas, fabricar, FOR | categorias, aba, comparar, atalhos, "+", "Vida 50% · 6/12", rodapé contextual, escolha do alvo do E | T06 |
| Javali | aviso (casco, cabeça, olho, poeira), contra-ataque, barra com números | sons na Planície, faixa da investida (a opção existe e não é usada), barra junto ao animal | T07 |
| Coelho e pegadas | orelha erguida, fuga para a toca, Shift funciona | entrada na toca visível, sinal de ruído, pegadas deixadas pelo movimento, precisão do Rastreamento | T07 |
| Pássaros | bandos com sombra | aves que pousam e fogem de você | T08 |
| Clima | vento e rajadas | chuva, trovoada, raios | T09 |
| Rajada | capim, copa e folhas reagem | a ordem capim → copa → folhas não é encenada | T09 |

## Decisões abertas (dependem de você)

As da próxima tarefa ficam em destaque. Cada prompt pergunta as suas, uma por vez, antes de implementar.

- **T06 (próxima):**
  - quantos atalhos de item e em que teclas;
  - como trocar o alvo do E;
  - onde mostrar "Vida 50% · 6/12";
  - H exige a Bolsa Simples;
  - quais categorias.
- **Pendências registradas no pedido 14 (item 7):**
  - **Lâmina enfraquecida contra o javali:** atravessa o couro na investida? (T07, decisão 1)
  - **Animais no mapa:** os javalis vivos aparecem no mapa dos blocos visitados, como hoje (o 14.2 os desenha com ícone), só depois de examinar a trilha, ou nunca? (T07, decisão 5)
  - **Tempos da cena final do Sênior:** pausa antes de reviver, duração da fala e da explosão. Hoje estão em `src/Vale/SeniorFinale.cs`. Ajustar depois de você jogar sem reposição de vida. Vai para a T08 (chefes).
- **T08, T09, T10–T16, T17, T18:** a lista de cada uma está no arquivo da tarefa.

## Conferência manual (o que só você consegue ver no seu PC)

- [ ] **F5 e F11 fora do editor:** desmarque "Integrar Jogo na Próxima Execução" (relatório [recuperacao-prototipo](../relatorios/2026-11-26-recuperacao-prototipo.md) §11) e confira a tela cheia.
- [ ] **Aparência do arco da guarda** do discípulo na ponte.
- [ ] **Modo de assistência no seu monitor:** áreas e os números de dano novos (tamanho e leitura).
- [ ] **Dificuldade real do Sênior** numa luta sem reposição artificial de vida.
- [ ] **Brilho azul do Reforço e rastro maior do corte:** estão do jeito que você queria?
- [ ] **Andar sobre a água e nadar:** o custo de 10 Ki/s está bom?
- [ ] **Salgueiro e rocha de ferro**, criados pelo `/create`: aparência aprovada? Se não, diga o que mudar.

## Fichas visuais

Para elementos que precisam de aprovação visual antes do código (fauna nova, chefes, roupas), use **`/asset-spec`**. Ele gera a ficha e os prompts de imagem em `design/assets/specs/`. Ligue a ficha à tarefa (T08, T18). As fichas de planejamento geradas pelo `/create` ficam em `user-docs/tasks/fichas/`.

## Histórico das tarefas

| Id | Fechada em | Commits |
|---|---|---|
| T05 + pedido 14 | 2026-11-27 | ver o relatório do pedido 14 |
