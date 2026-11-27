# Prompt 13: mira livre, técnicas secretas, bolsa visual, moedas e ambientação

- **Data:** 2026-11-26
- **De:** o criador, para a sessão principal
- **Decisões tomadas ao começar** (perguntas respondidas pelo criador):
  - Os itens 1 a 6 ("ambientação descrita acima") não vieram no texto. A ambientação usada é a **entrega 4 da campanha** (prompt 10): dia e noite, fogueira, vento, pássaros, terreno variado, transições, água rasa e funda, grama alta.
  - As mudanças manuais do criador no `Player.cs` foram enviadas antes, como estavam (commit `642da08`), e o trabalho seguiu por cima delas.
- **Resultado:** um relatório por entrega em `user-docs/relatorios/` (o índice lista todos).

## Pedido do criador (texto integral)

```text
Além da ambientação da Planície Verdejante descrita acima, quero as seguintes melhorias jogáveis no SpiritOfMurin. Organize o trabalho em entregas que eu consiga testar separadamente pelo PainelDeTestes.exe. Continue até concluir todas.

7. Mira livre pelo mouse e prioridade do alvo travado

Quando eu estiver sem alvo travado, quero poder andar para a esquerda, apontar o mouse para a direita e atacar para a direita. Isso vale para a espada, a faca e o Disparo de Qi, inclusive quando aciono o ataque pela tecla remapeada. Calcule a direção desde o personagem até o cursor no mundo, considerando a câmera, o zoom e o tamanho da janela. A pose, a lâmina, o projétil e a região que realmente acerta devem concordar.

Com um alvo travado pelo Espaço, o ataque usa a direção do alvo. Ao destravar, a mira volta ao cursor já no próximo ataque. Preserve os comandos relativos ao oponente, como o passo lateral e a estocada. Em Esc → Controles, mantenha disponível a opção de mirar pela direção do personagem para quem joga só com teclado. Clicar em um item ou fechar um painel deve consumir aquele clique na interface.

8. Técnica secreta — Disparo de Qi carregado

Quero descobrir esta técnica experimentando o botão que já uso para o ataque à distância. Um toque rápido ainda solta o disparo comum. Se eu mantiver o botão pressionado, o personagem concentra Qi na palma e segura o disparo até eu soltar. Enquanto carrega, posso ajustar a mira. Mostre a concentração na mão e o consumo na barra para eu perceber a força que estou acumulando.

O dano cresce com a quantidade de Qi investida. Ao investir exatamente 100 de Qi, a carga está completa. O próximo disparo carregado que atingir uma guarda ativa rompe essa defesa e abre uma janela perceptível para continuar o ataque. Manter o botão pressionado depois de completar a carga conserva o disparo pronto. Os 100 são o custo total dessa carga, já incluindo o custo do disparo. Com menos de 100 disponíveis, posso soltar a carga parcial com dano proporcional ao investimento. Uma carga parcial não garante ruptura.

Use os mesmos vínculos de controle configurados pelo jogador. Na Planície, respeite a ação contextual de esfolar com Qi junto à carcaça e as regras de selamento do Disparo de Qi. Abrir o inventário, pausar ou perder o foco da janela durante a carga deve cancelar a preparação sem lançar um projétil ao voltar. Documente o tratamento do Qi já investido e impeça cobrança dupla.

Quero que a técnica seja secreta de verdade. Antes da descoberta, preserve o mistério do comando. Na primeira execução bem-sucedida, mostre o reconhecimento e registre a técnica no Livro, com o comando remapeado, os custos e a diferença entre carga parcial e ruptura. Verifique também a Guarda Fechada da ponte: quero que a ruptura funcione quando merecida e que o recuo, a pista do túmulo e a solução pelo flanco continuem disponíveis.

9. Técnica — Reforço de Qi

Segurar Shift junto com o botão de ataque corpo a corpo concentra Qi na arma. A combinação deve funcionar com J, clique esquerdo ou o botão que eu configurar. Ao completar o gesto, gaste 75 de Qi uma única vez e reforce os próximos 3 ataques corpo a corpo. Cada ataque reforçado causa 10% a mais de dano e tem 5% de chance de romper uma defesa que atingir.

Quero ver o Qi percorrer a arma e um indicador simples de cargas: 3, 2, 1. Cada golpe executado gasta uma carga, inclusive se errar. Cada corte de um combo conta como um golpe. Faça uma única rolagem de ruptura por golpe contra a guarda atingida, para a chance não se multiplicar pelo número de quadros de contato. Compare o dano antes e depois do arredondamento: mesmo com a faca inicial, o bônus precisa funcionar no cálculo real.

Com menos de 75 de Qi, dê retorno claro e mantenha o saldo. Segurar a combinação continuamente deve ativar a técnica apenas uma vez. Enquanto houver cargas, preserve as restantes ao receber outra tentativa de ativação. Shift sozinho continua permitindo andar quieto e os comandos de passo continuam funcionando. Permita configurar o modificador em Esc → Controles. Registre a técnica no Livro quando for descoberta e preserve a descoberta ao meditar e continuar.

10. Inventário visual inspirado no protótipo

Eu gostava de abrir o inventário antigo e enxergar os itens que tinha comigo. Abra a mochila do protótipo HTML com itens dentro e use essa tela como referência. Quero reconhecer a faca, a erva, a carne, a pele, o sílex e o galho pela figura. Mostre o ícone de cada item, a quantidade da pilha e uma marca clara no equipamento em uso.

Ao selecionar um item, mostre nome, descrição curta e as ações possíveis naquele momento, como equipar, usar, comer ou fabricar. Quero conseguir abrir a bolsa, encontrar a erva e usá-la pelo mouse ou pelo teclado. Ao fabricar a faca, devo ver os materiais diminuírem e a faca aparecer na mesma bolsa. Preserve os itens, as quantidades, as receitas e o equipamento das jornadas já salvas. Teste a leitura em janela e em tela cheia. Entregue uma comparação do inventário antigo com a nova bolsa preenchida.

11. Dinheiro da jornada

Quero começar a construir a economia que depois será usada em vilas, cidades e comerciantes. Nesta etapa, use uma moeda simples, com saldo inteiro visível na bolsa, ícone próprio e o nome provisório “Moedas”. No despertar da Planície, o personagem começa sem dinheiro, coerente com a perda dos pertences. Jornadas antigas que ainda não têm esse campo devem carregar normalmente.

Quero testar o dinheiro durante o jogo. Coloque uma pequena bolsa de moedas em um ponto reconhecível da Planície e diga no relatório onde encontrá-la. Ao pegá-la com E, mostre “+N moedas” e atualize o saldo na hora. Voltar ao bloco, meditar, fechar e continuar deve preservar tanto o saldo quanto o fato de a bolsa já ter sido recolhida. Prepare uma operação para receber e outra para gastar, com recusa por saldo insuficiente. As lojas, os preços e os comerciantes ficam para a etapa das vilas.

12. O que quero conseguir testar pelo executável

Inclua os novos testes no PainelDeTestes.exe e no relatório que ele já gera. Mantenha os saves de teste separados da minha jornada. Desta vez, execute também o percurso com janela a partir do próprio painel, porque a última validação do executável cobriu o modo sem janela.

• Mira: andar para um lado e acertar para o outro, travar um alvo enquanto o cursor aponta em outra direção e destravar voltando ao cursor.
• Disparo: toque rápido, carga parcial, carga completa de 100, guarda rompida, saldo abaixo do necessário e pausa durante a preparação. Confira se o gesto sempre produz no máximo um projétil.
• Reforço: ativar com 75, tentar com 74, conferir os três golpes e o quarto já normal. Use testes reproduzíveis para os resultados de sucesso e falha da chance de 5%.
• Bolsa e dinheiro: equipar a faca, consumir uma erva, recolher moedas, voltar ao bloco e carregar o save. Confira também um save criado antes desta entrega.
• Ambientação: mantenha o percurso de dia e de noite nas duas sementes com os critérios do pedido anterior.

Ao fechar cada entrega, atualize user-docs/relatorios/ e o índice, siga /subir-git e envie os commits. No relatório final, inclua um roteiro curto para eu abrir PainelDeTestes.exe → Jogar e experimentar cada novidade por conta própria. Para as técnicas secretas, deixe o roteiro detalhado em uma seção separada, para eu poder tentar descobri-las primeiro.
```
