# Pedido 15 — T07: combate, rastreamento e leitura da fauna (e a diagonal)

*Enviado em 2026-11-27. Texto integral do criador. A primeira linha é o pedido da diagonal; o resto é o prompt da T07 colado. Relatório: [2026-11-28-t07-combate-fauna.md](../2026-11-28-t07-combate-fauna.md).*

```text
uma coisa estou achando um pouco estra a movitaça na diagonal pode melhorar para mim e 


Pode começar a T07 do SpiritOfMurin — Combate, rastreamento e leitura da fauna.

Leia GUIA.md, user-docs/tasks/README.md e user-docs/tasks/T07-combate-rastreamento-fauna.md. Use a especificação e os critérios de aceite dessa tarefa. Leia também o relatório da T06 para aproveitar a seleção de interação com E, o anel no chão e a roda do mouse. O visual que editei no protagonista continua aprovado.

Antes de implementar, apresente as sete decisões abertas da T07, uma por vez, com as opções, sua recomendação e o efeito que vou sentir jogando. Comece pela Lâmina enfraquecida contra o javali. Deixe claro que essa decisão é sobre o Passo da Lâmina Silenciosa. O Disparo de Ki Concentrado de 100 Ki e o Reforço de Ki de 75 já têm regras próprias. Para decidir a aparência da faixa e da barra de vida, mostre uma prancha HTML com as opções no mesmo enquadramento e no tamanho real do jogo. Diga como abri-la no Windows.

Quero conseguir testar três situações pelo Painel → Jogar:

1. Enfrentar o javali e entender cada contato. Se eu cortar durante o aviso e o couro aparar, quero ver a lâmina resvalar, ouvir o som seco e perceber que a Vida dele ficou igual. Se a investida me alcançar, o impacto, o empurrão e a perda de Vida devem coincidir. Se eu sair da linha e contra-atacar durante a recuperação, quero ver o corpo do javali reagir no instante em que a Vida dele cair. A faixa no chão deve aparecer quando ele fixar a direção e corresponder ao caminho que ele realmente percorre. Confira isso com a assistência ligada e desligada.

2. Seguir um coelho que realmente passou por ali. Quero ver as orelhas se erguerem para o lado certo, o corpo se encolher antes da arrancada, o arbusto reagir à passagem e o coelho mergulhar na toca. Aproxime-se uma vez andando e outra com Shift para comparar. As novas pegadas devem acompanhar o caminho que ele percorreu, inclusive se mudar de direção. Preserve as trilhas iniciais da clareira nas duas sementes. Ao examinar uma pegada, Rastreamento baixo dá uma direção aproximada e graus maiores revelam detalhes mais precisos. Se houver uma erva ao lado, quero poder escolher a pegada pela roda e confirmar com E.

3. Sentir a diferença entre uma técnica plena e uma enfraquecida. Compare o Disparo de Ki e o passo do prólogo com as versões recuperadas no nível 3. Quero perceber onde o projétil se desfaz e até onde o passo me leva. Use os valores reais dos dados para o alcance, o rastro e a colisão. Faça a comparação na mesma escala de câmera.

Organize javali, coelho, pegadas e efeitos segundo docs/elementos/PADRAO.md. No relatório, diga qual arquivo eu abro para editar à mão as orelhas, a poeira, a faixa da investida e a duração de uma pegada. Confira se esses sinais continuam legíveis durante uma rajada. Registre a conferência sob chuva como dependência da T09.

Inclua os testes no PainelDeTestes.exe e no script de testes com janela. Use saves isolados, as sementes 20261126 e 777 e a jornada antiga tests/fixtures/saves/jornada_antes_do_ki.json. Faça os testes específicos durante o desenvolvimento e rode a bateria completa uma vez no fim, com janela no monitor 2. Registre separadamente o resultado da rodada direta e o das repetições, como na T06.

Entregue um vídeo curto do duelo e outro da fuga até a toca, capturados apenas do viewport. Atualize o relatório, o índice e a T07 em user-docs/tasks/. Feche com /subir-git e me passe os links dos commits, das evidências e um roteiro curto para eu experimentar tudo pelo Painel → Jogar.
```
