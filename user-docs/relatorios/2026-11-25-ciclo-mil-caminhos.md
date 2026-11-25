# Primeiro ciclo de "Mil Caminhos do Dao" — 2026-11-25

> Este relatório **não traz a solução do túmulo**. Ela está separada em [2026-11-25-ciclo-mil-caminhos-SOLUCAO.md](2026-11-25-ciclo-mil-caminhos-SOLUCAO.md). Só abra depois de jogar.

## Pedido do criador (resumo)

**Encerrar o experimento dos sprites:**
- manter a aparência original o tempo todo, inclusive em links antigos com `?sprites=1`;
- guardar os PNGs e a comparação como histórico.

**Fechar o ciclo encontrar → investigar → aprender → voltar e usar:**
- reaproveitar o que já existe;
- implementar os elos que faltarem;
- verificar jogando desde o início normal, com uma partida de teste separada.

## Para jogar no Windows

Cole no Edge ou no Chrome:

`file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html`

Na tela de título, aperte Enter.

| Tecla | Ação |
|---|---|
| W A S D | andar |
| J | corte (aperte de novo para encadear até três) |
| K | onda de qi (gasta qi) |
| L | passo |
| Espaço | travar / destravar no alvo |
| E | examinar, pegar, fechar textos |
| Tab | Livro de Técnicas |
| M | mapa e missões |
| I | inventário |
| H | bálsamo |
| P ou Esc | pausa |
| N | som |
| O | trocar teclas (no título ou na pausa) |

O jogo não salva progresso entre sessões, só as teclas trocadas (no navegador). Os testes rodaram num perfil de navegador temporário, separado do seu.

## 1. Encerramento do experimento dos sprites

- A troca `?sprites=1` e o ponto fixo `?stage=bambuzal` saíram do protótipo. O `index.html` voltou ao estado do último commit antes desta iteração.
- Um link com `?sprites=1` agora mostra a aparência original. Conferido por captura, sem erro na página.
- As cópias dos PNGs e a procedência delas saíram de `prototypes/spiritofmurin/assets/`.
- Os PNGs finais foram para `assets/art/historico/2026-11-25-experimento-sprites/`. Continuam onde estavam:
  - os originais do ChatGPT, em `assets/source/raw/`;
  - as capturas, em `production/qa/evidence/2026-11-25_asset-001-002_*`;
  - a prancha, `2026-11-25-antes-depois-bambuzal.html`.
- Registros atualizados:
  - art bible 8.12 (cópia anterior em `design/art/historico/2026-11-25-art-bible-v4-antes-do-fim-do-experimento.md`);
  - spec `approval-poses-assets.md`;
  - manifesto (assets "Rejeitado — histórico").

## 2. O que já existia (conferido no código e jogando)

O protótipo já tinha o ciclo inteiro:

**Discípulo da ponte:**
- fala ao ver o jogador e entrega a missão "A guarda fechada";
- apara cortes frontais com faísca e empurrão;
- gira para manter a frente;
- telegrafa o golpe em vermelho-ocre;
- volta para a ponte se o jogador se afasta.

**Discípulo do vale:** a fala sobre as ervas da seita.

**Pista e túmulo:**
- o corpo no Vale de Bambus traz a metade do manual, com a anotação na margem e uma missão com direção no mapa;
- a porta do túmulo abre com o símbolo do manual;
- a marca na pedra e as pegadas ao redor da estátua ensinam o passo lateral, com pausa, som e nome.

**Livro e passo:**
- o Livro de Técnicas (Tab) registra o passo e as teclas;
- o passo lateral contorna o alvo travado, e o inimigo perde a noção do jogador por um instante.

**Combate e derrota:**
- a combinação Passo da Lâmina Silenciosa já existia;
- a derrota leva de volta ao início da área com a vida cheia, guardando o que foi aprendido.

## 3. O que a primeira partida de teste encontrou

A partida foi feita pelo início normal, sem atalhos.

- **Recuar sem morrer não dava pista nenhuma.** A frase "Uma trilha desce para o vale, a leste" só aparecia depois da primeira morte.
- **Na revanche, a tecla do "lado" muda depois do primeiro passo.** O lado é relativo ao alvo. De frente para ele, o lado é A ou D; depois de contornar, passa a ser W ou S. O jogador que repete a mesma tecla dá um passo curto para frente ou para trás e é aparado.
  - Resultado na revanche: 1 acerto e 4 aparadas; o jogador perdeu.
  - É o ponto em que mais se deixa de entender o golpe.
- **Ao acertar o flanco, nada mostrava por que tinha funcionado.**
- **Chegava-se à revanche com a vida baixa,** porque o primeiro encontro custa 2–3 corações e nada recompunha a vida antes do duelo.
- **Apertar Espaço no instante de um golpe às vezes não travava,** porque a tecla se perdia no congelamento do impacto.
- **A marca na pedra era um risco reto.** Você pediu uma marca curva.

## 4. O que foi implementado ou corrigido

