// Helpers that play the one-shot like a person at the keyboard: walk with WASD toward a
// point (a path found on the game's own tile map, read only), lock with Space, cut with J...
// Every move is a key press; nothing is written into the game.
export function makeBot(g, log) {
  const TS = 32;
  let seenEvents = 0;
  const held = new Set();
  async function setKeys(want) {
    for (const k of [...held]) if (!want.includes(k)) { await g.up(k); held.delete(k); }
    for (const k of want) if (!held.has(k)) { await g.down(k); held.add(k); }
  }
  const bot = {
    async st() {
      return g.eval(`(() => { const s = game.state, p = s.p; return {
        scene: game.scene, x: p.x, y: p.y, hp: p.hp, qi: Math.round(p.qi), face: p.face, act: p.act && p.act.kind, dead: p.dead,
        lock: s.lock ? s.lock.type : null, area: s.area, panel: s.panel && { title: s.panel.title, lines: s.panel.lines },
        speech: s.speech && s.speech.s, toasts: s.toasts.map(t => t.s), quests: s.quests.map(q => q.id + (q.done ? ':ok' : '')),
        hasManual: s.hasManual, doorOpen: s.doorOpen, slabRead: s.slabRead, fpIdx: s.fpIdx, learnedSide: s.learnedSide,
        gateOpen: s.gateOpen, comboFound: s.comboFound, deaths: s.deaths, reveal: s.reveal,
        enemies: s.enemies.map(e => ({ type: e.type, x: Math.round(e.x), y: Math.round(e.y), hp: e.hp, st: e.st, alive: e.alive, face: +e.face.toFixed(2), fooled: +e.fooled.toFixed(2), bridge: !!e.bridge })) }; })()`);
    },
    async events() {
      const ev = await g.eval(`game.events.slice(${seenEvents})`);
      seenEvents += ev.length; return ev;
    },
    async note(label) {
      const s = await bot.st();
      const ev = await bot.events();
      log.push({ label, t: Date.now(), state: { x: Math.round(s.x / TS * 10) / 10, y: Math.round(s.y / TS * 10) / 10, hp: s.hp, area: s.area, quests: s.quests, toasts: s.toasts, speech: s.speech, panel: s.panel, learnedSide: s.learnedSide, fpIdx: s.fpIdx, deaths: s.deaths }, events: ev.map(e => e.type + (e.kind ? ':' + e.kind : e.id ? ':' + e.id : e.on ? ':' + e.on : e.by ? ':' + e.by : e.name ? ':' + e.name : '')) });
      return s;
    },
    // path on tiles (BFS, 8-way without cutting corners), using the game's solidTile
    async path(tx, ty) {
      return g.eval(`(() => {
        const p = game.state.p, sx = Math.floor(p.x / TS), sy = Math.floor(p.y / TS), gx = ${Math.floor(tx / TS)}, gy = ${Math.floor(ty / TS)};
        const free = (x, y) => x >= 0 && y >= 0 && x < MW && y < MH && !solidTile(tileXY(x, y), false);
        const prev = new Map(); const q = [[sx, sy]]; prev.set(sx + ',' + sy, null);
        while (q.length) { const [x, y] = q.shift(); if (x === gx && y === gy) break;
          for (const [dx, dy] of [[1,0],[-1,0],[0,1],[0,-1],[1,1],[1,-1],[-1,1],[-1,-1]]) {
            const nx = x + dx, ny = y + dy, k = nx + ',' + ny; if (prev.has(k) || !free(nx, ny)) continue;
            if (dx && dy && (!free(x + dx, y) || !free(x, y + dy))) continue;
            prev.set(k, [x, y]); q.push([nx, ny]); } }
        let k = gx + ',' + gy; if (!prev.has(k)) return null; const out = [];
        while (k) { const [x, y] = k.split(',').map(Number); out.unshift([x * TS + 16, y * TS + 16]); const pr = prev.get(k); k = pr ? pr[0] + ',' + pr[1] : null; }
        return out; })()`);
    },
    // walk to a point; stops on arrival, on a panel, or when a stop() test says so
    async goTo(x, y, { tol = 6, timeout = 40000, stop = null, direct = false } = {}) {
      const t0 = Date.now();
      let route = direct ? [[0, 0]] : await bot.path(x, y);
      if (!route) { log.push({ label: `no path to ${x / TS},${y / TS}` }); return false; }
      route.push([x, y]);
      if (direct) route = [[0, 0], [x, y]];
      let i = 1, stuck = 0, last = null;
      while (Date.now() - t0 < timeout) {
        const s = await bot.st();
        if (stop && stop(s)) { await setKeys([]); return 'stopped'; }
        if (s.panel) { await setKeys([]); return 'panel'; }
        if (s.dead) { await setKeys([]); return 'dead'; }
        while (i < route.length - 1 && Math.hypot(route[i][0] - s.x, route[i][1] - s.y) < 12) i++;
        const [wx, wy] = route[Math.min(i, route.length - 1)];
        const dx = wx - s.x, dy = wy - s.y;
        if (i >= route.length - 1 && Math.hypot(dx, dy) < tol) { await setKeys([]); return true; }
        const keys = [];
        const dz = direct ? 1.5 : 3;
        if (dx > dz) keys.push('KeyD'); else if (dx < -dz) keys.push('KeyA');
        if (dy > dz) keys.push('KeyS'); else if (dy < -dz) keys.push('KeyW');
        await setKeys(keys);
        if (last && Math.hypot(last.x - s.x, last.y - s.y) < 0.5) { if (++stuck > 25) { route = await bot.path(x, y); if (route) { route.push([x, y]); i = 1; } stuck = 0; } } else stuck = 0;
        last = s;
        await g.sleep(30);
      }
      await setKeys([]);
      log.push({ label: `goTo timeout ${x / TS},${y / TS}` });
      return false;
    },
    async release() { await setKeys([]); },
  };
  return bot;
}
