// "Meditar e continuar a jornada": play from the title in a test browser profile, learn the
// side step on the normal route, meditate, really close the browser, reopen the same address
// with the same profile, Continue, read the technique in the book and use it in the rematch.
// Also: cancelling "Novo jogo" keeps the journey; loading twice does not duplicate items;
// damaged data and blocked storage leave the game playable.
// Usage: node tools/playtest/meditar-continuar.mjs <outdir>
import { launch } from './cdp.mjs';
import { makeBot } from './bot.mjs';
import { firstEncounterAndRetreat, manualTombAndLearn, rematch, bridge } from './percurso.mjs';
import { writeFileSync, mkdirSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const out = process.argv[2] || '.'; mkdirSync(out, { recursive: true });
const URL = 'file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html';
const TS = 32;
const PROFILE = mkdtempSync(join(tmpdir(), 'som-jornada-')); // the test profile: never the player's own
const log = [{ label: 'profile', path: PROFILE, url: URL }], checks = [];
const check = (name, ok, detail = '') => { checks.push({ name, ok: !!ok, detail }); console.log(`${ok ? 'OK  ' : 'FAIL'} ${name}${detail ? ' — ' + detail : ''}`); };
let n = 0;
const shot = g => async name => g.shot(`${out}/${String(++n).padStart(2, '0')}-${name}.png`);
const journeyView = g => g.eval(`(() => ({ inv: state.inv, invOrder: state.invOrder, reg: state.reg.map(r => r.name), quests: state.quests.map(q => q.id + (q.done ? ':ok' : '')),
  hp: state.p.hp, qi: Math.round(state.p.qi), learnedSide: state.learnedSide, hasManual: state.hasManual, doorOpen: state.doorOpen,
  chestOpen: state.chestOpen, potsBroken: state.pots.filter(p => !p.alive).length, herbsTaken: state.herbs.filter(h => h.taken).length,
  enemiesAlive: state.enemies.map(e => e.alive) }))()`);
const raw = g => g.eval(`(() => { try { return localStorage.getItem('spiritofmurin.jornada'); } catch (e) { return 'BLOCKED'; } })()`);
const title = g => g.eval('({ scene: game.scene, save: titleMenu.save.kind, sel: titleMenu.sel, confirm: titleMenu.confirm })');
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);

