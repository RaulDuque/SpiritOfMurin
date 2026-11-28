# GDD: Bolsa, fabricação, ficha, HUD e escolha de interação (T06)

*Criado: 2026-11-27 (T06 = entrega 6 da campanha) · Status: implementado · Decisões: `user-docs/tasks/T06-inventario-ficha-hud.md` ("Decisões tomadas"), tomadas sobre a prancha `user-docs/relatorios/2026-11-27-t06-prancha-bolsa.html`.*

## 1. Overview

A interface da Planície responde rápido a quatro perguntas:
- o que estou carregando, e o que está na mão;
- o que consigo fabricar, e o que falta;
- quanto o próximo ponto muda o personagem;
- o que o E vai fazer.

Para isso, a T06 traz:
- **bolsa** com abas por categoria e uma aba de **Fabricação**;
- **comparação** das ferramentas em golpes nas árvores e rochas de verdade;
- **8 atalhos de item**, além do H;
- **ficha** com botões "+" e prévia calculada com as fórmulas reais;
- **Vida completa** ao inspecionar;
- **escolha do alvo do E** com anel e lista;
- **rodapé contextual**;
- **ganhos agrupados** sobre a cabeça.

## 2. Player Fantasy

Jogar olhando para o personagem, não para menus. A bolsa diz em termos de jogo o que muda: "Pinheiro: 6 golpes → 3 golpes", e não "força +1". Os ganhos sobem da cabeça sem cobrir as pegadas, e o E nunca pega o objeto errado.

## 3. Detailed Rules

1. **Bolsa (I ou Tab).**
   - Abre sempre na aba **Tudo**. As abas são Tudo, Equipamentos, Ferramentas, Consumíveis, Materiais e Fabricação. A categoria vem do campo `category` de cada item em `plains_items.json`.
   - Troca de aba: Tab (Shift+Tab volta) ou clique.
   - As moedas ficam no alto. O item na mão tem ✓.
   - Ao selecionar algo que vai na mão, o detalhe compara com o que está na mão, em golpes, para cada elemento trabalhável que um dos dois trabalha. Depois vêm o alcance (se mudar) e o dano em animais, com FOR incluído.
2. **Fabricação.**
   - Cada receita lista os ingredientes como "✓/✗ Nome: tenho X de Y", a fogueira quando precisa, e a comparação do resultado com a ferramenta da mão.
   - Uma receita que acabou de ficar possível é **anunciada uma vez**, como mensagem importante.
   - Fabricar atualiza na mesma tela os materiais, o resultado e as ações.
3. **Atalhos de item.**
   - São 8: as ações `quick_1`…`quick_8`, nas teclas 1–6, Q e R (trocáveis em Esc → Controles).
   - **Na bolsa**, apertar um atalho com um item selecionado põe esse item no atalho (de novo: tira). Um item fica em um atalho só, e **nada é usado no mundo**.
   - **No mundo**, o atalho usa ou equipa o item. Com a pilha vazia ele diz "acabou"; sem item ligado, ele explica como ligar.
   - O que cada atalho guarda fica nas preferências (`quick_slots`) e vale depois de fechar e abrir o jogo.
   - O **H** continua sendo a erva e exige a Bolsa Simples. A barra de atalhos mostra o H, quando houver a Bolsa, e cada atalho em uso, com a quantidade (0 em vermelho, apagado).
4. **Ficha (C).**
   - Mostra os 7 atributos, cada um com "+" no fim da linha.
   - Passar o mouse ou W/S seleciona. Clicar no "+" ou E/Enter gasta 1 ponto. Sem pontos, os "+" ficam apagados e não fazem nada.
   - O quadro de prévia mostra a mudança do próximo ponto: `PlayerStats.Preview`, mais as linhas da T06. FOR mostra o dano da arma na mão, normal e crítico, com duas casas decimais. CON mostra quanto ainda reduz até o teto.
   - O cabeçalho mostra "Vida 60% · 6/10 · Vigor … · Ki …".
5. **Vida completa.** O HUD mostra só "Vida N%". Com o mouse sobre as barras aparece "Vida N% · atual/máxima".
6. **Alvo do E.**
   - Os alvos ao alcance (erva, pegada, carcaça, fogueira, esteira, pedras e galhos) são ordenados pela distância. Para as pegadas, conta só a mais próxima de cada trilha.
   - O alvo inicial é o mais perto. A **roda do mouse** (`target_next`/`target_prev`) passa pelos outros.
   - Uma escolha feita com a roda continua valendo enquanto aquele alvo estiver ao alcance.
   - Com 2 ou mais alvos aparecem um **anel no chão** sob o escolhido e uma **lista curta** ao lado do personagem. Com 1, nada além do aviso de sempre.
   - O E age só no escolhido. O K (esfolar com Ki) continua agindo na carcaça mais próxima.
7. **Rodapé contextual.** Mostra primeiro o que serve agora:
   - o aviso do alvo e "roda: trocar alvo";
   - "J cortar/quebrar" diante de um recurso com a ferramenta certa;
   - Shift na água funda;
   - Espaço com um animal perto, ou corte e passo em combate;
   - H quando há erva e falta Vida.

   Depois vêm o que o K faz e os painéis. A lista completa fica em Esc.
