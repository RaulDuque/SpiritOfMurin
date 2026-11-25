# Limpeza dos sprites e antes/depois no bambuzal — 2026-11-25

## Pedido do criador (resumo)

Limpar e exportar as duas imagens do ChatGPT com ferramentas gratuitas, sem mexer nos originais:
- tirar o fundo cinza, preservando a roupa cinza do discípulo, a lâmina e a fita;
- gerar PNGs de 64×64 com transparência binária e só cores da paleta mestra, respeitando o pivô (32, 59);
- validar os dois arquivos;
- integrar ao protótipo com `?sprites=1`;
- capturar o antes e o depois no mesmo bambuzal, conferindo a espada inteira e os pés no chão;
- entregar a prancha e o endereço para jogar;
- registrar no guia a direção 2D vista de cima em 3/4 e o caráter provisório do quadro de 64×64 e da paleta.

## O que foi feito

1. **Originais preservados.**
   - As imagens estão em `assets/source/raw/`, com 1254×1254 px e fundo cinza RGB ~123–125.
   - O espadachim de túnica marfim é `char_swordsman_idle_s_00.png`.
   - O discípulo do chapéu de palha é `char_disciple_guard_s_00.png`.
   - O script só lê os originais.
2. **Remoção do fundo** (Python 3.12 + Pillow 13 + numpy, já instalados; não foi preciso ambiente virtual nem rembg).
   - O cinza foi amostrado nas bordas.
   - Em seguida, foram removidas as regiões ligadas ao fundo, com tolerância de 9 níveis.
   - Depois, o script procurou vãos fechados com o mesmo cinza chapado: nenhum foi encontrado nas duas imagens.
   - A roupa do discípulo (~85–95) fica longe do fundo (~124), por isso foi preservada. O mesmo vale para a lâmina e a fita.
   - Os recortes sobre fundo claro e escuro estão em `assets/source/work/*_cutout_{claro,escuro}.png`, e não mostram halo cinza.
3. **Redução para 64×64.**
   - A escala foi calculada pela altura de cada personagem:
     - espadachim: 49 px, 19,1 px da imagem por pixel do sprite;
     - discípulo: 43 px, 22,6 px da imagem por pixel do sprite.
   - Cada pixel recebe a cor da paleta mais frequente na área correspondente, comparada em CIE Lab com as cores do personagem.
   - Pivô:
     - espadachim: pé de apoio (o de trás) na coluna 32, sola na linha 59;
     - discípulo: ponto do chão entre os pés na coluna 32, solas na linha 59.
4. **Regras de pixel art.**
   - Contorno `#16140f` só por fora; dentro, as linhas usam o tom escuro da roupa.
   - Pixels soltos foram removidos.
   - As lâminas foram redesenhadas com inclinação exata: 1:2 no espadachim, 1:1 no discípulo.
   - Cada pé tem 1 px de contato.
   - Correções à mão ficaram registradas no script: olhos de 1 px e gola cruzada do espadachim, guardas de bronze de 4 px nas duas espadas.
5. **Resultado repetível.** O desempate de cores variava entre execuções. Foi corrigido, e três execuções seguidas dão o mesmo arquivo.
6. **Validação** (`assets/source/work/validation.txt`).
   - Os dois PNGs têm 64×64, alpha só 0/255 e 15 cores, todas da paleta.
   - Cada lâmina é uma peça contínua.
   - Pivô:
     - espadachim: colunas 31–33 opacas na linha 59;
     - discípulo: coluna 32 vazia na linha 59, ponto médio entre os pés em 32,5.
7. **Protótipo.**
   - `?sprites=1` troca as figuras de código pelos sprites, só para jogador em repouso e discípulo em guarda, de frente.
   - `?stage=bambuzal` põe os dois parados no mesmo ponto ao sul do bambuzal central.
   - Cópias dos PNGs em `prototypes/spiritofmurin/assets/`, com o mesmo hash e com a procedência registrada em `assets/provenance.yaml`.
8. **Capturas e correção.**
   - Na primeira captura, os pés do discípulo, com base de 29 px, saíam da sombra de 22 px.
   - A sombra foi alargada para 34 px no modo sprite e as capturas foram refeitas.
   - Conferido nas imagens: espadas inteiras e pés sobre a sombra.

## Desvios em relação à spec (para avaliar jogando)

- **Espadachim:** lâmina com 16 px. A spec pede 22 px, mas a imagem gerada tem a mão mais baixa que a da spec.
- **Discípulo:** aba com ~30 px (spec: 21) e base com 29 px (spec: 14–16). A guarda da imagem gerada é bem mais aberta; mantive as proporções dela.
- O cenário ainda não usa a paleta mestra; só os atores do "depois" usam.

## Onde está

| O quê | Caminho |
|---|---|
| Sprites finais | `assets/art/characters/char_swordsman_idle_s_00.png`, `assets/art/characters/char_disciple_guard_s_00.png` |
| Scripts | `tools/asset-pipeline/clean_chatgpt_sprite.py`, `tools/asset-pipeline/validate_sprite.py` |
| Máscaras, recortes, registro e validação | `assets/source/work/` |
| Evidência | `production/qa/evidence/2026-11-25_asset-001-002_bambuzal_*` (1×, recortes 4× e nota) |
| Prancha lado a lado | `user-docs/relatorios/2026-11-25-antes-depois-bambuzal.html` |
| Registro no guia | art bible 8.4 e 8.12 (cópia anterior em `design/art/historico/2026-11-25-art-bible-v3-antes-da-comparacao.md`) |

## Para jogar no Windows

- **Com os sprites:** `file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html?sprites=1`. Para ver o protagonista em repouso, pare virado para baixo (tecla S).
- **Direto no ponto da comparação:** `file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html?stage=bambuzal&sprites=1`. Sem `&sprites=1`, o mesmo ponto mostra as figuras antigas.
