# GDD: Planície Verdejante — primeira progressão

*Criado: 2026-11-26 · Status: Implementado (primeira passada, números provisórios)*
*Fontes: `user-docs/futuro-do game/gpt.md` (atributos, graus de Esfolar, Rastreamento, Erboristeria, Preparo de Caça, faixas de nível das criaturas) e `design/gdd/plains-first-stretch.md` (o trecho que esta progressão amplia)*
*Pilares: 1 "Aprender é a recompensa", 3 "O mundo se lembra, e você também", 4 "Maestria nas mãos"*

## 1. Overview

A primeira progressão põe, por cima do trecho jogável da Planície:
- **nível e XP do personagem**, com pontos de atributo para seis atributos: VIT, INT, CON, AGI, SOR e QI;
- **vigor e Qi** como recursos visíveis no HUD;
- **quatro proficiências que sobem pelo uso:** Esfolar, Preparo de Caça, Erboristeria e Rastreamento;
- **níveis das criaturas**, revelados ao focar o animal;
- **trilhas de pegadas** que levam a animais reais;
- **os primeiros usos dos materiais:** assar carne, tiras de amarração e a Bolsa Simples;
- **Meditar e Continuar no Godot:** salvamento local, tela de título com Continuar e Novo jogo.

## 2. Player Fantasy

"Cada coisa que faço me ensina." Na primeira caça, a pele sai rasgada. Na segunda, a mão já sabe o caminho e a pele sai inteira. Com Qi, a lâmina guia a pele um grau acima.

O jogador lê uma pegada, segue a direção e encontra a toca. Volta à fogueira, assa a carne e recupera o fôlego. Sobe de nível e decide, vendo o número mudar antes de confirmar, se quer mais Vida, mais fôlego ou mais sorte. Medita, fecha o jogo e, ao voltar, a Planície é a mesma, com o que ele conquistou.

## 3. Detailed Rules

### 3.1 Nível e XP
- O personagem começa no nível 1. Para ir do nível L ao L+1 são precisos `xp_base · xp_growth^(L−1)` XP: 20, 30, 45, 68…
- Cada nível dá `points_per_level` (2) pontos de atributo.
- **De onde vem o XP:**
  - caça: `hunt_per_level[espécie] × nível da criatura × multiplicador de diferença de nível`;
  - primeira fabricação de cada receita: 5;
  - primeira vez que examina cada trilha: 3;
  - cada planta nova: 1;
  - cada carne assada: 2;
  - cada prática nova de proficiência: 1.
- **O HUD mostra** "Nv N · XP x/y (faltam z)" e, havendo pontos, "· N pontos (C)".

### 3.2 Atributos
Os valores de base são os do personagem de nível 1 atual: Vida 10, velocidade 105 px/s, passo de 34 px.

| Atributo | Efeito de 1 ponto |
|---|---|
| VIT | +2 de Vida máxima. A Vida atual sobe junto. O HUD continua com "Vida N%" e a cura pendente em verde |
| INT | +12% na potência dos preparos medicinais (Grama Sangue-de-Coelho e Raiz Amarga) |
| CON | +10 de vigor máximo e +10% na recuperação do vigor |
| AGI | +4% de velocidade ao andar, +5% de distância do passo e −5% no tempo de recuperação do corte e do passo. Cada bônus tem teto de 40% |
| SOR | +5 pontos percentuais na chance de material secundário. Base 10%: tendão na caça, uma planta ou uma lasca a mais na coleta |
| QI | +15 de Qi máximo (base 100) |

- **Ficha (C):** W/S escolhe o atributo. Antes de gastar, a ficha mostra a mudança concreta, por exemplo "Vida máxima: 10 → 12". E gasta 1 ponto.
- A prévia é calculada aplicando o ponto a uma cópia da ficha. Por isso nunca diverge do efeito real.

### 3.3 Vigor e Qi
- **Vigor** (barra ocre): o passo gasta 12, o corte gasta 3. Sem vigor, a ação não sai e aparece "Sem fôlego".
  - Volta sozinho, a 14/s, depois de 0,6 s sem gastar.
  - Carne assada devolve vigor na hora.
- **Qi** (barra verde-água, no estilo do protótipo): volta a 0,5/s, e meditar enche.
  - **Uso nesta entrega:** K numa carcaça esfola com Qi. Com Esfolar Iniciante ou acima e 15 de Qi, o material sai um grau acima.
  - Com Esfolar Mestre, K nas presas do javali faz a **extração espiritual** (Presa Espiritual).

### 3.4 Proficiências
- Cada proficiência sobe por prática: Esfolar, Preparo de Caça e Erboristeria em 1/4/9 práticas para os graus 1/2/3; Rastreamento em 1/3/6.
- Cada prática é registrada pela coisa em que foi feita: a carcaça, a planta, a trilha. Reabrir a mesma carcaça ou reexaminar a mesma pegada não dá prática nem XP.
- A ficha mostra o grau, o progresso ("3/4 para Adepto") e o benefício atual.

