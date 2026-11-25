// Full-loop playtest of "Mil Caminhos do Dao" from the normal start (title -> Enter), fresh
// progress, separate browser profile, keys only. Records what the player sees at each step,
// screenshots, and a list of checks. Usage:
//   node tools/playtest/ciclo-completo.mjs <outdir> [--lose]
// --lose: also lose the rematch on purpose once and come back (retry after defeat).
import { launch } from './cdp.mjs';
import { makeBot } from './bot.mjs';
import { writeFileSync, mkdirSync } from 'node:fs';

const out = process.argv[2] || '.'; mkdirSync(out, { recursive: true });
const LOSE = process.argv.includes('--lose');
const URL = 'file:///D:/Progr/SpiritOfMurin/prototypes/spiritofmurin/index.html';
const TS = 32;
const log = [], checks = [];
const check = (name, ok, detail = '') => { checks.push({ name, ok: !!ok, detail }); console.log(`${ok ? 'OK  ' : 'FAIL'} ${name}${detail ? ' — ' + detail : ''}`); };
const g = await launch({ url: URL });
const bot = makeBot(g, log);
let n = 0; const shot = async name => g.shot(`${out}/${String(++n).padStart(2, '0')}-${name}.png`);
const bridge = s => s.enemies.find(e => e.bridge);
const allText = s => [...s.toasts, s.speech || '', ...(s.panel ? [s.panel.title, ...s.panel.lines] : [])].join(' | ');

// lock on, then side step + cut until the target falls or time runs out
async function duel(timeLimit, { plan = 'side' } = {}) {
  const t0 = Date.now(); let flankShot = plan !== 'side', turn = 0, side = 'KeyD', hits = 0, parries = 0, flank = 0, words = new Set();
  let s = await bot.st();
  if (!s.lock) { await g.tap('Space'); await g.sleep(80); }
  while (Date.now() - t0 < timeLimit) {
    s = await bot.st();
    if (s.dead || !bridge(s).alive) break;
    if (!s.lock) { await g.tap('Space'); await g.sleep(80); continue; }
    const e = bridge(s), d = Math.hypot(e.x - s.x, e.y - s.y);
    const toward = [], away = [];
    const dx = e.x - s.x, dy = e.y - s.y;
    if (Math.abs(dx) > 8) { toward.push(dx > 0 ? 'KeyD' : 'KeyA'); away.push(dx > 0 ? 'KeyA' : 'KeyD'); }
    if (Math.abs(dy) > 8) { toward.push(dy > 0 ? 'KeyS' : 'KeyW'); away.push(dy > 0 ? 'KeyW' : 'KeyS'); }
    if (d > 62) { await g.hold(toward, 100); continue; }               // close the distance
    // no backing off: the side step itself slips the red-ochre strike
    if (plan === 'side') {
      // read the on-screen hint ("L + W / S: contornar") the way a player would, and use its keys
      const hint = await g.eval('state.learnedSide && lockTarget() ? sideKeys(state.p, lockTarget()) : ""');
      const keys = (hint.split(' / ')[turn % 2] || 'D').split('+').map(k => 'Key' + k);
      for (const k of keys) await g.down(k); await g.tap('KeyL', 40); for (const k of keys) await g.up(k);
      await g.sleep(140); await g.tap('KeyJ', 40); await g.sleep(120); await g.tap('KeyJ', 40);
      side = side === 'KeyD' ? 'KeyA' : 'KeyD';
    } else { await g.tap('KeyJ', 40); }
    await g.sleep(260);
    for (const ev of await bot.events()) { if (ev.type === 'hit') hits++; if (ev.type === 'parry') parries++; if (ev.type === 'flank') flank++; }
    turn++;
    const fx = await g.eval(`game.state.fx.filter(f => f.type === 'word').map(f => f.s)`); fx.forEach(w => words.add(w));
    if (flank > 0 && !flankShot) { flankShot = true; await shot('revanche-flanco'); }
  }
  await bot.release();
  return { hits, parries, flank, words: [...words], state: await bot.st() };
}

