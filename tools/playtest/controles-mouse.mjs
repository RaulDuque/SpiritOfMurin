// Mouse controls, played from the title in a test profile: left click cuts and chains, right
// click steps (no browser menu), the side step with the lock after the tomb, K still casts qi,
// J and L still work, clicks in menus never reach the game, a remap on the Teclas screen
// survives closing the browser, and a player's earlier keys and save are kept.
// Usage: node tools/playtest/controles-mouse.mjs <outdir>
import { launch } from './cdp.mjs';
import { makeBot } from './bot.mjs';
import { manualTombAndLearn } from './percurso.mjs';
import { writeFileSync, mkdirSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const out = process.argv[2] || '.'; mkdirSync(out, { recursive: true });
const URL = 'file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html';
const TS = 32;
// game pixel -> window pixel, from where the canvas really is (its scale depends on the window)
const at = async (g, gx, gy) => g.eval(`(() => { const r = canvas.getBoundingClientRect(); return [r.left + ${gx} * r.width / W, r.top + ${gy} * r.height / H]; })()`);
const PROFILE = mkdtempSync(join(tmpdir(), 'som-mouse-'));
const log = [{ label: 'profile', path: PROFILE }], checks = [];
const check = (name, ok, detail = '') => { checks.push({ name, ok: !!ok, detail }); console.log(`${ok ? 'OK  ' : 'FAIL'} ${name}${detail ? ' — ' + detail : ''}`); };
let n = 0;
const shot = g => async name => g.shot(`${out}/${String(++n).padStart(2, '0')}-${name}.png`);
const actions = evs => evs.filter(e => e.type === 'action').map(e => e.kind);
const CENTER = [640, 360];

try {
  // ============ Session 1: play with the mouse from the normal start =========================
  {
    const g = await launch({ url: URL, profile: PROFILE, port: 9501 }); const bot = makeBot(g, log); const sh = shot(g);
    try {
      await g.eval(`window.__ctx = []; addEventListener('contextmenu', e => __ctx.push(e.defaultPrevented))`);
      await g.sleep(300);
      await g.click(...CENTER); await g.sleep(400);
      let ev = await bot.events();
      check('a click on the title starts the game and is consumed', (await g.eval('game.scene')) === 'play' && !actions(ev).includes('cut'), actions(ev).join(','));
      // at the trail start, away from any guard (a parry would restart the chain): left clicks chain the cut
      // a recorder in the page notes every cut number while the clicks come at a player's pace
      await g.eval(`window.__ns = new Set(); (function f() { const a = state.p.act; if (a && a.kind === 'cut') __ns.add(a.n); requestAnimationFrame(f); })()`);
      for (let i = 0; i < 3; i++) { await g.click(...CENTER, 'left', 30); await g.sleep(120); }
      await g.sleep(600);
      const ns = await g.eval('[...__ns]');
      ev = await bot.events();
      const chain = [...new Set(ns)];
      check('left click cuts, and clicks in sequence chain up to three', chain.includes(1) && chain.includes(2) && chain.includes(3), `cut numbers seen: ${chain.join(',')}; actions ${actions(ev).join(',')}`);
      await sh('clique-esquerdo-corte');
      // the bridge: a left click on a raised guard is parried like J
      await bot.goTo(29 * TS + 16, 21 * TS);
      await bot.goTo(29 * TS + 16, 18.9 * TS, { stop: s => !!s.speech });
      await g.tap('Space'); await g.sleep(120); await bot.events();
      ev = [];
      for (let i = 0; i < 4 && !ev.some(e => e.type === 'parry'); i++) { await g.hold(['KeyW'], 200); await g.click(...CENTER); await g.sleep(400); ev.push(...await bot.events()); }
      check('on the bridge, a left click meets the guard like J', ev.some(e => e.type === 'parry'), ev.map(e => e.type).join(','));
      // right click = step (back, with S held), no browser menu
      await g.sleep(500);
      await g.down('KeyS'); await g.click(...CENTER, 'right'); await g.up('KeyS'); await g.sleep(250);
      ev = await bot.events();
      const ctx = await g.eval('__ctx');
      check('right click steps (same rule as L)', actions(ev).includes('step'), actions(ev).join(','));
      check('right click does not open the browser menu', ctx.length > 0 && ctx.every(Boolean), JSON.stringify(ctx));
      // K still casts
      await g.sleep(300); await g.tap('KeyK'); await g.sleep(250);
      ev = await bot.events();
      check('K still casts the qi wave', actions(ev).includes('qi'), actions(ev).join(','));
      await g.tap('Space');
      await bot.goTo(29 * TS + 16, 27 * TS); await g.sleep(1500);
      // learn the side step on the normal route (keyboard)
      const s = await manualTombAndLearn(g, bot);
      check('the side step is learned on the normal route', s.learnedSide);
      await bot.events();
      // a text panel closed with a click: no cut
      await bot.goTo(51 * TS, 23.6 * TS); await g.tap('KeyE'); await g.sleep(300);
      const hadPanel = !!(await bot.st()).panel;
      await g.click(...CENTER); await g.sleep(400);
      ev = await bot.events();
      check('a click closes a text panel without attacking', hadPanel && !(await bot.st()).panel && !actions(ev).includes('cut'), actions(ev).join(','));
      // at the statue: lock, side keys + right click = side step; left click = the flank
      await bot.goTo(51 * TS, 27.4 * TS, { direct: true });
      await g.tap('Space'); await g.sleep(150);
      let side = 0, flank = 0;
      for (let i = 0; i < 3; i++) {
        const hint = await g.eval('lockTarget() ? sideKeys(state.p, lockTarget()) : ""');
        const keys = (hint.split(' / ')[i % 2] || 'D').split('+').map(k => 'Key' + k);
        for (const k of keys) await g.down(k); await g.click(...CENTER, 'right', 40); for (const k of keys) await g.up(k);
        await g.sleep(140); await g.click(...CENTER); await g.sleep(500);
        for (const e of await bot.events()) { if (e.type === 'action' && e.kind === 'side') side++; if (e.type === 'flank') flank++; }
        if (i === 0) await sh('passo-lateral-mouse');
      }
      check('locked: side key + right click is the side step', side >= 2, `${side} side steps`);
      check('side step + left click lands on the flank', flank >= 1, `${flank} flank`);
      const hintText = await g.eval(`K2('step')`);
      check('the on-screen hint names both inputs of the step', /L\/Mouse dir\./.test(hintText), hintText);
      await g.tap('Space'); await g.sleep(200);
      // menus consume clicks: the book
      await bot.events();
      await g.tap('Tab'); await g.sleep(250); await g.click(...CENTER); await g.click(...CENTER, 'right'); await g.sleep(200); await g.tap('Tab'); await g.sleep(400);
      ev = await bot.events();
      check('clicks inside the book never reach the game', !actions(ev).length, actions(ev).join(','));
      // Teclas from the pause: click the qi row's mouse cap, press the middle button
      await g.tap('KeyP'); await g.sleep(200); await g.tap('KeyO'); await g.sleep(250);
      const row = await g.eval(`ACTIONS.findIndex(a => a[0] === 'qi')`);
      const cap = await at(g, 540, 42 + row * 18);
      await g.click(...cap); await g.sleep(200);
      const waiting = await g.eval('teclas.wait && teclas.col === 1');
      await g.click(...cap, 'middle'); await g.sleep(250);
      await sh('teclas-mouse-meio');
      check('Teclas: click a row, then a mouse button assigns it', waiting && (await g.eval('binds2.qi')) === 'Mouse1', await g.eval('binds2.qi'));
      await g.click(...CENTER, 'right'); await g.sleep(200); // back to the pause
      const sc = await g.eval('game.scene');
      await g.tap('Escape'); await g.sleep(400);
      ev = await bot.events();
      check('clicks in Teclas and the pause never reach the game', sc === 'pause' && !actions(ev).length, `${sc}; ${actions(ev).join(',')}`);
      await g.click(...CENTER, 'middle'); await g.sleep(300);
      ev = await bot.events();
      check('the new mouse binding works in play (middle = qi)', actions(ev).includes('qi'), actions(ev).join(','));
    } finally { check('no page errors (session 1)', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }
  // ============ Session 2: reopen the same profile: the remap is still there ================
  {
    const g = await launch({ url: URL, profile: PROFILE, port: 9502 }); const bot = makeBot(g, log); const sh = shot(g);
    try {
      await g.sleep(300);
      check('after closing the browser, the remap is kept', (await g.eval('binds2.qi')) === 'Mouse1' && (await g.eval('binds2.cut')) === 'Mouse0');
      await sh('titulo-controles');
      await g.tap('Enter'); await g.sleep(400); await bot.events();
      await g.click(...CENTER, 'middle'); await g.sleep(400);
      await g.tap('KeyJ'); await g.sleep(500);
      await g.tap('KeyL'); await g.sleep(400);
      await g.click(...CENTER); await g.sleep(500);
      await g.click(...CENTER, 'right'); await g.sleep(400);
      const ev = await bot.events();
      const a = actions(ev);
      check('after reopening: middle = qi, J and left = cut, L and right = step', a.filter(x => x === 'qi').length === 1 && a.filter(x => x === 'cut').length === 2 && a.filter(x => x === 'step').length === 2, a.join(','));
    } finally { check('no page errors (session 2)', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }
  // ============ Session 3: a player's earlier keys and save are kept =========================
  {
    const g = await launch({ url: URL, port: 9503 }); const bot = makeBot(g, log);
    try {
      // fixture: what a player had before this version: custom keys (qi on U) and a journey
      await g.eval(`localStorage.setItem('spiritofmurin.teclas', JSON.stringify(Object.assign({}, binds, { qi: 'KeyU' })));
        reset(); localStorage.setItem('spiritofmurin.jornada', JSON.stringify(snapshotJourney()));`);
      await g.send('Page.reload'); await g.sleep(1200);
      check('earlier custom keys are kept, mouse defaults added', (await g.eval('binds.qi')) === 'KeyU' && (await g.eval('binds2.cut')) === 'Mouse0' && (await g.eval('binds2.step')) === 'Mouse2');
      check('the earlier journey still offers Continuar', (await g.eval('titleMenu.save.kind')) === 'ok');
      await g.click(...(await at(g, 320, 86))); await g.sleep(500); // click "Continuar"
      const ev = await bot.events();
      check('a click on Continuar resumes the journey without attacking', (await g.eval('game.scene')) === 'play' && ev.some(e => e.type === 'load') && !actions(ev).length, actions(ev).join(','));
      await g.tap('KeyU'); await g.sleep(300);
      check('the custom key still works (U = qi)', actions(await bot.events()).includes('qi'));
    } finally { check('no page errors (session 3)', g.errors.length === 0, g.errors.join(' | ')); await g.close(); }
  }
} catch (err) {
  log.push({ label: 'harness error', err: String(err.stack || err) }); console.log('harness error', err);
} finally {
  writeFileSync(`${out}/log.json`, JSON.stringify(log, null, 1));
  writeFileSync(`${out}/checks.json`, JSON.stringify(checks, null, 1));
  console.log(`${checks.filter(c => c.ok).length}/${checks.length} OK`);
}
