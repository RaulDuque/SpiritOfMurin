# Recuperação do protótipo e da interface — 2026-11-26

> O pedido está em [prompts/11-recuperacao-prototipo.md](prompts/11-recuperacao-prototipo.md). O design está em `design/gdd/prototype-recovery.md`. A entrega de dia e noite ainda não tinha começado: o Git estava limpo depois da entrega 3, então não havia trabalho para preservar.

## 1. O que a comparação com o protótipo encontrou

Comparei as referências do protótipo (`2026-11-25_meditacao/regressao-ciclo/03, 05, 09, 17` e `2026-11-25_mouse/04`) com o mesmo trecho no Godot:

| Recurso | Protótipo HTML | Godot antes | Agora |
|---|---|---|---|
| **Ponte** | Tábuas sobre o rio | **Não aparecia**: a camada da água pintava água por cima das tábuas, e você parecia andar sobre o rio | Tábuas e corrimãos visíveis (o teste lê a cor do pixel da ponte) |
| **Arco da guarda** | Arco de ±65° na frente do alvo travado; claro na aparada; flanco tracejado quando enganado | Só um brilho curto na aparada | Igual ao protótipo |
| **Aviso do golpe** | Cunha de ±40°, o ângulo real do golpe | Cunha de ±30°, mas o golpe acertava em ±40° | ±40°, lido dos mesmos dados do teste de acerto |
| **Alvo travado** | Nome, barra e "guarda fechada / quebrada / postura de ataque" no alto | Nada | Nome, barra e estado (inclui "flanco aberto" e "recuperando") |
| **Mapa (M)** | Pergaminho; selo vermelho onde a missão surgiu e traço para a direção; coluna de missões | Mapa escuro, só um ponto ocre no destino da missão | Pergaminho, selos e traços, mais "Onde estou?", "Última pista" e missões |
| **Pistas** | Toasts; missões no mapa | Toasts sem registro | Cada pista dos eventos reais fica no mapa e é salva |
| **Minimapa** | **Não existia** (os sinais estavam no mapa do M) | Não existia | Novo: canto superior direito, com os sinais do mapa do M |
| **Livro** | Lista à esquerda, página à direita, teclas desenhadas, selo "feita por você", página rasgada | Lista simples com texto corrido | Igual ao protótipo, mais estado, números reais e rolagem |
| **Controles** | Tela "Teclas" com troca, aviso e restaurar | Só uma lista de ajuda | Esc → Controles, com mouse e teclado, salvo |

**Comparações lado a lado** (protótipo à esquerda, Godot à direita), em `production/qa/evidence/2026-11-26_recuperacao-prototipo/`:
- `comparacao-ponte.png`: a ponte e o corte aparado;
- `comparacao-mapa-apos-recuo.png` e `comparacao-mapa-apos-manual.png`: as duas etapas do mapa;
- `comparacao-livro.png`: o livro.

## 2. Tela cheia pelo F5: a causa confirmada

Reproduzi o seu caminho: abri o `project.godot` no Godot .NET 4.7.2 e apertei F5. O jogo agora grava a própria medida em `%APPDATA%\Godot\app_userdata\SpiritOfMurin\diagnostico_tela.txt`.

| Situação (monitor 1, 1920×1080) | Modo de execução | Janela do jogo | Área desenhada | Margens |
|---|---|---|---|---|
| F5, janela flutuante do editor | **incorporado ao editor** (aba Jogo), "Tamanho Fixo" | 1280×720 | 1280×720 | 0 dentro da janela do jogo |
| F5, janela flutuante **maximizada**, "Tamanho Fixo" | incorporado | **1280×720 fixa**, no centro de uma área de 1908×1008 | 1280×720 em (320, 210) | **314 esq., 314 dir., 144 cima, 144 baixo** |
| F5, maximizada, "Esticar para Preencher" | incorporado | 1908×1008 (medido pelo jogo) | Na captura, apareceu menor e borrada no centro: não confiável | — |
| Janela própria, tela cheia (F11), monitor 2 1600×900 | janela própria | 1600×900 | 1600×900, escala 2×, mundo 800×450 | 0 |

