using System.Net;
using System.Text;

namespace PainelTestes;

/// <summary>Escreve o relatório de uma rodada de testes: relatorio.html (abre no navegador) e relatorio.md.</summary>
public static class Report
{
    public static string Write(string dir, List<Result> results, bool window, int screen, int[] seeds, TimeSpan total, bool stopped, string godot)
    {
        int ok = results.Count(r => r.Ok && !r.Skipped), fail = results.Count(r => !r.Ok), skip = results.Count(r => r.Skipped);
        int checks = results.Sum(r => r.Total), passed = results.Sum(r => r.Passed);
        string mode = window ? $"com janela, monitor {screen + 1} (1280×720)" : "sem janela (headless)";
        string verdict = stopped ? "PARADO antes do fim" : fail == 0 ? "TODOS PASSARAM" : $"{fail} EXECUÇÃO(ÕES) COM FALHA";
        static string H(string s) => WebUtility.HtmlEncode(s);

        var h = new StringBuilder();
        h.Append("<!doctype html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>Relatório de testes</title><style>");
        h.Append(":root{--bg:#1c1a15;--panel:#26231c;--ink:#ece5d0;--dim:#a39e8c;--ok:#4fb889;--bad:#e0503a;--skip:#c9a14a;--line:#4e3824}");
        h.Append("body{margin:0;background:var(--bg);color:var(--ink);font:15px/1.5 system-ui,Segoe UI,sans-serif}main{max-width:1100px;margin:0 auto;padding:24px 16px}");
        h.Append("h1{font-size:24px;margin:0 0 4px}.sub{color:var(--dim)}.verdict{font-size:20px;font-weight:700;margin:16px 0;padding:12px 16px;border-radius:6px;background:var(--panel);border-left:6px solid}");
        h.Append(".v-ok{border-color:var(--ok)}.v-bad{border-color:var(--bad)}.cards{display:flex;gap:12px;flex-wrap:wrap;margin:12px 0}.card{background:var(--panel);padding:10px 14px;border-radius:6px;min-width:120px}.card b{display:block;font-size:22px}");
        h.Append("table{width:100%;border-collapse:collapse;margin:12px 0}th,td{text-align:left;padding:6px 8px;border-bottom:1px solid var(--line);vertical-align:top}th{color:var(--dim);font-weight:600}");
        h.Append(".ok{color:var(--ok);font-weight:700}.bad{color:var(--bad);font-weight:700}.skip{color:var(--skip);font-weight:700}details{background:var(--panel);margin:6px 0;padding:6px 10px;border-radius:6px}summary{cursor:pointer}");
        h.Append("li{margin:2px 0}li.f{color:var(--bad)}code{color:var(--dim)}.small{color:var(--dim);font-size:13px}@media(max-width:640px){td:nth-child(4),th:nth-child(4){display:none}}</style></head><body><main>");
        h.Append($"<h1>SpiritOfMurin — relatório de testes</h1><div class=\"sub\">{DateTime.Now:dd/MM/yyyy HH:mm} · {H(mode)} · sementes {string.Join(" e ", seeds)} · duração {total:hh\\:mm\\:ss}</div>");
        h.Append($"<div class=\"verdict {(fail == 0 && !stopped ? "v-ok" : "v-bad")}\">{H(verdict)}</div>");
        h.Append($"<div class=\"cards\"><div class=\"card\"><b class=\"ok\">{ok}</b>execuções OK</div><div class=\"card\"><b class=\"bad\">{fail}</b>com falha</div><div class=\"card\"><b class=\"skip\">{skip}</b>puladas (só com janela)</div><div class=\"card\"><b>{passed}/{checks}</b>verificações</div></div>");
        h.Append("<table><tr><th>Grupo</th><th>Execução</th><th>Resultado</th><th>O que verifica</th><th>Tempo</th></tr>");
        foreach (var r in results)
        {
            string st = r.Skipped ? "<span class=\"skip\">pulada</span>" : r.Ok ? $"<span class=\"ok\">OK {r.Passed}/{r.Total}</span>" : $"<span class=\"bad\">FALHA {r.Passed}/{r.Total}{(r.Exit != 0 && r.Total == 0 ? $" (saída {r.Exit})" : "")}</span>";
            h.Append($"<tr><td>{H(r.Suite.Group)}</td><td>{H(r.Suite.Name)}</td><td>{st}</td><td class=\"small\">{H(r.Suite.About)}</td><td>{(r.Skipped ? "" : $"{r.Time.TotalSeconds:0} s")}</td></tr>");
        }
        h.Append("</table><h2>Detalhes de cada execução</h2>");
        foreach (var r in results.Where(r => !r.Skipped))
        {
            h.Append($"<details{(r.Ok ? "" : " open")}><summary><b>{H(r.Suite.Group)} · {H(r.Suite.Name)}</b> — {(r.Ok ? "<span class=\"ok\">OK</span>" : "<span class=\"bad\">FALHA</span>")} {r.Passed}/{r.Total}</summary><ul>");
            foreach (var c in r.Checks) h.Append($"<li class=\"{(c.Ok ? "" : "f")}\">{(c.Ok ? "✔" : "✘")} {H(c.Text)}</li>");
            if (r.Checks.Count == 0) h.Append("<li class=\"f\">O teste não imprimiu verificações (veja o log).</li>");
            foreach (var m in r.Measures) h.Append($"<li><code>MEDIDA {H(m)}</code></li>");
            h.Append($"</ul><div class=\"small\">log: <code>{H(Path.GetFileName(r.LogFile))}</code></div></details>");
        }
        h.Append($"<p class=\"small\">Os testes usam saves e preferências próprios (<code>user://teste_*.json</code>): a jornada e as teclas do jogador não são tocadas. Godot: <code>{H(godot)}</code>. Pasta desta rodada: <code>{H(dir)}</code>.</p></main></body></html>");
        string html = Path.Combine(dir, "relatorio.html");
        File.WriteAllText(html, h.ToString(), new UTF8Encoding(false));

        var md = new StringBuilder();
        md.AppendLine($"# Relatório de testes — {DateTime.Now:dd/MM/yyyy HH:mm}").AppendLine();
        md.AppendLine($"- **Modo:** {mode}  \n- **Sementes:** {string.Join(", ", seeds)}  \n- **Duração:** {total:hh\\:mm\\:ss}  \n- **Resultado:** {verdict} ({passed}/{checks} verificações)").AppendLine();
        md.AppendLine("| Grupo | Execução | Resultado | Tempo |").AppendLine("|---|---|---|---|");
        foreach (var r in results) md.AppendLine($"| {r.Suite.Group} | {r.Suite.Name} | {(r.Skipped ? "pulada (só com janela)" : r.Ok ? $"OK {r.Passed}/{r.Total}" : $"**FALHA** {r.Passed}/{r.Total}")} | {(r.Skipped ? "" : $"{r.Time.TotalSeconds:0} s")} |");
        var failed = results.Where(r => !r.Ok).ToList();
        if (failed.Count > 0)
        {
            md.AppendLine().AppendLine("## Falhas").AppendLine();
            foreach (var r in failed) { md.AppendLine($"### {r.Suite.Group} · {r.Suite.Name}"); foreach (var c in r.Checks.Where(c => !c.Ok)) md.AppendLine($"- {c.Text}"); md.AppendLine($"- log: `{Path.GetFileName(r.LogFile)}`").AppendLine(); }
        }
        File.WriteAllText(Path.Combine(dir, "relatorio.md"), md.ToString(), new UTF8Encoding(false));
        return html;
    }
}
