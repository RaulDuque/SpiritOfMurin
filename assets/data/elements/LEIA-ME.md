# Elementos do mundo — um arquivo por elemento

Cada `.json` desta pasta (e das subpastas) é um elemento: uma árvore, uma rocha, um bambu ou um arbusto. O jogo lê todos ao começar a jornada (`src/Elements/ElementDef.cs`, `ElementCatalog.Load`). O padrão completo está em `docs/elementos/PADRAO.md`.

| Chave | O que é |
|---|---|
| `id` | nome estável (igual ao nome do arquivo). Nunca renomeie um id já publicado |
| `name` | nome mostrado ao jogador |
| `family` | `arvore`, `rocha`, `bambu` ou `arbusto` (decide o comportamento compartilhado) |
| `art` | qual desenho usar (`pinheiro`, `rocha`, `bambu`, `arbusto`…: classe `<Nome>Art` em `src/Elements/`) |
| `habitats` | onde o gerador põe este elemento e com que peso dentro da família: `grass` (relva), `bank` (margem), `bushes` (arbustos), `mud` (lamaçal) |
| `base_half_width`, `base_height` | a base que colide enquanto está em pé (px); 0 = não colide |
| `depleted_half_width`, `depleted_height` | a base depois de esgotado (toco); 0 = não colide |
| `toughness` | quantos golpes de força 1 ele aguenta; 0 = decorativo |
| `tools` | ferramentas que trabalham nele: `machado`, `picareta`, `faca` |
| `refusal` | o que o jogo diz quando a ferramenta na mão não serve |
| `leaves` | o que fica: `toco`, `cascalho`, `talos`, `nada` |
| `drops` | materiais ao esgotar: `item`, `min`, `max` (sorteio fixo pela semente e pelo lugar) |
| `bonus` | material bônus: `item`, `chance`, `per_sor` (soma por ponto de SOR), `cap` (teto), `count` |
| `xp` | XP ao esgotar |
| `contact_radius` | arbustos: distância (px) em que um corpo passando dobra as folhas |
| `veined` | rochas: `true` mostra o veio e pode ser minerada |
| `look` | números e cores livres que a arte lê (cada arte documenta os seus) |
