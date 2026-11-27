# Prompt 14: ajustes de atributos e Ki, organização do projeto, criação de conteúdo e Entrega 5

- **Data:** 2026-11-27
- **De:** o criador, para a sessão principal
- **Decisões tomadas ao começar** (perguntas respondidas pelo criador):
  - **Qi → Ki** no texto **e** no código. As chaves antigas do save (`qi`) continuam sendo lidas.
  - **FOR entra na ficha.** Isso substitui a decisão anterior "sem FOR" que aparece no item 6.A do pedido abaixo.
  - Números iniciais (proposta padrão, editáveis em `assets/data/plains/plains_progression.json`): FOR +6% de dano corpo a corpo por ponto; CON −3% de dano recebido por ponto (teto 45%), além do vigor; crítico 5% + 1% por ponto de SOR (teto 40%), ×1,5.
- **Ordem:** primeiro os ajustes rápidos da mensagem (entrega 14.0); depois os itens 1 a 8 do pedido 14.

## Mensagem do criador (texto integral)

```text
UM COSA TROCA O NOME DE QI PARA KI E COLOCA UM STATUO DE FOR (FORÇA) PARA PARA PDOER AUMENTAR O DANO E TAMBEM pode colocar a vida do inimigos   em numeros detro da barras de vida tipo se ooponte tem 150de vida ela fica barrad e vidda e la dentre 150/150  e se ele toma dando dimuida barra dos valores tambem colocar no modo de assitencia para mostar quanto de dado causa e leva e m cada ataque,  chnce de crititco  e  constituiçao aumenta defesa  ealem disso tudo 
```

## Pedido 14 (texto integral, colado pelo criador)

