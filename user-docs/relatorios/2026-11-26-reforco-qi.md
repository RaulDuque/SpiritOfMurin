# Técnica: Reforço de Qi (pedido 13, entrega 3) — 2026-11-26

> O pedido está em [prompts/13-mira-tecnicas-bolsa-moedas.md](prompts/13-mira-tecnicas-bolsa-moedas.md), item 9. O comando detalhado fica no fim, numa seção de spoiler.

## Regras implementadas

- **O gesto:** segure o **modificador** (Shift por padrão) e aperte o **corte** (J, clique esquerdo ou a tecla configurada).
  - O Qi percorre a lâmina por 0,35 s, e então a técnica ativa.
  - Se você soltar antes, nada é gasto.
  - Durante o gesto o personagem não corta.
- **Custo e cargas:**
  - a ativação gasta **75 Qi uma única vez** e reforça os **próximos 3 golpes corpo a corpo**;
  - continuar segurando não ativa de novo;
  - com cargas restantes, uma nova tentativa avisa "ainda está ativo" e preserva as cargas, sem gastar nada.
- **Com menos de 75 Qi:** aparece "Reforço de Qi: falta Qi (74/75)." e o saldo fica como estava.
- **O que gasta uma carga:** cada golpe executado, **mesmo errando**. Cada corte de um combo conta como um golpe, e a estocada também.
- **Dano sem perder o bônus no arredondamento:** cada golpe reforçado causa **10% a mais de dano**, no cálculo real.
  - A vida dos animais e dos inimigos passou a aceitar frações. Assim o bônus não some.
  - Espada: 2 → **2,2**.
  - **Faca: 1 → 1,1.** Antes da mudança, 1,1 arredondado voltava a 1 e o bônus da faca era perdido. Agora a vida cai 1,1.
  - O teste mede as duas armas.
- **Ruptura (5%):**
  - cada golpe reforçado que encontra uma guarda ativa faz **uma única rolagem**: a Guarda Fechada no Vale, a defesa do javali no aviso ou na investida na Planície;
  - a rolagem acontece uma vez por golpe e por guarda, nunca a cada quadro de contato da lâmina. O teste conta 1 rolagem por golpe;
  - na ruptura, a guarda fica aberta por 1,0 s, com "guarda rompida".
- **O que você vê:**
  - o Qi subindo pela lâmina durante o gesto;
  - um brilho jade correndo na arma enquanto há cargas;
  - **três marcas sobre a cabeça (3, 2, 1)**;
  - "reforço N" na linha de Qi.
- **Shift sozinho** continua andando quieto, e os passos continuam funcionando, inclusive Shift + direção + L, o passo longo na Planície.
- **Esc → Controles:** a linha **"Modificador do Reforço de Qi (com o corte)"** permite trocar o modificador. Ele pode dividir a tecla com o andar quieto, de propósito, sem conflito.
- **Livro:**
  - na primeira ativação aparece a faixa "Reforço de Qi" e a página no Livro, com o comando atual, o custo, as cargas, o bônus e a chance;
  - meditar e continuar preservam a descoberta;
  - a técnica não é selada no despertar: ela não é do Vale.

**Valores em dados:** em `assets/data/combat_techniques.json`, o bloco `qi_reinforce`:
- `qi_cost` 75;
- `hold` 0,35 s;
- `charges` 3;
- `bonus` 0,10;
- `chance` 0,05;
- `stagger` 1,0 s.

## Testes (com janela no monitor 2)

| Teste | Resultado |
|---|---|
| `NovidadesCheck --phase=reinforce`:<br>- ativar com 75 (uma vez, 3 cargas, sem cortar no gesto);<br>- descoberta e Livro;<br>- HUD "reforço 3";<br>- nova tentativa preserva as cargas;<br>- 74 é recusado e mantido;<br>- três golpes de 2,2 e o quarto de 2;<br>- faca 1,1;<br>- golpe no vazio gasta carga;<br>- combo de 3 gasta 3;<br>- rolagem fixa 0,01 rompe a Guarda Fechada (2,2 de dano), com 1 rolagem por golpe;<br>- rolagem fixa 0,99 é aparada;<br>- Shift sozinho anda quieto e L dá passo;<br>- Ctrl + U remapeados;<br>- Livro com "Ctrl + segurar U";<br>- meditar guarda a descoberta | **17/17** |
| Regressões | todas passaram (tabela abaixo) |

| Regressão | Resultado |
|---|---|
| Unidade | 30/30 |
| Controles | 14/14 |
| Planície, sementes 20261126 · 777 | 44/44 · 44/44 |
| Jornada nova · retomada | 31/31 · 11/11 |
| Prólogo do Vale | 32/32 |
| Técnicas seladas | 19/19 |
| Novidades: mira · disparo · disparo na Planície | 12/12 · 18/18 · 4/4 |
| Recuperação: vale · controls · controls2 | 20/20 · 13/13 · 4/4 |

**Evidências:** `production/qa/evidence/2026-11-26_reforco-qi/`
- `07-reforco-gesto.png`
- `08-reforco-cargas.png`
- `09-reforco-ruptura.png`

O teste também entrou no Painel e no script.

---

## Spoiler: como fazer

<details>
<summary>Abrir o comando</summary>

Com pelo menos 75 de Qi, **segure Shift e aperte J** (ou o clique esquerdo) **mantendo os dois** por meio segundo. O Qi sobe pela lâmina, aparecem três marcas sobre a cabeça, e os próximos três golpes saem reforçados.

</details>
