# Bolsa com figuras (pedido 13, entrega 4) — 2026-11-26

> O pedido está em [prompts/13-mira-tecnicas-bolsa-moedas.md](prompts/13-mira-tecnicas-bolsa-moedas.md), item 10.

## O que mudou

O inventário em lista de texto virou uma **bolsa com figuras**, inspirada na mochila do protótipo HTML (função `drawBag`).

**Visual:**
- pergaminho com rolos;
- uma grade de espaços claros;
- o espaço selecionado com borda jade.

**Figuras:** cada item tem uma figura desenhada por código, para ser reconhecido sem ler o nome (`src/Game/ItemIcons.cs`):

| Item | Figura |
|---|---|
| Faca de Pedra | Lasca de sílex amarrada num cabo de madeira |
| Erva (Grama Sangue-de-Coelho) | Hastes com pontas vermelhas, mais vivas quanto melhor a colheita |
| Carne | Carne com gordura e osso; a assada fica dourada |
| Pele e couro | Peles esticadas; os couros são mais escuros |
| Sílex | Lasca cinza afiada |
| Galho | Galho bifurcado com folha |
| Outros | Presas, tendão, tira de amarração, bolsa simples, raiz e as moedas (próxima entrega) |

Os itens do Vale usam as figuras do protótipo (bálsamo, pedra de Qi, erva, faixa, prata, pingente, espada).

**Espaços:**
- **quantidade da pilha** no canto de cada espaço;
- **marca do equipamento em uso:** um canto jade com "✓", e "— em uso" no nome;
- **fabricar** numa segunda fileira, com a figura do resultado e um "+".

**Item selecionado:**
- nome, descrição curta e efeito;
- o **botão da ação possível agora**: **Equipar**, **Usar**, **Comer** (carne assada), **Fabricar**, ou "Na mão" para o que já está equipado;
- se não dá para fazer agora, o motivo em vermelho (por exemplo, "Falta: 1 Sílex" ou "Só junto à fogueira");
- a carne crua avisa para assar na fogueira, e os materiais dizem que servem para fabricar.

**Mouse e teclado:**
- **mouse:** passar seleciona; clicar no espaço faz a ação principal; o botão também;
- **teclado:** setas ou WASD navegam (cima e baixo por fileira, esquerda e direita um a um), E ou Enter agem, I ou Esc fecham.

**Jornadas salvas:** os itens, as quantidades, as receitas e o equipamento continuam sendo os mesmos dados. As jornadas antigas carregam como antes; os testes de retomada e migração passaram.

**Comparação:** `production/qa/evidence/2026-11-26_bolsa-visual/comparacao-inventario-antigo-x-bolsa.png` mostra a lista antiga ao lado da bolsa nova preenchida.

## Testes (com janela no monitor 2)

| Teste | Resultado |
|---|---|
| `NovidadesCheck --phase=bag` (Planície):<br>- I abre a bolsa;<br>- figura e pilha de cada item;<br>- a receita da faca mostra 2/1;<br>- fabricar pelo teclado diminui o sílex e o galho e a faca aparece na bolsa;<br>- um clique na faca equipa e marca "em uso";<br>- a erva pelo teclado e a outra pelo botão de ação;<br>- carne assada com "Comer", crua com "asse na fogueira";<br>- tela cheia cabe;<br>- I fecha | **11/11** |
| Regressões:<br>- clique em tela cheia;<br>- Planície, semente 777;<br>- jornada nova e retomada, semente 777;<br>- prólogo do Vale;<br>- técnicas seladas e migrações de saves antigos;<br>- Planície e jornada nova, semente 20261126 (rodadas sem janela);<br>- percurso só com teclado | **8/8 · 44/44 · 31/31 · 11/11 · 32/32 · 19/19 · 2/2 · 1/1 · 44/44 · 31/31 · OK** |

**Evidências:** `production/qa/evidence/2026-11-26_bolsa-visual/`
- `10-bolsa-preenchida.png`
- `11-bolsa-faca-fabricada.png`
- `12-bolsa-tela-cheia.png`
- `comparacao-inventario-antigo-x-bolsa.png`

## Como experimentar

PainelDeTestes.exe → **Jogar**:
1. Na Planície, pegue sílex e galhos na margem do riacho e aperte **I**.
2. Vá até "Fabricar" e faça a faca (E ou clique).
3. Clique na faca para equipar.
4. Colha uma erva, fique ferido e use a erva pela bolsa.
