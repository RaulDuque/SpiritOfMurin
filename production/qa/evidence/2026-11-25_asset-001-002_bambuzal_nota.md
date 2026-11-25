# Nota da captura — ASSET-001 e ASSET-002 no bambuzal (2026-11-25)

- Página: `prototypes/spiritofmurin/index.html`
- Antes: `?stage=bambuzal` · Depois: `?stage=bambuzal&sprites=1`
- Seed: a padrão do `config.js` (nenhuma `seed=` no endereço)
- Ponto: jogador em (34,4 × 32, 28,3 × 32) px, discípulo do vale (d2) em (36,7 × 32, 28,3 × 32) px, os dois virados para S (face = π/2), discípulo parado (`static`); câmera começa no alvo.
- Tempo: 1,5 s de página (`.claude/scripts/page-shot.sh --size 640x360 --wait 1500 --full`), janela 640×360, escala 1×.
- Recortes 4×: região x 270–440, y 120–215 das capturas 1×.
- Conferido olhando: as duas lâminas inteiras; os pés dos dois sobre a sombra do chão. Na primeira captura, os pés do discípulo saíam da sombra; ela foi alargada para 34 px no modo sprite e as capturas foram refeitas.
- Hash dos sprites (sha256, 16 primeiros): espadachim `f85cadc952626fc8`, discípulo `f54b8aa8897dbb5f` (iguais em `assets/art/characters/` e na cópia do protótipo).
