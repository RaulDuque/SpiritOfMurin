// Minimal Chrome DevTools Protocol driver for playtesting the one-shot in a real browser.
// No dependencies (Node 22+: global WebSocket and fetch). It opens the game from disk in a
// separate, throw-away browser profile, so the player's own browser (and its saved key
// bindings) is never touched. Keys go through the page's normal keydown/keyup listeners:
// no debug shortcut, no state written from outside. The harness only READS window.game.state
// to know where the player is and what happened.
import { spawn } from 'node:child_process';
import { mkdtempSync, writeFileSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const BROWSERS = [
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
];
const sleep = ms => new Promise(r => setTimeout(r, ms));

const KEYS = {
  KeyW: ['w', 87], KeyA: ['a', 65], KeyS: ['s', 83], KeyD: ['d', 68], KeyJ: ['j', 74], KeyK: ['k', 75],
  KeyL: ['l', 76], KeyE: ['e', 69], KeyH: ['h', 72], KeyI: ['i', 73], KeyM: ['m', 77], KeyP: ['p', 80],
  KeyO: ['o', 79], KeyN: ['n', 78], Space: [' ', 32], Enter: ['Enter', 13], Escape: ['Escape', 27], Tab: ['Tab', 9],
  ArrowUp: ['ArrowUp', 38], ArrowDown: ['ArrowDown', 40], Backspace: ['Backspace', 8], KeyU: ['u', 85],
};

export async function launch({ url, port = 9333, width = 1280, height = 720, profile: profileDir = null, browser = process.env.PLAYTEST_BROWSER }) {
  // PLAYTEST_BROWSER=edge (or chrome) picks the browser; default: Chrome, else Edge
  const exe = browser ? BROWSERS.find(b => b.toLowerCase().includes(browser === 'edge' ? 'msedge' : 'chrome')) : BROWSERS.find(existsSync);
  if (!exe) throw new Error('no Chrome or Edge found');
  // a fixed profile folder keeps localStorage between launches (close and reopen); default: a fresh one
  const profile = profileDir || mkdtempSync(join(tmpdir(), 'som-playtest-'));
  const proc = spawn(exe, [
    '--headless=new', `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`,
    `--window-size=${width},${height}`, '--force-device-scale-factor=1', '--hide-scrollbars',
    '--autoplay-policy=no-user-gesture-required', '--mute-audio', '--no-first-run', '--no-default-browser-check',
    'about:blank',
  ], { stdio: 'ignore' });
  let targets;
  for (let i = 0; i < 100; i++) {
    try { targets = await (await fetch(`http://127.0.0.1:${port}/json`)).json(); if (targets.some(t => t.type === 'page')) break; } catch { /* not up yet */ }
    await sleep(100);
  }
  const page = targets.find(t => t.type === 'page');
  const ws = new WebSocket(page.webSocketDebuggerUrl);
  await new Promise((res, rej) => { ws.onopen = res; ws.onerror = rej; });
  let id = 0; const wait = new Map(); const errors = [];
  ws.onmessage = m => {
    const d = JSON.parse(m.data);
    if (d.id && wait.has(d.id)) { const { res, rej } = wait.get(d.id); wait.delete(d.id); d.error ? rej(new Error(d.error.message)) : res(d.result); }
    if (d.method === 'Runtime.exceptionThrown') errors.push(d.params.exceptionDetails.exception?.description || d.params.exceptionDetails.text);
  };
  const send = (method, params = {}) => new Promise((res, rej) => { const i = ++id; wait.set(i, { res, rej }); ws.send(JSON.stringify({ id: i, method, params })); });
  await send('Runtime.enable'); await send('Page.enable');
  await send('Page.navigate', { url });
  await sleep(1200);
  const g = {
    errors,
    async eval(expr) {
      const r = await send('Runtime.evaluate', { expression: expr, returnByValue: true, awaitPromise: true });
      if (r.exceptionDetails) throw new Error(r.exceptionDetails.exception?.description || r.exceptionDetails.text);
      return r.result.value;
    },
    async down(code) { const [key, vk] = KEYS[code]; await send('Input.dispatchKeyEvent', { type: 'keyDown', code, key, windowsVirtualKeyCode: vk }); },
    async up(code) { const [key, vk] = KEYS[code]; await send('Input.dispatchKeyEvent', { type: 'keyUp', code, key, windowsVirtualKeyCode: vk }); },
    async tap(code, ms = 60) { await g.down(code); await sleep(ms); await g.up(code); },
    async hold(codes, ms) { for (const c of codes) await g.down(c); await sleep(ms); for (const c of codes) await g.up(c); },
    // a mouse click at window pixels (x, y); button: 'left' | 'middle' | 'right'
    async click(x, y, button = 'left', ms = 50) {
      await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
      await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button, clickCount: 1 });
      await sleep(ms);
      await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button, clickCount: 1 });
    },
    async shot(path) { const r = await send('Page.captureScreenshot', { format: 'png' }); writeFileSync(path, Buffer.from(r.data, 'base64')); },
    sleep, send,
    // closes the browser the way a person does (Browser.close), so storage is flushed to disk
    async close() {
      try {
        const v = await (await fetch(`http://127.0.0.1:${port}/json/version`)).json();
        const bws = new WebSocket(v.webSocketDebuggerUrl);
        await new Promise((res, rej) => { bws.onopen = res; bws.onerror = rej; });
        bws.send(JSON.stringify({ id: 1, method: 'Browser.close' }));
        await new Promise(res => { proc.once('exit', res); setTimeout(res, 8000); });
      } catch { /* already gone */ }
      try { ws.close(); } catch { /* gone */ }
      if (proc.exitCode === null) proc.kill();
    },
    profile,
  };
  return g;
}
