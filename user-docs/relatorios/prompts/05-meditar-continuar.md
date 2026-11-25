# Prompt 05: "Meditar e continuar a jornada"

- **Data:** 2026-11-25
- **De:** o criador, para a sessão principal (sem subagentes)
- **Resultado:**
  - ponto de meditação nas pedras gêmeas, gravação local com versão de formato, "Continuar" e "Novo jogo" na tela de título;
  - teste com o navegador fechado de verdade;
  - relatório em `../2026-11-25-meditar-e-continuar.md`.

## Pedido do criador (texto integral)

```text
Próxima entrega do SpiritOfMurin: implementar "Meditar e continuar a jornada" no protótipo jogável.

Quero conseguir aprender o passo lateral, meditar, fechar o navegador e, no dia seguinte, voltar ao Vale de Bambus com a técnica disponível para enfrentar o discípulo. Use a aparência original aprovada e os controles atuais.

1. Crie o primeiro ponto de meditação

Leia o relatório do ciclo e confira o que já existe no protótipo. Escolha um local seguro e fácil de reencontrar no percurso entre a descoberta da técnica e a revanche. Reaproveite um marco do cenário. Ao me aproximar, mostre "E — Meditar". Ao interagir, grave o progresso e dê uma confirmação curta. A confirmação deve aparecer somente depois que a gravação funcionar.

2. Faça "Continuar" funcionar desde a tela de título

Na primeira partida, permita começar pelo fluxo atual. Quando houver progresso válido, destaque "Continuar" e ofereça "Novo jogo" com confirmação antes de substituir a jornada. Ao continuar, o personagem deve aparecer em posição segura no ponto de meditação, pronto para receber comandos.

Para este marco, basta um espaço de salvamento local com versão de formato. Guarde os dados que já têm efeito no jogo: técnicas e combinações descobertas, páginas do livro, manual e demais itens coletados, consumíveis, vida e qi no momento da meditação, progresso das missões e encontros importantes concluídos. Confira o estado real do código antes de definir os campos. Ao carregar, uma recompensa única já coletada deve continuar coletada, e uma passagem já aberta deve continuar aberta. Preserve as teclas personalizadas ao começar outra jornada.

3. Respeite a diferença entre morrer e encerrar a sessão

Durante a partida, preserve a regra já testada: perder um duelo permite tentar de novo com o aprendizado mantido. Fechar e reabrir o jogo retoma a última meditação confirmada. Deixe isso claro com uma frase curta ao salvar. Se o navegador bloquear a gravação ou os dados forem inválidos, mostre uma mensagem útil e mantenha o jogo acessível.

4. Teste como eu jogo no Windows

Use um perfil de navegador de teste separado do meu. Abra o index.html pelo caminho local que eu uso e jogue desde a tela de título. Aprenda o passo pelo percurso normal, medite, feche o navegador de verdade e reabra o mesmo endereço com o mesmo perfil. Escolha "Continuar", consulte a técnica no livro e use o passo na revanche. Verifique também que cancelar "Novo jogo" preserva a jornada e que carregar duas vezes não duplica itens.

Rode novamente as verificações existentes do ciclo para detectar regressões. Se o armazenamento depender da forma de abrir o protótipo, registre exatamente onde funcionou e me dê um endereço estável para usar nas próximas sessões.

5. Entregue a versão, o relatório e o commit

Salve o relatório da etapa em user-docs/relatorios/, atualize o índice README.md dessa pasta e registre o prompt conforme a organização existente. Inclua o que mudou, os testes executados, os resultados e qualquer limitação encontrada. Guarde evidências da meditação e da tela "Continuar" em production/qa/evidence/.

Faça commit dos arquivos desta entrega e push para RaulDuque/SpiritOfMurin. Confira se o commit chegou ao remoto. No final, me dê o link do commit, o link do relatório e três instruções curtas para eu testar no meu PC, sem revelar a solução do túmulo.
```
