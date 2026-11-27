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
| `arma` | **planejamento** (o personagem só tem espada e faca como armas de combate) | — | ficha `user-docs/tasks/fichas/<id>.md` + a tarefa T18 |
| `mob` (animal) | **planejamento**. Uma *variante numérica* de coelho ou javali (outra velocidade, audição, vida) dá para fazer nos `.tres` e em `plains_progression.json`; uma espécie nova é código novo | `Rabbit`/`RabbitBrain`, `Boar`/`BoarBrain` | ficha + tarefa (ligar à T08) |
| `inimigo` (humano, chefe) | **planejamento** | `ValeFighter` + `FighterArt` + `vale_combat.json` | ficha + tarefa (ligar à T08) |
| `roupa`, `capacete` | **planejamento** (as camadas trocáveis não existem: a túnica e o cabelo fazem parte do `Player.cs`) | — | ficha + a tarefa T18 (equipamento visual) |

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
