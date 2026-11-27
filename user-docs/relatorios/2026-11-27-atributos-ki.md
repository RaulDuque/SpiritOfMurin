# Ki, FOR, defesa, crítico e números de vida e de dano (pedido 14, entrega 14.0) — 2026-11-27

> O pedido está em [prompts/14-organizacao-criacao-conteudo.md](prompts/14-organizacao-criacao-conteudo.md). Esta entrega trata da sua mensagem curta, que veio antes do texto do pedido 14. O restante do pedido 14 vem nas entregas seguintes.

## O que você pediu e as decisões tomadas

| Pedido | Decisão (respondida por você ao começar) |
|---|---|
| Trocar Qi por Ki | No texto **e** no código. Jornadas e preferências antigas continuam valendo |
| Atributo FOR para aumentar o dano | +6% no dano **corpo a corpo** por ponto. Os disparos de Ki não usam FOR. Isso substitui a decisão anterior de "ficha sem FOR" |
| Constituição aumenta a defesa | Além do vigor, cada ponto tira 3% do dano recebido, com teto de 45% |
| Chance de crítico | 5% de base +1% por ponto de SOR, teto de 40%. O crítico causa ×1,5 |
| Vida do inimigo em números dentro da barra | "atual/máximo" dentro da barra, e os dois descem juntos |
| Modo de assistência mostra o dano causado e o recebido | Nova opção em Esc → Modo de assistência |

Todos os números ficam em `assets/data/plains/plains_progression.json` para ajustar depois de jogar. As chaves estão na seção "Onde mudar".

## 1. Qi agora é Ki

- **Na tela:** HUD ("Ki 100/100"), ficha ("KI Energia espiritual"), Livro ("Disparo de Ki", "Reforço de Ki", "Disparo de Ki Concentrado"), rodapé ("K disparo de Ki"), Controles, mensagens e o prólogo.
- **No código:** `KiWave.cs`, `KiPalm.cs`, `MaxKi`, `ki_wave`, `ki_charge`, `ki_reinforce`, a ação de entrada `ki`. Os GDDs, o art bible, o `GUIA.md` e a paleta acompanham. O `game-brief.md` e os históricos ficam como foram escritos. O `game-concept.md` ganhou uma nota: "Qi" nos documentos antigos é o mesmo Ki.
- **Sua jornada e suas teclas:** `src/Save/KiMigration.cs` lê os arquivos antigos e troca a grafia antiga pela nova. Isso vale para a energia, o atributo, os ids das técnicas, as páginas do Livro e a ação de controle. Conferi nos seus arquivos (só leitura): a sua jornada tem `qi_wave` e `qi_charge`, e as suas preferências ligam o Qi também ao **botão lateral do mouse (m:8)**. A verificação `oldsave` repete esse caso e confirma que o botão lateral continua disparando o Ki.
- **Evidência de save antigo:** guardei uma jornada real, gravada pelo jogo antes do renome, em `tests/fixtures/saves/jornada_antes_do_ki.json`. Ela também serve para as próximas entregas.

## 2. FOR, CON e crítico

| Atributo | Antes | Agora |
|---|---|---|
| **FOR** (novo, primeiro da ficha) | — | +6% de dano corpo a corpo por ponto |
| **CON** | +10 vigor, +10% recuperação do vigor | o mesmo, mais −3% de dano recebido (teto 45%) |
| **SOR** | +5 p.p. material secundário | o mesmo, mais +1 p.p. de crítico (base 5%, teto 40%) |

A ficha (C) mostra a prévia de cada um antes de gastar, por exemplo "Dano corpo a corpo: 130% → 136%", "Dano recebido: 100% → 97%" e "Chance de crítico (×1,5 de dano): 5% → 6%".

**Valores medidos no jogo** (`AtributosCheck`, fase `plains`):

| Situação | Resultado |
|---|---|
| Corte da faca (dano 1) com FOR 5 | tira **1,3** |
| O mesmo corte, crítico (rolagem fixa em 0) | tira **1,95** e mostra **"2! crítico"** |
| Golpe de 4 recebido com CON 5 | tira **3,4** e mostra **"-3,4"** |
| Jornada de antes do FOR | carrega com FOR 0 (nada muda) |

**No prólogo (Vale)** o personagem não tem ficha. Lá não há FOR, crítico nem defesa de CON, e a luta contra o Sênior continua exatamente como você testou.

## 3. Vida do inimigo em números

A barra do animal focado (Espaço, na Planície) e a do lutador travado (no Vale) ficaram um pouco mais altas e mostram **"atual/máximo"** dentro delas, por exemplo "6/6", "4,7/6" e "10/12". Quando o golpe entra, a barra e o número descem juntos. Dano fracionado (FOR, Reforço, crítico) aparece com uma casa decimal.

