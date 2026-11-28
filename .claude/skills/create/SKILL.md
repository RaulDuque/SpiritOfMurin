---
name: create
description: "Cria um elemento novo do SpiritOfMurin a partir de uma descrição em português, seguindo o padrão de elementos (docs/elementos/PADRAO.md): acha o exemplo mais próximo, cria desenho, dados, registro e uma prévia/teste, e diz quais arquivos criou e onde mexer. Use quando o criador escrever /create <tipo> \"<descrição>\" ou pedir para criar uma árvore, rocha, bambu, arbusto, ferramenta, arma, item, mob, inimigo, roupa ou capacete."
argument-hint: "<tipo> \"<descrição>\"   (tipos: arvore, rocha, bambu, arbusto, ferramenta, item, arma, mob, inimigo, roupa, capacete)"
user-invocable: true
---

# /create — criar um elemento no padrão do projeto

Leia antes: `docs/elementos/PADRAO.md` (o padrão), `docs/elementos/INDICE.md` (onde está cada coisa) e
`assets/data/elements/LEIA-ME.md` (o esquema dos JSON). Fale com o criador em português do Brasil.

## 0. Entender o pedido

- `$ARGUMENTS` = `<tipo> "<descrição>"`. Tire da descrição: o **nome** (vira o id: minúsculo, sem acento, `_`),
  a **forma**, as **cores**, o **comportamento** e **onde aparece**.
- **O id já existe?** Procure em `assets/data/elements/**`, `assets/data/plains/plains_items.json` e
  `docs/elementos/INDICE.md`. Se existir, **pare e pergunte** (uma pergunta, com opções):
  *criar uma variante* (id novo, por exemplo `salgueiro_chorao`; o original não muda) ou *editar o existente*
  (muda em todo lugar onde ele aparece). Nunca sobrescreva sem a resposta.
- **Uma escolha visual** (cor, pose) que a descrição não fixa: siga a regra do projeto. Monte uma prancha HTML com
  as opções em `user-docs/relatorios/` antes de perguntar. Se a descrição já disser a cor, use-a.

## 1. Escolher o caminho pelo tipo

| Tipo | Resultado hoje | Exemplo mais próximo (copiar dele) | Arquivos que o comando cria ou muda |
|---|---|---|---|
| `arvore` | **jogável** | `arvores/pinheiro.json` + `Flora/PinheiroArt.cs` | `assets/data/elements/arvores/<id>.json`; `src/Elements/Flora/<Nome>Art.cs`; uma linha em `ElementArts` (`src/Elements/ElementArt.cs`); materiais novos em `plains_items.json` + ícone em `ItemIcons.cs` |
| `rocha` | **jogável** | `rochas/rocha_quartzo.json` + `Rocks/RochaArt.cs` | o JSON (muitas vezes basta mudar `look.vein`, os materiais e o bônus, reaproveitando a arte `rocha`); arte nova só se a forma mudar |
| `bambu` | **jogável** | `bambus/bambu_verde.json` + `Flora/BambuArt.cs` | o JSON (`look.stalks`); arte nova só se a forma mudar |
| `arbusto` | **jogável** | `arbustos/arbusto.json` + `Flora/ArbustoArt.cs` | o JSON (`contact_radius`); arte nova para outra forma |
| `ferramenta` | **jogável** quando o tipo é `machado`, `picareta` ou `faca` | `machado_pedra` em `plains_items.json` + `src/Elements/Tools/ToolArt.cs` | item com `equip` (axe, pickaxe ou knife) e `tool` {kind, power, reach, damage}; receita; `case` em `ToolArt.Draw` (desenho na mão) e em `ItemIcons.cs` (ícone) |
| `item` (material, comida, erva) | **jogável** | `tora`, `broto_bambu` em `plains_items.json` | entrada no JSON (com `vigor_now`, `regen_amount` etc. se for usável), ícone, receita ou origem (drop de um elemento) |
| `arma` | **jogável** (T18) | `assets/data/equipamento/armas/sabre_ferro.json` + `src/Elements/Armas/SabreFerroArt.cs` | ver **§2b** |
| `mob` (animal) | **planejamento**. Uma *variante numérica* de coelho ou javali (outra velocidade, audição, vida) dá para fazer nos `.tres` e em `plains_progression.json`; uma espécie nova é código novo | `Rabbit`/`RabbitBrain`, `Boar`/`BoarBrain` | ficha + tarefa (ligar à T08) |
| `inimigo` (humano, chefe) | **planejamento** | `ValeFighter` + `FighterArt` + `vale_combat.json` | ficha + tarefa (ligar à T08) |
| `roupa` | **jogável** (T18) | `assets/data/equipamento/roupas/tunica_viajante.json` + `src/Elements/Roupas/TunicaViajanteArt.cs` | ver **§2b** |
| `capacete` | **jogável** (T18) | `assets/data/equipamento/capacetes/chapeu_bambu.json` + `src/Elements/Capacetes/ChapeuBambuArt.cs` | ver **§2b** |

