# Pedido 13 — fechamento: mira, técnicas, bolsa, moedas e ambientação — 2026-11-27

> O pedido está em [prompts/13-mira-tecnicas-bolsa-moedas.md](prompts/13-mira-tecnicas-bolsa-moedas.md). Cada entrega tem o seu relatório:
> - [mira pelo mouse](2026-11-26-mira-mouse.md)
> - [Disparo de Qi Concentrado](2026-11-26-disparo-concentrado.md) (secreto)
> - [Reforço de Qi](2026-11-26-reforco-qi.md)
> - [bolsa com figuras](2026-11-26-bolsa-visual.md)
> - [moedas](2026-11-26-moedas.md)
> - [ambientação](2026-11-27-ambientacao.md)

## Validação pelo próprio Painel (com janela)

O item 12 pediu o percurso com janela **a partir do PainelDeTestes.exe**. Rodei o Painel no modo automático, com janela no monitor 2, depois da última entrega.

| Rodada | Resultado | O que aconteceu |
|---|---|---|
| 1ª | 488/494 | 6 falhas: mira, Disparo carregado e controles. Repetidos um a um, todos passaram (a mira passou 12/12 duas vezes, o disparo 18/18, os controles 14/14). As falhas não se reproduzem e são compatíveis com mouse ou teclado reais chegando à janela de teste, que pega o foco no monitor 2. |
| 2ª | 493/494 | 1 falha: a carga parcial contra a Guarda Fechada não acertou o discípulo, que tinha se movido. O teste foi firmado: discípulo parado de guarda e personagem mais perto. Passou 3 de 3 depois. |
| **3ª** | **494/494** | **Todos passaram:** prólogo, recuperação, novidades (mira, disparo, reforço, bolsa, moedas, save antigo), técnicas seladas, tela e mapa, controles, Planície, jornada e ambientação de dia e de noite nas duas sementes. Duração: 6 min. |

**Evidência:** os relatórios gerados pelo Painel estão em `production/qa/evidence/2026-11-27_painel-final/`:
- `relatorio.html` e `relatorio.md`: a 3ª rodada;
- `rodada1-relatorio.md` e `rodada2-relatorio.md`: as rodadas anteriores.

O relatório que o Painel gerou fica na sua máquina, em `production/qa/execucoes/<data>/relatorio.html`.

**Saves e preferências:** os testes usam arquivos próprios (`user://teste_*.json`). A sua jornada e as suas teclas não são tocadas.

## Roteiro curto: PainelDeTestes.exe → Jogar

1. **Mira:** ande para a esquerda com A e aponte o mouse para a direita. O corte (J ou clique) e o Qi (K) vão para o cursor. Trave num inimigo com Espaço e o golpe vai para ele; Espaço de novo devolve ao cursor. Quem joga só com teclado troca em **Esc → Controles → "Mira: direção do personagem"**.
2. **Bolsa:** na Planície, pegue sílex e galhos na margem e aperte **I**. Os itens aparecem com figura e quantidade. Fabrique a faca (E ou clique), clique nela para equipar e use uma erva pela bolsa quando estiver ferido.
3. **Moedas:** na clareira onde você acorda, ande **dois passos à direita da esteira de meditação**. A bolsinha está ali: **E** dá +25. O saldo aparece no alto da bolsa (I). Medite, feche e continue: o saldo continua.
4. **Ambientação:** a hora passa sozinha (um dia = 8 minutos):
   - espere a noite perto da fogueira e veja a luz quente;
   - o relógio fica embaixo do minimapa;
   - ande pelos arbustos e o capim se abre;
   - nas rajadas voam folhas, e de dia passam pássaros com sombra;
   - entre no vau do riacho e veja as ondinhas e a lentidão.
5. **Técnicas:** existem duas novas para descobrir experimentando os botões de ataque. Se quiser descobrir sozinho, não leia a seção abaixo.

---

## Roteiro das técnicas (spoiler — leia só se quiser)

<details>
<summary>Disparo de Qi Concentrado (secreto)</summary>

1. Com Qi cheio, **segure K** (ou o seu botão de Qi) em vez de tocar. Depois de ~0,2 s a palma brilha, e a barra de Qi desce e mostra "palma N/100".
2. Mire com o mouse enquanto segura.
3. Continue segurando até **"palma 100 PRONTO"**. O disparo espera enquanto você segura.
4. **Solte**. A carga completa causa 6 de dano e **rompe a guarda**. Na ponte, a Guarda Fechada se abre: corte de frente logo depois.
5. Para ver a diferença, solte antes de 100: o dano é proporcional e o discípulo apara.
6. Abra a pausa ou o inventário durante a carga: o Qi volta para a barra e nada é disparado.
7. Depois da primeira carga, o Livro (B) ganha a página com o comando, o custo e a diferença entre carga parcial e ruptura.

</details>

<details>
<summary>Reforço de Qi</summary>

1. Com pelo menos 75 de Qi, **segure Shift e aperte J** (ou o clique), mantendo os dois por meio segundo. O Qi sobe pela lâmina.
2. Aparecem **três marcas sobre a cabeça**, e a linha de Qi diz "reforço 3". Os próximos 3 golpes corpo a corpo causam +10% de dano e têm 5% de chance de romper uma guarda.
3. Cada golpe gasta uma marca, mesmo errando.
4. Com 74 ou menos aparece "falta Qi". Tentar de novo com marcas restantes não gasta nada.
5. O modificador pode ser trocado em **Esc → Controles → "Modificador do Reforço de Qi"**.

</details>
