# Prompt 09: primeira progressão jogável da Planície

- **Data:** 2026-11-26
- **De:** o criador, para a sessão principal
- **Resultado:** relatório em `../2026-11-26-planicie-progressao.md`; design em `design/gdd/plains-progression.md`
- **Durante a execução:**
  - o criador pediu o jogo aberto para testar enquanto o código era escrito, e ele foi aberto;
  - pediu para esperar o fim dos testes dele antes de rodar os testes com janela, que roubam o foco.

## Pedido do criador (texto integral)

```text
Implemente a primeira progressão jogável da Planície Verdejante no Godot com C#. Parta do trecho que já existe: o jogador coleta recursos, fabrica a faca, caça coelhos, enfrenta javalis e usa ervas. Use o meu documento da Planície como fonte para os nomes, os materiais e os graus de habilidade.

Quero testar este percurso pelo F5: seguir pegadas até uma toca, conseguir minha primeira caça, aproveitar a carne e a pele, usar esses materiais, subir de nível e escolher um atributo. Depois quero meditar, fechar o jogo e continuar com o que conquistei.

1. Nível do jogador e das criaturas

Implemente nível, XP e pontos de atributo. Mostre quanto falta para o próximo nível e, antes de gastar um ponto, mostre a mudança concreta que ele vai causar. Use os valores atuais da Planície como base para o personagem de nível 1. Coloque as curvas e os ganhos em dados ajustáveis.

Coelhos e javalis devem ter faixas de nível próprias. Revele o nível da criatura ao examiná-la ou focá-la. Faça a diferença de nível ter efeito mensurável e mantenha os padrões que o jogador já consegue aprender: o coelho foge para a toca e o javali avisa antes de investir.

2. Ficha com os seis atributos que defini

- VIT — Vitalidade: aumenta a Vida máxima. Preserve o HUD com “Vida 80%” e a indicação da cura pendente.
- INT — Inteligência: melhora a potência dos preparos medicinais.
- CON — Constituição: aumenta o vigor físico. Faça o vigor ter uso e recuperação visíveis durante a partida.
- AGI — Agilidade: melhora a mobilidade e a recuperação do movimento. Preserve a leitura dos comandos de teclado e mouse.
- SOR — Sorte: influencia materiais secundários obtidos na coleta ou na caça. Exiba na ficha o que muda ao investir um ponto.
- QI — Energia espiritual: implemente Qi atual e máximo. Dê à energia um uso jogável nesta entrega, como aplicar Qi ao preservar um material quando a proficiência permitir.

3. Habilidades que evoluem pelo uso

Implemente proficiência separada para Esfolar, Preparo de Caça, Erboristeria e Rastreamento. Mostre o grau atual, o progresso até o próximo e o benefício que o jogador ganha. Uma nova caça ou planta pode dar prática; reabrir a mesma carcaça ou reexaminar a mesma pegada não duplica XP.

Para Esfolar, use os graus do meu documento: 0 Bruto gera pele rasgada, 1 Iniciante permite pele comum, 2 Adepto preserva couro intacto e 3 ou superior permite extração espiritual com gasto de Qi quando o material for compatível. O javali também deve fornecer os materiais previstos no brief. A disponibilidade de pele, carne e outras partes deve ser controlada separadamente em cada carcaça.

Para Rastreamento, coloque pegadas que levem a uma toca real ou a terra revirada de um javali presente naquela região. Quero conseguir olhar uma pista, seguir sua direção e encontrar o animal. Para Erboristeria, faça o ganho de domínio aparecer na qualidade ou potência da erva colhida.

4. Primeiros usos dos materiais

Permita assar carne na fogueira e consumi-la para recuperar vigor. Dê uso à pele rasgada como tira de amarração e ao couro comum em uma receita inicial utilizável, como uma bolsa. Mostre os ingredientes, o resultado e o efeito do item. A fabricação deve consumir a quantidade certa uma única vez.

5. Meditar e continuar no Godot

O protótipo HTML já tem uma referência desse fluxo. Implemente o salvamento local na Planície do Godot. Guarde a semente, o ponto de meditação, os recursos já coletados, o inventário, o equipamento, a Vida, o Qi, o nível, os pontos e as proficiências. Ao escolher Continuar, quero voltar à mesma Planície e reconhecer o lugar. Salvar e carregar não deve recriar uma carcaça já aproveitada nem multiplicar materiais.

6. Validação e entrega

Teste uma jornada nova e uma retomada depois de fechar o jogo. Verifique o primeiro ganho de nível, o efeito de cada atributo, a passagem de Esfolar 0 para 1, a diferença no material recebido, a carne assada e a fabricação. Nos testes de interface, abra a janela no monitor 2 conforme a configuração já usada. Reexecute os testes anteriores da Planície e dos controles.

Siga a regra de fechamento do CLAUDE.md. No relatório, diferencie o que foi observado em uma partida normal do que foi verificado com testes controlados. Entregue o link do commit, o relatório e um roteiro curto para eu conferir tudo pelo F5.
```

**Pedidos durante a sessão:**
- "pode inicar o jogo quero fazer alguns teste quanto vc vai escrevendo o codigo"
- Antes dos testes com janela: "Espera eu terminar".
