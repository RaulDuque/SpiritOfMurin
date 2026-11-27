# T08 — Fauna expandida, inimigos humanos, elites e chefes

**Estado:** planejada · **Frente:** B (parte 2) · **Depende de:** T07 (e T09 para patos e tartarugas de lago) · **Origem:** pedido 14 item 6.B (`user-docs/relatorios/prompts/14-organizacao-criacao-conteudo.md`), `user-docs/futuro-do game/minahvisao.md`, `gpt.md` e `opiniao-claude.md`

> As fichas abaixo são **ideias do criador e das conversas registradas em `user-docs/futuro-do game/`**, copiadas para não se perderem. Não são decisões fechadas: números, horários e níveis são propostas iniciais. O que for decidido vai para um GDD em `design/gdd/` e passa a ser a fonte única.

## Objetivo do jogador

Andar pela Planície e reconhecer cada bicho pelo jeito: o faisão que corre dois passos antes de voar, a víbora que se enrola em S, a manada que foge enquanto os machos seguram, a alcateia que cerca. Saber, pelo comportamento, o que vai acontecer e onde está a abertura. Caçar para obter materiais que dependem de habilidade (Esfolar, Preparo de caça, Extração), procurar as variantes raras e enfrentar elites e chefes que testam o que aprendeu.

## O que já existe hoje

- **Coelho de Campo** e **Javali da Mata** com cérebro, ajustes em dados, níveis (1–2 e 3–5), carcaça por partes, trilhas (`src/Creatures/`, `assets/data/plains/`; `design/gdd/plains-first-stretch.md` §3.3–3.4, `plains-progression.md` §3.6).
- **Inimigos humanos** só no prólogo: Discípulo da Guarda Fechada e Espadachim Sênior (`src/Vale/`, `design/gdd/campaign-prologue.md`).
- **Pássaros ambientais** (`src/Plains/Ambient.cs`, classe `Birds`; dados em `plains_ambient.json` → `birds`): bandos de 3–6 cruzam o céu de dia a cada ~22 s, a 70 px/s, 70–90 px de altura, com a sombra no chão mostrando onde o pássaro está; somem a 700 px do jogador; nenhum à noite; não interagem.
- **Habitats do gerador** (`PlainsGenerator`): água/margem, clareira, lamaçal, arbustos, relva; `per_habitat` decide a chance por tile. Não há lagos, bosque antigo nem espinhaço ainda.
- Níveis de criatura, foco (Espaço) e XP por caça (`plains-progression.md` §3.1 e §3.6).

## O que falta

Fichas por espécie (abaixo), o sistema comum que elas exigem e a ordem de entrega.

**Sistemas comuns novos:**
- **Grupo:** manada/alcateia/bando com líder, papéis (fugir, defender, cercar) e sorteio por grupo.
- **Percepção entre espécies:** predador → presa (lobo e falcão caçam coelho e filhote de cervo; bando de faisões fugindo denuncia predador).
- **Horário** (usa `DayNight.Phase`): espécies e variantes elegíveis por período.
- **Status de veneno** (víbora) com duração visível; renovação até um teto.
- **Raridade:** sorteio por indivíduo, por manada ou por alcateia, gravado no save; caçar e voltar em outro ciclo de repovoamento pode gerar novo encontro.
- **Ave interagível × bando ambiental** (seção própria abaixo).
- **Proficiências novas:** Extração (glândulas, quitina), Qinggong básico e arremesso de pedra, citadas em `gpt.md`.

## Fichas das espécies

Formato: **Habitat · Horário · Reação ao jogador · Com outras espécies · Movimento · Sinal de ataque/fuga · Abertura · Materiais · Raridade/variante · Nível sugerido**.

**Lobos da Planície (alcateia de 5–9, adultos e filhotes) e Alfa**
- Habitat: Espinhaço dos Lobos — árvores arranhadas, trilha acima do capim; uivos que se respondem.
- Horário: mais ativos ao entardecer e à noite.
- Jogador: caçam em grupo; um vem pela frente, outro circula. No começo da área, só um morde por vez. O Alfa muda o ritmo com um uivo e é o último a recuar.
- Outras espécies: caçam cervos e perseguem coelhos; a distração permite coletar ou finalizar um alvo enfraquecido.
- Movimento: trote em arco, cerco, recuo coordenado.
- Sinal: lobo abaixa as orelhas e encurta o passo antes da mordida; uivo do Alfa antecede a troca de ritmo.
- Abertura: depois da mordida, o lobo passa reto; quebrar o cerco ou usar terreno estreito.
- Materiais: Carne de Lobo, Couro Resistente, Presas/Dentes (pontas de flecha, adagas, colares).
- Pelagem negra ou branca; o Alfa é maior e sempre tem pigmentos vermelhos. **Alfa Carmesim** (1% por alcateia): manchas vermelhas naturais visíveis antes do combate, mais agressivo, Presa Sangrenta e Couro Rúnico (exige Esfolar Mestre, extração espiritual).
- Nível: lobos 5–8; Alfa 8–10. Elite natural da área.