8. **Ganhos sobre a cabeça.** Materiais, moedas e XP sobem da cabeça. O mesmo ganho, dentro da janela `gain_seconds`, soma na mesma linha. As mensagens comuns e as importantes têm tempos próprios, em `plains_hud.json`.
9. **Os painéis consomem a entrada.** Abrir, confirmar e fechar a bolsa, a ficha ou a pausa nunca corta, nunca dispara Ki e nunca coleta.

## 4. Formulas

| Fórmula | Definição | Exemplo |
|---|---|---|
| Golpes de uma ferramenta | `ceil(toughness / power)` se `kind ∈ tools` do elemento; senão "não corta / não quebra" | Machado de Pedra (1) no pinheiro (6): 6; Lapidado (2): 3 |
| Dano em animais na comparação | `dano_base · (1 + 0,06·FOR)`, onde o dano base é o da ferramenta ou o da faca | FOR 1, machado: 1,06; FOR 2: 1,12 |
| Teto de CON na prévia | ainda reduz `teto − 0,03·CON` pontos percentuais, o que equivale a `ceil(… / 0,03)` pontos de CON | CON 2: ainda reduz 39 pontos (13 pontos de CON) |
| Vida completa | `"Vida " + ceil(atual/máx·100) + "% · " + atual + "/" + máx` | 6/10 → "Vida 60% · 6/10" |

## 5. Edge Cases

- **Item no atalho que acabou:** o atalho continua ligado e mostra 0. Recolher mais faz o atalho voltar a funcionar sozinho.
- **Atalho apertado com a bolsa aberta:** liga ou desliga o item no atalho e nunca usa nada.
- **Duas pegadas da mesma trilha ao alcance:** contam como um alvo só ("pegada").
- **O alvo escolhido sai do alcance:** o E volta ao mais próximo.
- **Ficha sem pontos:** os "+" ficam apagados e clicar não faz nada.
- **O clique no 7º atributo (KI)** gasta em KI. Corrige um limite antigo que mandava o ponto para SOR.
- **Tab** abre a bolsa no mundo e, dentro dela, troca a aba (as duas ações compartilham a tecla).
- **A roda do mouse** passou a ser uma entrada válida nas ligações, para os padrões não se apagarem. A tela de Controles não captura a roda ao trocar uma tecla.
- **Jornada antiga:** carrega com os seus itens e equipamento, sem atalhos ligados. O H só aparece com a Bolsa Simples.

## 6. Dependencies

- `world-resources.md` (T05): elementos, ferramentas e receitas que a comparação e a Fabricação mostram.
- `plains-progression.md`: atributos, `PlayerStats.Preview`, FOR, CON, crítico, Bolsa Simples e H.
- `src/Game/Bindings.cs`, porque os atalhos e a roda são ações trocáveis.
- Recíproca: a T07 usa a escolha do alvo (examinar pegada perto de carcaça).

## 7. Tuning Knobs

| Ajuste | Onde | Faixa segura | Efeito |
|---|---|---|---|
| `gain_seconds` | `assets/data/plains/plains_hud.json` | 1,5–4 | quanto tempo um ganho fica e soma |
| `toast_seconds` / `important_seconds` | idem | 2–5 / 4–10 | tempo de leitura das mensagens |
| `category` de cada item | `plains_items.json` | equipamento, ferramenta, consumivel, material | em que aba aparece |
| teclas dos atalhos e da roda | `tools/godot/setup_input.gd` (padrões) e Esc → Controles | — | quais teclas |
| `InteractRadius` | `PlainsGame` (export) | 20–34 px | o que entra na lista de alvos |

## 8. Acceptance Criteria

Verificados por `tests/integration/InventarioCheck.cs` (fases bolsa, atalhos, atalhos2, ficha, entrada, oldsave; alvo, rodape e percurso nas sementes 20261126 e 777) e por `tests/unit/Plains/BagRulesTests.cs`:

1. A bolsa abre em Tudo com os 9 itens e "Moedas: 25". Um clique em Ferramentas mostra só machado e picareta, e o Tab segue para Consumíveis e Materiais.
2. A faca, comparada com o machado na mão: "Pinheiro: 6 golpes → não corta" e "Touceira de bambu: 3 golpes → 3 golpes".
3. Fabricação: "Sílex: tenho 2 de 3" na picareta. O Machado Lapidado mostra o que falta e "Pinheiro: 6 golpes → 3 golpes". Com o último ingrediente, a receita fica possível. Fabricar atualiza a mesma tela, e o lapidado derruba um pinheiro em 3 golpes.
4. 1, 2 e 3 na bolsa ligam o machado, a picareta e a erva, sem usar nada. No mundo, 2 e 1 trocam a ferramenta e 3 usa a erva até acabar ("acabou", 0 na barra). Num processo novo, os atalhos continuam.
5. O mouse nas barras mostra "Vida 60% · 6/10". Na ficha, o mouse no FOR mostra "1,06 → 1,12", e o "+" gasta. S e E gastam em CON, mostrando "ainda reduz 39 pontos". Sem pontos, o "+" não faz nada.
6. Fora da clareira, uma erva e uma pegada ao alcance aparecem com anel e lista, mais perto primeiro. A roda troca, e o E age só no escolhido.
7. Três galhos seguidos: um "+3 Galho Rijo". Junto à carcaça, o rodapé diz "tirar…" e "K esfolar com Ki"; longe, não.
8. Bolsa, ficha e pausa: E, atalho, Esc e cliques ficam nos painéis, sem cortar, sem Ki e sem coletar.
9. Percurso contínuo nas duas sementes. A jornada antiga carrega.