**Ficha de planejamento** (`user-docs/tasks/fichas/<id>.md`): a descrição do criador (literal), forma e cores,
comportamento (aviso, golpe, recuperação, fuga), números iniciais propostos, materiais e raridade, onde aparece,
de que exemplo reaproveitar o código e a tarefa em que ele entra. Marque o topo com **"PLANEJAMENTO — ainda não está
no jogo"**. Ligue a ficha na tarefa e no `user-docs/tasks/README.md`.

## 2. Criar (tipos jogáveis)

1. **Dados:** copie o JSON do exemplo mais próximo para o caminho novo. Troque `id`, `name`, `art`, `habitats`,
   `toughness`, `tools`, `refusal`, `drops`, `bonus`, `xp` e `look` conforme a descrição. Valores de jogo **só aqui**.
2. **Desenho** (se a forma for nova): crie `src/Elements/<Família>/<Nome>Art.cs` copiando a arte do exemplo.
   - Mantenha a assinatura `Draw(CanvasItem, ElementDef?, in ElementPose)` e os estados (em pé com progresso,
     caindo, esgotado).
   - Comente no topo da classe **onde mexer** em cada parte (forma, paleta, animação).
   - Registre a classe em `ElementArts` (`src/Elements/ElementArt.cs`).
3. **Itens novos** (materiais, ferramenta):
   - entrada em `assets/data/plains/plains_items.json`;
   - `case "<id>":` em `src/Game/ItemIcons.cs`;
   - receita, quando couber.
4. **Onde aparece:** para elementos, os `habitats` no JSON bastam (o gerador escolhe dentro da família, com sorteio
   fixo). Não mude os sorteios de `src/Plains/PlainsGenerator.cs`: as sementes antigas precisam manter o que têm.
5. **Compile:** `dotnet build`.

## 2b. Criar arma, roupa ou capacete (T18)

Leia antes `assets/data/equipamento/LEIA-ME.md` (o esquema) e `docs/elementos/EQUIPAMENTO.md` (camadas, ordem de desenho e
os três exemplos de edição). O protagonista aprovado é a referência: **nunca mude** `TunicaVerdeArt`, `CabecaArt` nem
`EspadaArt` ao criar uma peça nova.

1. **Escolha visual:** cor, forma da aba, curva da lâmina que a descrição não fixa → prancha HTML em `user-docs/relatorios/`
   com a peça **sobre o protagonista**, no tamanho real e ampliada, de frente, costas e perfil (a fase `prancha` do
   `tests/integration/EquipCheck.tscn` gera as faixas com os pixels do jogo: acrescente o visual em `BoardLooks`). Peças que
   mudam a silhueta passam pelo `/asset-spec`. Regra dos chapéus do protagonista: aba nivelada, ≤ 16 px, olhos à vista.
2. **Dados** (`assets/data/equipamento/<armas|roupas|capacetes>/<id>.json`, copie o exemplo da tabela):
   - arma: `weapon` {`art`, `kind` (`longa` = como a espada; `curta` = como a faca, também ferramenta com `equip: "knife"` e
     `tool`), `reach`, `damage`, `chain`, `startup`, `active`, `recovery`, `cancel_at`, `lunge`, `third_*`, `techniques`};
   - roupa: `wear` {`slot`: `corpo`, `art`, `defense`, `weight`}; capacete: `wear` {`slot`: `cabeca`, `art`, `defense`, `weight`};
   - sempre `id` (= nome do arquivo), `name`, `description` e **`onde`** (onde se consegue: aparece na bolsa e no relatório).
