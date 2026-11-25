// Walking behind tall scenery: bamboo, a pine and a boulder block only at their foot; the body
// above can be walked behind, the player's silhouette shows through, and the foot still blocks.
// Usage: node tools/playtest/passar-atras.mjs <outdir>
import { launch } from './cdp.mjs';
import { makeBot } from './bot.mjs';
import { writeFileSync, mkdirSync } from 'node:fs';

const out = process.argv[2] || '.'; mkdirSync(out, { recursive: true });
const URL = 'file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html';
const TS = 32, log = [], checks = [];
const check = (name, ok, detail = '') => { checks.push({ name, ok: !!ok, detail }); console.log(`${ok ? 'OK  ' : 'FAIL'} ${name}${detail ? ' — ' + detail : ''}`); };
const g = await launch({ url: URL, port: 9601 }); const bot = makeBot(g, log);
let n = 0; const shot = async name => g.shot(`${out}/${String(++n).padStart(2, '0')}-${name}.png`);
const tile = async (x, y) => g.eval(`tileXY(${x}, ${y})`);
const pos = async () => { const s = await bot.st(); return [s.x / TS, s.y / TS]; };
try {
  await g.tap('Enter'); await g.sleep(400);
  // ---- the bamboo grove in the middle of the trail (tiles 34-36 x 22-27)
  check('the grove is bamboo', (await tile(35, 22)) === 8 && (await tile(35, 25)) === 8);
  await bot.goTo(29 * TS + 16, 21.4 * TS);
  await bot.goTo(35.5 * TS, 21.4 * TS, { direct: true });
  await bot.goTo(35.5 * TS, 22.3 * TS, { direct: true, timeout: 4000 });
  let [x, y] = await pos();
  check('walks into the upper part of a bamboo row (behind the stalks)', Math.floor(y) === 22 && Math.floor(x) === 35, `${x.toFixed(2)},${y.toFixed(2)}`);
  check('the silhouette shows behind the bamboo', await g.eval('behindScenery(state.p)'));
  await shot('atras-do-bambu');
  // along the row, inside the grove
  await bot.goTo(34.4 * TS, 22.3 * TS, { direct: true, timeout: 4000 });
  [x, y] = await pos();
  check('walks along the row inside the grove', x < 34.8 && Math.floor(y) === 22, `${x.toFixed(2)},${y.toFixed(2)}`);
  // the foot still blocks: hold S
  await g.hold(['KeyS'], 700); [x, y] = await pos();
  check('the foot of the bamboo still blocks', y < 22.6, `stopped at y ${y.toFixed(2)}`);
  await shot('base-do-bambu-bloqueia');
  // ---- a pine on the trail side (25, 22)
  check('there is a pine at 25,22', (await tile(25, 22)) === 15);
  await bot.goTo(25.5 * TS, 21.3 * TS);
  await bot.goTo(25.5 * TS, 22.25 * TS, { direct: true, timeout: 4000 });
  [x, y] = await pos();
  check('walks behind the pine', Math.floor(y) === 22 && Math.floor(x) === 25 && await g.eval('behindScenery(state.p)'), `${x.toFixed(2)},${y.toFixed(2)}`);
  await shot('atras-do-pinheiro');
  await g.hold(['KeyS'], 700); [x, y] = await pos();
  check('the trunk still blocks', y < 22.75, `stopped at y ${y.toFixed(2)}`);
  // ---- a lone boulder (33, 29)
  check('there is a lone boulder at 33,29', (await tile(33, 29)) === 1 && await g.eval('isBoulder(33, 29)'));
  await bot.goTo(33.5 * TS, 28.3 * TS);
  await bot.goTo(33.5 * TS, 29.32 * TS, { direct: true, tol: 3, timeout: 4000 });
  [x, y] = await pos();
  check('walks behind the boulder', Math.floor(y) === 29 && await g.eval('behindScenery(state.p)'), `${x.toFixed(2)},${y.toFixed(2)}`);
  await shot('atras-da-pedra');
  await g.hold(['KeyS'], 700); [x, y] = await pos();
  check('the foot of the boulder still blocks', y < 29.75, `stopped at y ${y.toFixed(2)}`);
  // ---- walls stay walls: the river bank cliff row 13-14 and the water
  check('cliffs and borders are still full walls', await g.eval('solidAt(10 * TS + 16, 13 * TS + 2, false) && solidAt(1 * TS + 16, 20 * TS + 2, false)'));
} catch (err) { log.push({ label: 'harness error', err: String(err.stack || err) }); console.log('harness error', err); }
finally {
  check('no page errors', g.errors.length === 0, g.errors.join(' | '));
  writeFileSync(`${out}/checks.json`, JSON.stringify(checks, null, 1)); writeFileSync(`${out}/log.json`, JSON.stringify(log, null, 1));
  console.log(`${checks.filter(c => c.ok).length}/${checks.length} OK`);
  await g.close();
}
