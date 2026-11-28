# Equipamento do protagonista — camadas e como editar (T18)

*Criado em 2026-11-28 (T18). O padrão geral está em [PADRAO.md](PADRAO.md); os dados das peças, em
`assets/data/equipamento/LEIA-ME.md`.*

## 1. As camadas

O protagonista é desenhado em camadas, quadro a quadro, a partir de uma **pose** (`src/Elements/Corpo/FighterPose.cs`):
a vista (frente, costas, perfil), o lado, a inclinação do passo, onde estão a mão e a ponta da arma, o balanço da barra e
a altura da cabeça. O `src/Player/Player.cs` calcula a pose (`Pose`) e chama as camadas (`DrawFighter`):

| Camada | Arquivo | O que desenha |
|---|---|---|
| Corpo | `src/Player/Player.cs` | pernas, botas, mãos (pele), o brilho do Ki e do Reforço |
| Cabeça e cabelo | `src/Elements/Corpo/CabecaArt.cs` | rosto, cabelo, coque (linhas 0–2), faixa (linha 3), fita |
| Roupa | `src/Elements/Roupas/<Nome>Art.cs` | o corpo da túnica (3 matrizes) e as duas mangas |
| Chapéu | `src/Elements/Capacetes/<Nome>Art.cs` | o chapéu por cima da cabeça; `HairCover` diz quantas linhas do cabelo somem |
| Arma | `src/Elements/Armas/<Nome>Art.cs` | a arma da mão até a ponta (`InHand`) e o que aparece no corpo (`OnBody`: bainha) |

**Ordem de desenho:** de frente e de perfil: bainha → braço de trás → pernas → roupa → cabeça → chapéu → braço da frente
com a arma. De costas, os braços e a arma vão antes da roupa (ficam atrás do corpo). O contorno escuro de 1 px sai
sozinho em volta da união de tudo (`src/Player/PixelPainter.cs`).

**Matrizes:** cada linha é uma string, cada letra um pixel, `.` é vazio; as cores de cada letra ficam no topo da classe.
A roupa tem 20 colunas de frente e de costas e 16 de perfil (21 linhas); o chapéu, 16 colunas centradas (assim o perfil
espelha no lugar quando o personagem olha para a esquerda); a cabeça, 12 × 14.

**Ver o resultado:** Painel → **Inspeção** (ou `src/Dev/Inspecao.tscn`). Tab troca a arma, R a roupa, C o chapéu, Z a pose
(repouso, caminhada, cortes 1–3, estocada, passo, Ki, Reforço), Q/E giram a figura grande nas 8 direções e Espaço anima.
O texto da tela diz qual arquivo desenha cada peça. O Painel recompila e reabre a cena.

**Conferir que nada quebrou:** `tests/integration/EquipCheck.tscn -- --phase=retrato --ref=res://production/qa/evidence/2026-11-28_t18-equipamento/antes`
compara o protagonista padrão com o aprovado, pixel por pixel, em 12 poses × 8 direções × 5 coisas na mão.
`--phase=conjunto --ref=res://production/qa/evidence/2026-11-28_t18-equipamento/conjunto` faz o mesmo com o conjunto
aprovado. Depois de uma edição de propósito numa peça nova, regrave a referência dela com `--out=` (nunca a do padrão).

## 2. Três exemplos

### a) Alongar a lâmina do sabre

Arquivo `src/Elements/Armas/SabreFerroArt.cs`:

```csharp
public override float Length => 1.05f;   // 1 = a espada
```

Troque `1.05f` por `1.25f`: a lâmina fica 20% mais longa em todas as poses e direções, e a curva, o fio com os dentes e o
brilho do Reforço acompanham. **Isso só muda o desenho.** Para o golpe alcançar mais, mude também o alcance em
`assets/data/equipamento/armas/sabre_ferro.json`:

```json
"reach": 38,
```

Por exemplo, `44`. A bolsa passa a mostrar `Alcance (px): … → 44`, que é o mesmo número usado pelo golpe (o teste
`--phase=combate` confere isso).

### b) Alargar a faixa de uma túnica

Arquivo `src/Elements/Roupas/TunicaViajanteArt.cs`. A faixa fica nas linhas 8–10 da frente e das costas (`t` = jade gasto,
`n` = nó, `d` = dobra escura) e nas linhas 7–9 do perfil. Para alargar 1 px para baixo, troque a linha logo abaixo da dobra
pelas letras da faixa **nas três vistas**:

```text
frente, linha 11:  "....mmmmmrrrwrrr...."  →  "....tttttttttttt...."
costas, linha 11:  "....mmmmmrrrwrww...."  →  "....tttttttttttt...."
perfil, linha 10:  "....mmmmmrrw...."      →  "....tttttttt...."
```

Mantenha o mesmo número de letras em cada linha (20 na frente e nas costas, 16 no perfil). Na túnica verde do criador
(`TunicaVerdeArt.cs`) a faixa fica nas mesmas linhas. Ela é a referência aprovada: mude-a só de propósito, e depois
regrave a referência do padrão.

### c) Baixar a aba de um chapéu

Arquivo `src/Elements/Capacetes/ChapeuBambuArt.cs`. Cada matriz tem a copa (linhas 0–2), o cordão (linha 3), a aba
(linha 4) e a sombra sob a aba (linha 5). Para baixar a aba 1 px, repita a última linha da copa antes do cordão, nas três
matrizes (`Front`, `BackRows`, `Side`):

```text
"....kkmmmmkk....",   ← linha 2 (copa)
"....kkmmmmkk....",   ← cópia nova: tudo abaixo desce 1 px
"....ddddtddd....",   ← cordão
"kmmmmllllllmmmmk",   ← aba (agora na linha 5)
".kkkkkkkkkkkkkk.",   ← sombra sob a aba (linha 6)
```

A aba passa a cobrir também a primeira fileira de cabelo da testa. Os olhos (linha 9 da cabeça) continuam à vista. Pela
regra dos chapéus do protagonista, a aba fica nivelada, com no máximo 16 px, e não cobre os olhos. Quanto do cabelo
desaparece sob o chapéu é o `HairCover` da mesma classe: hoje `CabecaArt.BandRows`, que esconde coque e faixa (decisão 3
do criador).