**Raposa ruiva** *(ideia do pedido 14 e do exemplo do `/create`; não está nas fichas antigas)*
- Habitat: borda dos arbustos e das tocas. Horário: amanhecer e entardecer.
- Jogador: observa de longe e foge para os arbustos; não ataca.
- Outras espécies: caça coelhos (disputa a mesma toca que o jogador vigia).
- Sinal de fuga: senta e olha; a cauda baixa antes de correr em zigue-zague.
- Materiais (proposta): Pele de Raposa, carne. Variante: a definir (decisão aberta).
- Nível sugerido: 2–4.

**Patos** *(ideia do pedido 14; depende da T09)*
- Habitat: margem de lago e água rasa. Horário: dia.
- Jogador: andam na margem e entram na água ao ver o jogador; voam se ele insistir.
- Movimento: andar gingado na margem, nado lento, decolagem com respingos.
- Materiais (proposta): Penas, Carne de Ave. Raridade: a definir.
- Um critério de aceite da T09 usa um pato saindo da margem para a água.

**Gafanhoto-Lâmina (inseto)**
- Habitat: tufos altos de grama. Horário: dia quente.
- Jogador: defende um tufo específico; não persegue longe.
- Sinal: dobra as patas; salta até o ponto onde viu o jogador e golpeia ao pousar.
- Abertura: aterrissa além do alvo se ele sair da trajetória.
- Materiais: Quitina Leve (exige Esfolar/Extração), Pata Serrilhada (lâmina improvisada inicial).
- Variante: **Louva-a-Deus Jade** — maior, postura defensiva, dois golpes separados por uma pausa reconhecível; bom para treinar aparo.
- Nível: 2–4.

**Cervos da Planície (manada de 7–13) com defensores**
- Habitat: Bosque do Orvalho (árvore antiga, flores úmidas). Horário: dia; ao entardecer surgem sinais do raro.
- Jogador: fêmeas e filhotes fogem na hora; 2–3 machos com chifres ficam na retaguarda e lutam para a manada fugir. Um abaixa a galhada e avança; o outro guarda a rota de fuga. Quando a manada ganha distância, recuam.
- Outras espécies: presa de lobos e falcões (filhotes).
- Materiais: Carne de Cervo, Couro Macio (exige Esfolar), Chifres dos adultos (empunhaduras ou moídos para alquimia).
- Variante: **Cervo de Cristal** (1% por manada elegível; sortear 1% por cervo daria chance muito maior) — chifres translúcidos que acumulam Ki natural; Chifre de Cristal (catalisador espiritual); observar o raro de perto pode render uma descoberta.
- Nível: 4–6.

**Víbora da Grama**
- Habitat: grama alta e pedras quentes (Pedras do Riacho). Horário: mais ativa ao meio-dia.
- Jogador: não persegue longe; bote rápido se o jogador pisar perto; precisa voltar à pedra antes de repetir.
- Sinal: o capim balança, o corpo se enrola em S.
- Abertura: a volta para a pedra.
- Efeito: veneno leve com duração visível (dreno de Vida/Ki); nova mordida renova até um teto; Raiz Amarga neutraliza (já prevista em `plains-first-stretch.md` §3.5).
- Materiais: Pele de Cobra, Glândula de Veneno (Extração ou Alquimia básica).
- Variante: **Víbora Jade** — escamas verde-brilhantes; glândula para antídotos ou revestir lâminas.
- Nível: 2–4.

**Faisão / Pássaro de Penacho (bando de 3–5)**
- Habitat: Campos das Tocas, chão aberto. Horário: dia.
- Jogador: um sentinela ergue o pescoço; os outros correm dois passos e voam para as árvores — o intervalo permite um arremesso.
- Outras espécies: um bando fugindo sozinho denuncia um predador.
- Materiais: Penas (flechas, dardos), Carne de Ave.
- Variante: **Faisão Cauda-de-Fogo** — penas avermelhadas, valiosas para vender na primeira vila (T15) ou fazer leques/flechas.
- Nível: 1–3.