try {
  // ---------------- 0. title, normal start
  await shot('titulo');
  await g.tap('Enter'); await g.sleep(400);
  let s = await bot.note('start');
  check('normal start puts the player at the trail start', s.scene === 'play' && Math.abs(s.x / TS - 29) < 1, `${(s.x / TS).toFixed(1)},${(s.y / TS).toFixed(1)}`);

  // ---------------- 1. first encounter at the bridge
  await bot.goTo(29 * TS + 16, 21 * TS);
  await bot.goTo(29 * TS + 16, 18.6 * TS, { stop: s => !!s.speech });
  await g.sleep(350); s = await bot.note('bridge: first sight');
  check('the bridge disciple speaks on first sight', !!s.speech, s.speech || '');
  await shot('ponte-primeira-vista');
  await g.tap('Space'); await g.sleep(100);
  let parries = 0, saw = new Set();
  for (let i = 0; i < 4; i++) {
    await g.hold(['KeyW'], 220); await g.tap('KeyJ'); await g.sleep(120);
    if (i === 1) await shot('ponte-corte-aparado');
    await g.sleep(200);
    s = await bot.st(); saw.add(allText(s));
    for (const ev of await bot.events()) if (ev.type === 'parry') parries++;
    const fx = await g.eval(`game.state.fx.filter(f => f.type === 'word').map(f => f.s)`); fx.forEach(w => saw.add('word:' + w));
  }
  log.push({ label: 'what the player read during the frontal attempt', saw: [...saw] });
  check('frontal cuts are parried', parries >= 3, `${parries} parries`);
  // ---------------- 2. retreat and keep exploring
  await g.tap('Space');
  await bot.goTo(29 * TS + 16, 27 * TS);
  await g.sleep(1800); s = await bot.note('retreated');
  check('the player can retreat alive without the side step', !s.dead && s.deaths === 0, `hp ${s.hp}`);
  check('the disciple goes back to the bridge', ['return', 'idle'].includes(bridge(s).st), bridge(s).st);
  log.push({ label: 'lead after retreat', text: allText(s), quests: s.quests });
  check('retreating gives a lead to keep exploring', /vale/.test(allText(s)), allText(s));
  await shot('recuo');
  await g.tap('KeyM'); await g.sleep(300); await shot('mapa-apos-recuo'); await g.tap('KeyM'); await g.sleep(200);

  // ---------------- 3. the clue: half a manual
  await bot.goTo(29 * TS + 16, 34 * TS); await shot('encruzilhada');
  await bot.goTo(40.4 * TS, 34.6 * TS); await shot('corpo');
  s = await bot.st(); check('the corpse offers E', await g.eval('nearSpot()') === 'corpse');
  await g.tap('KeyE'); await g.sleep(300); s = await bot.note('manual');
  check('the manual gives a short lead to the tomb', s.panel && /túmulo/i.test(s.panel.lines.join(' ')), s.panel ? s.panel.lines.join(' / ') : 'no panel');
  await shot('manual');
  await g.tap('KeyE'); await g.sleep(300);
  await g.tap('KeyM'); await g.sleep(300); await shot('mapa-apos-manual'); await g.tap('KeyM'); await g.sleep(200);

  // ---------------- 4. follow the lead to the tomb
  await bot.goTo(51 * TS, 31.7 * TS); await shot('porta');
  check('the tomb door offers E', await g.eval('nearSpot()') === 'door');
  await g.tap('KeyE'); await g.sleep(300); s = await bot.note('door'); await g.tap('KeyE'); await g.sleep(300);
  check('the half manual opens the door', (await bot.st()).doorOpen);
  await bot.goTo(51 * TS, 23.6 * TS); await shot('laje');
  await g.tap('KeyE'); await g.sleep(300); s = await bot.note('slab'); await shot('laje-texto');
  await g.tap('KeyE'); await g.sleep(500); await shot('pegadas');

  // ---------------- 5. learn: a wrong try first, then the right order
  const marks = await g.eval('MARKS.map(m => [m.x, m.y])');
  await bot.goTo(marks[2][0] + 16, marks[2][1] - 24, { direct: true });
  await bot.goTo(marks[2][0], marks[2][1], { direct: true, tol: 3 });
  await g.sleep(300); s = await bot.note('wrong first footprint');
  check('a wrong footprint resets and tells the player', s.fpIdx === 0 && !s.learnedSide, allText(s));
  await shot('pegada-errada');
  await bot.goTo(53 * TS, 26 * TS, { direct: true });
  await bot.goTo(marks[0][0] + 20, marks[0][1] + 10, { direct: true });
  for (const i of [0, 1, 2]) { await bot.goTo(marks[i][0], marks[i][1], { direct: true, tol: 3 }); await g.sleep(250); }
  s = await bot.note('after the three footprints');
  check('the side step is learned at the tomb', s.learnedSide);
  check('the tomb leaves the player whole', s.hp === 5, `hp ${s.hp}`);
  await g.sleep(300); await shot('revelacao');
  await g.sleep(3400); s = await bot.note('after reveal'); await shot('apos-revelacao');
  log.push({ label: 'what the player is told after learning', text: allText(s) });
  // the book of techniques
  await g.tap('Tab'); await g.sleep(400); await shot('livro');
  const reg = await g.eval('game.state.reg.map(r => r.name + " [" + r.key + "]")');
  check('the book lists the side step with its keys', reg.some(r => /Passo Lateral/.test(r)), reg.join('; '));
  await g.tap('Tab'); await g.sleep(300);
  // practise on the statue
  await bot.goTo(51 * TS, 27.4 * TS, { direct: true });
  await g.tap('Space'); await g.sleep(100);
  let practice = { hits: 0, parries: 0 };
  for (let i = 0; i < 3; i++) {
    await g.down('KeyD'); await g.tap('KeyL', 40); await g.up('KeyD'); await g.sleep(150); await g.tap('KeyJ', 40); await g.sleep(500);
    for (const ev of await bot.events()) { if (ev.type === 'parry') practice.parries++; if (ev.type === 'flank') practice.hits++; }
  }
  s = await bot.note('statue practice'); await shot('pratica-estatua');
  log.push({ label: 'statue practice', practice, text: allText(s) });
  check('the statue answers a flank cut while practising', practice.hits > 0, `${practice.hits} flank, ${practice.parries} parried`);
  await g.tap('Space');

  // ---------------- 6. back to the bridge
  await bot.goTo(51 * TS, 32 * TS);
  await bot.goTo(29 * TS + 16, 22 * TS);
  if (LOSE) {
    // lose on purpose: attack him only from the front until the life runs out
    await bot.goTo(29 * TS + 16, 19.6 * TS, { stop: s => bridge(s).st !== 'idle' });
    await duel(40000, { plan: 'front' });
    const t0 = Date.now(); while (Date.now() - t0 < 6000) { s = await bot.st(); if (s.deaths > 0 && !s.dead) break; await g.sleep(200); }
    await g.sleep(800); s = await bot.note('after losing');
    check('losing sends the player back whole, knowledge kept', s.deaths === 1 && s.learnedSide && !s.dead && s.hp === 5,
      `deaths ${s.deaths}, hp ${s.hp}, at ${(s.x / TS).toFixed(1)},${(s.y / TS).toFixed(1)}, ${allText(s)}`);
    check('the disciple is whole again for the retry', bridge(s).alive && bridge(s).hp === 6, `hp ${bridge(s).hp}`);
    await shot('derrota-volta');
  }
  // the rematch; a lost try sends the player back to the start of the area, and he walks back
  let r, tries = 0;
  for (tries = 1; tries <= 3; tries++) {
    await bot.goTo(29 * TS + 16, 22 * TS);
    await bot.goTo(29 * TS + 16, 19.6 * TS, { stop: s => bridge(s).st !== 'idle' });
    if (tries === 1) await shot('revanche-inicio');
    r = await duel(45000);
    s = r.state;
    log.push({ label: `rematch try ${tries}`, hits: r.hits, parries: r.parries, flank: r.flank, words: r.words, hp: s.hp });
    if (!bridge(s).alive) break;
    const t0 = Date.now(); while (Date.now() - t0 < 6000) { s = await bot.st(); if (!s.dead && s.hp === 5) break; await g.sleep(200); }
  }
  await bot.note('rematch');
  check('the side step + cut lands on the flank', r.flank > 0, `${r.hits} hits, ${r.flank} on the flank, ${r.parries} parries, words ${r.words.join(',')}`);
  check('the rematch is won', !bridge(s).alive, `try ${tries}, hp ${s.hp}, deaths ${s.deaths}`);
  await shot('revanche-fim');
} catch (err) {
  log.push({ label: 'harness error', err: String(err.stack || err) }); console.log('harness error', err);
} finally {
  log.push({ label: 'page errors', errors: g.errors });
  check('no page errors', g.errors.length === 0, g.errors.join(' | '));
  writeFileSync(`${out}/log.json`, JSON.stringify(log, null, 1));
  writeFileSync(`${out}/checks.json`, JSON.stringify(checks, null, 1));
  await g.close();
}