**Causa:**
- pelo F5, o Godot 4.7 **incorpora o jogo ao editor** (aba Jogo, numa janela flutuante);
- o "Dimensionamento de Janela Integrada" vem em **Tamanho Fixo**, que prende o jogo no tamanho do projeto (1280×720);
- ao maximizar a janela flutuante, o editor desenha margens pretas nos quatro lados em volta desse retângulo fixo. É exatamente a sua captura;
- dentro do retângulo, o jogo já preenchia tudo (a correção da entrega 3 funciona);
- além disso, **incorporado ao editor, a tela cheia não existe**: o próprio Godot bloqueia mudar o modo da janela nesse caso (`Engine.IsEmbeddedInEditor`). Por isso o F11 não fazia nada pelo F5.

**O que mudou no jogo:**
- o jogo detecta que está incorporado e avisa uma vez, ao abrir: "O jogo está dentro do editor (aba Jogo): ali a tela cheia não funciona…";
- o F11 e o menu de pausa explicam por que a tela cheia não liga ali, em vez de não fazer nada;
- fora do editor, nada mudou: F11 liga a tela cheia real, sem bordas.

**Configuração local do editor (faça uma vez, no seu PC):**
1. Abra `D:\Progr\SpiritOfMurin\project.godot` no Godot .NET e aperte **F5**.
2. Na janela do jogo que abrir (a aba **Jogo**), clique no botão **⋮**, no canto superior direito, ao lado do tamanho "1280x720".
3. **Desmarque "Integrar Jogo na Próxima Execução".**
4. Feche o jogo e aperte **F5** de novo. Agora ele abre em janela própria: **F11** liga a tela cheia de verdade, e maximizar enche o monitor.

**Alternativa:** se quiser manter o jogo dentro do editor, no mesmo menu ⋮, em "Dimensionamento de Janela Integrada", escolha **"Esticar para Preencher"**. Nesse modo o F11 continua indisponível. No meu teste ele esticou a janela do jogo (1908×1008, medido pelo jogo), mas a captura mostrou a imagem menor e borrada. **Fica pendente de conferência no seu PC.**

As mesmas opções existem para todos os projetos em **Editor → Configurações do Editor → Executar → Posicionamento da Janela → Modo de Incorporação do Jogo**.

**Evidências da tela cheia** (pasta `tela-cheia/`):
- `f5-incorporado-janela-1280x720.png`, `f5-incorporado-maximizado-tamanho-fixo.png` e `f5-incorporado-maximizado-esticar.png`: capturas reais do monitor 1, com legenda (resolução, modo, área desenhada);
- `monitor2-tela-cheia-antes.png` e `monitor2-tela-cheia-depois.png`: o monitor 2 inteiro (1600×900), sem recorte, com legenda.

**Por que o monitor 2 foi montado:** essas duas imagens foram montadas com a imagem do próprio jogo e a medida exata da janela. Uma captura direta da tela pegou o seu navegador aberto no monitor 2. Apaguei na hora, antes de qualquer commit, e os testes não usam mais captura da área de trabalho.

**O que fica pendente no F5:** com "Integrar" desmarcado, o F5 roda o mesmo jogo em janela própria que os testes medem. Não apertei F5 nesse modo porque você estava usando o PC. **Pendente de conferência manual.**

## 3. Minimapa e pistas no mapa

- **Minimapa:**
  - fica no canto superior direito, logo abaixo da semente, no Vale e na Planície;
  - mostra o chão já visto (Vale) ou os blocos explorados (Planície), os lugares, os selos de missão com o traço de direção, os seus marcadores e você (quadrado jade piscando, com um traço para onde olha);
  - some atrás de painéis, do título e do escurecimento;
  - o teste confere a posição dele, sem encostar na semente, nas barras nem no rodapé, em 1280×720, maximizada (800×438) e tela cheia (800×450).
- **Mapa (M), com a coluna da direita:**
  - **Onde estou?:** área, ponte ou margem, para onde você olha; na Planície, também os passos e a direção até a clareira e o terreno;
  - **Última pista;**
  - **Missões.**
- **Pistas registradas no Vale:**
  - o recuo na ponte: "Uma trilha desce para o vale, a leste.";
  - as três aparadas;
  - a anotação do manual: "…entre as pedras gêmeas";
  - a laje e as pegadas;
  - a inscrição da fenda;
  - o Passo Lateral;
  - o portão aberto.
- **Pistas registradas na Planície:** o despertar, cada rastro examinado (com o traço para onde segue) e a volta das técnicas no nível 3.
- **Ordem de descoberta:** antes do encontro, o mapa diz "Nenhuma ainda"; depois do recuo, a pista é a trilha a leste; depois do manual, a nova missão e a pista apontam para as pedras gêmeas.
- **Salvamento:** tudo fica salvo com a jornada (`quest_at`, `clues`) e volta depois de meditar e Continuar.

