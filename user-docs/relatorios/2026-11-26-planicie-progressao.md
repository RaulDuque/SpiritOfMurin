# Planície Verdejante: primeira progressão — 2026-11-26

> O pedido está em [prompts/09-planicie-progressao.md](prompts/09-planicie-progressao.md). O design, com regras, fórmulas e faixas seguras, está em `design/gdd/plains-progression.md`. Este trabalho amplia o [primeiro trecho](2026-11-26-planicie-primeiro-trecho.md).

## Resultado

Pelo F5 já dá para fazer o percurso que você pediu:
- seguir as pegadas da clareira até uma toca;
- caçar o primeiro coelho;
- tirar a pele e a carne;
- assar a carne na fogueira e comer;
- cortar tiras da pele rasgada;
- subir para o nível 2 e gastar um ponto na ficha, vendo antes o que muda;
- meditar na esteira, fechar o jogo e voltar com **Continuar**.

O jogo agora abre numa **tela de título**. Sem jornada salva, só aparece **Novo jogo**. Depois de meditar, aparece **Continuar**, com a data, o nível e a semente.

## Roteiro curto para conferir pelo F5

1. Abra o Godot .NET, abra o projeto e aperte **F5**. No título, **E** (ou Enter) em **Novo jogo**.
2. Na margem do riacho, a leste, pegue **sílex** e **galho rijo** com **E**. Abra o inventário (**I**), fabrique a **Faca de Pedra** e equipe com **E**.
3. **Siga as pegadas:**
   1. Na beira oeste da clareira começa uma fileira de pegadas pequenas.
   2. **E** numa delas diz "Pegadas pequenas, de coelho. Seguem para o noroeste." e dá a primeira prática de Rastreamento.
   3. **E** de novo diz a distância, e as próximas pegadas brilham.
   4. Siga as pegadas até a toca.
4. **Foque o coelho:** **Espaço** mostra "Coelho de Campo · Nv 1 · Arisco" e a barra de vida.
5. **A primeira caça:** espere o coelho sair da toca, chegue perto segurando **Shift** e corte com **J**.
6. **Carcaça:** **E** tira a **Pele Rasgada** (Esfolar Bruto, que sobe para Iniciante) e **E** de novo tira a **Carne Macia**.
7. **Fogueira**, a leste da clareira:
   1. **E** assa a carne.
   2. Dê dois passos com **L**: a barra ocre de vigor cai.
   3. No inventário, coma a **Carne Macia Assada**: o vigor volta.
8. No inventário, fabrique **Tira de Amarração** (2) com a pele rasgada. Nessa hora deve aparecer "**Nível 2!**".
9. **C** abre a ficha: escolha um atributo com W/S, leia o que muda e aperte **E**.
10. **E** na **esteira de palha** (nordeste da clareira) medita e grava.
11. Feche o jogo, abra de novo, aperte **F5** e escolha **Continuar**. Você acorda na esteira com tudo o que tinha.

**Para ir além:**
- uma segunda caça dá **Pele Comum**;
- Pele Comum + 2 tiras = **Bolsa Simples**, e **H** passa a usar uma erva direto;
- numa terceira caça, **K** na carcaça esfola com Qi: sai **Pele Fina** e gasta 15 de Qi;
- no lamaçal (siga as pegadas de casco que também saem da clareira) há o javali. Ele dá **couro**, **Carne Rija** e **presas**.

## Controles novos

| Ação | Tecla |
|---|---|
| Examinar pegada, assar na fogueira, meditar na esteira | **E** |
| Esfolar com Qi (junto a uma carcaça) | **K** |
| Focar o animal mais próximo (nome, nível, temperamento, vida) | **Espaço** |
| Ficha: nível, XP, pontos, atributos, proficiências | **C** |
| Usar a melhor erva da Bolsa Simples | **H** |

Os controles anteriores não mudaram: WASD, Shift, I, J ou clique esquerdo, L ou clique direito. A linha de ajuda no pé da tela mostra todos.

## O que ficou jogável, item por item