```text
PEDIDO 14 — Organização do SpiritOfMurin, criação de conteúdo e planejamento das próximas entregas

A ordem que eu quero agora é esta: primeiro organize as tarefas e estabeleça um padrão que me permita criar e modificar os elementos do jogo. Em seguida, use esse padrão na Entrega 5, com árvores, pedras, bambus e ferramentas. Prepare também os prompts das entregas futuras para que possamos continuar de outra sessão sem perder minhas ideias.

Gostei das mudanças que fiz pessoalmente no protagonista. A versão atual de src/Player/Player.cs é minha referência visual aprovada. Quero ter essa mesma liberdade com o restante do jogo. Minha experiência ideal é conseguir perguntar “onde eu mudo a copa deste pinheiro?”, abrir o arquivo indicado, editar o desenho, apertar para testar e ver exatamente o que mudou.

Leia o CLAUDE.md, os documentos em user-docs/futuro-do game/, os prompts anteriores, o índice de user-docs/relatorios/, os três relatórios visuais e o relatório final do pedido 13. Confira o estado real da branch e do diretório de trabalho. Use o commit d856c96 como referência da entrega que recebi e considere o que tiver sido acrescentado depois. Preserve minhas edições manuais.

1. CRIE UMA PASTA DE TASKS QUE SEJA O PONTO DE RETOMADA DO PROJETO

Crie user-docs/tasks/ com um README.md que eu consiga abrir e entender em poucos minutos. Nele quero ver o que está pronto, o que está parcial, qual tarefa está em andamento, o que posso testar agora, o que depende de decisão minha e qual prompt devo mandar em seguida.

Dê a cada tarefa um identificador estável, um nome descritivo e um arquivo próprio quando houver trabalho suficiente para justificar isso. Dentro dele registre o objetivo do jogador, o estado atual, o que falta, as dependências, as decisões abertas, os critérios de aceite, o roteiro de teste e um prompt pronto para executar aquela entrega. Quando terminar, ligue a tarefa ao relatório, às evidências e aos commits. Aproveite os documentos existentes por referência para manter uma única versão de cada decisão.

Faça a conciliação dos pedidos anteriores. Por exemplo, já há pássaros de ambientação e vento na Planície, mas ainda quero aves com comportamento próprio e variação de clima. Já há relatos do coelho fugindo e do javali avisando a investida, enquanto o levantamento mais recente classifica parte disso como pendente. Rode o jogo e identifique, por comportamento, o que já existe, o que precisa ficar mais visível e o que realmente falta implementar. As 494 verificações são a referência da última rodada aprovada.

2. DEFINA A REGRA DE ORGANIZAÇÃO DOS ELEMENTOS

Registre no CLAUDE.md uma regra curta para as próximas entregas e escreva o padrão completo em um documento de referência. Todo elemento deve ter um nome estável e um lugar fácil de localizar e editar. Isso vale desde uma pedra até o Livro de Técnicas.

Para cada elemento ou família, quero conseguir localizar:
- o desenho, a paleta, as dimensões, o pivô e as animações;
- os dados editáveis, como vida, alcance, velocidade, resistência, peso, custo e materiais;
- o comportamento no mundo, a interação, a colisão e a ordem de desenho;
- o ícone e as informações que aparecem na bolsa, HUD, mapa ou livro;
- onde o elemento é colocado no Vale feito à mão ou gerado por habitat e semente;
- o identificador usado pelo inventário e pelo save, quando aplicável;
- a forma mais rápida de mudar algo e conferir o resultado dentro do jogo.

Aplique isso a personagens, inimigos humanos, animais, chefes, NPCs, armas, ferramentas, roupas, capacetes, técnicas, itens, ervas, árvores, bambus, arbustos, pedras e objetos interativos. Na interface, organize HUD, bolsa, fabricação, ficha, mapa, minimapa, livro, pausa e controles por responsabilidade.

Famílias podem compartilhar código. Um novo coelho pode reutilizar a fuga para a toca, mas precisa ter aparência e parâmetros próprios. Uma nova árvore pode reutilizar o sistema de corte e ter outra copa, resistência e tabela de materiais. Uma técnica nova deve usar o mesmo sistema que resolve o custo e o acerto que aparecem no Livro. Escolha a estrutura de pastas depois de examinar o projeto atual e mostre exemplos com caminhos reais.

Faça a migração em partes verificáveis. Comece pelos elementos usados na próxima entrega e indexe os demais com sua localização atual e a tarefa que vai organizá-los. Se mover o desenho do protagonista para um arquivo próprio, preserve minha versão pixel por pixel e faça uma comparação no mesmo cenário e pose. Preserve IDs e saves ao reorganizar arquivos.

3. ESCREVA UM GUIA QUE EU POSSA USAR PARA EDITAR O JOGO À MÃO

Quero um relatório completo em português em user-docs/relatorios/, ligado ao índice. Inclua uma tabela “Quero mudar isto → abro este arquivo → edito este trecho → testo desta forma”. Use exemplos que existam de verdade depois da reorganização.

Mostre passo a passo como:
- mudar a silhueta, a cor e o balanço de uma árvore ou de um bambu;
- mudar as orelhas, a cor, a animação e a distância de fuga do coelho;
- criar uma arma com desenho, empunhadura, ícone, alcance e dano;
- criar roupa ou capacete e conferir as camadas sobre o protagonista de frente, de costas e de perfil;
- criar um inimigo com aviso, golpe ativo, recuperação e materiais;
- criar um item ou técnica e fazê-lo aparecer na bolsa ou no Livro;
- ajustar um componente da interface sem procurar em todo o HUD.

Diferencie o que já pode ser feito hoje do que depende de uma tarefa futura, como equipamento visual de cabeça ou vendedores. Para cada exemplo já suportado, dê um caminho curto de prévia. Se uma pequena cena de inspeção facilitar isso, aproveite a infraestrutura de testes e permita abrir o elemento diretamente, trocar a direção ou o estado e vê-lo no tamanho real do jogo.

4. CRIE UM COMANDO DE AUTORIA PARA O CLAUDE CODE

Quero poder escrever uma descrição em português e receber uma base que siga o padrão do projeto. Confira como os comandos e skills deste repositório são descobertos pelo Claude Code e implemente a forma utilizável de /create <tipo> <descrição>. Documente a sintaxe exata que funcionar na minha instalação.

Exemplos de como quero usar:
  /create inimigo "Discípulo errante com lança curta, que recua depois de errar a estocada"
  /create mob "Raposa ruiva que observa de longe e foge para os arbustos"
  /create arvore "Salgueiro de margem, com galhos baixos que balançam com o vento"
  /create arma "Sabre de ferro gasto, de uma mão, com faixa verde na empunhadura"
  /create roupa "Túnica de viajante com abertura lateral"
  /create capacete "Chapéu de bambu trançado"

Para cada tipo, o comando deve encontrar o exemplo mais próximo, aproveitar os sistemas existentes e criar os arquivos específicos, os dados iniciais, o registro necessário e uma maneira de visualizar ou testar. Quero que a resposta diga exatamente quais arquivos ele criou e onde eu posso mexer. Se um elemento com o mesmo nome já existir, preserve o original e trate explicitamente a diferença entre criar uma variante e editar o existente.

Se o sistema de uma categoria ainda estiver planejado, o comando pode gerar a ficha e a tarefa de integração. Identifique esse resultado como planejamento. Para considerar um elemento jogável, ele precisa carregar, aparecer na situação prevista e responder à interação descrita. Valide o fluxo com uma variante de árvore usada na Entrega 5 e com um segundo tipo em uma cena de teste isolada.

5. PRIMEIRA ENTREGA JOGÁVEL — ÁRVORES, PEDRAS, BAMBUS E ARBUSTOS

Retome a Entrega 5 da campanha. Quero sair da fogueira, recolher sílex e galhos, fabricar e equipar uma ferramenta de corte e outra de mineração, encontrar um recurso e trabalhar nele.

Árvores: o golpe alcança o tronco, aparecem lascas ou entalhes, o progresso fica legível e o último golpe derruba a árvore ou deixa um toco. Atualize a colisão junto com a mudança visual. Varie troncos e copas. Confira a passagem por trás da copa e a volta para a frente.

Pedras: diferencie rocha decorativa de rocha explorável. Faça as rachaduras crescerem conforme os golpes válidos. Coloque materiais e chances em dados editáveis. Quero pedra comum e a possibilidade de um achado mineral coerente com a rocha. SOR influencia a chance de material bônus. Dê à madeira e à pedra pelo menos um uso já testável.

Bambus: quero poder distinguir uma haste que aceita corte, vê-la ceder e recolher bambu. Use as regras compartilhadas de ferramenta, alcance e coleta onde fizer sentido, com desenho e reação próprios.

Arbustos: acrescente reação de contato ao vento que já existe. Quando um coelho atravessar uma moita, quero ver as folhas se abrirem ou inclinarem na direção da passagem e depois voltarem ao repouso. Faça o mesmo funcionar com o jogador e com outros animais que cruzarem a vegetação. O efeito deve continuar compreensível durante uma rajada e quando dois corpos passarem próximos.

Relacione o estado de cada recurso com a semente e sua identidade no mundo. Verifique um recurso parcialmente trabalhado e outro já esgotado ao sair do bloco, voltar, meditar, fechar e continuar. Cada ruptura libera os materiais uma única vez. Preserve a travessia da ponte, a clareira inicial, a legibilidade das pegadas e o espaço de fuga dos animais.

6. CRIE OS PROMPTS DAS ENTREGAS FUTURAS

Deixe cada frente abaixo com tarefas, dependências, decisões de design e critérios jogáveis. Use as fichas antigas de fauna, flora, habilidades, variantes e materiais como fonte. Quero que essas ideias fiquem registradas mesmo quando a implementação for acontecer mais tarde.

A. Inventário, ficha, HUD e escolha de interação
Complete a Entrega 6. Inclua categorias, aba de fabricação, comparação com o equipamento atual, atalhos configuráveis, H para a erva, botões “+” clicáveis e prévia de atributos. Confira VIT, INT, CON, AGI, SOR e a relação de QI com a reserva de energia. A decisão registrada para esta ficha é sem FOR. Preserve Vida em porcentagem no HUD e ofereça o valor completo ao inspecionar, por exemplo “Vida 50% · 6/12”. Planeje rodapé contextual, ganhos e XP agrupados perto do personagem e escolha clara quando uma erva, pegada ou carcaça disputam a mesma interação.

B. Combate, rastreamento, fauna e chefes
Complete a Entrega 7 com o javali e o coelho reais do jogo. Separe o aviso, o impacto, a aparada e a recuperação. Confira a barra do animal focado, a diferença visual das técnicas enfraquecidas e a leitura dos avisos mesmo com vento e chuva. As pegadas devem acompanhar o deslocamento do animal, e a precisão da informação deve crescer com Rastreamento.

Planeje lobos em alcateia e o Alfa, raposas, patos, insetos, cervos com defensores, víboras, faisões, tartarugas, falcões e as variantes raras já descritas. Inclua novos inimigos humanos, elites e chefes. Para cada um, registre habitat, horário, reação ao jogador e a outras espécies, movimento, sinais de ataque ou fuga, abertura para resposta, materiais e raridade. Use os pássaros ambientais como base visual para distinguir um bando distante de uma ave com a qual posso interagir.

C. Clima e corpos d’água
Planeje calmaria, vento forte, chuva leve, chuva forte, trovoada e raios. Defina transições, duração, sinais visuais e sonoros e como o clima interage com o horário, a visibilidade e os animais. Registre como decisão separada se raios podem causar dano. Planeje lagos reconhecíveis no mapa, margens, áreas rasas e profundas, peixes e patos. Inclua um teste em que eu consiga observar um peixe no lago e ver um pato sair da margem para a água.

D. Escavação, cavernas e dungeons de tesouro
Planeje a pá como ferramenta para abrir buracos em solos apropriados e procurar materiais ou minérios. Diferencie escavar terra, minerar rocha exposta e explorar uma caverna. Defina pistas no terreno, estados persistentes, compatibilidade com a geração em blocos e proteção de entradas e pontos de missão.

Quero cavernas como cenários separados, com entrada e retorno à mesma posição da superfície. Dentro delas pode haver fauna, inimigos, chefes e uma passagem para uma dungeon de tesouro. Planeje salas, armadilhas legíveis, rotas de retorno e recompensas como materiais, moedas, ouro e técnicas. A semente e o save devem lembrar o que já explorei, abri ou recolhi. Faça um prompt específico para uma primeira caverna jogável e outro para a primeira dungeon.

E. Nível 10, reinos de cultivo e missão de avanço
Ao alcançar o nível 10, o personagem chega ao limite do reino de cultivo atual. Ele continua podendo caminhar, lutar, caçar e cumprir a missão de avanço. O nível 11 só fica disponível depois de concluir essa missão. Mostre na ficha qual é o reino, qual limite foi alcançado e onde encontrar a pista da missão.

Preserve a recuperação enfraquecida das técnicas no nível 3. Separe nível do personagem, reino de cultivo, proficiência de habilidade e nível de criatura. Deixe para eu decidir o nome dos primeiros reinos, a missão de ruptura e o destino da XP recebida durante a trava. Inclua testes de nível 9 para 10, ganho de XP travado, conclusão da missão, liberação única, save e retomada.

F. NPCs, biomas, vilarejos e grandes cidades
Planeje NPCs encontrados pelo mundo, seus papéis, diálogos, missões e ligação com os caminhos gerados. Quero biomas com identidade própria e assentamentos feitos à mão encaixados no mundo por semente, desde um vilarejo até uma cidade grande.

Organize os serviços previstos: comerciantes, ferreiros, alquimistas, cozinheiros, tavernas, escolas marciais e leilão. Faça um prompt para um primeiro vilarejo onde eu possa entrar, conversar, vender material e comprar algo usando as moedas da jornada. Deixe a expansão para uma cidade com bairros, serviços especializados e leilão em tarefas próprias. Conecte esses lugares aos ciclos de caça, coleta, fabricação, cultivo e descoberta de técnicas.

7. PENDÊNCIAS E DECISÕES QUE PRECISAM CONTINUAR VISÍVEIS

Registre a atualização do PainelDeTestes.exe da raiz. Se estiver livre, substitua pela versão publicada e confira que os testes recentes aparecem na lista. Se estiver aberto no meu PC, continue o restante da entrega e me diga ao final o que falta para a troca.

Mantenha uma lista curta de conferência manual: F5 e F11 fora do editor, aparência do arco da guarda, assistência no meu monitor e dificuldade real do Sênior em uma luta sem reposição artificial de vida. Registre as decisões abertas sobre a Lâmina enfraquecida contra o javali, a descoberta dos animais no mapa e os tempos da cena final do Sênior. Ligue o fluxo de /asset-spec às próximas fichas visuais quando fizer sentido.

8. TESTES, EVIDÊNCIAS E FECHAMENTO

Inclua no PainelDeTestes.exe os novos testes da Entrega 5. Use saves isolados, as sementes 20261126 e 777 e pelo menos uma jornada salva antes desta mudança. Confira ferramenta correta e incorreta, alcance do golpe, progresso visual, colisão, coleta única, persistência, uso dos materiais e reação dos arbustos a um animal. Faça também uma verificação reproduzível da influência de SOR na tabela de bônus.

Guarde uma captura antes/depois no mesmo ponto e um vídeo curto, capturado apenas do jogo, mostrando uma árvore sendo cortada, uma pedra sendo quebrada, um bambu cedendo e um animal atravessando um arbusto. Mantenha na Entrega 8 os testes finais que dependem das Entregas 6 e 7.

Quero receber ao final:
1. O link da pasta de tasks e do índice com a ordem das próximas entregas.
2. O relatório “Como criar e editar elementos do SpiritOfMurin”, com caminhos reais.
3. A sintaxe e dois exemplos testados do /create, com os arquivos gerados.
4. O que já posso jogar pelo Painel → Jogar e onde encontrar cada interação.
5. Os links das evidências, dos relatórios e dos commits publicados pelo /subir-git.
6. O próximo prompt pronto para copiar, correspondente à primeira tarefa aberta depois desta entrega.

Nesta rodada, execute a organização, o guia, o /create e a primeira entrega jogável de interação com o cenário. Para clima avançado, fauna expandida, cavernas, dungeons, cultivo e cidades, entregue o planejamento e os prompts no índice de tasks. Assim eu consigo testar a primeira parte enquanto a visão completa do jogo fica registrada.
```