**Tartaruga de Riacho**
- Habitat: poças, pedras úmidas, margens frias. Horário: dia.
- Jogador: lenta e pacífica; atacada, recolhe-se no casco (quase todo dano físico frontal anulado); espera, espia e volta a andar.
- Abertura: quando estica o pescoço. Deixá-la em paz libera a margem.
- Materiais: Carapaça Rachada (escudo improvisado), Carne de Tartaruga.
- Variante: **Tartaruga Musgo-Antigo** — carapaça coberta de ervas espirituais; Casco Espiritual e Erva de Água; ideia: colher a erva ao se aproximar com calma, sem matar.
- Nível: 1–3.

**Falcão-de-Vento**
- Habitat: áreas abertas, voa em círculos. Horário: dia.
- Jogador/outras: desce em rasante para caçar coelhos ou filhotes de cervo; pode mirar o jogador.
- Sinal: a **sombra atravessa o chão antes do rasante**.
- Abertura: pousa por um instante após o rasante e abre as asas para subir.
- Materiais: Penas de Voo (melhores que as de faisão), Garras, Carne.
- Variante: **Falcão Asas-de-Nuvem** — penas branco-azuladas com rastro de vento; Pluma Leve (bônus em técnicas de movimento).
- Nível: 4–7.

**Variantes raras já existentes nas fichas** (1–2% por surgimento elegível, salvo indicação): Coelho Lunar (pelagem prateada, mais veloz ao entardecer/noite, Pelo de Prata para talismãs e costura leve); Javali Casca-de-Ferro (pelos escuros como agulhas, frente resistente e flancos acessíveis, Presa de Aço para forja); e as citadas acima. Rastreamento grau 3 detecta variação de Ki no ambiente quando um raro surgiu por perto (`gpt.md`).

**Inimigos humanos, elites e chefes** *(as fichas antigas não descrevem; tudo aqui é proposta)*
- Discípulo errante com lança curta, que recua depois de errar a estocada (exemplo do `/create` no pedido 14).
- Outras ideias a decidir: bandidos de estrada nos caminhos gerados, caçadores furtivos disputando a mesma presa, discípulos de seita rival.
- Elites: versão de espécie com líder (Alfa, macho dominante da manada) ou humano com técnica própria.
- Chefe natural da área: Alfa Carmesim com a alcateia (`gpt.md`, "Chefe/Elite Natural da Área").
- Todos seguem a linguagem de combate da T07 (aviso, ativo, recuperação, abertura) e do prólogo (`visual-mobs.md` §4).

## Ave interagível × bando distante (base: `Ambient.cs`)

- **Bando ambiental** (já existe): alto no céu, pequeno, sem contorno, a sombra longe do pássaro, nunca focável, passa e some. Continua sendo só ambientação — e pode virar **sinal** (um bando que levanta de repente denuncia um predador perto).
- **Ave interagível:** está **no chão** (faisão, pato) ou **baixa** (falcão no rasante, com a sombra grudada nela); tem contorno escuro como os outros animais, tamanho maior, entra no foco (Espaço) com nome e nível, e reage ao jogador.
- Reaproveitar o desenho de asas e a sombra do `Birds` para o voo do faisão que decola e do falcão; a diferença de leitura vem da altura (distância entre ave e sombra), do contorno e do foco.

## Sub-entregas sugeridas

| Id | Conteúdo | Depende de |
|---|---|---|
| T08a | Faisões + regra ave interagível × bando ambiental | T07 |
| T08b | Víbora (status de veneno, Raiz Amarga como antídoto) e tartaruga do riacho | T07 |
| T08c | Gafanhoto-Lâmina e Louva-a-Deus Jade (treino de aparo) | T07 |
| T08d | Raposa + percepção predador → presa (raposa e coelho) | T08a |
| T08e | Cervos em manada com defensores; Cervo de Cristal; Bosque do Orvalho | T08d |
| T08f | Lobos em alcateia, Alfa e Alfa Carmesim; Espinhaço dos Lobos | T08e |
| T08g | Falcão-de-Vento e Asas-de-Nuvem | T08a, T08d |
| T08h | Patos | T09 (lagos) |
| T08i | Variantes raras restantes (Coelho Lunar, Casca-de-Ferro) e Rastreamento grau 3 | T08e |
| T08j | Primeiro inimigo humano da Planície, elite e chefe | T08f, T14 |

## Dependências

- **T07:** linguagem de combate, pegadas dinâmicas, barra de foco.
- **T09:** lagos (patos, tartarugas de lago) e chuva (comportamento).
- **T14:** caminhos e NPCs para inimigos humanos e venda de materiais.
- `docs/elementos/PADRAO.md` e o `/create mob` para criar cada espécie.