| Pedido | Como ficou |
|---|---|
| Nível, XP e pontos | Nível 1 no começo. São 20 XP para o nível 2, 30 para o 3 e 45 para o 4. Cada nível dá 2 pontos. O HUD mostra "Nv 1 · XP 8/20 (faltam 12)" e, havendo pontos, "· 2 pontos (C)" |
| Mudança antes de gastar | A ficha mostra, para o atributo escolhido, as linhas "atual → depois", por exemplo "Vida máxima: 10 → 12". Ela aplica o ponto a uma cópia da ficha, então a prévia é sempre o efeito real |
| Base do nível 1 | Vida 10, velocidade 105 px/s, passo de 34 px: os valores do trecho anterior. Vigor 50 e Qi 100, este igual ao do protótipo |
| Curvas e ganhos em dados | `assets/data/plains/plains_progression.json` |
| Níveis das criaturas | Coelho 1–2, javali 3–5, fixos pela semente. **Espaço** revela o nível; Rastreamento Adepto também o revela pela pegada |
| Diferença de nível com efeito | Um nível acima dá ao coelho +12% de audição e +8% de corrida. Dá ao javali +1 acerto de vida, +6% de investida e +0,5 de dano. O XP da caça sobe para presas mais fortes e cai para as mais fracas |
| Padrões preservados | O coelho continua fugindo para a toca e o javali continua avisando 0,8 s antes de investir, em todos os níveis |
| **VIT** | +2 de Vida máxima por ponto. O HUD continua com "Vida N%" e a cura pendente em verde |
| **INT** | +12% na potência dos preparos medicinais: a erva comum vai de 4,0 para 4,5 de Vida |
| **CON** | +10 de vigor máximo e +10% de recuperação. **O vigor tem uso**: o passo gasta 12 e o corte 3; sem vigor, aparece "Sem fôlego". Ele volta sozinho e com carne assada |
| **AGI** | +4% de velocidade ao andar (medida: 105,0 → 109,2 px/s), +5% de distância do passo e −5% no tempo de recuperação do corte e do passo |
| **SOR** | Chance de material secundário de 10% para 15% (tendão na caça, uma planta ou lasca a mais na coleta). A ficha mostra a mudança. O sorteio é fixo por objeto, então gravar e carregar não sorteia de novo |
| **QI** | Barra de Qi atual/máximo. **Uso:** K na carcaça esfola com Qi (−15), e o material sai um grau acima |
| Proficiências pelo uso | Esfolar, Preparo de Caça, Erboristeria e Rastreamento. A ficha mostra o grau, o progresso ("3/4 para Adepto") e o benefício. Cada prática vale uma vez por carcaça, planta ou trilha |
| Graus de Esfolar | 0 Bruto: Pele Rasgada. 1 Iniciante: Pele Comum. 2 Adepto: Pele Fina ou Couro Grosso, intactos. 3 Mestre: extração espiritual com Qi (Presa Espiritual do javali). Com Qi, a partir de Iniciante, um grau acima |
| Materiais do javali | Couro (pelo grau de Esfolar), Carne Rija e presas |
| Partes separadas | Pele, carne e presas são controladas uma a uma em cada carcaça. Uma carcaça usada pela metade volta só com o que falta |
| Rastreamento | Pegadas reais levam a uma toca ou ao lugar de um javali que existe. Uma trilha de coelho e uma de javali saem sempre da clareira |
| Erboristeria | A erva colhida muda com o grau: amassada (cura 3), comum (4), bem colhida (5), seiva viva (6, mais rápido) |
| Carne assada | Na fogueira: Carne Macia Assada (+25 de vigor) e Carne Rija Assada (+40), com +25% por grau de Preparo |
| Pele rasgada e couro comum | Tira de Amarração (1 pele rasgada → 2 tiras; 1 couro rasgado → 3). Bolsa Simples: Pele Comum + 2 tiras, ou Couro Comum + 1 tira. Efeito: **H** usa a melhor erva sem abrir o inventário |
| Ingredientes, resultado e efeito | No inventário, cada receita mostra os ingredientes com quanto você tem, o resultado e o efeito do item. A fabricação consome uma vez |
| Meditar e Continuar | **Esteira de palha** (nordeste da clareira):<br>- E medita, enche o Qi e o vigor e grava a jornada;<br>- grava em `user://jornada_planicie.json` (no Windows, `%APPDATA%\Godot\app_userdata\SpiritOfMurin\`);<br>- lê de volta e só confirma se a leitura bater.<br><br>**Novo jogo** com jornada salva pede confirmação, e "Não, manter a jornada" vem marcado. Uma gravação danificada mostra um aviso |
| O que a gravação guarda | Semente, ponto de meditação, coletas, animais mortos e partes tiradas, trilhas examinadas, inventário, equipamento, Vida, vigor, Qi, nível, XP, pontos, atributos, proficiências e receitas já feitas |

## O que foi observado numa partida normal × o que foi verificado com testes controlados

### Partida normal (percurso jogado só com teclas, pela tela de título)

O `tests/integration/PlainsRoute.tscn` abre a cena principal **pela tela de título**, como o F5. Ele joga **só apertando teclas**: sem teletransporte, sem chamar código do jogo e sem dar XP de graça. Depois relata o que aconteceu, sem dar veredito. Resultado, nas sementes 20261126 e 777:

| Momento | O que apareceu na tela |
|---|---|
| Título | só "Novo jogo" (sem jornada salva) |
| Despertar | "Vida 100%", "Nv 1 · XP 0/20 (faltam 20)" |
| Sílex, galho, faca pelo inventário | "+1 Sílex (tem 1)", "Fabricou 1 Faca de Pedra (Sílex −1, Galho Rijo −1)", "+5 XP (primeira Faca de Pedra)" |
| Primeira pegada | "Pegadas pequenas, de coelho. Seguem para o noroeste." / "Rastreamento: Iniciante, +1 XP!" / "+3 XP (pista nova)" |
| E de novo na pegada | "… O rastro termina a uns 11 passos." (sem XP) |
| Fim da trilha | 24 a 28 px da toca |
| Espaço | "Coelho de Campo · Nv 1 · Arisco" |
| Caça | "Coelho de Campo (Nv 1) abatido…" / "+6 XP (caça Nv 1)" |
| Carcaça | "+1 Pele Rasgada" / "Esfolar: Iniciante, +1 XP!"; "+1 Carne Macia" / "Preparo de Caça: Iniciante, +1 XP!" |
| Fogueira | "Assou 1 Carne Macia Assada (Carne Macia −1). Efeito: +31 vigor." / **"Nível 2! +2 pontos de atributo"** |
| Dois passos e comer | vigor 26/50 → 50/50 |
| Tiras | "Fabricou 2 Tira de Amarração (Pele Rasgada −1)." |
| Ficha | "Vida máxima: 10 → 12"; E → "VIT 1 — Vida máxima: 10 → 12" |
| Esteira | "Meditação gravada. Qi e vigor cheios." (arquivo lido de volta: Ok) |

**Dois problemas apareceram só nesse percurso normal e foram corrigidos:**
1. **A pegada ao lado de uma erva:** na semente 20261126, a primeira pegada ficava junto de uma erva, e **E** pegava a erva em vez de examinar a pegada. Os testes controlados não pegaram isso, porque eles se posicionam em cima da pegada. **Correção:** as duas trilhas garantidas agora começam **dentro** da clareira, onde não há objetos, e a fogueira foi para o lado leste.
2. **O primeiro nível dependia da pista:** sem a pista, o percurso terminava em **19/20 XP**, a 1 do nível. **Correção:** cada prática nova de proficiência vale 1 XP (`xp.practice` nos dados). O percurso agora fecha o nível 2 na fogueira, com folga.

### Testes controlados

Nos testes controlados, o personagem é teletransportado até o lugar, e às vezes o teste dá XP diretamente (dito em cada caso). As ações em si (E, K, J, L, I, C, H, Espaço) são teclas de verdade.

| Teste | O que confere | Resultado |
|---|---|---|
| Unidade (`tests/unit/`) | progressão (nível 1→2 com 20 XP e 2 pontos; a prévia de cada atributo é igual ao efeito; teto da AGI); prática conta uma vez e Esfolar sobe de 0 para 1; materiais por grau e com Qi (Bruto não usa Qi; Mestre faz extração espiritual); carnes e bônus do Preparo; qualidade da erva; sorteio secundário fixo e SOR que só acrescenta; nível da criatura fixo e na faixa, mais forte e com o mesmo aviso e recomposição; XP pela diferença de nível; gravação: ida e volta, danificada, outro formato; trilhas levam a animais reais e saem da clareira. Mais os 18 anteriores | **28/28** |
| `JourneyCheck --phase=new`, sementes 20261126 e 777 | jornada nova; faca uma vez; pista (XP uma vez; Iniciante faz brilhar); **seguir as pegadas andando** até a toca; Espaço com o nível; caça; Pele Rasgada e Esfolar 0→1; carne; carcaça vazia some; fogueira; o passo gasta vigor; carne assada devolve +31; o vigor volta sozinho; tiras uma vez; **primeiro nível**; segundo coelho dá **Pele Comum**; bolsa uma vez; **H**; terceiro coelho com **K** dá **Pele Fina** e −15 Qi; ficha com prévia; **cada atributo** (VIT +2, INT 4,0→4,5, CON +10, SOR 10→15%, QI +15, AGI 105,0→109,2 px/s medidos no corpo); meditar grava. *Controlado: o teste dá XP para ter os 6 pontos* | **31/31** nas duas |
| `JourneyCheck --phase=resume` (outro processo, depois de fechar) | título com Continuar em primeiro; **E** continua; mesma semente e lugar; mesmo inventário (nada multiplicado); mesmos nível, XP, pontos, atributos e proficiências; mesmas Vida, Qi e vigor; faca na mão; coletas não voltam; **carcaças usadas não voltam** e os animais caçados não reaparecem; **carcaça pela metade volta só com a carne**; trilha já examinada não dá XP de novo; meditar outra vez grava os mesmos bens | **11/11** nas duas |
| `PlainsCheck` (os testes anteriores, ampliados), sementes 20261126 e 777 | os 41 de antes, mais: foco no javali com o nível (Nv 4 e Nv 3); o javali cai nas aberturas e dá couro, Carne Rija e presas uma vez cada | **44/44** nas duas |
| `ControlsCheck` (espada, campo de teste) | os 14 de antes | **14/14** |

**Com janela, no monitor 2** (`--screen 1`; todos os logs registram "window on screen 1 of 2"), depois das correções abaixo:
- testes de unidade: 28/28;
- ControlsCheck: 14/14;
- PlainsCheck: 44/44 + 44/44;
- jornada nova: 31/31 + 31/31;
- retomada: 11/11 + 11/11.

Os logs `*-janela-*.log` estão nas evidências.

**Defeitos de jogo achados pelos testes com janela e pelo percurso normal, e corrigidos antes do commit:**
1. **A pegada escondia a carcaça.** O coelho morre junto à toca, onde a trilha termina. Quando uma pegada ficava mais perto que a carcaça, o **E** examinava a pegada em vez de esfolar. Apareceu no PlainsCheck com janela, na semente 777.
   **Correção:** a pegada só tem prioridade se estiver pelo menos 10 px mais perto que a carcaça, a erva ou a fogueira.
2. **A fogueira fora de alcance.** A base da fogueira é sólida. Parado encostado nela, o personagem ficava a 24–28 px, e o alcance do **E** era 26 px: às vezes não dava para assar. Apareceu no percurso normal.
   **Correção:** a fogueira e a esteira agora alcançam a 38 px.

Depois disso, o percurso normal chegou ao nível 2 em 5 de 5 execuções da semente 20261126 e nas duas da 777.

## Evidências

Pasta `production/qa/evidence/2026-11-26_progressao/`:
- `jornada-20261126/` e `jornada-777/`: capturas da jornada (nova jornada, pegada examinada, fim da trilha, foco com nível, vigor gasto, primeiro nível, esfolar com Qi, ficha com prévia, meditação, título com Continuar, retomada).
- `percurso-normal-20261126/` e `percurso-normal-777/`: capturas do percurso jogado só com teclas, pela tela de título. Os `.log` ao lado trazem o que apareceu em cada etapa.
- `percurso-normal.gif`: o mesmo percurso em vídeo curto (semente 20261126, 5 quadros/s).
- `planicie-20261126/` e `planicie-777/`: capturas do PlainsCheck.
- `*-janela-*.log`: saídas de todos os testes com janela no monitor 2.

## Números ainda provisórios

Todos estão em dados:
- XP 20/30/45 e 2 pontos por nível;
- efeitos por ponto (VIT 2, INT 12%, CON 10, AGI 4–5%, SOR 5%, QI 15);
- vigor 50, passo 12, corte 3, recuperação 14/s;
- Qi 100 e custo de 15 por esfola com Qi;
- limiares de prática 1/4/9 (Rastreamento 1/3/6, Erboristeria 2/5/10);
- faixas de nível das criaturas e força por nível;
- XP da caça 6 e 9 por nível.

## Limitações

- **A gravação só acontece ao meditar.** O que acontecer depois da última meditação se perde ao fechar (como no protótipo).
- **Couro Grosso e Presa Espiritual** ainda não têm receita. Tendão e presas comuns também não. As descrições dizem "mais adiante".
- **Extração espiritual:** só as presas do javali são compatíveis nesta entrega. O Coelho Lunar e outras variantes ficam para depois.
- **A bolsa** não aumenta a capacidade: o inventário ainda não tem limite. O efeito dela é a erva à mão (H).
- **Teclas novas** (C, H): já estão no mapa de entradas. A tela de configurar teclas do protótipo ainda não existe no Godot.