| Grau | Esfolar | Preparo de Caça | Erboristeria | Rastreamento |
|---|---|---|---|---|
| 0 Bruto | Pele Rasgada / Couro Rasgado (só para tiras) | 1 carne; a carne assada recupera o vigor base | erva amassada (cura 3) | animal e direção |
| 1 Iniciante | Pele Comum / Couro Comum; com Qi, um grau acima | +25% de vigor ao assar | cortada no talo (cura 4) | distância; as 3 próximas pegadas brilham |
| 2 Adepto | Pele Fina / Couro Grosso (intactos) | 2 carnes; +50% | bem colhida (cura 5) | revela o nível do animal |
| 3 Mestre | extração espiritual com Qi (Presa Espiritual) | 2 carnes; +75% | seiva viva (cura 6, mais rápido) | a trilha inteira à frente fica brilhando |

- **Carcaças:** cada parte é controlada separadamente. Coelho: pele e carne. Javali: couro, Carne Rija e presas (o que o brief prevê). Uma carcaça usada pela metade volta só com o que falta.

### 3.5 Rastreamento: trilhas
- O coelho e o javali garantidos do início sempre deixam uma trilha que começa dentro da clareira. Os outros deixam trilha com `trail_chance` (60%), de 4 a 7 tiles.
- As pegadas são espaçadas a 22 px e apontam para o animal. Pata para o coelho, casco para o javali.
- E numa pegada examina:

  > "Pegadas pequenas, de coelho. Seguem para o noroeste."
  - com Iniciante, a distância;
  - com Adepto, o nível do animal.

### 3.6 Níveis das criaturas
- **Coelho:** níveis 1–2. **Javali:** níveis 3–5. O nível é fixo pela semente e pelo id do animal.
- **Efeito mensurável por nível acima do mínimo:**
  - coelho: +12% de audição e +8% de velocidade de corrida;
  - javali: +1 acerto de vida, +6% de velocidade de investida e +0,5 de dano (arredondado para baixo).
- **XP:** a caça vale `× clamp(1 + 0,15 × (nível da criatura − nível do personagem), 0,4, 1,6)`.
- **O que o jogador aprende não muda com o nível:** o congelamento do coelho antes de fugir, o aviso de 0,8 s e a recomposição de 1 s do javali.
- **Espaço** foca o animal mais próximo a até 170 px: "Javali da Mata · Nv 4 · Territorial", com a barra de vida dele.

### 3.7 Materiais
- **Fogueira** (leste da clareira): E assa uma carne crua. As receitas "na fogueira" no inventário só ficam ativas perto dela.
  - Carne Macia Assada: +25 de vigor. Carne Rija Assada: +40.
  - Somam +25% de vigor por grau de Preparo.
- **Receitas:**
  - Tira de Amarração ×2: 1 Pele Rasgada (ou ×3 de 1 Couro Rasgado);
  - Bolsa Simples: 1 Pele Comum + 2 Tiras (ou 1 Couro Comum + 1 Tira).
- **Bolsa Simples:** com ela, **H** usa a melhor erva sem abrir o inventário.
- O inventário mostra os ingredientes, o resultado e o efeito de cada receita. A fabricação consome a quantidade certa, uma vez.

### 3.8 Meditar e Continuar
- **Esteira de palha** (nordeste da clareira): E medita.
  - Enche o Qi e o vigor.
  - Marca o ponto onde se desperta depois de desmaiar ou de usar Continuar.
  - Grava a jornada em `user://jornada_planicie.json` (formato 1) e a lê de volta. Só confirma se a leitura bater.
  - Não medita com um javali avisando ou investindo por perto.
- **O que a gravação guarda:**
  - semente e ponto de meditação;
  - objetos coletados;
  - animais mortos, com a posição da carcaça, e partes tiradas;
  - trilhas examinadas;
  - inventário em ordem e equipamento;
  - Vida, vigor e Qi;
  - nível, XP, pontos e atributos;
  - prática e chaves de prática;
  - receitas já feitas.
- **Título:**
  - **Continuar** aparece em destaque, com a data, o nível e a semente.
  - **Novo jogo** pede confirmação: "Não, manter a jornada" vem marcado.
  - Uma gravação danificada mostra um aviso, e o jogo começa normalmente.

## 4. Formulas

| Fórmula | Definição | Exemplo |
|---|---|---|
| XP para o próximo nível | `round(20 · 1,5^(L−1))` | L1→2: 20; L2→3: 30; L3→4: 45 |
| XP da caça | `round(base_espécie · nível · clamp(1 + 0,15·(nc − np), 0,4, 1,6))` | Coelho Nv 1, personagem Nv 1: 6 · 1 · 1 = 6. Javali Nv 4, personagem Nv 1: 9 · 4 · 1,45 = 52 |
| Vida máxima | `10 + 2·VIT` | VIT 1 → 12 |
| Potência medicinal | `1 + 0,12·INT` | INT 1: erva comum 4 → 4,48 |
| Vigor máximo / recuperação | `50 + 10·CON` / `14·(1 + 0,1·CON)` por s | CON 1 → 60 e 15,4/s |
| AGI | `1 + min(0,4, 0,04·AGI)` velocidade; `1 + min(0,4, 0,05·AGI)` passo; `1 − min(0,4, 0,05·AGI)` recuperação | AGI 1 → 109,2 px/s |
| Chance secundária | `min(0,9, 0,10 + 0,05·SOR)`; o sorteio é fixo por (semente, id) | SOR 1 → 15%. Mais SOR só acrescenta objetos que dão o secundário |
| Qi máximo | `100 + 15·QI` | QI 1 → 115 |
| Grau de proficiência | número de limiares atingidos pela prática | Esfolar com 1 prática: Iniciante |
| Vigor da carne assada | `vigor_do_item · (1 + 0,25·Preparo)` | Carne Macia com Preparo 1: 31 |
| Nível da criatura | `min + floor(sorteio(semente, "lvl:"+id) · (max − min + 1))` | javalis da semente 20261126: Nv 3–5 |