## 4. Ponte e Guarda Fechada

- **Ponte desenhada:** as tábuas aparecem de novo. Essa era a perda mais visível.
- **Mesma aproximação do protótipo:** o discípulo fica na margem norte, na boca da ponte, e acorda quando você sobe a trilha. A fala é "Sua lâmina não passa pela minha guarda."
- **Leitura da aparada:**
  - o arco da guarda fica claro;
  - aparece a palavra "aparado" e faíscas;
  - você é empurrado para trás;
  - depois de duas aparadas seguidas, ele contra-ataca mais cedo.
- **Abertura pelo flanco:**
  - depois do Passo Lateral, enquanto ele está enganado, o resto do círculo aparece tracejado em jade e o alto da tela diz "flanco aberto";
  - o corte entra com "flanco".
- **Coerência medida:** o teste confere que o arco desenhado e o resultado batem, de frente (aparado) e de fora do arco (flanco), com a assistência ligada e desligada.

## 5. Controles (Esc → Controles)

- **Tela:**
  - cada ação tem nome em português, entrada principal e segunda (tecla ou botão do mouse), cada uma com **Alterar**;
  - **Alterar** espera a próxima tecla ou botão, aplica na hora, e **Esc** cancela;
  - se a entrada já é de outra ação, as duas trocam e uma linha avisa (ex.: "K era de 'Qi': as duas trocaram");
  - a troca é recusada se deixaria a outra ação sem entrada;
  - botões **Restaurar padrões**, **Lista completa de comandos** e **Voltar**.
- **Salvamento:** nas preferências a cada troca e ao fechar o jogo.
- **Textos que acompanham a troca:** o rodapé, o mapa, o livro (as teclas desenhadas), as dicas de combate, os prompts ("E · examinar") e a ficha.
- **Caso completo testado:**
  1. trocar o corte para U com cliques;
  2. fechar com cliques (Voltar, Voltar ao jogo): **nenhum clique virou corte**;
  3. U corta e J não corta mais;
  4. **novo processo:** U continua cortando e o livro mostra "U → U → U";
  5. Restaurar padrões devolve J.

## 6. Modo de assistência (Esc → Modo de assistência → Mostrar áreas de combate)

- **Liga e desliga** durante a partida, e a preferência fica salva.
- **O que aparece**, desenhado com os mesmos números do combate:
  - **guarda:** setor claro de ±65° com borda cheia;
  - **flanco:** o resto do círculo, tracejado em jade, mais forte quando o inimigo está enganado;
  - **aviso:** contorno tracejado vermelho que se enche do corpo para fora;
  - **golpe ativo:** área vermelha cheia, de borda grossa;
  - **recuperação:** contorno fino e oco que encolhe;
  - **sua lâmina:** as mesmas três fases, em jade claro;
  - **javali:** corredor da investida, anel de "desvia" e anel aberto na recuperação.
- **Desligada:** ficam as pistas normais (postura, arco breve da aparada, poeira do casco, reação ao acerto).
- **Vídeo** `video-controles-assistencia-duelo.gif`: Esc → Controles → corte trocado para U → Modo de assistência ligado → o duelo na ponte (aparado, aviso, golpe e recuperação desenhados).

## 7. Livro de Técnicas e HUD

- **Livro:**
  - em tela larga, lista à esquerda, agrupada em técnicas, combinações e fragmentos, e página à direita;
  - a página mostra nome, tipo, selo de estado, o comando em teclas (atualizado com os seus controles), como usar, **os números reais** da versão em uso e da outra, e a origem;
  - na Planície, as técnicas seladas mostram "Selada desde o despertar: volta enfraquecida no nível 3", com os números das duas versões;
  - no nível 3, a página diz "Enfraquecida, em uso: custo 45 Qi, 10 vigor · alcance 105 px · dano 1" e "Plena era: 30 Qi · 210 px · dano 2";
  - em painel estreito, lista em cima e página embaixo, com rolagem;
  - clique, passagem do mouse, roda, teclas de andar e Esc;
  - a "Página rasgada" aparece até você descobrir a combinação, e o selo "feita por você" depois.
- **HUD:**
  - o rodapé ganhou uma faixa escura e lista também mapa e pausa;
  - Vida em porcentagem, Vigor e Qi seguem no canto;
  - a ação contextual fica sobre o personagem;
  - painéis (mapa, livro, controles, pausa) ficam por cima das falas e do nome da área;
  - a lista completa de comandos está em Esc → Lista de comandos.

