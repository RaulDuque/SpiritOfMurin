# Prompt 11: recuperação do protótipo e melhoria da interface

- **Data:** 2026-11-26
- **De:** o criador, para a sessão principal
- **Resultado:** `user-docs/relatorios/2026-11-26-recuperacao-prototipo.md`

## Pedido do criador (texto integral)

```text
Quero uma entrega de recuperação do protótipo e melhoria da interface do SpiritOfMurin no Godot. Minha prioridade agora é conseguir jogar o Vale com a ponte, as pistas, o minimapa e o Livro de Técnicas que eu já conhecia, usando controles configuráveis e uma tela cheia que realmente aproveite o monitor.

A aparência atual dos personagens continua aprovada. Preserve a campanha, a Planície procedural, a progressão e os saves existentes. Se a entrega de dia e noite, vento, pássaros, terreno e água já estiver em andamento, confira o estado do Git e preserve o trabalho feito. Priorize as correções abaixo antes de iniciar outra expansão.

1. Compare o Godot com o protótipo jogável

Leia o último relatório de tela cheia e mapa em user-docs/relatorios/. Abra o protótipo HTML, percorra o trecho da ponte e consulte estas referências:

- production/qa/evidence/2026-11-25_meditacao/regressao-ciclo/03-ponte-corte-aparado.png
- production/qa/evidence/2026-11-25_meditacao/regressao-ciclo/05-mapa-apos-recuo.png
- production/qa/evidence/2026-11-25_meditacao/regressao-ciclo/09-mapa-apos-manual.png
- production/qa/evidence/2026-11-25_meditacao/regressao-ciclo/17-livro.png

Para o minimapa e os controles, procure também a implementação e as demais evidências em production/qa/evidence/. Compare como cada recurso aparece, como o jogador o usa e como ele reage ao progresso. Use essa comparação para implementar as correções nesta entrega.

2. Corrija a tela cheia no caminho que eu uso no Windows

Eu abro project.godot no Godot .NET e aperto F5. Na minha captura, o jogo continua como um retângulo central com margens pretas nos quatro lados. O relatório anterior verificou uma janela independente em 1600×900 e registrou que a aba Game do editor ficou sem medição.

Reproduza pelo F5, confira se o jogo está incorporado ao editor ou em janela própria e teste o F11 pelo mesmo caminho. Compare com a execução independente. Meça a área disponível, a área realmente desenhada e as margens. Investigue o que limita o tamanho, inclusive o modo incorporado, o viewport, a escala e qualquer contêiner com tamanho fixo.

Em um monitor 16:9, quero o mundo ocupando a área jogável inteira, com personagens proporcionais e texto nítido. A interface deve acompanhar o redimensionamento. Confira os cliques nos cantos depois de alternar entre janela, maximizada e tela cheia. Se houver uma configuração local do editor necessária para o F5 abrir desse jeito, me dê o caminho exato em português e diga qual opção escolher. Se conseguir testar apenas a janela independente, registre o F5 como pendente.

3. Traga de volta o minimapa e as pistas no mapa de M

Durante a exploração, quero ver um minimapa discreto no canto da tela, tanto no Vale quanto na Planície. Recupere os sinais visuais do protótipo para eu reconhecer minha posição, a direção em que estou olhando e os pontos de referência descobertos. Na Planície, o minimapa deve acompanhar os blocos gerados e conservar a exploração quando eu voltar ou carregar a partida. Confira a posição dele junto da semente e do HUD em diferentes tamanhos de janela.

Ao apertar M, quero poder responder a duas perguntas: “Onde estou?” e “Qual foi a última pista que encontrei?”. Na ponte, registre a pista que aparece quando eu recuo. Depois de encontrar a metade do manual, atualize o mapa e o texto conforme a referência dessa etapa. As pistas devem vir dos eventos reais da campanha e respeitar o que descobri. Compare o mapa antes do encontro, depois do recuo e depois do manual, inclusive após salvar e continuar.

4. Recupere a ponte e a leitura da Guarda Fechada

Compare a ponte do Vale no HTML e no Godot a partir da mesma aproximação. Confira o desenho da travessia, as margens do rio, as colisões, o posto do discípulo e o enquadramento do duelo. Quero reconhecer a ponte ao chegar e entender pelo movimento do discípulo quando meu corte frontal foi aparado. Recupere o arco da guarda, a reação ao impacto e a abertura pelo flanco conforme o comportamento do protótipo. Jogue o trecho desde a aproximação até o recuo e a revanche.

5. Coloque a troca de controles em Esc → Controles

Transforme a tela atual de ajuda em uma tela utilizável com mouse e teclado. Para cada ação, mostre o nome em português, a tecla atual e o botão de mouse, quando aplicável. Quero clicar em “Alterar”, pressionar a nova entrada e já poder usá-la. Inclua cancelar, avisar quando outra ação já usa aquela entrada e restaurar os padrões.

Salve as preferências ao fechar o jogo. Os atalhos mostrados no HUD, no mapa, no livro e nas dicas de combate precisam acompanhar a troca. Teste um caso completo: trocar o corte, fechar o painel, atacar pela nova entrada, reiniciar o jogo e conferir se ela continua funcionando. O clique usado para fechar o menu deve terminar no menu, sem virar um ataque.

6. Adicione Esc → Modo de assistência

Quero uma opção “Mostrar áreas de combate”, que eu possa ligar e desligar durante a partida. Quando ligada, quero enxergar o arco protegido pela guarda ou escudo, a área ativa do golpe e a direção de ataques anunciados, como a investida do javali. A abertura de flanco precisa ser fácil de identificar. Diferencie aviso, golpe ativo e recuperação pela forma ou pelo movimento, além da cor.

Desenhe os indicadores a partir dos mesmos dados de alcance, direção e tempo usados pelo combate. Se um corte aparece fora do arco de guarda, o resultado precisa ser coerente com o que estou vendo. Essa preferência deve ser salva. Quando eu desligar a assistência, mantenha as pistas normais que fazem parte do jogo, como a postura, o arco breve da aparada, a poeira do casco e a reação ao acerto. Teste o mesmo duelo com a assistência ligada e desligada.

7. Melhore o Livro de Técnicas e a organização do HUD

Use 17-livro.png e o livro do HTML como referência de conteúdo e identidade. Preserve técnicas, fragmentos do manual, inscrições, combinações descobertas e pistas. Integre a história atual: o que eu aprendi no Vale continua registrado quando acordo na Planície. Quero entender o que foi selado e o que voltou enfraquecido no nível 3.

Aproveite o espaço do painel. Em tela larga, use uma lista navegável de um lado e a página selecionada do outro. Mostre nome, estado, descrição e comando atualizado. Quando fizer diferença para jogar, mostre custo de Qi e Vigor, alcance ou efeito da versão enfraquecida usando os valores reais do jogo. Em janela menor, adapte a disposição e permita rolagem. Dê suporte a clique, teclado e retorno por Esc.

Revise também o HUD e o rodapé. Preserve a Vida em porcentagem que pedi. Garanta contraste e espaço para ler Vida, Vigor, Qi e a ação contextual sem cobrir o combate. Deixe a lista completa de comandos acessível na pausa.

8. Valide pelo percurso normal e feche a entrega

Faça um percurso jogável: título → Vale → ponte → corte aparado → recuo → mapa com a primeira pista → manual → mapa atualizado → Livro de Técnicas. Confira o minimapa enquanto caminha. Teste também um save da Planície, a retomada da exploração e o livro com as técnicas seladas ou enfraquecidas.

Guarde comparações lado a lado do protótipo e do Godot para a ponte, as duas etapas do mapa e o livro. Para a tela cheia, capture o monitor inteiro, sem recortar as bordas, e identifique resolução, modo de execução e área desenhada. Grave um vídeo curto abrindo o Esc, trocando um controle e ativando a assistência antes de enfrentar o discípulo. Rode os testes existentes e acrescente cobertura para as regressões encontradas.

Atualize o índice e publique o relatório em user-docs/relatorios/. Explique a causa confirmada das bordas, o que foi recuperado do protótipo, como abrir cada recurso e o que ainda depende de conferência manual. Feche com /subir-git, commits atômicos e push. No fim, me entregue os links do relatório, das evidências e dos commits, mais um roteiro curto para eu conferir tudo pelo F5 no meu PC.
```
