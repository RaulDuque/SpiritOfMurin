# Relatórios e prompts

Documentação de cada sessão de trabalho para o criador revisar: o que foi decidido, o que ficou pendente e os pedidos (prompts) enviados aos agentes, com as respostas.

## Índice

| Data | Arquivo | Conteúdo |
|---|---|---|
| 2026-11-25 | [2026-11-25-art-bible-sessao.md](2026-11-25-art-bible-sessao.md) | Relatório da sessão do art bible: decisões por seção, retorno sobre o protótipo e pendências |
| 2026-11-25 | [2026-11-25-ad-art-bible-revisao.md](2026-11-25-ad-art-bible-revisao.md) | Revisão final do art bible (AD-ART-BIBLE): veredito, achados e correções propostas |
| 2026-11-25 | [2026-11-25-escolhas-visuais.html](2026-11-25-escolhas-visuais.html) | Prancha visual (abrir no navegador): cores da vida no HUD, famílias de cor, referência do protagonista e medidas de repouso, correções de texto |
| 2026-11-25 | [prompts/01-secao3-revisao-personagens.md](prompts/01-secao3-revisao-personagens.md) | Pedido ao diretor de arte: redesenho do protagonista e do discípulo (Seção 3) |
| 2026-11-25 | [prompts/02-secao9-referencias.md](prompts/02-secao9-referencias.md) | Pedido ao diretor de arte: referências (Seção 9) |
| 2026-11-25 | [prompts/03-correcoes-secoes-1-e-2.md](prompts/03-correcoes-secoes-1-e-2.md) | Pedido ao diretor de arte: correções da revisão final |
| 2026-11-25 | [2026-11-25-asset-spec-poses.html](2026-11-25-asset-spec-poses.html) | Prancha visual (abrir no navegador): espada do protagonista em repouso (A/B/C), braço do discípulo, cores do discípulo, teste do preto chapado |
| 2026-11-25 | [prompts/04-asset-spec-poses.md](prompts/04-asset-spec-poses.md) | /asset-spec: pedidos ao diretor de arte e ao artista técnico, respostas e onde estão os dois prompts de imagem |
| 2026-11-25 | [2026-11-25-paleta-mestra.html](2026-11-25-paleta-mestra.html) | Prancha visual da paleta mestra (61 cores + 3 de reserva): tira, cena de teste, famílias, conferência automática. Aprovada; arquivo em `assets/palettes/pal_master_64.gpl` |
| 2026-11-25 | [2026-11-25-limpeza-sprites-e-antes-depois.md](2026-11-25-limpeza-sprites-e-antes-depois.md) | Limpeza das imagens do ChatGPT para 64×64, validação, integração `?sprites=1`, desvios da spec e endereços para jogar |
| 2026-11-25 | [2026-11-25-antes-depois-bambuzal.html](2026-11-25-antes-depois-bambuzal.html) | (Histórico: o criador escolheu a aparência original) Prancha lado a lado (abrir no navegador): antes e depois no bambuzal, recortes 4×, original × sprite, conferência automática |
| 2026-11-25 | [2026-11-25-ciclo-mil-caminhos.md](2026-11-25-ciclo-mil-caminhos.md) | Fim do experimento dos sprites (fica a aparência original) e primeiro ciclo de "Mil Caminhos do Dao": o que existia, o que mudou, o que foi verificado jogando, endereço e controles. **Sem a solução do túmulo** |
| 2026-11-25 | [2026-11-25-ciclo-mil-caminhos-SOLUCAO.md](2026-11-25-ciclo-mil-caminhos-SOLUCAO.md) | **Solução** da descoberta (ponte, vale, túmulo, pegadas, revanche). Abrir só depois de jogar |
| 2026-11-25 | [2026-11-25-meditar-e-continuar.md](2026-11-25-meditar-e-continuar.md) | Meditar e continuar a jornada: ponto de meditação nas pedras gêmeas, gravação local (formato 1), Continuar e Novo jogo no título, testes fechando o navegador de verdade (Edge e Chrome), limitações. Sem spoiler |
| 2026-11-25 | [prompts/05-meditar-continuar.md](prompts/05-meditar-continuar.md) | Pedido do criador para a etapa "Meditar e continuar a jornada" (texto integral) |
| 2026-11-25 | [../futuro-do game/opiniao-claude.md](../futuro-do%20game/opiniao-claude.md) | Opinião do Claude sobre o futuro do jogo: mundo gerado por semente + lugares feitos à mão, prólogo, Godot com C# |
| 2026-11-25 | [2026-11-25-controles-mouse.md](2026-11-25-controles-mouse.md) | Controles no mouse (esquerdo = corte, direito = passo), tela de Teclas com coluna de mouse, cliques nos menus não vazam para o jogo; testes e regressões |
| 2026-11-25 | [prompts/06-controles-mouse.md](prompts/06-controles-mouse.md) | Pedido do criador para os controles no mouse e o rumo das próximas etapas (texto integral) |
| 2026-11-25 | [2026-11-25-passar-atras-do-cenario.md](2026-11-25-passar-atras-do-cenario.md) | Colisão só na base de pedras soltas, pinheiros e bambus; silhueta do personagem atrás do cenário; testes e regressões |

## Prompts de imagem do criador

Ficam junto às imagens, em `user-docs/assets/`:
- `user-docs/assets/protagonista/promp.md`
- `user-docs/assets/mundo/mundo-promp.md` (vazio em 2026-11-25)

Os prompts de imagem do `/asset-spec` (protagonista em repouso, discípulo em guarda) estão completos em `design/assets/specs/approval-poses-assets.md`; o resumo e os pedidos aos agentes estão em `prompts/04-asset-spec-poses.md`.
