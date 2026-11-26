# Writes SpiritOfMurin's input actions into project.godot, through Godot itself, so the
# serialization is exactly what the editor would write. Safe to run again: it replaces them.
# Run: <godot> --headless --path . -s res://tools/godot/setup_input.gd
extends SceneTree

func key(physical: Key) -> InputEventKey:
	var e := InputEventKey.new()
	e.physical_keycode = physical
	e.device = -1 # any keyboard, as the editor writes it
	return e

func mouse(button: MouseButton) -> InputEventMouseButton:
	var e := InputEventMouseButton.new()
	e.button_index = button
	e.device = -1 # any mouse
	return e

func _init() -> void:
	# The HTML prototype's defaults: WASD (and arrows) to walk, J or left click to cut,
	# L or right click to step, K for the qi wave, Space to lock, E to examine / pick up,
	# I or Tab for the inventory, Shift to sneak (walk quietly), C for the character sheet,
	# H to use a herb from the bag. K (qi) skins with Qi, Space (lock) focuses an animal.
	var actions := {
		"move_up": [key(KEY_W), key(KEY_UP)],
		"move_down": [key(KEY_S), key(KEY_DOWN)],
		"move_left": [key(KEY_A), key(KEY_LEFT)],
		"move_right": [key(KEY_D), key(KEY_RIGHT)],
		"cut": [key(KEY_J), mouse(MOUSE_BUTTON_LEFT)],
		"step": [key(KEY_L), mouse(MOUSE_BUTTON_RIGHT)],
		"qi": [key(KEY_K)],
		"lock": [key(KEY_SPACE)],
		"use": [key(KEY_E)],
		"inventory": [key(KEY_I), key(KEY_TAB)],
		"sneak": [key(KEY_SHIFT)],
		"sheet": [key(KEY_C)],
		"heal": [key(KEY_H)],
	}
	for name in actions:
		ProjectSettings.set_setting("input/" + name, {"deadzone": 0.2, "events": actions[name]})
	var err := ProjectSettings.save()
	print("input actions written: ", actions.keys(), " save=", err)
	quit(0 if err == OK else 1)
