// PROTOTYPE - NOT FOR PRODUCTION
// Question: Lutar depois de aprender o passo lateral parece claramente diferente de lutar antes, e encadear passo, corte e qi com as mãos é divertido?
// Date: 2026-11-24
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
window.CONFIG = {
  seed: 1, // every gameplay random call reads rng(), seeded from this (or ?seed= in the address)
  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },

  // ---- Player ----
  player_speed: { value: 105, min: 60, max: 180, step: 5 },        // px/s walking
  player_hp: { value: 5, min: 1, max: 10, step: 1 },               // hearts (jade diamonds)
  qi_max: { value: 100, min: 30, max: 200, step: 10 },             // qi pool
  qi_regen: { value: 22, min: 0, max: 80, step: 1 },               // qi per second
  qi_cost: { value: 30, min: 5, max: 80, step: 5 },                // qi per wave (K)
  invuln_after_hit: { value: 0.6, min: 0, max: 2, step: 0.05 },    // s of mercy after being hit
  buffer_time: { value: 0.2, min: 0.05, max: 0.5, step: 0.01 },    // s a pressed J/K/L waits for a cancel window

  // ---- Cut chain (J) ----
  cut_damage: { value: 1, min: 1, max: 5, step: 1 },               // damage of cuts 1 and 2
  cut3_damage: { value: 2, min: 1, max: 6, step: 1 },              // damage of the third cut
  cut_range: { value: 46, min: 24, max: 90, step: 2 },             // px reach from the body
  cut_arc: { value: 110, min: 40, max: 180, step: 5 },             // degrees of the swing
  cut_startup: { value: 0.07, min: 0.02, max: 0.2, step: 0.01 },   // s before the blade is live
  cut_active: { value: 0.08, min: 0.03, max: 0.2, step: 0.01 },    // s the blade is live
  cut_recovery: { value: 0.24, min: 0.08, max: 0.5, step: 0.01 },  // s after the blade
  cut_cancel_at: { value: 0.13, min: 0.05, max: 0.4, step: 0.01 }, // s into a cut when J/K/L may cancel it
  thrust_range: { value: 64, min: 30, max: 110, step: 2 },         // px reach of forward+J
  thrust_damage: { value: 1, min: 1, max: 5, step: 1 },

  // ---- Steps (L) ----
  short_step_dist: { value: 34, min: 10, max: 80, step: 2 },       // px of the plain step (forward/back only)
  short_step_time: { value: 0.16, min: 0.06, max: 0.4, step: 0.01 },
  side_step_angle: { value: 95, min: 40, max: 160, step: 5 },      // degrees around the locked target
  side_step_time: { value: 0.16, min: 0.06, max: 0.4, step: 0.01 },
  side_step_iframes: { value: 0.14, min: 0, max: 0.4, step: 0.01 },// s untouchable during the side step
  dash_dist: { value: 112, min: 60, max: 200, step: 4 },           // px of the long step without a lock (crosses the gap)
  combo_window: { value: 0.45, min: 0.15, max: 1, step: 0.05 },    // s allowed between side step -> cut -> qi

  // ---- Guards ----
  guard_half: { value: 65, min: 20, max: 120, step: 5 },           // degrees either side of a guard's facing that are parried
  parry_push: { value: 34, min: 0, max: 90, step: 2 },             // px the parry throws the player back

  // ---- Disciple of the Closed Guard ----
  dis_hp: { value: 6, min: 1, max: 20, step: 1 },
  dis_turn: { value: 300, min: 90, max: 720, step: 10 },           // degrees per second (beats walking around him)
  dis_fooled: { value: 0.38, min: 0, max: 1, step: 0.02 },         // s he loses track after a side step
  dis_windup: { value: 0.5, min: 0.2, max: 1.2, step: 0.02 },      // s of red-ochre telegraph
  dis_reach: { value: 64, min: 30, max: 110, step: 2 },
  dis_cooldown: { value: 1.1, min: 0.3, max: 3, step: 0.05 },
  dis_recover: { value: 0.55, min: 0.2, max: 1.5, step: 0.05 },

  // ---- Senior Swordsman ----
  sen_hp: { value: 10, min: 1, max: 30, step: 1 },
  sen_turn: { value: 330, min: 90, max: 720, step: 10 },
  sen_fooled: { value: 0.26, min: 0, max: 1, step: 0.02 },
  sen_closed_time: { value: 3.4, min: 1, max: 8, step: 0.1 },      // s in the closed guard before the attack stance
  sen_windup: { value: 0.38, min: 0.15, max: 1, step: 0.02 },      // s telegraph of the first strike of his combo
  sen_combo_recover: { value: 0.8, min: 0.2, max: 2, step: 0.05 }, // s open after his three-strike combo
  sen_speed: { value: 85, min: 30, max: 160, step: 5 },            // px/s advance in the attack stance

  // ---- Shared hits ----
  enemy_damage: { value: 1, min: 1, max: 3, step: 1 },
  hitstop: { value: 0.07, min: 0, max: 0.2, step: 0.01 },          // s the world freezes on a landed hit
  shake: { value: 3, min: 0, max: 8, step: 1 },                    // px of screen shake

  // ---- Qi and the Silent Blade Step ----
  wave_speed: { value: 380, min: 150, max: 800, step: 10 },        // px/s of the qi wave
  wave_range: { value: 210, min: 80, max: 400, step: 10 },         // px it travels
  wave_damage: { value: 1, min: 1, max: 5, step: 1 },
  lamina_damage: { value: 3, min: 1, max: 8, step: 1 },            // Passo da Lâmina Silenciosa
  lamina_stagger: { value: 1.5, min: 0.3, max: 3, step: 0.1 },     // s the guard stays broken
};
