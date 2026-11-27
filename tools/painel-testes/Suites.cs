namespace PainelTestes;

/// <summary>Uma execução de teste: uma cena do Godot com os seus argumentos (cada fase é um processo, como fechar e abrir o jogo).</summary>
public sealed record Suite(string Group, string Name, string Scene, string[] Args, bool WindowOnly = false, string About = "");

/// <summary>Os testes do projeto, na ordem em que dependem uns dos outros (a mesma do tools/godot/testes-janela.ps1).</summary>
public static class Suites
{
    /// <summary>As fases do teste do pedido 13 (mira, técnicas, bolsa, moedas), na ordem.</summary>
    public static readonly (string Phase, string About)[] Novidades =
    {
        ("aim", "Mira pelo mouse: andar para um lado e acertar para o outro; tecla remapeada; faca; Qi; alvo travado tem prioridade; destravar volta ao cursor; mira por teclado; outras janelas."),
        ("charge", "Disparo de Qi carregado (secreto): toque, carga parcial, carga completa de 100, guarda rompida, sem Qi, pausa/inventário/foco cancelam, tecla remapeada, descoberta e Livro; no máximo um projétil."),
        ("charge-plains", "Disparo carregado na Planície: selado antes do nível 3, enfraquecido depois; junto à carcaça o botão esfola."),
        ("reinforce", "Reforço de Qi: ativar com 75, recusar com 74, três golpes +10% e o quarto normal, faca 1 → 1,1, rolagem de ruptura fixa (sucesso e falha), Shift sozinho anda quieto, teclas remapeadas."),
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
        if (quick) return l;
        foreach (var ph in new[] { "full", "sealed", "resume2", "resume3", "migrate", "migrate2" })
            l.Add(new("Técnicas seladas", ph, tech, new[] { $"--phase={ph}" }, About: "Selo no despertar e volta enfraquecida no nível 3, com medições."));
        foreach (var (ph, about) in new[] { ("measure", "Bordas antes e depois em janela, maximizada, tela cheia e 1920x1080."), ("click", "Mouse em tela cheia; fechar menu sem cortar; F11."),
            ("map", "Mapa: marcador, explorar, meditar."), ("map2", "Mapa depois de carregar.") })
            l.Add(new("Tela e mapa", ph, disp, new[] { $"--phase={ph}", "--prefs=user://teste_tela_prefs.json" }, WindowOnly: ph is "measure" or "click", About: about));
        l.Add(new("Controles", "Teclado e mouse", "res://tests/integration/ControlsCheck.tscn", Array.Empty<string>(), About: "Andar, corte, passo, mouse."));
        foreach (int s in seeds)
        {
            l.Add(new("Planície", $"semente {s}", "res://tests/integration/PlainsCheck.tscn", new[] { $"--seed={s}" }, About: "Mundo da semente, faca, coelho, javali, ervas."));
            l.Add(new("Jornada", $"nova, semente {s}", "res://tests/integration/JourneyCheck.tscn", new[] { "--phase=new", $"--seed={s}" }, About: "Jornada nova até meditar."));
            l.Add(new("Jornada", $"retomada, semente {s}", "res://tests/integration/JourneyCheck.tscn", new[] { "--phase=resume", $"--seed={s}" }, About: "Título → Continuar."));
        }
        return l;
    }
}
