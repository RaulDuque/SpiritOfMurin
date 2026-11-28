# Pedido 16 — T18: armas, roupas e capacetes trocáveis

*Enviado em 2026-11-28. Texto integral do criador, com as respostas dadas durante a sessão. Relatório: [2026-11-28-t18-equipamento.md](../2026-11-28-t18-equipamento.md).*

```text
Pode começar a T18 do SpiritOfMurin — armas, roupas e capacetes trocáveis. Quero conseguir criar uma peça, equipá-la pela bolsa, vê-la no personagem durante a luta e depois abrir o arquivo certo para ajustar o desenho à mão.

Leia GUIA.md, user-docs/tasks/README.md, user-docs/tasks/T18-equipamento-visual.md e docs/elementos/PADRAO.md. Parta da main atual e preserve minhas edições locais. O protagonista que editei em src/Player/Player.cs, com a roupa verde e a faixa da cintura, é a referência aprovada.

1. Guarde o visual original antes de separar as camadas. Capture o personagem no viewport, nas oito direções, usando poses e quadros reproduzíveis de repouso, caminhada, corte, passo e Ki. Depois de separar cabeça/cabelo, roupa e arma na mão, repita as capturas com o mesmo equipamento, direção, quadro de animação e escala. Compare os pixels do personagem. Se aparecer diferença no visual padrão, localize a causa e corrija antes de usar o novo conjunto como referência.

2. Passe pelas quatro decisões abertas da T18 comigo, uma de cada vez:
   - Roupa muda só a aparência, também dá defesa ou tem peso que afeta o Vigor?
   - Armas novas variam em desenho e atributos ou também trazem técnicas próprias?
   - Cada modelo de chapéu pode definir quanto do coque fica coberto?
   - O primeiro conjunto será túnica de viajante, chapéu de bambu trançado e sabre de ferro gasto?

Para as escolhas visuais, use /asset-spec e mostre uma prancha HTML com as peças sobre o meu protagonista, no tamanho real do jogo e ampliadas. Inclua frente, costas e perfil. Quero ver especialmente como a aba do chapéu encontra o cabelo e como a mão segura o sabre. Diga como abrir a prancha no Windows.

3. Implemente o conjunto aprovado pelo percurso normal da campanha. Quero encontrar ou fabricar as peças, abrir a bolsa com I, comparar com o que estou usando e equipar arma, roupa e capacete separadamente. Mostre o ícone e a marca de equipado em cada item. Ao tirar o chapéu, o cabelo e o coque devem reaparecer corretamente. Ao tirar a roupa alternativa, quero recuperar minha túnica verde. Registre onde conseguir cada peça para eu poder testar pelo Painel → Jogar.

4. Use a espada do prólogo e a faca de pedra como os primeiros casos do sistema de armas. Mantenha o funcionamento das ferramentas e dos atalhos da T06. Para o sabre, confira se a empunhadura acompanha a mão durante o combo, se a ordem das camadas funciona quando viro de costas e se o alcance mostrado na bolsa corresponde ao golpe em jogo. Teste a mira livre pelo mouse, o alvo travado e o brilho do Reforço de Ki com a arma nova.

5. Torne jogáveis /create arma, /create roupa e /create capacete. Cada comando deve gerar a definição do item, o desenho editável por código, o ícone, o registro na bolsa e a prévia no personagem. Use o próprio conjunto aprovado para validar o fluxo. Na Inspeção, quero trocar cada peça, girar o personagem nas oito direções e conferir corte, passo e Ki. No guia de edição, mostre três exemplos concretos: onde alongar a lâmina, alargar a faixa de uma túnica e baixar a aba de um chapéu.

6. Valide pelo PainelDeTestes.exe. Com saves isolados e as sementes 20261126 e 777, teste equipar, combinar peças, desequipar, meditar, fechar e continuar. Carregue também tests/fixtures/saves/jornada_antes_do_ki.json e confira se a aparência e o equipamento antigos são recuperados. Rode os testes específicos durante o desenvolvimento e a bateria completa com janela no monitor 2 ao final. Registre separadamente as verificações aprovadas e qualquer execução que termine com código de falha, mesmo se as verificações dela tiverem passado.

Feche com /subir-git. Atualize a T18 e o índice de tasks. Entregue o relatório, a comparação do protagonista padrão antes/depois, uma captura do conjunto equipado e um vídeo curto trocando as peças pela bolsa e usando o sabre. Capture apenas o viewport. Ao final, me passe os links e dois caminhos curtos no executável: Painel → Inspeção para editar o visual e Painel → Jogar para experimentar o equipamento na campanha.
```

**Respostas durante a sessão:**
1. Roupa: "Defesa e peso no Vigor".
2. Armas: "Também técnicas próprias". Sobre a técnica do sabre (Meia-Lua, Saque do Viajante ou Corte Fluido): "GOSTEI DE TODAs podecolocar ela como tecnica  fuuras que vao sendo descobertas".
3. Chapéu e coque: "B: cobre coque e faixa".
4. Conjunto: "Sim, esse conjunto".

Pergunta no meio da sessão: "so uuma coisa todos os equipamentos arvos platantas armas rouupas tudo itens estao sendo feito em arquivos proprios ne para que eu possa editar". A resposta está no relatório, na seção "Arquivos próprios".

Pedido no fim da sessão: "uma  coisa vamos criar um mapa de deploy  um mapa com todos os itens  que eu possa iniciar para testar as nova aparecencias". Foi feito o **mapa de testes** (Painel → "Mapa de testes (todos os itens)"); ver o relatório, seção 5b.
