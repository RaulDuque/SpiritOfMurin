# GUIA — leia este arquivo primeiro

Você é um assistente de IA (Claude, Cursor, Codex, ChatGPT ou outro) chegando ao **SpiritOfMurin**.
Este guia diz **o que é o projeto, o que estudar e em que ordem**. Ele não substitui os
documentos: aponta para eles. Se algo aqui contradisser o código ou um documento de
design, valem o código e o documento. Atualizado em 2026-11-27.

---

## 1. O projeto em 30 segundos

- **O jogo:** RPG de ação 2D de mundo aberto para PC, tema Murim (artes marciais e cultivo chinês). O protagonista é um espadachim cultivador (espada + Ki) que busca o Dao da Espada. Câmera top-down 3/4, teclado WASD + mouse.
- **Conceito:** "Mil Caminhos do Dao". Aprender é a recompensa: técnicas são descobertas explorando (manuais, túmulos, marcas na pedra) e mudam o que o jogador consegue fazer, não só o dano.
- **Estado atual:**
  - O jogo roda no **Godot 4.7.2 .NET (C#)** e abre pela campanha (`src/Campaign/Campaign.tscn`).
  - O **prólogo** se passa no Vale: ponte, discípulo de guarda, túmulo da mestra, revanche, Sênior.
  - Depois do prólogo, o jogador acorda sem nada na **Planície Verdejante**, gerada por semente. Lá há coleta, faca de pedra, coelho, javali, ervas, bolsa, moedas, progressão e ciclo de dia e noite.
- **Quem faz:** um criador solo, programador backend (C#, C, C++, PHP). É o primeiro jogo dele, um projeto hobby sem prazo. Ele fala **português do Brasil**.
- **Repositório:** `github.com/RaulDuque/SpiritOfMurin` (remoto `origin`). O remoto `upstream` é o framework de agentes: **nunca faça push nele**.

## 2. Ordem de estudo

Leia nesta ordem. Os itens marcados com ★ são obrigatórios antes de mexer em qualquer coisa.

### a) Regras de trabalho ★
1. ★ `docs/contexto-para-assistentes.md`: preferências do criador, regras, caminhos e testes num só lugar.
2. ★ `CLAUDE.md`, seções "Collaboration Protocol" e "Fluxo de trabalho: fechamento de cada entrega". Diz como cada entrega termina: build, testes, relatório, commit e push.
3. ★ `.claude/rules/commits-atomicos.md`: uma mudança lógica por commit.
4. `.claude/docs/coding-standards.md`: doc comments, valores de jogo fora do código, testes.

### b) Onde paramos ★
5. ★ `production/session-state/active.md`, se existir. Ele não vai para o git.
6. ★ As **últimas linhas** de `user-docs/relatorios/README.md` e o relatório mais recente. Cada entrega tem um relatório com o que mudou, como testar e o que ficou pendente.
7. `git log --oneline -15`.

### c) Visão e design do jogo
8. ★ `design/gdd/game-concept.md`: conceito, pilares e escopo. É a **fonte da verdade** do design.
9. `design/game-brief.md`: o pedido original do criador, nas palavras dele.
10. Os GDDs de cada sistema, em `design/gdd/`:
    - `campaign-prologue.md`: prólogo no Vale e a passagem para a Planície;
    - `prototype-recovery.md`: o que foi trazido do protótipo de navegador para o Godot;
    - `plains-first-stretch.md`: mundo por semente, coleta, criaturas;
    - `plains-progression.md`: XP, atributos, proficiências;
    - `display-and-map.md`: tela cheia, minimapa e mapa.
11. `design/art/art-bible.md`: identidade visual e paleta. **Importante:** o visual é procedural, desenhado em código. O teste com sprites foi rejeitado (seção 8.12).
12. `user-docs/futuro-do game/` (`minahvisao.md`, `gpt.md`, `opiniao-claude.md`): para onde o jogo vai (regiões, espécies, profissões, alquimia).

### d) Código
13. ★ `docs/engine-reference/godot/` (comece por `VERSION.md`): **seu conhecimento de Godot provavelmente é anterior à 4.5.** Confira as APIs aqui.
14. `src/CLAUDE.md`: convenções do código.
15. O mapa de `src/` (namespaces `SpiritOfMurin.*`):

| Pasta | O que tem |
|---|---|
| `Campaign/` | Cena principal (F5): conduz prólogo → Planície |
| `Vale/` | Prólogo: combate do Vale, lutadores, Sênior, efeitos |
| `Plains/` | Planície: gerador por semente (`PlainsGenerator`), blocos, mundo, HUD, ambientação |
| `Player/` | Personagem, ajustes (`PlayerTuning`), desenho em pixels, palma de Ki |
| `Combat/` | Arte dos lutadores, onda de Ki, dados das técnicas |
| `Creatures/` | Coelho, javali (corpo, "cérebro", ajustes), carcaça, níveis |
| `Items/` | Inventário, catálogo de itens, carteira (moedas) |
| `Progression/` | Atributos, proficiências, regras de coleta |
| `Game/` | Mira, assistência, bolsa, livro, controles, mapa, tela |
| `Save/` | Jornada (`SaveGame`) e preferências (`Prefs`) |
| `World/`, `Audio/`, `Main/`, `Dev/` | Mundo comum, sons, campo de teste da espada, vitrine |

16. Os dados de ajuste ficam em `assets/data/`: `PlayerTuning.tres`, `combat_techniques.json`, `vale_combat.json` e `plains/`. **Valores de jogo não são fixados no código.**

### e) Testes e evidências
17. `tools/painel-testes/LEIA-ME.md`: o `PainelDeTestes.exe` (Jogar, Vitrine, rodar testes com e sem janela).
18. `tools/godot/testes-janela.ps1`: a bateria completa com janela no monitor 2. A última rodada deu 494/494.
19. Os testes ficam em `tests/unit/UnitRunner.tscn` (runner próprio, atributo `[UnitTest]`) e em `tests/integration/*Check.tscn` (fases via `-- --phase=...`).
20. As evidências (capturas) de mudanças visíveis ficam em `production/qa/evidence/<data>_<tema>/`.

### f) Só se precisar
- **Protótipo antigo de navegador:** `prototypes/spiritofmurin/index.html` e os testes por CDP em `tools/playtest/`.
- **`.claude/skills/*/SKILL.md`:** roteiros do Claude Code, como `subir-git` para fechar entregas. Fora do Claude Code, leia e siga os passos à mão.
- **`.claude/agents/`:** papéis dos agentes especialistas. É contexto, não obrigação.
- **`README.md` e `AGENTS.md` da raiz:** são do **framework** de agentes (Claude/Codex Game Studios), não do jogo.
- **Arquivos `*-SOLUCAO.md` e seções de spoiler:** contêm soluções de quebra-cabeças. **Não leia em voz alta para o criador nem copie para instruções.**

## 3. Regras que não podem ser quebradas

1. **Falar com o criador em português do Brasil, uma decisão por vez.** O ciclo é: pergunta → opções → decisão → rascunho → aprovação.
2. **Escolha visual se mostra antes de se perguntar.** Monte uma prancha HTML em `user-docs/relatorios/` com as opções. Nunca peça para ele escolher um hex que não vê.
3. **No fim de cada entrega que mude arquivos:** `dotnet build`, testes aplicáveis, relatório em `user-docs/relatorios/` + índice `README.md`, commits atômicos (`git add` por arquivo), push para `origin`, e a resposta com branch, link do commit, link do relatório e testes. **Esse commit e push já estão autorizados.**
4. **Nunca capture a área de trabalho do Windows** em testes ou evidências. Já pegou o navegador do criador. Use a imagem do viewport do jogo.
5. **Testes não tocam a jornada nem as teclas do criador.** Use os arquivos `user://teste_*.json`.
6. **Não mude a stack.** Godot 4.7.2 .NET em `C:/Users/Raul/Tools/Godot/4.7.2-stable-mono/`. O `csproj` mira `net10.0`, com build offline e fonte NuGet local. O renderer é Compatibility.
7. **Passos pequenos.** Pergunte antes de grandes levas de conteúdo e mantenha o histórico (versões antigas vão para pastas `historico/`, não são apagadas).

## 4. Primeira mensagem sugerida ao criador

Depois de estudar, confirme o entendimento em poucas linhas. Diga onde o projeto parou, segundo o relatório mais recente, e pergunte qual é o próximo pedido. Não comece a mudar arquivos sem um pedido.
