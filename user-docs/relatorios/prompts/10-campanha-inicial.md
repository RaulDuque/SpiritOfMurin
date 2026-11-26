# Prompt 10: campanha inicial e próxima evolução da Planície (entregas 0 a 8)

- **Data:** 2026-11-26
- **De:** o criador, para a sessão principal
- **Resultado:** cada entrega tem o seu relatório em `user-docs/relatorios/` (o índice lista todos). O design da campanha está em `design/gdd/campaign-prologue.md`.
- **Durante a execução:**
  - "esse teste que voce executa extemame rapio exite alguma forma de eu rodar ele ? e vi aqui o teste falto o personagem prisipal se rarremsado pela explosao e começar deitado no chao nas planicies"
  - "a ideia apos o senior se esplodior o personagem e lançado fora da tela tipo para fazer sentido ele aparecer nas planices"

## Pedido do criador (texto integral)

```text
Vamos implementar a campanha inicial e a próxima evolução da Planície Verdejante no SpiritOfMurin, em Godot .NET com C#. Quero jogar a sequência completa pelo F5 no Windows. Continue com a aparência procedural que aprovei, a visão 2D de cima em 3/4 e os controles por teclado e mouse.

Use este pedido como direção criativa atual. Leia o CLAUDE.md, o último relatório local de progressão, o relatório da Planície e os documentos em user-docs/futuro-do game/. Consulte o protótipo HTML para recuperar a sequência jogável do Vale, o mapa, o Livro de Técnicas e os comportamentos de combate que já funcionavam.

Trabalhe pelas entregas abaixo. Execute, teste e registre cada uma seguindo a regra de fechamento do projeto. A meta final é conseguir jogar tudo pelo fluxo normal de Novo jogo e Continuar.

ENTREGA 0 — Fechar o trabalho que estava aguardando a janela do jogo

Confira o estado do Git e identifique as correções locais da pegada próxima à erva e do XP que faltava para o nível 2. Se essa entrega ainda estiver aberta, rode os testes com janela no monitor 2, grave o percurso normal, atualize o relatório e faça o commit e o push previstos. Se já tiver sido publicada, use o commit existente como ponto de partida.

Preserve minha jornada pessoal. Use perfis de teste separados. Nas entregas seguintes, aproveite a ficha, as proficiências, o inventário, a meditação e o salvamento já implementados.

ENTREGA 1 — Novo jogo começa no prólogo do Vale

O começo oficial da campanha é a área do protótipo anterior. Ao escolher Novo jogo, quero jogar o percurso original: explorar o Vale, encontrar o discípulo, descobrir o manual, chegar à estátua da mestra, aprender o passo lateral, usar a técnica e chegar ao Espadachim Sênior. Porte ou reutilize no Godot o que faltar para esse percurso. A Planície continua acessível diretamente por uma cena de teste.

A luta contra o Sênior precisa ser vencida de verdade. Se eu perder, uso a regra normal de derrota e posso tentar de novo. Quando eu vencer:

1. A luta termina e o jogador percebe a vitória.
2. Após uma pausa breve, o Sênior revive. A postura muda e a energia dele cresce a ponto de mover poeira, folhas e tecido ao redor.
3. Ele encara o protagonista e diz: “Você acha que pode me derrotar? Pois vou garantir que não.”
4. Ele concentra a energia no próprio corpo e se explode. A onda de choque lança o personagem para longe.
5. Faça a passagem que pedi: tela escura, transição para a claridade e despertar na Planície Verdejante. Use som, silêncio e retorno dos sons da natureza para dar peso à passagem. Permita avançar a fala e pular a sequência depois de já tê-la visto.
6. Na Planície, recupero o controle de mãos vazias. Equipamentos e itens do prólogo foram perdidos. O Livro continua registrando o que aprendi, agora marcado como “Selada”.

Trate vitória, explosão e despertar como etapas persistentes. Fechar e reabrir durante a transição deve retomar em um ponto seguro. A perda de equipamento e a mudança de região acontecem uma única vez. Quem já tem um save da Planície continua ali com seus itens, atributos, experiência e proficiências.

ENTREGA 2 — Selo e retorno enfraquecido das técnicas no nível 3

No despertar, sele todas as técnicas aprendidas no prólogo, inclusive a da estátua e o disparo de Qi. A movimentação comum, a caça, o corte com a faca equipada e a esquiva básica continuam possíveis. Verifique cada entrada para que uma técnica selada não possa ser acionada por mouse, teclado, combo ou atalho.

Quero caçar e progredir. Ao atingir o nível 3, as técnicas que eu já conhecia voltam em estado “Enfraquecida”. Mostre uma mensagem curta na primeira vez e atualize o Livro com comando, custo e efeito atual.

- Passo lateral da mestra: percorre uma distância menor, consome Vigor e Qi e exige mais Qi que sua versão plena. A parte ofensiva da sequência associada causa menos dano.
- Disparo de Qi: percorre uma distância menor, causa menos dano e consome Vigor e Qi. Seu custo de Qi é maior que no prólogo.
- Se faltar um dos recursos, dê retorno claro. Uma tentativa recusada não consome o outro recurso.
- Guarde em dados os valores das versões plena e enfraquecida. No relatório, compare custo de Qi, custo de Vigor, distância e dano com os números efetivamente testados.
- Na Planície, preserve usos contextuais existentes. Confira em especial K, que aparece no rodapé como “esfolar com Qi” e que no protótipo também servia ao disparo. Perto de uma carcaça, o jogador deve saber qual ação será executada. Um único comando nunca deve disparar e gastar recursos duas vezes.
- Teste a passagem do nível 2 para o 3, a retomada de um save no nível 2 e a migração de um save existente de nível 3 ou superior. Preserve as técnicas registradas e evite desbloqueios ou avisos duplicados.

ENTREGA 3 — Tela cheia, câmera e mapa do mundo

Reproduza o problema do vídeo: janela grande ou maximizada com a partida pequena no centro e bordas pretas nos quatro lados. Meça a área do jogo no F5 embutido, na janela independente, na janela maximizada e em tela cheia real no monitor 2. Confira resolução-base, escala, proporção, eventual SubViewport e configuração de execução do editor.

Quero tela cheia de verdade, acessível pelo menu e por F11, com preferência salva. Em 1920×1080, o mundo deve ocupar a área disponível, com proporção correta, pixel art nítida, personagem confortável de enxergar e HUD legível. Verifique também o clique após redimensionar: o item que aparece sob o cursor é o que deve ser selecionado. Compare antes/depois no mesmo monitor e informe a causa encontrada.

Recupere o mapa em M, acessível também pelo menu. No Vale, mostre os lugares descobertos do prólogo. Na Planície, construa o mapa a partir da semente e dos blocos explorados. Mostre posição e direção do jogador, clareira, fogueira, cursos d’água e pontos de interesse já conhecidos. A exploração revela o mapa aos poucos. Dê zoom, legenda curta e um comando para recentralizar. Um marcador manual deve permitir assinalar algo que eu queira revisitar. Salve a exploração e os marcadores com a jornada.

ENTREGA 4 — Dia, noite e ambiente vivo

Implemente amanhecer, dia, entardecer e noite com transições suaves. Mostre um indicador discreto do horário, mantenha o caminho legível à noite e dê presença à luz da fogueira. Guarde o horário ao meditar. Deixe os períodos disponíveis para a futura fauna noturna; as novas criaturas noturnas ficam para outra entrega.

Implemente vento em rajadas. Quero perceber uma rajada passando pela cena: primeiro o capim inclina, depois uma copa se mexe e algumas folhas atravessam a clareira. Adicione pássaros ambientando o céu, com pequenas revoadas e sombras discretas. Preserve a leitura das pegadas e dos avisos de ataque no meio desses efeitos.

Melhore a variedade do terreno mantendo a geração determinística:
- Varie altura e forma das árvores, largura das copas, inclinação de troncos, tamanho e formato das pedras. Crie grupos reconhecíveis para ajudar na orientação.
- Suavize as transições grama–terra–areia–água com bordas, cantos e pequenos detalhes de margem. Use Terrains ou uma solução compatível com o gerador atual. Confira as emendas entre blocos.
- Verifique a ordenação por Y pelos pés e pela base dos troncos e rochas. Quero passar atrás da copa e reaparecer na frente naturalmente. Preserve a colisão só onde o objeto toca o chão.
- Na água rasa, reduza levemente o movimento e mostre ondulações ou respingos nos pés. Diferencie a água profunda visualmente e respeite as regras de travessia da campanha. Na grama alta, faça a folhagem reagir à passagem.

ENTREGA 5 — Cortar árvores, quebrar pedras e obter materiais

Permita interagir com árvores e rochas. Integre as ferramentas ao ciclo que já existe: sílex e galhos caídos continuam acessíveis a quem acordou sem nada; a partir deles, deve ser possível avançar até uma ferramenta de corte e uma de mineração. Mostre no inventário as receitas, os materiais disponíveis e o que falta.

Ao atingir um objeto com a ferramenta adequada, mostre o progresso no próprio mundo: lascas e mudança no tronco, marcas e rachaduras na rocha. Ao concluir, a árvore cai ou vira toco, a pedra se parte e a colisão acompanha a nova forma.

Defina em dados as tabelas de materiais, quantidades e chances por tipo de árvore ou rocha. Inclua um recurso comum útil e a possibilidade de material bônus. Mostre o que caiu e dê pelo menos um uso jogável à madeira e à pedra obtidas. Faça SOR influenciar o bônus conforme o sistema atual. Guarde o resultado por objeto para que sair do bloco, meditar ou recarregar não multiplique drops nem restaure uma árvore já derrubada por engano.

ENTREGA 6 — Inventário, ficha e HUD para jogar no PC

Melhore o inventário existente. Quero abri-lo e responder rapidamente: “O que equipei? O que posso usar? O que consigo fabricar com o que tenho?”

- Mostre ícone, nome e quantidade em pilhas. Inclua categorias para equipamentos, ferramentas, consumíveis e materiais.
- Ao selecionar um item, mostre sua função real, qualidade, efeito e ações disponíveis. Quero equipar a faca ou ferramenta, usar uma erva e comparar o que está na mão por teclado ou mouse.
- Coloque as receitas em uma aba de fabricação. Mostre ingredientes no formato “tenho / necessário”, resultado e motivo quando a receita depender de fogueira, ferramenta ou proficiência. Atualize as quantidades imediatamente.
- Adicione atalhos configuráveis para os itens que uso com frequência. Preserve H para a erva quando aplicável. Abrir ou fechar uma tela não pode executar ataque, passo ou consumo no mundo.
- Na ficha, adicione botões “+” clicáveis, com navegação por teclado, prévia do próximo valor e indicação dos pontos disponíveis. Confira os atributos realmente presentes no modelo: minha ficha foi definida com VIT, INT, CON, AGI, SOR e QI. Use FOR somente se houver decisão posterior registrada no projeto.
- Mantenha a Vida em porcentagem no HUD e a cura pendente em verde, conforme já pedi. Na ficha ou ao inspecionar a barra, mostre também o valor absoluto, por exemplo “Vida 50% · 6/12”. Vigor e Qi continuam com atual/máximo.
- Coloque fundo legível nos atalhos do rodapé. Reduza a faixa a ações úteis para o contexto e deixe a lista completa em Controles. Mostre coleta e XP perto do personagem, agrupando ganhos repetidos. Preserve as mensagens importantes pelo tempo necessário para leitura.
- Quando uma pegada e uma erva estiverem próximas, mostre antes do E qual será o alvo. Se ambas forem alcançáveis, ofereça uma maneira simples de escolher. Teste fora da clareira também: deslocar uma pista inicial resolveu aquele caso, mas a geração pode produzir a mesma proximidade em outro bloco.

ENTREGA 7 — Combate, rastreamento e leitura da fauna

Faça o próprio javali avisar a investida: casco raspando, cabeça baixa, corpo alinhado e poeira. Acrescente uma faixa curta e discreta no chão indicando a trajetória fixada, com opção para reduzir esse auxílio. Mostre a diferença entre três resultados: golpe aparado no couro, investida que acerta e contra-ataque que entra durante a recuperação. Use um brilho breve, reação do corpo, som e impacto moderado. Mostre uma barra compacta de Vida ou postura junto ao animal focado enquanto houver combate. O efeito visual precisa coincidir com o contato que causou dano.

Use a mesma linguagem para o jogador entender o próprio ataque. Na comparação entre prólogo e Planície, o disparo enfraquecido deve terminar visivelmente mais perto e o passo deve cobrir menos chão. Faça a diferença ser perceptível durante uma luta.

Confira o comportamento existente dos coelhos. Preserve as duas arrancadas e a fuga para a toca ao ouvirem passos. Torne visíveis a orelha erguida, o momento de fuga e a entrada na toca. Aproximar-se com Shift deve produzir uma diferença observável. Oriente pegadas pelo sentido real do deslocamento; ao examiná-las, mostre uma indicação breve na direção da toca ou do animal. Valorize a proficiência de Rastreamento com a precisão dessa informação.

ENTREGA 8 — Testes, evidências e publicação

Conserve as regressões de controles, Planície, jornada e retomada nas duas sementes já usadas. Acrescente verificações para:
- vencer o Sênior, assistir à ressurreição e à explosão e despertar sem itens;
- confirmar as técnicas aprendidas no Livro como seladas; testar uma tentativa nos níveis 1 e 2, desbloquear no 3 e medir distância, custo e dano;
- carregar saves anteriores, salvar durante mudanças de fase e retomar sem perder progresso ou repetir recompensas;
- abrir o mapa, marcar um ponto, explorar outro bloco e reencontrar o marcador depois de carregar;
- colher madeira e pedra, voltar ao mesmo bloco e conferir inventário, colisão e ausência de drops duplicados;
- usar teclado e mouse no inventário e na ficha em janela e em tela cheia;
- observar dia, entardecer e noite na mesma paisagem, com o cenário legível;
- seguir uma pegada junto de um recurso e diferenciar o aviso, o bloqueio e a abertura do javali.

Use o monitor 2 para os testes com janela e um perfil separado do meu. Faça um percurso pelo fluxo normal de Novo jogo e Continuar. Para transições raras, efeitos e medição de alcance, pode complementar com testes controlados, identificados no relatório.

Quero três evidências fáceis de abrir: uma comparação da tela cheia antes/depois; um vídeo do fim do prólogo até os primeiros passos na Planície; e um vídeo curto mostrando mapa, inventário, mudança de horário e coleta de madeira ou pedra. Deixe os valores de balanceamento em dados para podermos ajustar depois de jogar.

Siga o CLAUDE.md para relatório, índice, commit e push de cada entrega. Ao concluir, me dê os links dos commits e relatórios, o caminho exato para abrir o projeto no Windows e um roteiro curto de teste pelo F5. Registre o que foi observado jogando, o que foi verificado automaticamente e qualquer pendência real.
```
