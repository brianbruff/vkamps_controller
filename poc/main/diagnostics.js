// Diagnostics singleton — ring-buffer logger for the third-party tester handoff.
// All log entries are mirrored to renderer over IPC so the DiagnosticsView can
// render them live, and dumped to a plain-text file via saveDiag().

const { EventEmitter } = require('events');
const { app } = require('electron');
const os = require('os');

const MAX_ENTRIES = 5000;

class Diagnostics extends EventEmitter {
  constructor() {
    super();
    this.entries = [];
    this.startedAt = new Date().toISOString();
  }

  log(level, category, message, detail) {
    const entry = {
      ts: new Date().toISOString(),
      level: String(level || 'info'),
      category: String(category || 'info'),
      message: String(message ?? ''),
    };
    if (detail !== undefined && detail !== null) {
      entry.detail = detail;
    }
    this.entries.push(entry);
    if (this.entries.length > MAX_ENTRIES) {
      this.entries.splice(0, this.entries.length - MAX_ENTRIES);
    }
    try { this.emit('entry', entry); } catch { /* ignore listener errors */ }
    return entry;
  }

  info(category, message, detail)  { return this.log('info',  category, message, detail); }
  warn(category, message, detail)  { return this.log('warn',  category, message, detail); }
  error(category, message, detail) { return this.log('error', category, message, detail); }
  debug(category, message, detail) { return this.log('debug', category, message, detail); }

  getAll() {
    return this.entries.slice();
  }

  clear() {
    this.entries = [];
    this.emit('cleared');
  }

  // ---- text dump helpers ----

  static padRight(str, n) {
    str = String(str);
    return str.length >= n ? str : str + ' '.repeat(n - str.length);
  }

  static formatTs(iso) {
    // 2026-04-28T16:42:01.123Z → "2026-04-28 16:42:01.123" (local time)
    try {
      const d = new Date(iso);
      const pad = (n, w = 2) => String(n).padStart(w, '0');
      const yyyy = d.getFullYear();
      const mm = pad(d.getMonth() + 1);
      const dd = pad(d.getDate());
      const hh = pad(d.getHours());
      const mi = pad(d.getMinutes());
      const ss = pad(d.getSeconds());
      const ms = pad(d.getMilliseconds(), 3);
      return `${yyyy}-${mm}-${dd} ${hh}:${mi}:${ss}.${ms}`;
    } catch {
      return iso;
    }
  }

  static formatDetail(detail, indent) {
    if (detail == null) return '';
    const pad = ' '.repeat(indent);
    const lines = [];
    if (typeof detail !== 'object') {
      lines.push(`${pad}${String(detail)}`);
      return lines.join('\n');
    }
    // Determine longest key for column alignment.
    const keys = Object.keys(detail);
    const longest = keys.reduce((a, k) => Math.max(a, k.length), 0);
    for (const k of keys) {
      const v = detail[k];
      const valStr = Diagnostics._stringifyValue(v);
      const valLines = valStr.split('\n');
      const keyCol = Diagnostics.padRight(k, longest);
      lines.push(`${pad}${keyCol} : ${valLines[0]}`);
      const cont = ' '.repeat(indent + longest + 3);
      for (let i = 1; i < valLines.length; i++) {
        lines.push(`${cont}${valLines[i]}`);
      }
    }
    return lines.join('\n');
  }

  static _stringifyValue(v) {
    if (v == null) return String(v);
    if (typeof v === 'string') return v;
    if (typeof v === 'number' || typeof v === 'boolean') return String(v);
    if (Array.isArray(v)) return v.map(Diagnostics._stringifyValue).join(', ');
    if (v && typeof v === 'object') {
      try { return JSON.stringify(v); } catch { return String(v); }
    }
    return String(v);
  }

  /**
   * Produce the full plain-text dump described in the task spec (3d).
   * The settings snapshot is supplied by the caller so we don't reach back
   * into main/config.js here (avoids circular deps).
   */
  dumpToText({ settings } = {}) {
    const lines = [];
    const sep = '='.repeat(80);
    const sub = '-'.repeat(80);

    const platform = `${process.platform} ${process.arch} (${os.type()} ${os.release()})`;
    const generated = Diagnostics.formatTs(new Date().toISOString());
    let appVersion = '';
    try { appVersion = (app && typeof app.getVersion === 'function') ? app.getVersion() : ''; } catch {}

    lines.push(sep);
    lines.push('VKAmp POC — Diagnostic Log');
    lines.push(sep);
    lines.push('');
    lines.push(`Generated:        ${generated}`);
    lines.push(`App version:      ${appVersion || '(unknown)'}`);
    lines.push(`Electron version: ${process.versions.electron || '(unknown)'}`);
    lines.push(`Node version:     ${process.versions.node || '(unknown)'}`);
    lines.push(`Platform:         ${platform}`);
    lines.push('');

    lines.push(sub);
    lines.push('Settings snapshot');
    lines.push(sub);
    if (settings && typeof settings === 'object') {
      const order = [
        'mode', 'lanIp', 'tcpPort', 'udpPort', 'comPort', 'baudRate',
        'koef', 'voltage', 'cat', 'antenna', 'antennaMap',
        'voltsMode', 'tempUnit', 'sound', 'inputIndicator', 'alwaysOnTop',
        'verboseRxLogging', 'showFahrenheit',
      ];
      const seen = new Set();
      const longest = Math.max(15, ...Object.keys(settings).map(k => k.length));
      const writeRow = (k, v) => {
        const val = Array.isArray(v) ? v.join(',') : (v === '' || v == null ? '(unset)' : String(v));
        const catLabels = { 0: 'RF', 1: 'Icom', 2: 'Yaesu', 3: 'Kenwood/Flex', 4: 'Anan/SunSDR', 5: 'Manual' };
        let extra = '';
        if (k === 'cat' && Object.prototype.hasOwnProperty.call(catLabels, Number(v))) {
          extra = ` (${catLabels[Number(v)]})`;
        }
        lines.push(`${Diagnostics.padRight(k + ':', longest + 2)}${val}${extra}`);
      };
      for (const k of order) {
        if (k in settings) { writeRow(k, settings[k]); seen.add(k); }
      }
      for (const k of Object.keys(settings)) {
        if (!seen.has(k)) writeRow(k, settings[k]);
      }
    } else {
      lines.push('(settings unavailable)');
    }
    lines.push('');

    lines.push(sub);
    lines.push(`Event log (oldest first; ring buffer cap = ${MAX_ENTRIES})`);
    lines.push(sub);

    for (const e of this.entries) {
      const ts = Diagnostics.formatTs(e.ts);
      const level = Diagnostics.padRight(String(e.level || '').toUpperCase(), 6);
      const cat = Diagnostics.padRight(String(e.category || '').toUpperCase(), 11);
      const head = `[${ts}] ${level} ${cat}`;
      lines.push(`${head} ${e.message}`);
      if (e.detail !== undefined && e.detail !== null) {
        // Indent detail to align under the message column.
        const indent = head.length + 1;
        lines.push(Diagnostics.formatDetail(e.detail, indent));
      }
    }
    lines.push(sep);
    lines.push('End of log.');
    lines.push('');
    return lines.join('\n');
  }
}

// Singleton
const diag = new Diagnostics();
module.exports = diag;
module.exports.Diagnostics = Diagnostics;