## Decisões abertas (do criador)

1. **Ordem das espécies:** começar por faisões (A, recomendado: reaproveita os pássaros), por víbora/tartaruga (B) ou por lobos (C, mais impacto e mais risco)?
2. **Raposa e pato** não estão nas fichas antigas: materiais e variante rara? (propor e mostrar numa prancha).
3. **Lobos à noite:** a alcateia só existe à noite (A), fica mais agressiva à noite (B, recomendado) ou ignora o horário (C)?
4. **Veneno:** drena Vida (A), Ki (B) ou ambos (C, `gpt.md`)? Recomendado: Vida no começo.
5. **Primeiro inimigo humano da Planície:** discípulo errante com lança (A, recomendado), bandido de estrada (B) ou caçador furtivo (C)?
6. **Chefe da área:** o Alfa Carmesim é o chefe (raro, opcional) ou existe um chefe garantido separado?

## Critérios de aceite (jogáveis e testáveis)

Cada sub-entrega fecha só quando a sua espécie:
1. aparece no habitat e horário previstos nas duas sementes (contagem por bloco dentro da faixa em dados);
2. mostra o sinal antes da ação, e a ação acontece depois do tempo do sinal (medido);
3. tem a abertura descrita e um golpe nela causa dano; fora dela, o resultado previsto (resvala, foge, recolhe);
4. reage ao jogador conforme a ficha (foge, defende, cerca) e a outra espécie prevista (predação testada em cena isolada);
5. deixa os materiais previstos, uma vez por carcaça, com a regra de proficiência;
6. sorteia a variante na unidade certa (indivíduo, manada, alcateia) com taxa reproduzível por semente, e o resultado persiste ao salvar e continuar;
7. tem desenho, dados, ícones e lugar de criação conforme `docs/elementos/PADRAO.md`.
Para a ave: um bando ambiental nunca entra no foco; um faisão no chão entra, com nome e nível.

## Roteiro de teste

1. Unidade por espécie: máquina de estados (sinal → ação → abertura), sorteio de variante por unidade, regras de grupo.
2. Integração: cena isolada por espécie (como a vitrine) e a Planície nas duas sementes.
3. Taxas raras: rodar N sementes e conferir a frequência esperada (sem aleatoriedade no teste: semente fixa).
4. Evidências do viewport por espécie: sinal, ação, abertura, variante.

## Prompt pronto para executar

```text
Tarefa T08 do SpiritOfMurin — Fauna expandida. Execute a sub-entrega <T08a|T08b|...> (veja a tabela "Sub-entregas sugeridas").

Antes de tudo, leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T08-fauna-expandida-chefes.md. Leia as fichas originais em user-docs/futuro-do game/ (minahvisao.md e gpt.md) e user-docs/relatorios/2026-11-26-visual-mobs.md. Confirme que a T07 está pronta.

Na primeira sub-entrega, pergunte-me a ordem das espécies. Para cada espécie, pergunte as "Decisões abertas" que a afetam, uma por vez, com opções e recomendação; mostre o desenho proposto numa prancha HTML em user-docs/relatorios/ antes de pedir aprovação. Registre as decisões num GDD em design/gdd/ (fonte única) e deixe a T08 apontar para ele.

Crie cada espécie com o padrão de docs/elementos/PADRAO.md (pode usar /create mob), reaproveitando o cérebro, a carcaça, as pegadas e a linguagem de combate da T07. Para aves, use src/Plains/Ambient.cs (Birds) como base visual e garanta que um bando distante nunca entre no foco e uma ave interagível entre.

Regras:
- Habitat, horário, níveis, materiais e taxas raras em assets/data/; sorteio de variante por indivíduo/manada/alcateia, gravado no save.
- Testes com saves isolados (user://teste_*.json), sementes 20261126 e 777, a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json, janela no monitor 2; inclua no PainelDeTestes (tools/painel-testes/Suites.cs) e em tools/godot/testes-janela.ps1.
- Evidências só do viewport do jogo, em production/qa/evidence/<data>_fauna-<espécie>/. Nunca capture a área de trabalho.
- Termine todas as partes da sub-entrega e rode a bateria completa com janela uma única vez no fim.
- Feche seguindo o CLAUDE.md: dotnet build, testes, relatório em user-docs/relatorios/ + índice, commits atômicos, push para origin. Atualize a sub-entrega em user-docs/tasks/README.md com links.
```
