# Base do jogo no Godot com C# — 2026-11-25

> O pedido está em [prompts/07-setup-engine-godot.md](prompts/07-setup-engine-godot.md).

## Resultado

O projeto SpiritOfMurin agora abre no Godot 4.7.2 com C#. Apertando **F5**:
- o personagem aparece numa cena 2D vista de cima em 3/4, com a aparência do protótipo;
- ele anda com WASD, corta com J ou clique esquerdo e dá o passo com L ou clique direito;
- a câmera o acompanha.

O protótipo HTML continua igual, para comparar a sensação.

## Versões usadas

| Item | Versão e caminho |
|---|---|
| Godot | **4.7.2 stable, edição .NET** (oficial, baixada do GitHub do Godot; SHA-512 conferido) em `C:\Users\Raul\Tools\Godot\4.7.2-stable-mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe` |
| Godot que já estava instalado | 4.7.2 stable **padrão** em `C:\Users\Raul\Tools\Godot\4.7.2-stable\`. Não abre projetos C#; continua lá para projetos em GDScript |
| .NET SDK | **11.0.100** (já instalado) |
| Runtimes .NET | 10.0.15 e 11.0.0 (já instalados). O Godot sobe no runtime mais novo |
| Alvo do projeto | **net10.0** (versão de suporte longo) |
| Pacotes do Godot para C# | os que vêm junto com o editor, registrados como fonte local do NuGet "GodotSharp-4.7.2-offline". O projeto compila sem internet |

**Por que net10.0 e não net8.0:** a internet caiu no meio da configuração. O pacote de referência do .NET 8 não estava no PC, e o do .NET 10 estava. O Godot 4.7.2 roda o código em .NET 10 sem diferença para o jogo.

## Onde fica

| Arquivo | Caminho exato |
|---|---|
| **project.godot** | `D:\Progr\SpiritOfMurin\project.godot` (raiz do repositório) |
| Projeto C# | `D:\Progr\SpiritOfMurin\SpiritOfMurin.csproj` |
| Cena principal (F5) | `res://src/Main/Main.tscn` |
| Personagem | `src/Player/Player.tscn`, `Player.cs` (movimento, corte, passo, desenho do protótipo), `PixelPainter.cs` |
| Ajustes do personagem | `assets/data/PlayerTuning.tres`: velocidade, tempos do corte e do passo, valores iniciais do `config.js` do protótipo |
| Campo de teste | `src/World/Ground.cs` (relva e trilha), `src/World/Obstacle.cs` (pinheiro, pedra, bambu com colisão só na base) |
| Mapa de entradas | no `project.godot` (seção `[input]`), escrito por `tools/godot/setup_input.gd` |
| Teste dos controles | `tests/integration/ControlsCheck.tscn` e `ControlsCheck.cs` |

**Pastas que não são do jogo** (relatórios, protótipo, evidências, design, documentação, ferramentas, fontes de arte) levam um arquivo `.gdignore`, para o Godot não importá-las.

## Controles que funcionam

| Ação | Teclado | Mouse |
|---|---|---|
| Andar (8 direções) | W A S D (e setas) | — |
| Corte, encadeia até 3 | J | clique esquerdo |
| Passo (sem direção: para trás; com direção: para aquele lado) | L | clique direito |

- O golpe sai na direção para onde o personagem olha, que é a última direção em que ele andou.
- Os tempos vêm do protótipo: janela de encadeamento de 0,13 s, aperto guardado por 0,2 s e passo de 34 px em 0,16 s.
- Já estão no mapa de entradas, para as próximas etapas: K (qi), Espaço (travar) e E (examinar).
- No editor, as teclas ficam em Project › Project Settings › Input Map.

**Ainda não há:** inimigos, travar no alvo, passo lateral, vida e qi. Esta etapa é só a base.

## Testes

**Compilação:** `dotnet build`, 0 avisos e 0 erros.

**`tests/integration/ControlsCheck.tscn` — 14 de 14 OK**, rodado de duas formas:
- sem janela: `--headless`;
- **com janela**, como no F5, salvando as capturas.

Ele carrega a cena principal e aperta teclas e botões do mouse pela própria entrada do Godot:

| Verificação | Resultado |
|---|---|
| D anda para a direita e vira o rosto para a direita | 70 px |
| W anda para cima | 53 px |
| A+S anda na diagonal | — |
| J corta | — |
| O corte segue a direção do personagem | 0° olhando para a direita; −90° olhando para cima, com clique |
| J três vezes encadeia os golpes 1-2-3 | — |
| Três cliques esquerdos encadeiam os golpes 1-2-3 | — |
| L sem direção dá o passo para trás | 35 px |
| Clique direito + D dá o passo para a direita | 39 px |
| A câmera acompanha | terminou a menos de 1 px do personagem, depois de sair do ponto inicial |

**Outras execuções:**
- A cena principal rodou sozinha (`--quit-after`), com e sem janela, sem erro.
- A conferência de coerência da configuração bate em tudo. A única exceção é o comando de testes gdUnit4, que ainda não existe: ele chega com o `/test-setup`.

**Evidências:** `production/qa/evidence/2026-11-25_godot-base/`, com o início, o corte para a direita, o corte para cima com clique, o passo e a câmera depois de seguir.

**Não testado:** apertar F5 dentro do editor. Testei o mesmo que o F5 executa: o projeto rodando com janela.

## Como abrir no seu PC

1. Abra **`C:\Users\Raul\Tools\Godot\4.7.2-stable-mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe`**. É a edição .NET; a outra não roda C#.
2. No Gerenciador de Projetos, clique em **Importar** e escolha `D:\Progr\SpiritOfMurin\project.godot`.
3. Com o projeto aberto, aperte **F5**. Na primeira vez, o Godot compila o C# antes de rodar.

**Dica:** crie um atalho para esse `.exe` na área de trabalho, porque a pasta tem duas edições do Godot.

## Configuração do projeto registrada

- `CLAUDE.md`: Godot 4.7.2, C#, .NET SDK e importação do `docs/engine-reference/godot/VERSION.md`.
- `project.yaml`: blocos engine (com o caminho do Godot .NET), specialists, naming e commands. O comando `build` fica a definir até existir uma predefinição de exportação para Windows.
- `.claude/docs/technical-preferences.md`: preenchido para Godot C#, PC e teclado/mouse.
- `docs/engine-reference/godot/VERSION.md`: versão instalada registrada.
- `.gitignore`: o `SpiritOfMurin.csproj` passa a ir para o repositório. A regra herdada do Unity ignorava todo `.csproj`.

## Próxima entrega combinada

O primeiro trecho da **Planície Verdejante gerado por semente**, com coelho, javali, ervas e faca de pedra. O campo de teste de hoje (`Ground.cs` e `Main.cs`) já posiciona o cenário a partir de uma semente fixa e vai dar lugar ao gerador.

Para isso falta escolher a biblioteca de testes. Sugiro rodar o `/test-setup` com gdUnit4, que tem suporte a C#.
