# Prompt 06: controles no mouse

- **Data:** 2026-11-25
- **De:** o criador, para a sessão principal (sem subagentes)
- **Resultado:** relatório em `../2026-11-25-controles-mouse.md`
- **Contexto:** veio logo depois da opinião sobre o futuro do jogo, em `user-docs/futuro-do game/opiniao-claude.md`. O criador escolheu Godot com C# para as próximas etapas.

## Pedido do criador (texto integral)

```text
Sim, implemente o mouse no protótipo atual. Quero testar estes controles:

- Botão esquerdo: corte, inclusive os cliques em sequência para encadear até três golpes.
- Botão direito: passo, usando a mesma lógica do L. Com o alvo travado, respeite a direção pressionada e as técnicas que o personagem já aprendeu.
- WASD continua movendo, Espaço continua travando o alvo e K continua lançando a onda de qi. Quero poder alternar entre mouse e teclado durante a partida, então mantenha J e L funcionando também.

Na tela de Teclas, permita atribuir um botão do mouse a uma ação e salvar a configuração. Mostre os nomes em português, como "Mouse esquerdo" e "Mouse direito". Durante a partida, o clique direito sobre o jogo deve executar o passo sem abrir o menu do navegador. Nos menus, clicar para selecionar, remapear ou fechar uma tela deve consumir o clique, para o personagem não atacar ao voltar ao jogo.

Teste pelo início normal: clique esquerdo para cortar e encadear, direito para dar o passo, passo lateral com alvo travado depois de aprendido, e K para a onda de qi. Troque uma atribuição na tela de Teclas, feche o navegador e confira se ela continua funcionando ao reabrir. Preserve os saves e as teclas que eu já personalizei.

Registre o resultado em user-docs/relatorios/, atualize o índice e suba a mudança para RaulDuque/SpiritOfMurin. Me passe o link do commit e o endereço para eu jogar no Windows.

Para as próximas etapas, siga a visão que discutimos em user-docs/futuro-do game/opiniao-claude.md. O conteúdo atual serve de base para o prólogo: o jogador experimenta o personagem forte, perde para um chefe e acorda sem equipamento na Planície Verdejante. O terreno da Planície será gerado por semente, aos poucos, conforme a exploração. Cidades e túmulos serão feitos à mão e encaixados nesse mundo. As nove espécies, seus habitats, as ervas e as profissões continuam no escopo da região completa.

Nesta entrega, faça os controles. Depois que eu testar, seguimos para /setup-engine e para o primeiro trecho de geração com coelho, javali, ervas e faca de pedra.
```

Antes, o criador tinha dito: "o jogo é meu passatempo, então data não importa; gosto de C# com Godot, podemos ir seguindo com ele".
