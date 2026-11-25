# Meditar e continuar a jornada — 2026-11-25

> Sem a solução do túmulo. Ela continua em [2026-11-25-ciclo-mil-caminhos-SOLUCAO.md](2026-11-25-ciclo-mil-caminhos-SOLUCAO.md).
> O pedido está em [prompts/05-meditar-continuar.md](prompts/05-meditar-continuar.md).

## Resultado

Agora é possível aprender o passo lateral, meditar, fechar o navegador, reabrir o mesmo endereço e escolher **Continuar**. Você volta à pedra da meditação com a técnica, o livro, os itens e as missões como estavam, e o passo funciona na revanche.

A aparência e os controles não mudaram.

**Endereço estável** (o mesmo de antes, no Edge ou no Chrome):

`file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html`

## O que mudou

Tudo em `prototypes/spiritofmurin/index.html`.

**Ponto de meditação**
- **Onde:** ao pé da pedra gêmea da esquerda, logo abaixo da porta do túmulo, no Vale de Bambus.
- **Por que ali:**
  - fica no caminho entre o túmulo e a revanche;
  - reaproveita as pedras gêmeas, o marco que o manual já cita;
  - é seguro: o inimigo mais próximo só reage a menos de 120 px e está a cerca de 340 px.
- **No cenário:** uma esteira de palha e um fio de qi sobem ao lado da pedra.
- Ao chegar perto aparece **"E — Meditar"**. A letra segue a tecla configurada para examinar.

**Meditar**
- Grava a jornada, lê de volta o que gravou e **só confirma se a leitura bater**.
- A confirmação diz: "Meditação gravada." e "Se cair em duelo, volta à trilha. Se fechar o jogo, volta a esta pedra."
- Com um inimigo em guarda perto, não medita e explica por quê.
- Se o navegador bloquear a gravação, aparece: "A meditação não foi gravada: o navegador bloqueou o armazenamento." e "O jogo continua, mas fechar agora perde o progresso desde o início." O jogo segue normalmente.

**Tela de título**
- **Sem jornada salva:** o fluxo de sempre (Enter começa).
- **Com jornada válida:**
  - **Continuar** aparece em destaque, com data e hora da meditação, e **Novo jogo** logo abaixo;
  - W/S ou as setas escolhem;
  - Enter, Espaço ou E confirma.
- **Novo jogo:** pergunta "Começar uma nova jornada?". A opção já marcada é **"Não, manter a jornada"**, e Esc também volta. Só "Sim, começar de novo" apaga a jornada salva. As teclas trocadas ficam.
- **Dados danificados:** o título avisa "A jornada salva estava danificada e não pôde ser carregada. Comece uma nova." e o jogo começa normalmente.
- **Navegador que não deixa guardar:** o título avisa e o jogo funciona, só sem Continuar.

**Continuar**
- O personagem aparece na esteira, virado para a câmera, com a câmera já sobre ele, e responde aos comandos na hora.
- Aparece "Você desperta junto às pedras gêmeas, onde meditou."

**Morrer e fechar o jogo são coisas diferentes**
- Morrer em duelo continua como antes: você volta ao início da trilha (ou à entrada do acampamento, depois da ponte) com a vida cheia e o que aprendeu.
- Fechar e reabrir retoma a **última meditação gravada**. O que aconteceu depois dela, incluindo uma vitória, não fica.

## O que a gravação guarda

Os campos vêm do estado real do código. Formato versão 1, na chave `spiritofmurin.jornada` do navegador; as teclas continuam em `spiritofmurin.teclas`.

| Grupo | Campos |
|---|---|
| Técnicas e livro | todas as páginas do Livro de Técnicas (técnicas, a combinação descoberta, fragmentos como o manual e a inscrição) |
| Descobertas | manual pego, porta do túmulo aberta, marca lida, passo lateral aprendido, combinação descoberta, inscrição lida |
| Passagens e recompensas únicas | portão do acampamento, baú, potes quebrados, ervas colhidas, itens caídos ainda no chão |
| Itens | inventário e ordem (bálsamos, pedras de qi, ervas, prata, faixas, pingente) |
| Personagem | vida e qi no momento da meditação |
| Missões | cada missão, onde foi pega e se foi cumprida |
| Encontros | quais inimigos foram vencidos e quais já falaram; ponto de retorno ao morrer (trilha ou acampamento) |
| Ensino | avisos já mostrados uma vez, contagem de aparadas e de golpes no flanco (a dica de teclas some depois de 3) |
| Mapa | as partes do mapa que você já viu |

Ao carregar, cada campo passa por uma conferência de tipos e limites. Qualquer coisa fora do esperado conta como dado danificado, e nada é aplicado pela metade.

## Testes executados