let saved = null, savedRaw = null;
try {
  // ======================= Session 1: first journey, learn, meditate, close ==================
  {
    const g = await launch({ url: URL, profile: PROFILE, port: 9401 }); const bot = makeBot(g, log); const sh = shot(g);
    try {
      await g.sleep(300);
      const t = await title(g);
      check('first time: the title has no saved journey and starts the current way', t.scene === 'title' && t.save === 'none', JSON.stringify(t));
      await sh('titulo-primeira-vez');
      await g.tap('Enter'); await g.sleep(400);
      check('Enter starts the first journey', (await bot.st()).scene === 'play');
      await firstEncounterAndRetreat(g, bot);
      const s = await manualTombAndLearn(g, bot, sh);
      check('the side step is learned on the normal route', s.learnedSide);
      // on the way: break the pot by the west stone and pick up what falls (one-time rewards)
      await bot.goTo(51 * TS, 32.2 * TS);
      await bot.goTo(46 * TS, 33.4 * TS);
      await g.hold(['KeyW'], 60); await g.tap('KeyJ'); await g.sleep(900);
      for (const [x, y] of [[46, 32.6], [45.5, 32.9], [46.5, 32.9], [46, 33.2]]) await bot.goTo(x * TS, y * TS, { direct: true, tol: 4, timeout: 3000 });
      await g.sleep(300);
      const got = await g.eval('({ inv: state.inv, pot: state.pots[4].alive, ground: state.pickups.length })');
      log.push({ label: 'pot and pickups before meditating', got });
      check('a one-time reward was taken before meditating', !got.pot && Object.keys(got.inv).length > 0, JSON.stringify(got));
      // to the meditation stone
      await bot.goTo(49.1 * TS, 33.5 * TS);
      check('the meditation spot is reached and offers E', (await g.eval('nearSpot()')) === 'meditate');
      await g.sleep(200); await sh('meditar-prompt');
      check('nothing was saved before meditating', (await raw(g)) === null);
      await g.tap('KeyE'); await g.sleep(250);
      const ev = await bot.events(), st = await bot.st();
      const ok = ev.some(e => e.type === 'save' && e.ok);
      savedRaw = await raw(g);
      check('meditating writes the journey and confirms only after it', ok && savedRaw && st.toasts.some(x => /Meditação gravada/.test(x)), st.toasts.join(' | '));
      check('the confirmation says what death and closing do', st.toasts.some(x => /fechar o jogo/.test(x)));
      await sh('meditacao-gravada');
      saved = await journeyView(g);
      log.push({ label: 'journey at meditation', saved, bytes: savedRaw && savedRaw.length });
    } finally { check('no page errors', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
    log.push({ label: 'session 1 closed (Browser.close)' });
  }

  // ======================= Session 2: reopen, cancel new game, continue, book, rematch ========
  {
    const g = await launch({ url: URL, profile: PROFILE, port: 9402 }); const bot = makeBot(g, log); const sh = shot(g);
    try {
      await g.sleep(300);
      let t = await title(g);
      check('after closing and reopening, the title offers Continuar, highlighted', t.save === 'ok' && t.sel === 0, JSON.stringify(t));
      await sh('titulo-continuar');
      // Novo jogo -> confirmation -> cancel (default option), then again with Esc
      await g.tap('KeyS'); await g.sleep(150); await g.tap('Enter'); await g.sleep(250);
      t = await title(g); check('Novo jogo asks for confirmation first', t.confirm === true);
      await sh('novo-jogo-confirmacao');
      await g.tap('Enter'); await g.sleep(250);
      t = await title(g);
      check('cancelling Novo jogo keeps the journey', !t.confirm && t.scene === 'title' && (await raw(g)) === savedRaw);
      await g.tap('Enter'); await g.sleep(250); await g.tap('Escape'); await g.sleep(250);
      t = await title(g);
      check('Esc also cancels and keeps the journey', !t.confirm && t.scene === 'title' && (await raw(g)) === savedRaw);
      await g.tap('KeyW'); await g.sleep(150); await g.tap('Enter'); await g.sleep(500);
      let s = await bot.st();
      const m = await g.eval('[SPOTS.meditate.x, SPOTS.meditate.y]');
      check('Continuar puts the player at the meditation stone', s.scene === 'play' && Math.hypot(s.x - m[0], s.y - m[1]) < 12, `${(s.x / TS).toFixed(1)},${(s.y / TS).toFixed(1)}`);
      const loaded = await journeyView(g);
      check('the journey comes back as meditated', same(loaded, saved), same(loaded, saved) ? 'inventory, book, quests, hp, qi, doors, rewards' : JSON.stringify({ loaded, saved }));
      await sh('continuar-na-pedra');
      const x0 = s.x; await g.hold(['KeyD'], 300); s = await bot.st();
      check('the player is ready for commands', s.x > x0 + 10, `moved ${(s.x - x0).toFixed(0)} px`);
      // the book: go to the side step page
      await g.tap('Tab'); await g.sleep(300);
      const idx = await g.eval(`bookEntries().findIndex(r => r.name === 'Passo Lateral')`);
      for (let i = 0; i < idx; i++) { await g.tap('ArrowDown', 40); await g.sleep(60); }
      const page = await g.eval(`bookEntries()[state.bookSel].name`);
      check('the book shows the side step page', page === 'Passo Lateral', page);
      await sh('livro-passo-lateral');
      await g.tap('Tab'); await g.sleep(300);
      // the rematch
      const r = await rematch(g, bot);
      s = await bot.st();
      check('the side step lands on the flank in the rematch', r.flank > 0, `${r.hits} hits, ${r.flank} on the flank, ${r.parries} parried`);
      check('the rematch is won after continuing', r.won, `try ${r.tries}, hp ${s.hp}`);
      await sh('revanche-apos-continuar');
    } finally { check('no page errors', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }

  // ======================= Session 3: load again: nothing duplicated ========================
  {
    const g = await launch({ url: URL, profile: PROFILE, port: 9403 }); const bot = makeBot(g, log);
    try {
      await g.sleep(300);
      check('the save is unchanged by playing without meditating', (await raw(g)) === savedRaw);
      await g.tap('Enter'); await g.sleep(500);
      const again = await journeyView(g);
      check('loading a second time does not duplicate items or reopen rewards', same(again, saved), JSON.stringify(again.inv));
      const s = await bot.st();
      check('an unsaved victory is not kept: the journey resumes at the last meditation', bridge(s).alive);
      // the one-time reward stays taken: the corpse has nothing left
      await bot.goTo(40.4 * TS, 34.6 * TS); await g.tap('KeyE'); await g.sleep(300);
      const p = (await bot.st()).panel;
      check('the half manual stays collected', p && /não carrega mais nada/.test(p.lines.join(' ')), p ? p.lines.join(' ') : 'no panel');
      await g.tap('KeyE'); await g.sleep(200);
    } finally { check('no page errors', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }

  // ======================= Session 3b: confirmed new game keeps the custom keys ==============
  {
    const g = await launch({ url: URL, profile: PROFILE, port: 9406 }); const bot = makeBot(g, log); const sh = shot(g);
    try {
      // test fixture: keys the player changed on the Teclas screen (cut on U instead of J)
      await g.eval(`localStorage.setItem('spiritofmurin.teclas', JSON.stringify({ cut: 'KeyU' }))`);
      await g.send('Page.reload'); await g.sleep(1200);
      await g.tap('KeyS'); await g.sleep(150); await g.tap('Enter'); await g.sleep(250);
      await g.tap('KeyS'); await g.sleep(150); await sh('novo-jogo-sim'); await g.tap('Enter'); await g.sleep(500);
      const s = await bot.st();
      const keys = await g.eval(`localStorage.getItem('spiritofmurin.teclas')`);
      check('confirmed Novo jogo erases the journey and starts at the trail', s.scene === 'play' && (await raw(g)) === null && Math.abs(s.x / TS - 29) < 1 && !s.learnedSide);
      check('confirmed Novo jogo keeps the custom keys', keys === JSON.stringify({ cut: 'KeyU' }) && (await g.eval(`binds.cut`)) === 'KeyU', keys);
    } finally { check('no page errors', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }

  // ======================= Session 4: damaged data ===========================================
  {
    const g = await launch({ url: URL, port: 9404 }); const sh = shot(g);
    try {
      // test fixture: a damaged save written before the page loads
      await g.eval(`localStorage.setItem('spiritofmurin.jornada', '{"format":1,"game":"spiritofmurin","p":{"hp":"muito"}}')`);
      await g.send('Page.reload'); await g.sleep(1200);
      const t = await title(g);
      check('damaged data: the title explains and the game stays playable', t.save === 'invalid', JSON.stringify(t));
      await sh('dados-danificados');
      await g.tap('Enter'); await g.sleep(400);
      check('damaged data: Enter starts a new journey', (await g.eval('game.scene')) === 'play');
    } finally { check('no page errors', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }

  // ======================= Session 5: storage blocked ========================================
  {
    const g = await launch({ url: 'about:blank', port: 9405 }); const bot = makeBot(g, log); const sh = shot(g);
    try {
      // test fixture: a browser that refuses to store (as with strict privacy settings)
      await g.send('Page.addScriptToEvaluateOnNewDocument', { source: `Storage.prototype.setItem = function () { throw new DOMException('blocked', 'SecurityError'); };` });
      await g.send('Page.navigate', { url: URL }); await g.sleep(1200);
      await g.tap('Enter'); await g.sleep(400);
      await bot.goTo(29 * TS + 16, 34 * TS);
      await bot.goTo(43 * TS, 33 * TS);
      await bot.goTo(49.1 * TS, 33.5 * TS);
      await g.tap('KeyE'); await g.sleep(250);
      const s = await bot.st(), ev = await bot.events();
      check('blocked storage: a useful message and no false confirmation', ev.some(e => e.type === 'save' && e.ok === false) && !s.toasts.some(x => /Meditação gravada/.test(x)), s.toasts.join(' | '));
      await sh('armazenamento-bloqueado');
      const x0 = s.x; await g.hold(['KeyA'], 300);
      check('blocked storage: the game keeps playing', (await bot.st()).x < x0 - 10);
    } finally { check('no page errors', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }
} catch (err) {
  log.push({ label: 'harness error', err: String(err.stack || err) }); console.log('harness error', err);
} finally {
  writeFileSync(`${out}/log.json`, JSON.stringify(log, null, 1));
  writeFileSync(`${out}/checks.json`, JSON.stringify(checks, null, 1));
  console.log(`${checks.filter(c => c.ok).length}/${checks.length} OK`);
}
