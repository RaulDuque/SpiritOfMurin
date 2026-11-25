// Quick persistence check in one browser: new journey, walk to the meditation stone, meditate,
// close the browser, reopen the same address with the same profile, Continue.
// Usage: PLAYTEST_BROWSER=edge node tools/playtest/persistencia-rapida.mjs
import { launch } from './cdp.mjs';
import { makeBot } from './bot.mjs';
import { mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
const URL = 'file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html', TS = 32;
const profile = mkdtempSync(join(tmpdir(), 'som-persist-'));
let g = await launch({ url: URL, profile, port: 9420 }); let bot = makeBot(g, []);
await g.tap('Enter'); await g.sleep(400);
await bot.goTo(29 * TS + 16, 34 * TS); await bot.goTo(43 * TS, 33 * TS); await bot.goTo(49.1 * TS, 33.5 * TS);
await g.tap('KeyE'); await g.sleep(300);
const ua = await g.eval('navigator.userAgent');
const ok1 = (await g.eval('game.events.filter(e => e.type === "save").map(e => e.ok)')).join(',');
await g.close();
g = await launch({ url: URL, profile, port: 9421 });
await g.sleep(300);
const t = await g.eval('titleMenu.save.kind');
await g.tap('Enter'); await g.sleep(400);
const p = await g.eval('[Math.round(state.p.x / 32 * 10) / 10, Math.round(state.p.y / 32 * 10) / 10, game.scene]');
console.log(JSON.stringify({ browser: ua.match(/(Edg|Chrome)\/[\d.]+/g), saveOk: ok1, titleAfterReopen: t, continuedAt: p }));
await g.close();