## 5. Edge Cases

- **Reabrir uma carcaça vazia ou reexaminar uma trilha:** nada acontece (sem item, prática ou XP), só o texto da pegada.
- **K com Esfolar Bruto:** a pele sai rasgada e aparece o aviso de que ainda não sabe guiar o Qi. K sem Qi suficiente: material normal, sem gasto.
- **K nas presas abaixo de Mestre:** presas comuns e aviso.
- **Carcaça usada pela metade e gravada:** ao continuar, volta só com a parte que falta.
- **Animal morto depois da gravação:** fechar sem meditar perde esse progresso. O jogo retoma a última meditação.
- **Usar erva com a Vida cheia ou carne com o vigor cheio:** recusado.
- **Gastar ponto sem pontos:** recusado.
- **VIT com a Vida pela metade:** a Vida sobe junto com o máximo (+2).
- **Desmaio:** acorda na esteira, ou no despertar se ainda não meditou, com Vida e vigor cheios.
- **Gravação danificada:** aviso no título e jogo novo.
- **Novo jogo com jornada salva:** só apaga depois de "Sim, começar de novo".
- **Fogueira longe:** as receitas de assar aparecem desativadas no inventário, com "na fogueira".
- **H sem bolsa:** explica que é preciso fabricar a Bolsa Simples. H sem ervas: "a bolsa está sem ervas".

## 6. Dependencies

- **Depende de:**
  - `plains-first-stretch.md` (mundo por semente, faca, coelho, javali, ervas);
  - `src/Player` (vigor, multiplicadores, Vida máxima ajustável);
  - mapa de entradas (`sheet` C, `heal` H; `qi` K e `lock` Espaço já existiam).
- **Usado por (próximas entregas):** saída para o Vale, espada no posto de vigia, outras espécies, Coelho Lunar (extração espiritual compatível), fogueira e meditação no Vale.
- **Retrocompatibilidade:** o `plains-first-stretch.md` passa a depender deste documento para coleta, carcaças e HUD.

## 7. Tuning Knobs

Tudo está em `assets/data/plains/plains_progression.json`, com o esquema no bloco `_doc`. Itens e receitas ficam em `plains_items.json`; as trilhas, em `plains_worldgen.json` (`trail_*`).

| Ajuste | Faixa segura | O que muda |
|---|---|---|
| `xp_base` | 15–30 | Ritmo do primeiro nível. Com 20, o percurso normal (faca, pista, coelho, carne assada, tiras) chega ao nível 2 |
| `xp_growth` | 1,3–1,7 | Ritmo dos níveis seguintes |
| `per_point.*` | metade a dobro dos valores atuais | Força de cada atributo. AGI tem teto em `agi_cap` |
| `base.step_vigor_cost` | 8–18 | Com 50 de vigor, cerca de 4 passos seguidos |
| `base.qi_preserve_cost` | 10–30 | Quantas esfolas com Qi por meditação |
| `skills.*.thresholds` | crescentes | Quantas práticas por grau |
| `creatures.*` | — | Faixas de nível e força por nível. Não mexem nos tempos de aviso |

## 8. Acceptance Criteria

1. Jornada nova: nível 1, mãos vazias, Vida, vigor e Qi cheios, e o HUD mostra quanto falta para o próximo nível.
2. A primeira pegada da clareira diz o animal e a direção, e dá Rastreamento e XP uma vez. Seguindo as pegadas a pé, chega-se à toca.
3. Espaço mostra o nome, o nível e o temperamento do animal.
4. Esfolar 0 dá Pele Rasgada e sobe para 1. A caça seguinte dá Pele Comum. K com Qi dá Pele Fina e gasta 15 de Qi.
5. O javali dá couro (pelo grau de Esfolar), Carne Rija e presas, cada um uma vez.
6. A carne assada na fogueira recupera o vigor gasto nos passos. As tiras e a bolsa consomem os materiais uma vez. H usa a erva da bolsa.
7. O primeiro nível dá 2 pontos. A ficha mostra a mudança antes de gastar, e cada atributo muda o número prometido; AGI é medida na velocidade real.
8. Meditar grava. Depois de fechar e abrir: Continuar no título restaura a semente, o lugar, o inventário, o nível, os pontos, os atributos, as proficiências, a Vida, o Qi e o vigor. Carcaças usadas não voltam, coletas não voltam e gravar de novo não multiplica nada.