Tudo em `prototypes/spiritofmurin/index.html`.

**Primeiro encontro: o discípulo protege a frente**
- Cada corte aparado acende o arco da guarda dele (visível mesmo sem travar) e mostra "aparado".
- Na terceira aparada, ele mesmo diz: "De frente, nada passa pela Guarda Fechada." O aviso "De frente, essa guarda não cai" continua.

**Recuar e continuar explorando**
- Ao recuar sem o manual, o discípulo da ponte volta ao posto dizendo: "Fuja, então. Pela frente, ninguém passa por mim."
- Na mesma hora aparece, uma vez só, "Uma trilha desce para o vale, a leste". É a mesma pista que antes exigia morrer.

**A descoberta no túmulo**
- A marca na parede agora é um corte curvo.
- O texto fala de um golpe que contorna algo e de pegadas que saem da frente da estátua para o lado.
- O rastro que o seu corte deixa no chão ganhou a mesma curva, como pede o "Todo Golpe Deixa Marca".

**Espaço para experimentar**
- Aprender o passo recompõe a vida e o qi ("a respiração assenta aos pés da mestra").
- Depois da revelação aparecem duas instruções:
  - com o alvo travado, L + direção para o lado;
  - "Experimente na estátua: Espaço trava nela."
- A estátua mostra o arco da guarda e, tracejado, o lado aberto. Um corte no flanco faz a pedra responder.

**Revanche: consequência clara e por que funcionou**
- Travado numa guarda, aparece sob o personagem "L + [teclas]: contornar", com as teclas certas para a posição atual (por exemplo "A / D" de frente e "W / S" ao lado).
  - A dica some depois de três acertos no flanco.
- Enquanto o inimigo está enganado pelo passo, o lado aberto aparece tracejado em ciano.
- Um corte que entra pelo flanco mostra a palavra "flanco" e dois arcos: em cinza, para onde a guarda olhava; em ciano, por onde a lâmina entrou.
  - Na primeira vez aparece: "A guarda ficou olhando para onde você estava. O flanco estava aberto."
- Se o jogador usar L sem travar perto de uma guarda, uma dica lembra de travar antes (Espaço).

**Outras correções**
- Espaço apertado durante o congelamento de impacto agora trava normalmente.
- As outras respostas que o combate já oferecia continuam como estavam:
  - a combinação passo → corte → qi que quebra a guarda;
  - recuar com o passo curto;
  - a onda de qi;
  - a estocada.

## 5. O que foi verificado jogando

O jogador automático está em `tools/playtest/` (`ciclo-completo.mjs`, com `bot.mjs` e `cdp.mjs`):
- abre o jogo pelo arquivo, num perfil de navegador temporário;
- aperta as teclas de verdade pela página, como um teclado;
- só **lê** o estado do jogo para saber onde está;
- não usa atalho de depuração;
- na revanche, lê a dica de teclas na tela como um jogador leria.

**Rodada final** (`node tools/playtest/ciclo-completo.mjs production/qa/evidence/2026-11-25_ciclo-dao --lose`): **20 de 20 verificações OK**, sem erro na página.

| Trecho | Resultado |
|---|---|
| Início normal pelo título | OK |
| Discípulo fala ao ver o jogador | OK |
| Cortes frontais aparados | OK (3) |
| Recuar vivo sem o passo lateral | OK (3 corações) |
| Discípulo volta à ponte | OK |
| Pista ao recuar | OK ("vale, a leste" e a fala dele) |
| Corpo oferece E | OK |
| Manual dá pista curta até o túmulo | OK |
| Porta oferece E e abre com o manual | OK |
| Pegada errada zera e avisa, depois dá para tentar de novo | OK |
| Passo lateral aprendido no túmulo | OK |
| Túmulo recompõe a vida | OK (5) |
| Livro lista o passo com as teclas | OK |
| Estátua responde ao golpe no flanco | OK |
| Perder o duelo de propósito volta ao início com vida cheia e o passo guardado | OK |
| Discípulo volta inteiro para a nova tentativa | OK |
| Passo + corte acerta o flanco na revanche | OK (6 acertos, 4 no flanco) |
| Revanche vencida | OK (portão do acampamento aberto, 3 corações) |

**Equilíbrio da revanche:** num teste separado de diagnóstico (com atalho, só para medir), contornando e cortando, a revanche foi vencida 4 de 4 vezes, em 5–9 s, perdendo 1–3 corações.

As capturas de cada passo estão em `production/qa/evidence/2026-11-25_ciclo-dao/` (01 a 22), com `checks.json` e `log.json`.

## Não verificado

- O som. O teste roda sem áudio.
- O duelo final com o Espadachim Sênior. Está fora do ciclo pedido e não foi alterado.
- A leitura por uma pessoa de primeira viagem. O jogador automático sabe onde fica cada coisa; o que ele prova é que o caminho existe e funciona sem atalhos, não que é fácil de achar. Esse teste é o seu.