## 4. Números de dano no Modo de assistência

O caminho é **Esc → Modo de assistência → "Mostrar números de dano (causado e recebido)"**. A opção começa desligada e fica salva nas suas preferências.

- **Dano causado:** número claro subindo do alvo.
- **Crítico:** número maior e dourado, com "!". Na primeira vez aparece também a palavra "crítico".
- **Dano recebido:** vermelho, com sinal de menos, sobre o personagem.
- Golpes aparados ou desviados não mostram número, porque não tiraram Vida.
- **Áreas e números são independentes.** O menu de pausa resume o que está ligado, por exemplo "Modo de assistência (áreas e números de dano visíveis)".

## Evidências (imagens do próprio jogo)

Pasta: `production/qa/evidence/2026-11-27_atributos-ki/`.

| Arquivo | O que mostra |
|---|---|
| `01-ficha-com-for.png` | Ficha com 7 atributos, FOR primeiro, prévia "130% → 136%" |
| `02-barra-e-numero-de-dano.png` | Javali focado "4,7/6" e o número "1,3" do corte |
| `03-critico.png` | "2! crítico" dourado, barra em "2,8/6" |
| `04-dano-recebido.png` | "-3,4" em vermelho, acima da cabeça (subi o número e aumentei a letra depois da primeira captura, em que ele ficava sobre o cabelo) |
| `05-menu-assistencia.png` | A nova linha no Modo de assistência |
| `06-vale-barra-com-numeros.png` | Discípulo travado no Vale com "10/12" |

## Testes

- **Unidade:** 41/41. Há dois testes novos:
  - `DamageRulesTests`: FOR só no corpo a corpo, crítico pela rolagem, teto do crítico, CON até o teto, Reforço × FOR × crítico, formato "3,9".
  - `KiMigrationTests`: jornada antiga carrega com Ki, KI e o Livro renomeados; a migração é idempotente. **Vi esse teste falhar sem a migração** ("expected 85, got 0") e passar com ela.
- **Integração nova `AtributosCheck`**, nas três fases:
  - `plains` 16/16;
  - `vale` 4/4;
  - `oldsave` 8/8.
- Já está no `tools/godot/testes-janela.ps1` e na lista do Painel (`tools/painel-testes/Suites.cs`, grupo "Atributos e Ki (pedido 14)").
- **Bateria completa com janela no monitor 2:** rodou uma vez no fim do pedido 14, como você pediu ("finalizar e só depois testar"): 556/556 depois de corrigir uma contagem de teste. Detalhes no [relatório final do pedido 14](2026-11-27-pedido14-organizacao-e-entrega5.md#9-testes).

## Como testar pelo Painel → Jogar

1. Continue a sua jornada. O HUD e o Livro já dizem Ki, e o K (e o seu botão lateral) continuam disparando.
2. Ao subir de nível, abra a ficha (C). **FOR** está em primeiro. Escolha com W/S e veja a prévia antes de gastar com E.
3. Foque um javali com Espaço: a barra mostra "6/6" (ou o máximo dele).
4. Esc → Modo de assistência → ligue "Mostrar números de dano". Cada corte que entra mostra o número, e um golpe do javali mostra "-N" sobre você.
5. Com SOR alto os críticos aparecem mais vezes (5% de base).

## Onde mudar

| Quero mudar… | Arquivo | Trecho |
|---|---|---|
| quanto cada FOR soma | `assets/data/plains/plains_progression.json` | `per_point.for_melee_damage` |
| defesa de CON e o teto | o mesmo | `per_point.con_damage_reduction`, `per_point.con_reduction_cap` |
| chance e força do crítico | o mesmo | `base.crit_chance`, `per_point.sor_crit_chance`, `per_point.crit_cap`, `base.crit_multiplier` |
| a conta do dano | `src/Combat/DamageRules.cs` | `Outgoing`, `Incoming` |
| cor, tamanho e duração dos números de dano | `src/Game/CombatText.cs` | constantes no topo da classe |
| a barra com números | `src/Plains/PlainsHud.cs` | `DrawFocusBar`, `FocusBarW`, `FocusBarH` |
| as fórmulas e os critérios | `design/gdd/plains-progression.md` | seção 3.2, seção 4 e critério 8a |

## Pendências

- **Balanceamento:** os valores são a proposta padrão e precisam ser confirmados jogando. O dano base é pequeno (faca 1, espada 2), então FOR aparece em décimos.
- **PainelDeTestes.exe:** a lista nova está no código-fonte do Painel. A troca do `.exe` da raiz faz parte do item 7 do pedido 14 e entra no fechamento dele.
