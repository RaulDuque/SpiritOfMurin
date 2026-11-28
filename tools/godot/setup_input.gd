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
	# L or right click to step, K for the ki wave, Space to lock, E to examine / pick up,
	# I or Tab for the inventory, Shift to sneak (walk quietly), C for the character sheet,
	# H to use a herb from the bag. K (ki) skins with Ki, Space (lock) focuses an animal.
	# Shift + cut is the Reforço de Ki ("reinforce", configurable apart from sneak).
	var actions := {
		"move_up": [key(KEY_W), key(KEY_UP)],
		"move_down": [key(KEY_S), key(KEY_DOWN)],
		"move_left": [key(KEY_A), key(KEY_LEFT)],
		"move_right": [key(KEY_D), key(KEY_RIGHT)],
		"cut": [key(KEY_J), mouse(MOUSE_BUTTON_LEFT)],
		"step": [key(KEY_L), mouse(MOUSE_BUTTON_RIGHT)],
		"ki": [key(KEY_K)],
		"lock": [key(KEY_SPACE)],
		"use": [key(KEY_E)],
		"inventory": [key(KEY_I), key(KEY_TAB)],
		"sneak": [key(KEY_SHIFT)],
		"reinforce": [key(KEY_SHIFT)], # held with the cut: the Reforço de Ki (Shift by default, shared with sneak on purpose)
		"sheet": [key(KEY_C)],
		"heal": [key(KEY_H)],
		"book": [key(KEY_B)],
		"map": [key(KEY_M)],
		"fullscreen": [key(KEY_F11)],
		"menu": [key(KEY_ESCAPE), key(KEY_P)],
		# T06: the item shortcuts (H stays the herb), the wheel chooses among the targets of E, Tab changes the bag's tab
		"quick_1": [key(KEY_1)], "quick_2": [key(KEY_2)], "quick_3": [key(KEY_3)], "quick_4": [key(KEY_4)],
		"quick_5": [key(KEY_5)], "quick_6": [key(KEY_6)], "quick_7": [key(KEY_Q)], "quick_8": [key(KEY_R)],
		"target_next": [mouse(MOUSE_BUTTON_WHEEL_DOWN)], "target_prev": [mouse(MOUSE_BUTTON_WHEEL_UP)],
		"bag_tab": [key(KEY_TAB)],
	}
	# actions that were renamed: "qi" became "ki" on 2026-11-27 (pedido 14)
	for retired in ["qi"]:
		if ProjectSettings.has_setting("input/" + retired):
			ProjectSettings.set_setting("input/" + retired, null)
	for name in actions:
		ProjectSettings.set_setting("input/" + name, {"deadzone": 0.2, "events": actions[name]})
	var err := ProjectSettings.save()
	print("input actions written: ", actions.keys(), " save=", err)
	quit(0 if err == OK else 1)