Todos rodaram num **perfil de navegador de teste**, uma pasta temporária nova a cada execução, nunca o seu perfil. O jogo foi aberto pelo **mesmo endereço local** acima, jogado só com teclas, a partir da tela de título.

**1. `tools/playtest/meditar-continuar.mjs` — 34 de 34 OK**

Evidências em `production/qa/evidence/2026-11-25_meditacao/`, com `checks.json`, `log.json` e 12 capturas.

- **Sessão 1:**
  - título sem jornada; Enter começa;
  - pelo percurso normal: encontro na ponte, recuo, manual, túmulo, passo aprendido;
  - quebra um pote e pega a prata;
  - a pedra oferece "E — Meditar"; nada estava gravado antes;
  - meditar grava e confirma, com a frase sobre morrer e fechar;
  - **o navegador é fechado de verdade** (encerramento normal, que grava em disco).
- **Sessão 2** (mesmo perfil, mesmo endereço):
  - "Continuar" em destaque;
  - "Novo jogo" pede confirmação; "Não" e Esc mantêm a jornada intacta, conferida byte a byte;
  - "Continuar" põe o personagem na pedra com inventário, livro, missões, vida, qi, porta e recompensas iguais aos da meditação;
  - o personagem anda na hora;
  - o livro abre na página "Passo Lateral";
  - revanche vencida na primeira tentativa, com golpes no flanco.
- **Sessão 3** (reabrir de novo):
  - a gravação não mudou;
  - carregar outra vez **não duplica** a prata nem reabre o pote;
  - a vitória da sessão 2, que não foi meditada, não fica: o discípulo está de volta;
  - o corpo do vale "não carrega mais nada".
- **Sessão 3b:**
  - com teclas personalizadas (corte trocado para U), "Novo jogo" → "Sim" apaga a jornada e começa na trilha;
  - as teclas continuam trocadas.
- **Sessão 4:** com dados danificados, o título avisa e Enter começa uma jornada nova.
- **Sessão 5:**
  - com o armazenamento bloqueado, meditar mostra a mensagem de erro e não mostra confirmação falsa;
  - o jogo continua jogável.
- Nenhum erro de página em nenhuma sessão.

Em duas dessas verificações escrevi direto no armazenamento do navegador de teste, porque não há como provocar a situação jogando: os dados danificados e as teclas trocadas da sessão 3b. No bloqueio do armazenamento, o navegador de teste foi configurado para recusar gravações. Todo o resto foi jogado.

**2. Regressão: `tools/playtest/ciclo-completo.mjs --lose` — 20 de 20 OK**

Evidência em `production/qa/evidence/2026-11-25_meditacao/regressao-ciclo/`.

Na primeira rodada, a verificação "cortes frontais aparados" falhou com 2 aparadas em vez de 3. O motivo foi o tempo do jogador automático: um dos 4 cortes saiu fora de alcance depois do empurrão, e nenhum acertou. Duas rodadas seguintes deram 20 de 20. A verificação foi reescrita para o que importa — **nenhum corte frontal acerta e pelo menos 2 são aparados** — e a rodada final deu 20 de 20.

**3. Persistência por navegador: `tools/playtest/persistencia-rapida.mjs`**

Medita, fecha e reabre com o mesmo perfil.

| Navegador | Gravou | Título depois de reabrir | Continuou em |
|---|---|---|---|
| Microsoft Edge 158 | sim | Continuar | pedra da meditação (49,1; 33,4) |
| Google Chrome 158 | sim | Continuar | pedra da meditação (49,1; 33,4) |

## Onde a gravação funciona (e limitações)

- A gravação fica **no navegador e no perfil** em que você jogou. Uma jornada feita no Edge não aparece no Chrome, e vice-versa.
- Funcionou abrindo o arquivo pelo endereço `file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html`. Parâmetros depois do `?` não mudam onde a gravação fica.
- Se um dia o jogo for aberto por um servidor local (`http://localhost/...`), o navegador trata como outro lugar, e a jornada gravada pelo arquivo não aparece.
- Janela anônima (InPrivate) apaga tudo ao fechar.
- Limpar os dados de navegação do navegador apaga a jornada.
- Há um único espaço de gravação; meditar de novo substitui a jornada anterior.
- **Inimigos:** a gravação guarda quem foi vencido, não onde estava. Ao continuar, os inimigos vivos estão nos postos deles.
- **Fim da partida:** depois de vencer o Espadachim Sênior, a partida vai para a tela final sem meditar. Ao reabrir, "Continuar" volta à última meditação, de antes da vitória.
- **Vida e qi:** voltam exatamente como estavam na meditação; meditar não cura. Se meditou com pouca vida, continua com pouca vida.
- **Som:** não foi verificado; os testes rodam sem áudio.