3. **Desenho** (`src/Elements/<Armas|Roupas|Capacetes>/<Nome>Art.cs`, copie o exemplo):
   - arma: herde `ArmaArt` (`Length`, `InHand`, `OnBody`; `Curved` + `Spine` se a lâmina for curva, para o brilho do Reforço
     seguir a curva); registre em `ArmaArts` (`src/Elements/Armas/ArmaArt.cs`);
   - roupa: herde `MatrixRoupaArt` (3 matrizes: frente e costas 20 colunas, perfil 16, 21 linhas; paleta; mangas); registre
     em `RoupaArts` (`src/Elements/Roupas/RoupaArt.cs`);
   - capacete: herde `CapaceteArt` (matrizes de **16 colunas** centradas, `left = -8`, para o perfil espelhar no lugar;
     `HairCover` = 0, `CabecaArt.BunRows` ou `CabecaArt.BandRows`); registre em `CapaceteArts`;
   - comente no topo da classe **onde mexer** (comprimento, faixa, aba, cores).
4. **Ícone:** `case "<id>":` em `src/Game/ItemIcons.cs`.
5. **Como se consegue:** uma receita em `assets/data/plains/plains_items.json` (`recipes`), ou o id na lista `trouxa` de
   `assets/data/equipamento/_regras.json`. Não mude os sorteios do gerador.
6. **Técnica própria de arma** (decisão 2 do criador): uma entrada em `assets/data/equipamento/_tecnicas_armas.json`
   (`state`: `jogavel` só se o efeito existir no código e tiver teste; senão `futura`) e o id em `weapon.techniques`.
7. **Compile:** `dotnet build`. A bolsa já mostra a peça (o catálogo lê a pasta sozinho), com ícone, marca de equipado e
   a comparação (alcance, dano, combo e tempo para armas; defesa e peso para roupas e chapéus).

**Prévia:** `"$GODOT" --path . src/Dev/Inspecao.tscn -- --arma=<art> --roupa=<art> --capacete=<art> --pose=corte2 --dir=0`
(Tab arma · R roupa · C chapéu · Z pose · Q/E gira nas 8 direções · Espaço anima). Captura: `--shot=res://production/qa/evidence/<data>_create-<id>/inspecao.png`.

**Teste** (com janela no monitor 2): `res://tests/integration/EquipCheck.tscn -- --phase=catalogo --out=res://production/qa/evidence/<data>_create-<id>/`.
Ele lê **todos** os arquivos da pasta (nenhuma lista para atualizar) e falha se faltar `onde`, desenho registrado, ícone
próprio, números do golpe, slot certo ou uma origem (receita/trouxa); salva `previa_<id>.png` com a peça nas 8 direções.
Depois rode `--phase=retrato --ref=res://production/qa/evidence/2026-11-28_t18-equipamento/antes`: o protagonista padrão
tem de continuar **idêntico** (0 px de diferença). Pasta vazia ou exceção = **NOT ASSESSED** (falha).

## 3. Pré-visualizar e testar

- **Prévia:** `src/Dev/Inspecao.tscn`. Pelo Godot (F6 com a cena aberta) ou pela linha de comando:
  `"$GODOT" --path . src/Dev/Inspecao.tscn -- --elemento=<id> --estado=meio`.
  - Estados: `inteiro`, `meio`, `quase`, `caindo`, `esgotado`.
  - Para ver uma ferramenta na mão: `--ferramenta=machado`.
  - Guarde uma captura com `--shot=res://production/qa/evidence/<data>_create-<id>/inspecao.png`.
- **Teste:** o `tests/integration/ElementosCheck.tscn` lê **todos** os arquivos de `assets/data/elements` sozinho
  (nenhuma lista para atualizar). Rode, com janela no monitor 2:
  - `-- --phase=catalogo`: o arquivo carrega, o id bate com o nome, a arte existe, o elemento nasce em algum habitat,
    recusa a ferramenta errada, se esgota no número de golpes da ferramenta certa e dá itens que existem;
  - `-- --phase=mundo --seed=20261126` e `--seed=777`: aparece em volta da clareira, no seu habitat, e responde à
    ferramenta (ele não aparece se os `habitats` forem raros demais para essas sementes; diga isso ao criador).
  Uma pasta vazia ou uma exceção dão **NOT ASSESSED** (falha), nunca aprovação.
- **Só é "jogável"** quando carrega, aparece onde deve e responde à interação descrita, com o teste passando. Senão,
  diga que é planejamento.

## 4. Responder ao criador

Liste **exatamente**:
- os arquivos criados;
- os arquivos alterados, com a linha registrada;
- onde mexer para mudar a forma, as cores, os números e o lugar onde aparece;
- o comando de prévia;
- o resultado do teste;
- se o resultado é **jogável** ou **planejamento**.

Depois feche a entrega com `/subir-git` (regra do `CLAUDE.md`).
