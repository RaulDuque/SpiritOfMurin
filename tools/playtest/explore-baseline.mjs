// Baseline: play the current loop from the normal start and record what a player sees.
// Usage: node tools/playtest/explore-baseline.mjs <outdir>
import { launch } from './cdp.mjs';
import { makeBot } from './bot.mjs';
import { writeFileSync, mkdirSync } from 'node:fs';

const out = process.argv[2] || '.'; mkdirSync(out, { recursive: true });
const URL = 'file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html';
const TS = 32;
const log = [];
const g = await launch({ url: URL });
const bot = makeBot(g, log);
try {
  await g.shot(`${out}/00-titulo.png`);
  await g.tap('Enter'); await g.sleep(400);
  await bot.note('start');
  // 1. to the bridge disciple
  await bot.goTo(29 * TS + 16, 21 * TS);
  await bot.goTo(29 * TS + 16, 18.6 * TS, { stop: s => !!s.speech });
  await g.sleep(300); await bot.note('first sight of the bridge disciple');
  await g.shot(`${out}/01-ponte-fala.png`);
  await g.tap('Space'); await g.sleep(100);
  for (let i = 0; i < 5; i++) { await g.hold(['KeyW'], 250); await g.tap('KeyJ'); await g.sleep(300); }
  await g.shot(`${out}/02-ponte-aparado.png`);
  await bot.note('after five frontal cuts');
  // 2. retreat
  await g.tap('Space');
  await bot.goTo(29 * TS + 16, 27 * TS);
  await g.sleep(1500); const s2 = await bot.note('retreated south');
  log.push({ label: 'bridge disciple after retreat', d: s2.enemies.find(e => e.bridge) });
  await g.shot(`${out}/03-recuo.png`);
  // 3. where would a player go? look at the map
  await g.tap('KeyM'); await g.sleep(300); await g.shot(`${out}/04-mapa.png`); await g.tap('KeyM'); await g.sleep(200);
  // 4. the corpse
  await bot.goTo(29 * TS + 16, 34 * TS);
  await g.shot(`${out}/05-trilha-leste.png`);
  await bot.goTo(39.6 * TS, 34.2 * TS);
  await g.shot(`${out}/06-corpo.png`);
  await bot.note('at the corpse'); await g.tap('KeyE'); await g.sleep(300);
  await bot.note('manual panel'); await g.shot(`${out}/07-manual.png`);
  await g.tap('KeyE'); await g.sleep(300);
  await bot.note('after manual');
  // 5. the tomb door
  await bot.goTo(51 * TS, 32.3 * TS); await g.shot(`${out}/08-porta.png`);
  await g.tap('KeyE'); await g.sleep(300); await bot.note('door'); await g.tap('KeyE'); await g.sleep(300);
  await bot.goTo(51 * TS, 23.6 * TS); await g.shot(`${out}/09-laje.png`);
  await g.tap('KeyE'); await g.sleep(300); await bot.note('slab'); await g.shot(`${out}/10-laje-texto.png`); await g.tap('KeyE'); await g.sleep(500);
  await g.shot(`${out}/11-pegadas.png`);
  writeFileSync(`${out}/log.json`, JSON.stringify(log, null, 1));
} finally {
  log.push({ label: 'page errors', errors: g.errors });
  writeFileSync(`${out}/log.json`, JSON.stringify(log, null, 1));
  await g.close();
}
