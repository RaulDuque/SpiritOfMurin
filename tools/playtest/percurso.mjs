// Shared stretches of the normal route, played with keys only (see bot.mjs). Each returns
// what it saw; the tests decide what to check.
const TS = 32;
export const bridge = s => s.enemies.find(e => e.bridge);

// title -> bridge: first sight, a few frontal cuts, then retreat south
export async function firstEncounterAndRetreat(g, bot) {
  await bot.goTo(29 * TS + 16, 21 * TS);
  await bot.goTo(29 * TS + 16, 18.6 * TS, { stop: s => !!s.speech });
  await g.sleep(350);
  await g.tap('Space'); await g.sleep(100);
  let parries = 0;
  for (let i = 0; i < 4; i++) {
    await g.hold(['KeyW'], 220); await g.tap('KeyJ'); await g.sleep(320);
    for (const ev of await bot.events()) if (ev.type === 'parry') parries++;
  }
  await g.tap('Space');
  await bot.goTo(29 * TS + 16, 27 * TS);
  await g.sleep(1500);
  return { parries, state: await bot.st() };
}

// the corpse in the valley, the tomb door, the mark, the footprints in the right order
export async function manualTombAndLearn(g, bot, shot = async () => {}) {
  await bot.goTo(29 * TS + 16, 34 * TS);
  await bot.goTo(40.4 * TS, 34.6 * TS);
  await g.tap('KeyE'); await g.sleep(300); await g.tap('KeyE'); await g.sleep(300);
  await bot.goTo(51 * TS, 31.7 * TS);
  await g.tap('KeyE'); await g.sleep(300); await g.tap('KeyE'); await g.sleep(300);
  await bot.goTo(51 * TS, 23.6 * TS);
  await g.tap('KeyE'); await g.sleep(300); await g.tap('KeyE'); await g.sleep(500);
  const marks = await g.eval('MARKS.map(m => [m.x, m.y])');
  await bot.goTo(53 * TS, 26 * TS, { direct: true });
  await bot.goTo(marks[0][0] + 20, marks[0][1] + 10, { direct: true });
  for (const i of [0, 1, 2]) { await bot.goTo(marks[i][0], marks[i][1], { direct: true, tol: 3 }); await g.sleep(250); }
  await shot('revelacao');
  await g.sleep(3600);
  return bot.st();
}

// lock on the bridge disciple and fight with side step + cut, reading the on-screen key hint
export async function duelSide(g, bot, timeLimit = 45000) {
  const t0 = Date.now(); let turn = 0, hits = 0, flank = 0, parries = 0;
  let s = await bot.st();
  while (Date.now() - t0 < timeLimit) {
    s = await bot.st();
    if (s.dead || !bridge(s).alive) break;
    if (!s.lock) { await g.tap('Space'); await g.sleep(80); continue; }
    const e = bridge(s), d = Math.hypot(e.x - s.x, e.y - s.y), dx = e.x - s.x, dy = e.y - s.y, toward = [];
    if (Math.abs(dx) > 8) toward.push(dx > 0 ? 'KeyD' : 'KeyA');
    if (Math.abs(dy) > 8) toward.push(dy > 0 ? 'KeyS' : 'KeyW');
    if (d > 62) { await g.hold(toward, 100); continue; }
    const hint = await g.eval('state.learnedSide && lockTarget() ? sideKeys(state.p, lockTarget()) : ""');
    const keys = (hint.split(' / ')[turn % 2] || 'D').split('+').map(k => 'Key' + k);
    for (const k of keys) await g.down(k); await g.tap('KeyL', 40); for (const k of keys) await g.up(k);
    await g.sleep(140); await g.tap('KeyJ', 40); await g.sleep(120); await g.tap('KeyJ', 40); await g.sleep(320);
    for (const ev of await bot.events()) { if (ev.type === 'hit') hits++; if (ev.type === 'flank') flank++; if (ev.type === 'parry') parries++; }
    turn++;
  }
  await bot.release();
  return { hits, flank, parries, state: await bot.st() };
}

// the rematch, walking back after a lost try (up to `tries`)
export async function rematch(g, bot, tries = 3) {
  let r, s;
  for (let t = 1; t <= tries; t++) {
    await bot.goTo(29 * TS + 16, 22 * TS);
    await bot.goTo(29 * TS + 16, 19.6 * TS, { stop: s => bridge(s).st !== 'idle' });
    r = await duelSide(g, bot);
    s = r.state;
    if (!bridge(s).alive) return { ...r, tries: t, won: true };
    const t0 = Date.now(); while (Date.now() - t0 < 6000) { s = await bot.st(); if (!s.dead && s.hp > 0 && (await g.eval('state.dying')) <= 0) break; await g.sleep(200); }
    await g.sleep(500);
  }
  return { ...r, tries, won: false };
}
