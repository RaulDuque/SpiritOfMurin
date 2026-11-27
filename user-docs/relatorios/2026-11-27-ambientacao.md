# Ambientação da Planície (pedido 13, entrega 6) — 2026-11-27

> O pedido está em [prompts/13-mira-tecnicas-bolsa-moedas.md](prompts/13-mira-tecnicas-bolsa-moedas.md). Os itens 1 a 6 ("a ambientação descrita acima") não vieram no texto. Por escolha do criador, a ambientação implementada é a **entrega 4 da campanha** (prompt 10).

## O que mudou

**Dia e noite**
- **A hora passa:** um dia inteiro dura 8 minutos reais. Jornada nova e despertar começam às 8h.
- **Cor do mundo:** muda ao longo do dia:
  - madrugada azulada;
  - amanhecer rosado;
  - dia limpo;
  - entardecer alaranjado;
  - noite azulada.
- **A noite continua legível:** a **fogueira acende uma luz quente que tremula**, e há uma **luz fraca em volta do personagem** para ler o combate.
- **O HUD (o painel na tela) não é tingido.**
- **Relógio:** fica embaixo do minimapa, com um pequeno sol ou lua num arco, a parte do dia e a hora ("Noite · 23:00").
- **Meditar salva a hora,** e Continuar volta nela. Jornadas antigas começam às 8h.

**Vento**
- **Rajadas:** uma brisa constante e rajadas que **correm pelo chão como uma onda**, então o capim se curva em sequência, não tudo de uma vez.
- **Capim alto:** os tufos ficam nos campos de arbusto e espalhados na relva. Eles **se curvam com o vento e se abrem quando o personagem passa**.
- **Copas e arbustos** balançam com o vento.
- **Folhas** soltas atravessam a tela nas rajadas.

**Pássaros**
- Bandos de 3 a 6 cruzam o céu de dia, batendo as asas. Cada um tem **a sombra no chão**, que mostra onde ele realmente está.
- À noite não há pássaros.

**Terreno**
- **Variedade:**
  - pinheiros de 3 a 5 andares, em três tons de verde, às vezes com um pinheiro menor ao lado;
  - pedras redondas, lajes achatadas ou uma pedra com pedrinhas em volta;
  - arbustos com frutinhas ou flores brancas.
- **Transições suaves:** entre relva, arbustos, lamaçal, areia da margem, vau e água funda, o terreno de cima avança sobre o vizinho em uma franja pontilhada de 6 px, em vez de um corte reto de 32 px.
  - O vizinho é lido pelo gerador da semente, então **a franja é a mesma dos dois lados da divisa entre blocos**.
- **Água rasa (vau):**
  - o personagem anda a **60% da velocidade**;
  - deixa **ondinhas** a cada passo;
  - continua clara e com pedras.
- **Água funda:** ficou **mais escura e com marcas longas de onda**, e continua bloqueando a passagem.

**Camadas:** os objetos continuam ordenados pelos pés. O capim fica na camada do chão; folhas e pássaros, por cima.

**Valores em dados:** `assets/data/plains/plains_ambient.json`
- duração do dia e hora inicial;
- paleta por hora;
- luzes da noite;
- vento;
- folhas;
- pássaros;
- velocidade e ondinhas na água rasa;
- densidade do capim e raio em que ele se abre.

## Testes (com janela no monitor 2, nas duas sementes)

| Teste | 20261126 | 777 |
|---|---|---|
| `AmbientCheck --phase=day`:<br>- meio-dia sem tinta e relógio no HUD;<br>- a hora corre;<br>- calmarias e rajadas;<br>- capim em cada bloco;<br>- a rajada curva mais que a calmaria;<br>- folhas na rajada;<br>- o capim se abre em volta do personagem;<br>- bando de pássaros com sombra;<br>- vau a 0,6× da velocidade, com ondinhas;<br>- água funda bloqueia;<br>- franjas de transição;<br>- pinheiros e pedras de formas diferentes;<br>- ordenação das camadas | **15/15** | **15/15** |
| `--phase=night`:<br>- noite azulada com "Noite · 23:00";<br>- luz da fogueira;<br>- luz do personagem;<br>- legível perto e mais escuro longe;<br>- entardecer quente;<br>- meditar salva a hora | **6/6** | **6/6** |
| `--phase=resume`: Continuar traz a hora | **1/1** | **1/1** |

**Regressões com janela:**
- unidade 33/33;
- Planície 44/44 nas duas sementes;
- jornada nova 31/31 e retomada 11/11 nas duas sementes;
- controles 14/14;
- moedas 8/8, 1/1 e 4/4.

**Evidências:** `production/qa/evidence/2026-11-27_ambientacao/`

| Imagens | Momento |
|---|---|
| `dia-*.png` · `noite-*.png` · `entardecer-*.png` | dia, noite e entardecer, nas duas sementes |
| `capim-*.png` | o capim se abrindo |
| `passaros-*.png` | o bando de pássaros |
| `agua-*.png` | a água |

O teste também entrou no `PainelDeTestes.exe` (grupo "Ambientação", por semente) e no script.

## Como experimentar

PainelDeTestes.exe → **Jogar**. A hora passa sozinha, então um dia leva 8 minutos:
- espere a noite perto da fogueira da clareira;
- caminhe pelos arbustos para ver o capim abrir;
- entre no vau do riacho e veja as ondinhas e a lentidão.
