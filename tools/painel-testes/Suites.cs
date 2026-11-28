namespace PainelTestes;

/// <summary>Uma execução de teste: uma cena do Godot com os seus argumentos (cada fase é um processo, como fechar e abrir o jogo).</summary>
public sealed record Suite(string Group, string Name, string Scene, string[] Args, bool WindowOnly = false, string About = "");

/// <summary>Os testes do projeto, na ordem em que dependem uns dos outros (a mesma do tools/godot/testes-janela.ps1).</summary>
public static class Suites
{
    /// <summary>As fases do teste do pedido 13 (mira, técnicas, bolsa, moedas), na ordem.</summary>
    public static readonly (string Phase, string About)[] Novidades =
    {
        ("aim", "Mira pelo mouse: andar para um lado e acertar para o outro; tecla remapeada; faca; Ki; alvo travado tem prioridade; destravar volta ao cursor; mira por teclado; outras janelas."),
        ("charge", "Disparo de Ki carregado (secreto): toque, carga parcial, carga completa de 100, guarda rompida, sem Ki, pausa/inventário/foco cancelam, tecla remapeada, descoberta e Livro; no máximo um projétil."),
        ("charge-plains", "Disparo carregado na Planície: selado antes do nível 3, enfraquecido depois; junto à carcaça o botão esfola."),
        ("reinforce", "Reforço de Ki: ativar com 75, recusar com 74, três golpes +10% e o quarto normal, faca 1 → 1,1, rolagem de ruptura fixa (sucesso e falha), Shift sozinho anda quieto, teclas remapeadas."),
        ("bag", "Bolsa com figuras: ícones e pilhas; fabricar a faca pelo teclado (materiais diminuem, faca aparece); equipar com clique; usar a erva pelo teclado e pelo botão; tela cheia."),
        ("money", "Moedas: começa sem dinheiro; bolsinha junto à esteira dá +25; voltar ao bloco mantém recolhida; receber e gastar, recusa por saldo; meditar salva."),
        ("money2", "Continuar: saldo e bolsinha recolhida voltam."),
        ("oldsave", "Save de antes desta entrega (sem moedas) carrega com itens e equipamento; o despertar começa sem dinheiro."),
    };

    /// <summary>As fases do teste do pedido 14.0 (FOR, CON, crítico, números de vida e de dano, Qi → Ki).</summary>
    public static readonly (string Phase, string About)[] Atributos =
    {
        ("plains", "FOR 5 aumenta o corte da faca (1 → 1,3); crítico ×1,5 com '!'; CON 5 recebe 3,4 de 4; vida do javali em números dentro da barra; números de dano ligam e desligam no Modo de assistência; FOR salvo."),
        ("vale", "Prólogo: vida do discípulo em números na barra; o golpe mostra o dano; sem ficha, sem FOR nem crítico."),
        ("oldsave", "Qi → Ki: jornada real de antes do renome continua com Ki, KI e o Livro; a tecla do Qi num botão do mouse vira a do Ki."),
        ("diagonal", "Andar na diagonal: 0,8 da velocidade em cada eixo (cerca de 13% mais distância que reto); reto continua 105 px/s."),
    };

