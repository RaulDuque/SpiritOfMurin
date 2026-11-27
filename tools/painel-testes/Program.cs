using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PainelTestes;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Painel());
    }
}

/// <summary>O resultado de uma execução: o que o teste imprimiu, as verificações e o tempo.</summary>
public sealed class Result
{
    public Suite Suite = null!;
    public int Passed, Total, Exit;
    public bool Skipped;
    public TimeSpan Time;
    public List<(bool Ok, string Text)> Checks = new();
    public List<string> Measures = new();
    public string LogFile = "";
    public bool Ok => Skipped || (Exit == 0 && Total > 0 && Passed == Total);
}

/// <summary>
/// A janela do Painel de Testes: Jogar, rodar os testes (com janela, para assistir o jogo se jogar sozinho, ou sem
/// janela, mais rápido), parar, e abrir o relatório. No fim de cada rodada escreve um relatório HTML (e um .md)
/// em production/qa/execucoes/&lt;data&gt;/ e o abre no navegador.
/// </summary>
public sealed class Painel : Form
{
    private readonly ComboBox _monitor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
    private readonly ComboBox _seeds = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly CheckBox _slow = new() { Text = "Lento (para acompanhar)", AutoSize = true };
    private readonly TextBox _log = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f), BackColor = Color.FromArgb(22, 22, 18), ForeColor = Color.FromArgb(236, 229, 208) };
    private readonly Label _status = new() { AutoSize = true, Text = "Pronto." };
    private readonly ProgressBar _bar = new() { Width = 300, Height = 18 };
    private readonly Button _play = new() { Text = "▶  Jogar", Width = 150, Height = 40 };
    private readonly Button _all = new() { Text = "Rodar todos os testes (com janela)", Width = 250, Height = 40 };
    private readonly Button _quick = new() { Text = "Rodar prólogo + recuperação (com janela)", Width = 280, Height = 40 };
    private readonly Button _headless = new() { Text = "Rodar todos sem janela (rápido)", Width = 230, Height = 40 };
    private readonly Button _stop = new() { Text = "Parar", Width = 90, Height = 40, Enabled = false };
    private readonly Button _report = new() { Text = "Abrir último relatório", Width = 170, Height = 30 };
    private readonly Button _godotBtn = new() { Text = "Escolher o Godot…", Width = 140, Height = 30 };
    private readonly Button _showcase = new() { Text = "Vitrine (personagem e mobs)", Width = 200, Height = 40 };
    private readonly CheckBox _live = new() { Text = "Modo ao vivo: recompilar e reabrir ao salvar o código", AutoSize = true, Checked = true };
    private FileSystemWatcher? _watch;
    private System.Windows.Forms.Timer _debounce = new() { Interval = 900 };
    private Process? _game;
    private string _gameScene = "";
    private bool _building;
    private CancellationTokenSource? _cts;
    private Process? _running;
    private readonly string _root;
    private string _godot;
    private string _lastReport = "";

    private static readonly string SettingsFile = Path.Combine(AppContext.BaseDirectory, "PainelDeTestes.json");

    public Painel()
    {
        Text = "SpiritOfMurin — Painel de Testes";
        Width = 1100; Height = 720; StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(34, 32, 26); ForeColor = Color.FromArgb(236, 229, 208);
        _root = FindProject() ?? "";
        _godot = LoadGodot();

        _monitor.Items.AddRange(new object[] { "Monitor 1", "Monitor 2", "Monitor 3" }); _monitor.SelectedIndex = Math.Min(1, Screen.AllScreens.Length - 1);
        _seeds.Items.AddRange(new object[] { "Semente 20261126", "Semente 777", "As duas sementes" }); _seeds.SelectedIndex = 2;
        foreach (var b in new[] { _play, _showcase, _all, _quick, _headless, _stop, _report, _godotBtn }) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(78, 56, 36); b.ForeColor = ForeColor; }
        _play.BackColor = Color.FromArgb(29, 94, 87);

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(8), WrapContents = false };
        top.Controls.AddRange(new Control[] { _play, _showcase, _all, _quick, _headless, _stop });
        _showcase.BackColor = Color.FromArgb(29, 94, 87);
        var liveRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, Padding = new Padding(8, 2, 8, 2) };
        liveRow.Controls.Add(_live);
        var opts = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8, 4, 8, 4), WrapContents = false };
        opts.Controls.AddRange(new Control[] { new Label { Text = "Janela no:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, _monitor, _seeds, _slow, _report, _godotBtn });
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(8, 6, 8, 4) };
        bottom.Controls.AddRange(new Control[] { _bar, _status });
        Controls.Add(_log); Controls.Add(bottom); Controls.Add(liveRow); Controls.Add(opts); Controls.Add(top);

        _play.Click += async (_, _) => await Launch("");
        _showcase.Click += async (_, _) => await Launch("res://src/Dev/Vitrine.tscn");
        _debounce.Tick += async (_, _) => { _debounce.Stop(); await Rebuild(); };
        StartWatch();
        _all.Click += async (_, _) => await RunAll(window: true, quick: false);
        _quick.Click += async (_, _) => await RunAll(window: true, quick: true);
        _headless.Click += async (_, _) => await RunAll(window: false, quick: false);
        _stop.Click += (_, _) => { _cts?.Cancel(); try { _running?.Kill(true); } catch { } };
        _report.Click += (_, _) => OpenLastReport();
        _godotBtn.Click += (_, _) => ChooseGodot();

        Log("SpiritOfMurin — Painel de Testes");
        Log(_root.Length > 0 ? $"Projeto: {_root}" : "Projeto não encontrado: deixe o PainelDeTestes.exe dentro da pasta do projeto (onde fica o project.godot).");
        Log(File.Exists(_godot) ? $"Godot: {_godot}" : "Godot não encontrado: clique em \"Escolher o Godot…\" e aponte o Godot_v4.7.2-stable_mono_win64_console.exe.");
        Log("");
        Log("▶ Jogar compila o código e abre o jogo com a sua jornada normal.");
        Log("Vitrine mostra o protagonista nas 8 direções, deitado, ajoelhado, um boneco que você controla, e os mobs em todas as poses.");
        Log("Modo ao vivo: com o jogo ou a vitrine aberta pelo painel, ao salvar um arquivo .cs ele recompila e reabre sozinho (uns segundos).");
        Log("Os testes usam saves e preferências próprios (user://teste_*.json): a sua jornada e as suas teclas não são tocadas.");
        Log("Com janela, você assiste o jogo se jogar sozinho no monitor escolhido. No fim, o relatório abre no navegador.");
        // linha de comando: PainelDeTestes.exe --auto [--sem-janela] [--rapido] roda sozinho e fecha (o relatório fica na pasta)
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("--auto"))
            Shown += async (_, _) => { _noOpen = true; await RunAll(window: !args.Contains("--sem-janela"), quick: args.Contains("--rapido")); Close(); };
    }

    private bool _noOpen;

    // ------------------------------------------------------------------ setup
    private static string? FindProject()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && d != null; i++, d = d.Parent)
            if (File.Exists(Path.Combine(d.FullName, "project.godot"))) return d.FullName;
        return File.Exists(@"D:\Progr\SpiritOfMurin\project.godot") ? @"D:\Progr\SpiritOfMurin" : null;
    }

    private static string LoadGodot()
    {
        try { if (File.Exists(SettingsFile)) { var s = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SettingsFile)); if (s != null && s.TryGetValue("godot", out var g) && File.Exists(g)) return g; } } catch { }
        string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string guess = Path.Combine(user, @"Tools\Godot\4.7.2-stable-mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe");
        if (File.Exists(guess)) return guess;
        try
        {
            var tools = Path.Combine(user, @"Tools\Godot");
            if (Directory.Exists(tools)) { var f = Directory.GetFiles(tools, "Godot_v4.7*_mono_win64_console.exe", SearchOption.AllDirectories).FirstOrDefault(); if (f != null) return f; }
        }
        catch { }
        return guess;
    }

    private void ChooseGodot()
    {
        using var dlg = new OpenFileDialog { Filter = "Godot .NET (console)|Godot*mono*console.exe|Executáveis|*.exe", Title = "Escolha o Godot_v4.7.2-stable_mono_win64_console.exe" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _godot = dlg.FileName;
        File.WriteAllText(SettingsFile, JsonSerializer.Serialize(new Dictionary<string, string> { ["godot"] = _godot }));
        Log($"Godot: {_godot}");
    }

    private void Log(string s)
    {
        if (InvokeRequired) { BeginInvoke(() => Log(s)); return; }
        _log.AppendText(s + Environment.NewLine);
    }

    private void SetBusy(bool busy)
    {
        foreach (var b in new[] { _play, _showcase, _all, _quick, _headless, _godotBtn }) b.Enabled = !busy;
        _stop.Enabled = busy;
    }

    private bool Ready()
    {
        if (_root.Length == 0) { MessageBox.Show(this, "Não achei o project.godot. Deixe o PainelDeTestes.exe na pasta do projeto.", "Painel de Testes"); return false; }
        if (!File.Exists(_godot)) { MessageBox.Show(this, "Não achei o Godot. Clique em \"Escolher o Godot…\".", "Painel de Testes"); return false; }
        return true;
    }

    // ------------------------------------------------------------------ play
    /// <summary>
    /// Compiles the game's C# (dotnet build). The game only shows a code change after this: the editor's F5
    /// compiles by itself, a plain run of Godot does not. Errors go to the log (file, line, message).
    /// </summary>
    private async Task<bool> Build()
    {
        Log("Compilando o código…");
        var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = _root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true, StandardOutputEncoding = Encoding.UTF8 };
        foreach (var a in new[] { "build", Path.Combine(_root, "SpiritOfMurin.csproj"), "-nologo", "-v", "q", "-clp:NoSummary" }) psi.ArgumentList.Add(a);
        var sw = Stopwatch.StartNew();
        string text;
        try
        {
            using var p = Process.Start(psi)!;
            var o = p.StandardOutput.ReadToEndAsync(); var e = p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();
            text = await o + await e;
            if (p.ExitCode == 0) { Log($"Compilado ({sw.Elapsed.TotalSeconds:0.0} s)."); return true; }
        }
        catch (Exception ex) { Log("Não consegui rodar o dotnet: " + ex.Message); return false; }
        Log("ERRO DE COMPILAÇÃO — o jogo continua com a versão anterior até corrigir:");
        foreach (var line in text.Split('\n').Select(l => l.Trim()).Where(l => l.Contains(": error ")).Distinct().Take(15))
        {
            var m = Regex.Match(line, @"([^\\/]+\.cs)\((\d+),(\d+)\): error (\w+): (.*?)( \[.*)?$");
            Log(m.Success ? $"   {m.Groups[1].Value} linha {m.Groups[2].Value}: {m.Groups[5].Value}" : "   " + line);
        }
        return false;
    }

    /// <summary>Compiles and opens the game ("" = the normal start) or a scene (the vitrine).</summary>
    private async Task Launch(string scene)
    {
        if (!Ready() || _building) return;
        _building = true; SetBusy(true);
        try
        {
            if (!await Build()) { MessageBox.Show(this, "O código tem erro de compilação. Veja as linhas no painel.", "Painel de Testes"); return; }
            Start(scene);
        }
        finally { _building = false; SetBusy(false); }
    }

    private void Start(string scene)
    {
        try { if (_game != null && !_game.HasExited) _game.Kill(true); } catch { }
        string gui = _godot.Replace("_console.exe", ".exe");
        if (!File.Exists(gui)) gui = _godot;
        var psi = new ProcessStartInfo(gui) { WorkingDirectory = _root, UseShellExecute = false };
        psi.ArgumentList.Add("--path"); psi.ArgumentList.Add(_root);
        if (scene.Length > 0) { psi.ArgumentList.Add("--resolution"); psi.ArgumentList.Add("1280x960"); psi.ArgumentList.Add(scene); }
        _game = Process.Start(psi); _gameScene = scene;
        Log(scene.Length > 0 ? "Vitrine aberta." : "Jogo aberto (a sua jornada normal).");
    }

    // modo ao vivo: a saved .cs (or game data) rebuilds and reopens what the panel opened
    private void StartWatch()
    {
        if (_root.Length == 0) return;
        _watch = new FileSystemWatcher(Path.Combine(_root, "src")) { IncludeSubdirectories = true, Filter = "*.*", NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName };
        FileSystemEventHandler h = (_, e) =>
        {
            if (!_live.Checked || !(e.FullPath.EndsWith(".cs") || e.FullPath.EndsWith(".tscn"))) return;
            BeginInvoke(() => { _debounce.Stop(); _debounce.Start(); });
        };
        _watch.Changed += h; _watch.Created += h; _watch.Renamed += (o, e) => h(o, e);
        _watch.EnableRaisingEvents = true;
    }

    private async Task Rebuild()
    {
        if (_building || _cts != null && !_cts.IsCancellationRequested && _stop.Enabled) return;
        bool open = _game != null && !_game.HasExited;
        Log("");
        Log($"{DateTime.Now:HH:mm:ss} código salvo.");
        _building = true;
        try
        {
            if (!await Build()) return;
            if (open) { Start(_gameScene); Log("Reaberto com o código novo."); }
            else Log("Pronto. Abra o jogo ou a vitrine para ver.");
        }
        finally { _building = false; }
    }

    // ------------------------------------------------------------------ tests
    private async Task RunAll(bool window, bool quick)
    {
        if (!Ready()) return;
        if (!await Build()) { Log("Os testes não rodaram: corrija o erro de compilação."); return; }
        int[] seeds = _seeds.SelectedIndex switch { 0 => new[] { 20261126 }, 1 => new[] { 777 }, _ => new[] { 20261126, 777 } };
        var suites = Suites.Build(seeds, quick);
        int screen = _monitor.SelectedIndex; // Godot numbers screens from 0
        bool slow = _slow.Checked;
        string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string dir = Path.Combine(_root, "production", "qa", "execucoes", stamp);
        Directory.CreateDirectory(dir);
        _cts = new CancellationTokenSource();
        SetBusy(true);
        _bar.Maximum = suites.Count; _bar.Value = 0;
        var results = new List<Result>();
        var started = DateTime.Now;
        Log(""); Log($"=== {(window ? $"Com janela no monitor {screen + 1}" : "Sem janela")} · {suites.Count} execuções · {DateTime.Now:dd/MM/yyyy HH:mm} ===");
        for (int i = 0; i < suites.Count; i++)
        {
            if (_cts.IsCancellationRequested) break;
            var s = suites[i];
            _status.Text = $"{i + 1}/{suites.Count}: {s.Group} · {s.Name}";
            if (!window && s.WindowOnly)
            {
                results.Add(new Result { Suite = s, Skipped = true });
                Log($"[--]  {s.Group} · {s.Name}: só com janela (pulado)");
                _bar.Value = i + 1; continue;
            }
            var r = await RunOne(s, window, screen, slow, dir, _cts.Token);
            results.Add(r);
            Log($"[{(r.Ok ? "OK" : "FALHA")}]  {s.Group} · {s.Name}: {r.Passed}/{r.Total} ({r.Time.TotalSeconds:0} s)");
            foreach (var c in r.Checks.Where(c => !c.Ok)) Log($"        FALHOU: {c.Text}");
            _bar.Value = i + 1;
        }
        bool stopped = _cts.IsCancellationRequested;
        _lastReport = Report.Write(dir, results, window, screen, seeds, DateTime.Now - started, stopped, _godot);
        int fails = results.Count(r => !r.Ok);
        string summary = stopped ? "Parado antes do fim." : fails == 0 ? "Todos os testes passaram." : $"{fails} execução(ões) com falha.";
        _status.Text = summary; Log(summary); Log($"Relatório: {_lastReport}");
        SetBusy(false);
        if (!_noOpen) OpenLastReport();
    }

    private async Task<Result> RunOne(Suite s, bool window, int screen, bool slow, string dir, CancellationToken ct)
    {
        var r = new Result { Suite = s };
        var psi = new ProcessStartInfo(_godot) { WorkingDirectory = _root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true, StandardOutputEncoding = Encoding.UTF8 };
        psi.ArgumentList.Add("--path"); psi.ArgumentList.Add(_root);
        if (window) { foreach (var a in new[] { "--windowed", "--resolution", "1280x720", "--screen", screen.ToString() }) psi.ArgumentList.Add(a); }
        else psi.ArgumentList.Add("--headless");
        psi.ArgumentList.Add(s.Scene);
        psi.ArgumentList.Add("--");
        foreach (var a in s.Args) psi.ArgumentList.Add(a);
        if (slow && window) psi.ArgumentList.Add("--lento");
        var sw = Stopwatch.StartNew();
        var output = new StringBuilder();
        using var p = new Process { StartInfo = psi };
        p.OutputDataReceived += (_, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };
        p.Start(); _running = p;
        p.BeginOutputReadLine(); p.BeginErrorReadLine();
        try { await p.WaitForExitAsync(ct).WaitAsync(TimeSpan.FromMinutes(10), ct); }
        catch { try { p.Kill(true); } catch { } }
        _running = null;
        r.Time = sw.Elapsed;
        r.Exit = p.HasExited ? p.ExitCode : -1;
        string text; lock (output) text = output.ToString();
        string safe = Regex.Replace($"{s.Group}-{s.Name}", @"[^\w\-]+", "_");
        r.LogFile = Path.Combine(dir, safe + ".log");
        File.WriteAllText(r.LogFile, text);
        foreach (var line in text.Split('\n'))
        {
            string l = line.TrimEnd('\r');
            if (l.StartsWith("OK  ") || l.StartsWith("OK ")) r.Checks.Add((true, l[3..].Trim()));
            else if (l.StartsWith("FAIL")) r.Checks.Add((false, l[4..].Trim()));
            else if (l.StartsWith("MEDIDA")) r.Measures.Add(l[6..].Trim());
            var m = Regex.Match(l, @"(\d+)/(\d+) OK");
            if (m.Success) { r.Passed = int.Parse(m.Groups[1].Value); r.Total = int.Parse(m.Groups[2].Value); }
        }
        if (r.Total == 0 && r.Checks.Count > 0) { r.Total = r.Checks.Count; r.Passed = r.Checks.Count(c => c.Ok); }
        return r;
    }

    private void OpenLastReport()
    {
        if (_lastReport.Length == 0)
        {
            var dir = Path.Combine(_root, "production", "qa", "execucoes");
            if (Directory.Exists(dir)) _lastReport = Directory.GetDirectories(dir).OrderBy(d => d).Select(d => Path.Combine(d, "relatorio.html")).LastOrDefault(File.Exists) ?? "";
        }
        if (_lastReport.Length > 0 && File.Exists(_lastReport)) Process.Start(new ProcessStartInfo(_lastReport) { UseShellExecute = true });
        else Log("Nenhum relatório ainda.");
    }
}
