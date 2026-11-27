# Técnica secreta: Disparo de Qi Concentrado (pedido 13, entrega 2) — 2026-11-26

> O pedido está em [prompts/13-mira-tecnicas-bolsa-moedas.md](prompts/13-mira-tecnicas-bolsa-moedas.md), item 8.
>
> **Aviso de spoiler:** o comando exato está só na última seção. As anteriores descrevem as regras sem dizer como fazer.

## Regras implementadas

- **O toque comum não mudou:** um toque rápido no botão do ataque à distância continua sendo o Disparo de Qi de sempre (30 Qi, dano 2).
- **Carga e mira:** existe uma forma de concentrar Qi na palma antes de disparar. Durante a concentração, o personagem fica parado e a mira acompanha o cursor (ou o alvo travado).
- **O que você vê:**
  - na mão, uma esfera jade que cresce, com faíscas puxadas para dentro;
  - na barra de Qi, o Qi sai da barra e aparece como um trecho claro depois dela, com o texto "palma N/100";
  - na carga completa, a esfera ganha anéis pulsantes e a barra diz "palma 100 PRONTO".
- **Custo e dano:**
  - o custo total da carga completa é **exatamente 100 Qi**, já incluindo o disparo: nada mais é cobrado ao disparar;
  - a regeneração de Qi pausa durante a carga;
  - o dano é proporcional ao Qi investido: `dano = arredondar(6 × investido ÷ 100)`, mínimo 1. Com 60 Qi, o dano é 4; com 100, é 6;
  - **só a carga completa rompe a guarda.** A guarda fica aberta por 1,4 s ("guarda rompida"), e nesse tempo um corte de frente entra;
  - **uma carga parcial é aparada** pela Guarda Fechada, como o disparo comum;
  - com menos de 100 Qi, a carga para no que houver e sai como carga parcial.
- **Projéteis:** cada gesto produz **no máximo um projétil**.
- **Cancelamento** (abrir o inventário, pausar ou o jogo perder o foco da janela durante a concentração):
  - **o Qi já investido volta inteiro para a barra**;
  - nenhum projétil sai, nem quando você solta o botão depois de voltar;
  - aparece a mensagem "A concentração se desfaz: N de Qi voltam à barra.";
  - como o Qi voltou, não há cobrança dupla, e o próximo gesto começa do zero.
- **Controles:** a técnica usa o mesmo botão de Qi que você configurar em Controles. O teste troca para O e confere.
- **Planície:**
  - junto a uma carcaça, o botão continua esfolando com Qi, sem carregar;
  - enquanto o Disparo de Qi está selado (antes do nível 3), a técnica também fica selada e só aparece a recusa;
  - no nível 3, ela volta **enfraquecida**: 100 Qi + 15 de vigor, dano 3, alcance 150 px, guarda aberta 0,8 s;
  - na Planície, a carga completa também rompe a investida do javali, que fica aberto.
- **Segredo de verdade:**
  - antes da descoberta, nem o Livro nem a lista de comandos falam da técnica;
  - **na primeira execução bem-sucedida** aparecem a faixa "Disparo de Qi Concentrado", uma mensagem e o som de aprendizado;
  - o Livro ganha a página com o comando (com a tecla atual), os custos e a diferença entre carga parcial e ruptura;
  - a descoberta é salva ao meditar e volta ao continuar. Se ela acontecer no Vale, a técnica é selada no despertar junto com as outras.
- **A Guarda Fechada da ponte continua jogável:**
  - a ruptura só acontece com a carga completa (100 Qi, quase toda a barra), então é merecida;
  - o recuo, a pista "uma trilha desce para o vale", o manual, o túmulo e a solução pelo flanco continuam iguais. O RecoveryCheck vale e vale2 e o prólogo inteiro passaram.

**Valores em dados:** em `assets/data/combat_techniques.json`, o bloco `qi_charge` tem uma versão plena (`full`) e uma enfraquecida (`weak`):
- `qi_cost`: custo total da carga completa;
- `hold`: tempo segurando até a carga começar (0,22 s);
- `drain`: Qi por segundo puxado para a palma (90 / 70);
- `damage`, `range`, `speed` e `stagger`.

## Testes (com janela no monitor 2)

| Teste | Resultado |
|---|---|
| `NovidadesCheck --phase=charge` (Vale): toque comum; carga parcial de 60 (dano 4, sem ruptura, sem custo extra); descoberta e página do Livro; carga completa que espera pronta; disparo da carga completa (dano 6, rompe, Qi 0); carga parcial aparada pela Guarda Fechada; carga completa que rompe a guarda e abre janela; corte de frente que entra na janela; sem Qi; pausa, inventário e perda de foco cancelam e devolvem o Qi; tecla remapeada O; página com custos e "parcial × ruptura"; meditar guarda a descoberta | **18/18** |
| `NovidadesCheck --phase=charge-plains`: selado na Planície; volta enfraquecido no nível 3 (dano 3, rompe); junto à carcaça o botão esfola | **4/4** |
| Regressões: prólogo do Vale, técnicas seladas (6 fases, com o Passo da Lâmina), RecoveryCheck vale e vale2, mira, unidade | **32/32 · 3/3, 19/19, 2/2, 1/1, 2/2, 1/1 · 20/20 · 3/3 · 12/12 · 30/30** |

**Evidências:** `production/qa/evidence/2026-11-26_disparo-concentrado/`
- `03-carga-parcial-palma.png`
- `04-carga-completa-pronta.png`
- `05-guarda-rompida.png`
- `06-livro-disparo-concentrado.png`

Os testes também entraram no `PainelDeTestes.exe` e no `testes-janela.ps1`.

---

## Spoiler: como fazer (leia só se quiser)

<details>
<summary>Abrir o comando da técnica secreta</summary>

1. **Segure** o botão de Qi (K, ou o que você configurou) em vez de tocar. Depois de ~0,2 s, a palma começa a brilhar e o Qi desce na barra.
2. Mire com o mouse enquanto segura.
3. Segure até a barra dizer **"palma 100 PRONTO"** (~1,1 s com a barra cheia). Pode continuar segurando: o disparo espera.
4. **Solte** para disparar. Na ponte, com a carga completa, a Guarda Fechada se rompe: corte de frente logo em seguida.

</details>