## 8. Testes (com janela no monitor 2, perfil de teste separado)

**Teste novo** `tests/integration/RecoveryCheck` (o percurso normal, com teclas e mouse de verdade):

| Fase | O que verifica | Resultado |
|---|---|---|
| `vale` | Título → Novo jogo → Vale: minimapa; mapa antes do encontro sem pista; aproximação da ponte; **tábuas desenhadas**; trava; corte aparado coerente com o arco (assistência desligada e ligada); corte de fora do arco no flanco; **recuo com a pista**; mapa após o recuo; manual; **mapa atualizado**; livro lado a lado com o comando atual; meditar salva missões, selos e pistas | **20/20** |
| `vale2` | Continuar: mapa com a última pista; revanche com Passo Lateral e corte pelo flanco | **3/3** |
| `controls` / `controls2` | Troca do corte por mouse; conflito; cancelar; fechar sem cortar; rodapé; preferência; novo processo; livro com U; restaurar | **13/13 · 4/4** |
| `assist` / `assist2` | Liga pela pausa; salva; continua no novo processo; desliga | **5/5 · 2/2** |
| `book` | Livro da Planície: selada com os dois números; nível 3 enfraquecida com números reais; painel estreito empilhado; clique; Esc | **7/7** |
| `layout` | Minimapa nas três telas; os quatro cantos do mouse batem em janela, maximizada e tela cheia | **7/7** |
| `video` | Gravação do vídeo | **3/3** |

**Regressões:** veja a seção 10.

**O que é controlado:**
- teletransportes de posição;
- o Passo Lateral aprendido direto na fase `vale2`;
- a vida restaurada durante a revanche;
- o XP do nível 3 dado direto.

## 9. Como abrir cada recurso

| Recurso | Como |
|---|---|
| Minimapa | Aparece sozinho ao explorar (canto superior direito) |
| Mapa, "Onde estou?", "Última pista" | **M**, ou Esc → Mapa |
| Controles | **Esc → Controles** → clique em **Alterar** → aperte a nova tecla ou botão |
| Modo de assistência | **Esc → Modo de assistência → Mostrar áreas de combate** |
| Lista de comandos | Esc → Lista de comandos (ou, em Controles, "Lista completa de comandos") |
| Livro de Técnicas | **B** |
| Tela cheia | **F11**, fora do editor; veja a seção 2 |

## 10. Resultado da regressão

Todos os testes rodaram com janela no monitor 2, pelo script `tools/godot/testes-janela.ps1`, e passaram. O teste novo também entrou no script.

| Bateria | Resultado |
|---|---|
| Unidade | 30/30 |
| Controles | 14/14 |
| Planície, sementes 20261126 · 777 | 44/44 · 44/44 |
| Jornada nova, sementes 20261126 · 777 | 31/31 · 31/31 |
| Jornada retomada, sementes 20261126 · 777 | 11/11 · 11/11 |
| Vale: prólogo · resume-vale · resume-ending · old-save · skip | 32/32 · 5/5 · 2/2 · 2/2 · 2/2 |
| Técnicas: full · sealed · resume2 · resume3 · migrate · migrate2 | 3/3 · 19/19 · 2/2 · 1/1 · 2/2 · 1/1 |
| Tela (DisplayCheck): measure · click · map · map2 | 5/5 · 8/8 · 9/9 · 2/2 |
| Recuperação (RecoveryCheck): vale · vale2 · controls · controls2 · assist · assist2 · book · layout | 20/20 · 3/3 · 13/13 · 4/4 · 5/5 · 2/2 · 7/7 · 7/7 |

Os logs ficam em `production/qa/evidence/2026-11-26_recuperacao-prototipo/regressao/` e `logs/`, só na sua máquina: o projeto não envia `*.log` para o Git.

## 11. Pendências e conferência manual

- **F5 com "Integrar Jogo" desmarcado:** confira se a janela própria abre e o F11 enche o monitor.
- **"Esticar para Preencher"** (jogo dentro do editor): no meu teste a imagem apareceu menor e borrada na captura. Confira no seu PC.
- **Visual da guarda e da assistência no seu monitor:** as cores e a espessura podem pedir ajuste depois que você jogar.
- **Rodapé:** em 640 px de largura, a linha de atalhos corta o fim; a lista completa fica na pausa.
- **Livro estreito:** só aparece em janelas menores que ~460 px de jogo. Foi testado ajustando o painel, não com uma janela pequena de verdade.
