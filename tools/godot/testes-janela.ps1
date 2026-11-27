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
# the seal and the weakened return of the techniques (delivery 2)
foreach ($phase in 'full', 'sealed', 'resume2', 'resume3', 'migrate', 'migrate2') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/TechCheck.tscn -- "--phase=$phase" @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
# full screen, mouse clicks and the map (delivery 3)
foreach ($phase in 'measure', 'click', 'map', 'map2') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/DisplayCheck.tscn -- "--phase=$phase" '--prefs=user://teste_tela_prefs.json' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
# the prototype's recovery (pedido 11): bridge, guard, clues, map, book, controls, assist, layout
foreach ($phase in 'vale', 'vale2', 'controls', 'controls2', 'assist', 'assist2', 'book', 'layout') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/RecoveryCheck.tscn -- "--phase=$phase" '--prefs=user://teste_rec_prefs.json' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
# pedido 13: aim, techniques, bag, money
foreach ($phase in 'aim', 'charge', 'charge-plains', 'reinforce', 'bag', 'money', 'money2', 'oldsave') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/NovidadesCheck.tscn -- "--phase=$phase" '--prefs=user://teste_nov_prefs.json' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
# pedido 14.0: FOR, CON, critical, Vida in numbers, damage numbers, Qi -> Ki with old files
foreach ($phase in 'plains', 'vale', 'oldsave') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/AtributosCheck.tscn -- "--phase=$phase" '--prefs=user://teste_atr_prefs.json' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
# pedido 14, campaign delivery 5: trees, rocks, bamboo, bushes (both seeds, then a journey from before)
foreach ($sd in 20261126, 777) {
  foreach ($phase in 'work', 'resume', 'bush') {
    & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/RecursosCheck.tscn -- "--phase=$phase" "--seed=$sd" '--prefs=user://teste_rec5_prefs.json' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
  }
}
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/RecursosCheck.tscn -- '--phase=oldsave' '--prefs=user://teste_rec5_prefs.json' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
# pedido 14: every element in assets/data/elements (isolated catalogue, then the world in both seeds)
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/ElementosCheck.tscn -- '--phase=catalogo' @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
foreach ($sd in 20261126, 777) {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/ElementosCheck.tscn -- '--phase=mundo' "--seed=$sd" @slow; if ($LASTEXITCODE -ne 0) { $fail++ }
}
if ($SoPrologo) { if ($fail -eq 0) { Write-Host "Prologo: todos os testes passaram." } else { Write-Host "$fail execucao(oes) com falha." }; exit $fail }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/unit/UnitRunner.tscn; if ($LASTEXITCODE -ne 0) { $fail++ }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/ControlsCheck.tscn; if ($LASTEXITCODE -ne 0) { $fail++ }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/PlainsCheck.tscn -- --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
# the journey: a new one until meditating, then the game opened again (title, Continuar)
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/JourneyCheck.tscn -- --phase=new --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
& $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/JourneyCheck.tscn -- --phase=resume --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
# the Planície's ambience (day and night, both seeds)
foreach ($ph in 'day', 'night', 'resume') {
  & $godot --path . --windowed --resolution 1280x720 --screen $screen res://tests/integration/AmbientCheck.tscn -- "--phase=$ph" --seed=$Seed; if ($LASTEXITCODE -ne 0) { $fail++ }
}
if ($fail -eq 0) { Write-Host "Todos os testes passaram." } else { Write-Host "$fail execucao(oes) com falha." }
exit $fail
