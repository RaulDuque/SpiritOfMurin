# Prompt 08: primeiro trecho jogável da Planície Verdejante

- **Data:** 2026-11-26
- **De:** o criador, para a sessão principal
- **Resultado:** relatório em `../2026-11-26-planicie-primeiro-trecho.md`; design em `design/gdd/plains-first-stretch.md`

## Pedido do criador (texto integral)

```text
Vamos para a próxima entrega combinada do SpiritOfMurin: o primeiro trecho jogável da Planície Verdejante no Godot 4.7.2 .NET com C#.

Quero apertar F5, acordar sem equipamento, explorar um terreno que se forma conforme caminho, encontrar recursos pelo ambiente, fabricar minha primeira faca e enfrentar um javali. Use a minha visão em user-docs/futuro-do game/ como fonte para o mundo e os documentos da Planície para fauna e recursos.

1. Geração do mundo por semente

Parta da base atual em Godot. Gere a Planície em blocos ao redor do jogador conforme ele explora. A mesma semente e as mesmas coordenadas devem produzir o mesmo terreno, independentemente da direção pela qual cheguei. Ao voltar, quero reconhecer a margem do riacho, os arbustos e o lamaçal. Se já peguei um recurso, atravessar a borda de um bloco e voltar deve preservar essa coleta durante a partida.

Neste trecho, implemente três habitats conectados: margem com sílex e galhos, campo de arbustos com tocas e lamaçal com javalis e Raiz Amarga. Garanta um início caminhável, com os primeiros materiais acessíveis. Mostre a semente em um local discreto para eu conseguir repetir um mundo de que gostei. A estrutura deve permitir encaixar cidades e túmulos feitos à mão nas próximas etapas.

2. Começo sem equipamento e primeira ferramenta

Para testar esta entrega pelo F5, abra diretamente no despertar da Planície. Na campanha, esse despertar continua previsto para depois da derrota no prólogo.

Use a ação de interação já mapeada em E. Quero recolher sílex e galho rijo, vê-los no inventário e fabricar uma faca de pedra consumindo os materiais uma única vez. O personagem acorda de mãos vazias. Ao equipar a faca, J ou clique esquerdo executa um corte curto coerente com ela. Preserve a movimentação, o passo com L ou clique direito e a base de combate com espada já testada.

3. Dois animais com comportamentos distintos

Coelho de Campo: aparece perto de tocas e arbustos. Ao perceber passos, ergue as orelhas e corre para a toca. Quero conseguir observar para onde vai, me aproximar com cuidado e usar a faca para aproveitar uma caça. Nesta entrega, a carcaça deve fornecer carne e pele, cada uma coletável uma vez.

Javali da Mata: aparece onde há lama e terra revirada. Antes da investida, raspa o casco, abaixa a cabeça e fixa a direção. Em seguida, avança em linha reta e precisa se recompor. Quero conseguir desviar andando para o lado e aproveitar a recuperação para acertá-lo. Faça o dano acontecer quando o golpe realmente alcançar o alvo. Deixe o tempo de aviso, a velocidade e a recuperação ajustáveis nos dados do jogo.

4. Ervas com efeito durante a partida

Coloque a Grama Sangue-de-Coelho junto às pedras. Depois de sofrer dano, devo conseguir usá-la pelo inventário e ver a Vida se recuperar ao longo do tempo. Coloque a Raiz Amarga junto à terra que o javali fuçou, para que o comportamento do animal me ajude a encontrar a planta. Mostre o nome do item ao coletar e a quantidade disponível.

5. Testes e entrega

Use os testes que já rodam no projeto. Se executar o /test-setup, verifique a compatibilidade do gdUnit4 com as versões instaladas. Caso a instalação encontre um bloqueio, registre o motivo e continue a entrega com o executor atual.

Teste duas sementes diferentes e, na mesma semente, visite os mesmos blocos em ordens diferentes. Confira a continuidade nas bordas, a coleta sem duplicação, a fabricação da faca, a fuga do coelho, o aviso e a recuperação do javali, o dano e a cura pela erva. Reexecute os testes anteriores de controles na cena apropriada.

Rode a cena com janela pelo percurso normal. Entregue uma captura ampla da Planície e um vídeo curto mostrando o javali avisando, investindo e deixando a abertura para o contra-ataque. Registre em user-docs/relatorios/ o que ficou jogável, a semente usada, os controles e os testes realizados. Atualize o índice, faça commit e push para RaulDuque/SpiritOfMurin e me passe o link do commit e as instruções para testar pelo F5 no Windows.
```
