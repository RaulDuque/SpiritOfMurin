# Runs SpiritOfMurin's checks with a window, on the monitor you choose (default: monitor 2).
# Usage (PowerShell, from the repo root):  .\tools\godot\testes-janela.ps1            # monitor 2
#                                          .\tools\godot\testes-janela.ps1 -Monitor 1 # monitor 1
#                                          .\tools\godot\testes-janela.ps1 -Seed 777
#                                          .\tools\godot\testes-janela.ps1 -SoPrologo -Lento  # only the prologue, slowed down to watch
# Godot numbers screens from 0, so monitor 2 is --screen 1. Exit code 0 when every check passes.
param([int]$Monitor = 2, [int]$Seed = 20261126, [switch]$Lento, [switch]$SoPrologo)
$godot = "C:/Users/Raul/Tools/Godot/4.7.2-stable-mono/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe"
$screen = [Math]::Max(0, $Monitor - 1)
$fail = 0
$slow = @(); if ($Lento) { $slow = @('--lento') }
$prefs = '--prefs=user://teste_vale_prefs.json'
# the campaign's prologue and its ending (each phase is a separate run, like closing and opening the game)
foreach ($phase in 'prologue', 'resume-vale', 'resume-ending', 'old-save', 'skip') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/ValeCheck.tscn -- "--phase=$phase" $prefs @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
if ($SoPrologo) { if ($fail -eq 0) { Write-Host "Prologo: todos os testes passaram." } else { Write-Host "$fail execucao(oes) com falha." }; exit $fail }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/unit/UnitRunner.tscn; if ($LASTEXITCODE -ne 0) { $fail++ }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/ControlsCheck.tscn; if ($LASTEXITCODE -ne 0) { $fail++ }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/PlainsCheck.tscn -- --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
# the journey: a new one until meditating, then the game opened again (title, Continuar)
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/JourneyCheck.tscn -- --phase=new --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/JourneyCheck.tscn -- --phase=resume --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
if ($fail -eq 0) { Write-Host "Todos os testes passaram." } else { Write-Host "$fail execucao(oes) com falha." }
exit $fail