    /// <summary>Monta a lista para as sementes escolhidas. <paramref name="quick"/> deixa só o prólogo, a recuperação e a unidade.</summary>
    public static List<Suite> Build(int[] seeds, bool quick)
    {
        var l = new List<Suite>();
        const string vale = "res://tests/integration/ValeCheck.tscn", tech = "res://tests/integration/TechCheck.tscn",
            disp = "res://tests/integration/DisplayCheck.tscn", rec = "res://tests/integration/RecoveryCheck.tscn";
        l.Add(new("Unidade", "Testes de unidade", "res://tests/unit/UnitRunner.tscn", Array.Empty<string>(), About: "Fórmulas, gerador do mundo, progressão, técnicas."));
        foreach (var (ph, about) in new[] {
            ("prologue", "Novo jogo → Vale → ponte, manual, túmulo, passo lateral, Sênior, explosão, despertar na Planície."),
            ("resume-vale", "Continuar da meditação no Vale."), ("resume-ending", "Jogo fechado durante o fim do prólogo."),
            ("old-save", "Save antigo (formato 1) continua na Planície."), ("skip", "Pular o fim já visto.") })
            l.Add(new("Prólogo (Vale)", ph, vale, new[] { $"--phase={ph}", "--prefs=user://teste_vale_prefs.json" }, About: about));
        foreach (var (ph, about) in new[] {
            ("vale", "Ponte, guarda, recuo e pista, mapa, manual, livro, meditar."), ("vale2", "Continuar: pistas mantidas; revanche pelo flanco."),
            ("controls", "Esc → Controles: troca do corte por mouse, conflito, fechar sem cortar."), ("controls2", "Novo processo: a tecla nova continua; restaurar padrões."),
            ("assist", "Modo de assistência liga e salva."), ("assist2", "Continua ligado; desliga."),
            ("book", "Livro da Planície: selado e enfraquecido com números reais."), ("layout", "Minimapa em três tamanhos; cantos do mouse em janela, maximizada e tela cheia.") })
            l.Add(new("Recuperação do protótipo", ph, rec, new[] { $"--phase={ph}", "--prefs=user://teste_rec_prefs.json" }, WindowOnly: ph == "layout", About: about));
        const string nov = "res://tests/integration/NovidadesCheck.tscn";
        foreach (var (ph, about) in Novidades)
            l.Add(new("Novidades (pedido 13)", ph, nov, new[] { $"--phase={ph}", "--prefs=user://teste_nov_prefs.json" }, About: about));
        const string atr = "res://tests/integration/AtributosCheck.tscn";
        foreach (var (ph, about) in Atributos)
            l.Add(new("Atributos e Ki (pedido 14)", ph, atr, new[] { $"--phase={ph}", "--prefs=user://teste_atr_prefs.json" }, About: about));
        const string inv = "res://tests/integration/InventarioCheck.tscn";
        foreach (var (ph, about) in new[] {
            ("bolsa", "T06: bolsa com abas (Tudo, Equipamentos, Ferramentas, Consumíveis, Materiais, Fabricação), moedas, comparação em golpes, \"tenho X de Y\", fabricar na mesma tela, machado lapidado derruba em 3."),
            ("atalhos", "T06: 1/2/3 na bolsa põem itens nos atalhos sem usar nada; no mundo trocam a ferramenta e usam a erva até acabar (0 e aviso)."),
            ("atalhos2", "T06: os atalhos continuam depois de fechar e abrir; Controles lista Atalho 1 e Próximo alvo na roda."),
            ("ficha", "T06: mouse na barra mostra \"Vida 60% · 6/10\"; ficha com +, prévia do dano da arma (FOR) e do teto de CON; mouse e teclado."),
            ("entrada", "T06: abrir, confirmar e fechar bolsa, ficha e pausa não corta, não dispara Ki, não coleta."),
            ("oldsave", "T06: jornada antiga carrega com itens, faca, atributos e moedas; abas e atalhos vazios.") })
            l.Add(new("Bolsa, ficha e HUD (T06)", ph, inv, new[] { $"--phase={ph}", "--prefs=user://teste_inv_prefs.json" }, About: about));
        foreach (int s in seeds)
            foreach (var (ph, about) in new[] {
                ("alvo", "T06: fora da clareira, erva e pegada ao alcance: anel e lista, roda do mouse troca, E age só no escolhido."),
                ("rodape", "T06: três galhos viram um só \"+3 Galho Rijo\" sobre a cabeça; junto à carcaça o rodapé diz E e K."),
                ("percurso", "T06: percurso contínuo: falta sílex, a receita diz, recolhe, fabrica, equipa, usa, compara com o lapidado, gasta um ponto, escolhe o alvo.") })
                l.Add(new("Bolsa, ficha e HUD (T06)", $"{ph}, semente {s}", inv, new[] { $"--phase={ph}", $"--seed={s}", "--prefs=user://teste_inv_prefs.json" }, About: about));
        const string fauna = "res://tests/integration/FaunaCheck.tscn";
        foreach (int s in seeds)
            foreach (var (ph, about) in new[] {
                ("duelo", "T07: aparo no aviso (a lâmina resvala, som seco, Vida igual); investida acerta (impacto, empurrão e Vida no mesmo quadro); contra-ataque na recuperação (recuo no quadro em que a Vida cai)."),
                ("faixa", "T07: a faixa da investida aparece quando a direção trava e termina onde a investida termina, com a assistência ligada e desligada; modos curta e desligada."),
                ("barra", "T07: barra compacta com números sobre o animal focado ou o javali em luta; a barra do alto continua; some 3 s depois."),
                ("lamina", "T07: o Passo da Lâmina Silenciosa enfraquecido atravessa o aviso com metade do dano (decisão B)."),
                ("coelho", "T07: ouve andando a ~95 px e com Shift a ~26; orelhas, agachado antes do salto, arbusto balança, mergulho na toca."),
                ("pegadas", "T07: pegadas frescas seguem o caminho real (também a curva) e somem em 90 s; o exame fica mais preciso com Rastreamento; a roda escolhe a pegada em vez da erva."),
                ("tecnicas", "T07: Disparo de Ki pleno termina em 210 px e o enfraquecido em 105, desfazendo-se ali; passo longo 112 × 60 px."),
                ("mapa", "T07: o javali só aparece no mapa depois de visto ou rastreado, e isso fica salvo."),
                ("vento", "T07: na rajada a faixa da investida continua legível."),
                ("oldsave", "T07: a jornada de antes do Ki (fixture) carrega; nenhum javali no mapa; faixa completa por padrão."),
                ("trilhas", "T07: as trilhas geradas da clareira continuam as mesmas e não somem.") })
                l.Add(new("Combate e fauna (T07)", $"{ph}, semente {s}", fauna, new[] { $"--phase={ph}", $"--seed={s}", "--prefs=user://teste_fauna_prefs.json" }, About: about));
        if (quick) return l;
        foreach (var ph in new[] { "full", "sealed", "resume2", "resume3", "migrate", "migrate2" })
            l.Add(new("Técnicas seladas", ph, tech, new[] { $"--phase={ph}" }, About: "Selo no despertar e volta enfraquecida no nível 3, com medições."));
        foreach (var (ph, about) in new[] { ("measure", "Bordas antes e depois em janela, maximizada, tela cheia e 1920x1080."), ("click", "Mouse em tela cheia; fechar menu sem cortar; F11."),
            ("map", "Mapa: marcador, explorar, meditar."), ("map2", "Mapa depois de carregar.") })
            l.Add(new("Tela e mapa", ph, disp, new[] { $"--phase={ph}", "--prefs=user://teste_tela_prefs.json" }, WindowOnly: ph is "measure" or "click", About: about));
        l.Add(new("Controles", "Teclado e mouse", "res://tests/integration/ControlsCheck.tscn", Array.Empty<string>(), About: "Andar, corte, passo, mouse."));
        foreach (int s in seeds)
            foreach (var (ph, about) in new[] {
                ("work", "Entrega 5: fabricar machado e picareta; cortar pinheiro (entalhes, queda, toco); recusa da faca; rocha com veio (rachaduras, cascalho) e pedra sem veio; bambu cedendo à faca; sair do bloco e voltar; machado lapidado; meditar."),
                ("resume", "Entrega 5: Continuar traz toco, pinheiro meio cortado, rocha quebrada e inventário, sem materiais em dobro."),
                ("bush", "Entrega 5: coelho e personagem atravessando o arbusto (folhas inclinam e voltam), também na rajada.") })
                l.Add(new("Recursos (Entrega 5)", $"{ph}, semente {s}", "res://tests/integration/RecursosCheck.tscn", new[] { $"--phase={ph}", $"--seed={s}", "--prefs=user://teste_rec5_prefs.json" }, About: about));
        l.Add(new("Elementos (padrão)", "catálogo", "res://tests/integration/ElementosCheck.tscn", new[] { "--phase=catalogo" }, About: "Todo arquivo de assets/data/elements: carrega, id estável, arte existe, habitat, ferramenta errada recusada, esgota com a certa e dá itens reais (inclui o que o /create criar)."));
        foreach (int s in seeds)
            l.Add(new("Elementos (padrão)", $"no mundo, semente {s}", "res://tests/integration/ElementosCheck.tscn", new[] { "--phase=mundo", $"--seed={s}" }, About: "Cada família aparece em volta da clareira; cada elemento trabalhável responde à ferramenta no seu habitat."));
        l.Add(new("Recursos (Entrega 5)", "save antigo", "res://tests/integration/RecursosCheck.tscn", new[] { "--phase=oldsave", "--prefs=user://teste_rec5_prefs.json" }, About: "Jornada de antes da Entrega 5: tudo em pé, faca na mão corta bambu."));
        foreach (int s in seeds)
        {
            l.Add(new("Planície", $"semente {s}", "res://tests/integration/PlainsCheck.tscn", new[] { $"--seed={s}" }, About: "Mundo da semente, faca, coelho, javali, ervas."));
            l.Add(new("Jornada", $"nova, semente {s}", "res://tests/integration/JourneyCheck.tscn", new[] { "--phase=new", $"--seed={s}" }, About: "Jornada nova até meditar."));
            l.Add(new("Jornada", $"retomada, semente {s}", "res://tests/integration/JourneyCheck.tscn", new[] { "--phase=resume", $"--seed={s}" }, About: "Título → Continuar."));
            l.Add(new("Ambientação", $"dia, semente {s}", "res://tests/integration/AmbientCheck.tscn", new[] { "--phase=day", $"--seed={s}" }, About: "Meio-dia, relógio, vento e capim, folhas, pássaros com sombra, vau lento com ondinhas, água funda bloqueia, transições, variedade, camadas."));
            l.Add(new("Ambientação", $"noite, semente {s}", "res://tests/integration/AmbientCheck.tscn", new[] { "--phase=night", $"--seed={s}" }, About: "Noite azulada e legível, luz da fogueira e do personagem, entardecer, meditar salva a hora."));
            l.Add(new("Ambientação", $"retomada da hora, semente {s}", "res://tests/integration/AmbientCheck.tscn", new[] { "--phase=resume", $"--seed={s}" }, About: "Continuar traz a hora salva."));
        }
        return l;
    }
}
