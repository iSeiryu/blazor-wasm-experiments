window.neonSnakeAudio = (() => {
    let audioCtx = null;
    let masterGain = null;

    const getCtx = () => {
        if (!audioCtx) {
            audioCtx = new AudioContext();
            masterGain = audioCtx.createGain();
            masterGain.connect(audioCtx.destination);
        }

        if (audioCtx.state === "suspended") {
            audioCtx.resume();
        }

        return audioCtx;
    };

    const playEatSound = () => {
        try {
            const ac = getCtx();
            if (!ac) return;
            const now = ac.currentTime;

            const osc1 = ac.createOscillator();
            const gain1 = ac.createGain();
            osc1.connect(gain1);
            gain1.connect(masterGain);
            osc1.type = "sine";
            osc1.frequency.setValueAtTime(520, now);
            osc1.frequency.exponentialRampToValueAtTime(1040, now + 0.13);
            gain1.gain.setValueAtTime(0.28, now);
            gain1.gain.exponentialRampToValueAtTime(0.001, now + 0.28);
            osc1.start(now);
            osc1.stop(now + 0.28);

            const osc2 = ac.createOscillator();
            const gain2 = ac.createGain();
            osc2.connect(gain2);
            gain2.connect(masterGain);
            osc2.type = "sine";
            osc2.frequency.setValueAtTime(1040, now + 0.03);
            osc2.frequency.exponentialRampToValueAtTime(2080, now + 0.15);
            gain2.gain.setValueAtTime(0.12, now + 0.03);
            gain2.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
            osc2.start(now + 0.03);
            osc2.stop(now + 0.22);
        } catch {
            // Ignore audio failures silently to avoid interrupting gameplay.
        }
    };

    const playHitSound = () => {
        try {
            const ac = getCtx();
            if (!ac) return;
            const now = ac.currentTime;

            const osc1 = ac.createOscillator();
            const gain1 = ac.createGain();
            osc1.connect(gain1);
            gain1.connect(masterGain);
            osc1.type = "sawtooth";
            osc1.frequency.setValueAtTime(240, now);
            osc1.frequency.exponentialRampToValueAtTime(55, now + 0.18);
            gain1.gain.setValueAtTime(0.38, now);
            gain1.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
            osc1.start(now);
            osc1.stop(now + 0.22);

            const osc2 = ac.createOscillator();
            const gain2 = ac.createGain();
            osc2.connect(gain2);
            gain2.connect(masterGain);
            osc2.type = "square";
            osc2.frequency.setValueAtTime(160, now);
            osc2.frequency.exponentialRampToValueAtTime(38, now + 0.09);
            gain2.gain.setValueAtTime(0.28, now);
            gain2.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
            osc2.start(now);
            osc2.stop(now + 0.12);
        } catch {
            // Ignore audio failures silently to avoid interrupting gameplay.
        }
    };

    const DEATH_NOTES = [
        { freq: 440, endFreq: 330, start: 0.0, dur: 0.22 },
        { freq: 330, endFreq: 220, start: 0.19, dur: 0.22 },
        { freq: 220, endFreq: 100, start: 0.36, dur: 0.55 }
    ];

    const playDeathSound = () => {
        try {
            const ac = getCtx();
            if (!ac) return;
            const now = ac.currentTime;

            for (const n of DEATH_NOTES) {
                const osc = ac.createOscillator();
                const gain = ac.createGain();
                osc.connect(gain);
                gain.connect(masterGain);
                osc.type = "sawtooth";
                osc.frequency.setValueAtTime(n.freq, now + n.start);
                osc.frequency.exponentialRampToValueAtTime(n.endFreq, now + n.start + n.dur);
                gain.gain.setValueAtTime(0.32, now + n.start);
                gain.gain.exponentialRampToValueAtTime(0.001, now + n.start + n.dur + 0.05);
                osc.start(now + n.start);
                osc.stop(now + n.start + n.dur + 0.08);
            }
        } catch {
            // Ignore audio failures silently to avoid interrupting gameplay.
        }
    };

    // ── Monster death: electronic zap burst ─────────────────────────────────
    const playMonsterDeathSound = () => {
        try {
            const ac = getCtx();
            if (!ac) return;
            const now = ac.currentTime;

            const osc1 = ac.createOscillator();
            const g1 = ac.createGain();
            osc1.connect(g1); g1.connect(masterGain);
            osc1.type = 'sawtooth';
            osc1.frequency.setValueAtTime(420, now);
            osc1.frequency.exponentialRampToValueAtTime(42, now + 0.28);
            g1.gain.setValueAtTime(0.22, now);
            g1.gain.exponentialRampToValueAtTime(0.001, now + 0.30);
            osc1.start(now); osc1.stop(now + 0.32);

            const osc2 = ac.createOscillator();
            const g2 = ac.createGain();
            osc2.connect(g2); g2.connect(masterGain);
            osc2.type = 'square';
            osc2.frequency.setValueAtTime(950, now);
            osc2.frequency.exponentialRampToValueAtTime(65, now + 0.10);
            g2.gain.setValueAtTime(0.11, now);
            g2.gain.exponentialRampToValueAtTime(0.001, now + 0.13);
            osc2.start(now); osc2.stop(now + 0.15);
        } catch { /* ignore */ }
    };

    // ── Egg hatch: whimsical ascending chirp ────────────────────────────────
    const playEggHatchSound = () => {
        try {
            const ac = getCtx();
            if (!ac) return;
            const now = ac.currentTime;

            const notes = [{ f: 520, t: 0.00 }, { f: 780, t: 0.07 }, { f: 1040, t: 0.13 }];
            for (const n of notes) {
                const osc = ac.createOscillator();
                const g = ac.createGain();
                osc.connect(g); g.connect(masterGain);
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(n.f, now + n.t);
                osc.frequency.linearRampToValueAtTime(n.f * 1.12, now + n.t + 0.06);
                g.gain.setValueAtTime(0.13, now + n.t);
                g.gain.exponentialRampToValueAtTime(0.001, now + n.t + 0.09);
                osc.start(now + n.t); osc.stop(now + n.t + 0.10);
            }
        } catch { /* ignore */ }
    };

    // ── Space ambient background music ──────────────────────────────────────
    const BEAT = 0.30; // seconds per melody step  (~100 bpm 8th notes)
    const MELODY = [
        { f: 220,   v: 0.065 }, // A3
        { f: 329.6, v: 0.060 }, // E4
        { f: 392,   v: 0.055 }, // G4
        { f: 329.6, v: 0.050 }, // E4
        { f: 293.7, v: 0.060 }, // D4
        { f: 220,   v: 0.058 }, // A3
        { f: 0,     v: 0     }, // rest
        { f: 293.7, v: 0.062 }, // D4
        { f: 329.6, v: 0.065 }, // E4
        { f: 392,   v: 0.060 }, // G4
        { f: 440,   v: 0.058 }, // A4
        { f: 392,   v: 0.055 }, // G4
        { f: 329.6, v: 0.060 }, // E4
        { f: 293.7, v: 0.058 }, // D4
        { f: 0,     v: 0     }, // rest
        { f: 220,   v: 0.075 }, // A3 (accent)
    ];

    audioCtx = getCtx();

    let musicPlaying = false;
    let musicNoteIndex = 0;
    let nextNoteTime = 0;
    let musicTimeoutId = null;
    let droneOscs = [];
    let droneGains = [];
    let echoDelay = null;
    let echoFeedback = null;

    const buildEcho = (ac) => {
        if (echoDelay) return;
        echoDelay = ac.createDelay(1.0);
        echoDelay.delayTime.value = BEAT * 2;
        echoFeedback = ac.createGain();
        echoFeedback.gain.value = 0.28;
        const lpf = ac.createBiquadFilter();
        lpf.type = 'lowpass';
        lpf.frequency.value = 1800;
        echoDelay.connect(echoFeedback);
        echoFeedback.connect(lpf);
        lpf.connect(echoDelay);
        echoDelay.connect(masterGain);
    };

    const scheduleMelody = () => {
        if (!musicPlaying) return;
        const ac = getCtx();
        if (!ac) return;
        while (nextNoteTime < ac.currentTime + 0.35) {
            const note = MELODY[musicNoteIndex % MELODY.length];
            const t = nextNoteTime;
            if (note.f > 0) {
                const osc = ac.createOscillator();
                const g = ac.createGain();
                osc.type = 'triangle';
                osc.frequency.value = note.f;
                osc.connect(g);
                g.connect(masterGain);
                if (echoDelay) g.connect(echoDelay);
                const dur = BEAT * 0.68;
                g.gain.setValueAtTime(0, t);
                g.gain.linearRampToValueAtTime(note.v, t + 0.018);
                g.gain.setValueAtTime(note.v, t + dur * 0.55);
                g.gain.exponentialRampToValueAtTime(0.001, t + dur);
                osc.start(t); osc.stop(t + dur + 0.05);
            }
            // Low bass pulse every 8 steps
            if (musicNoteIndex % 8 === 0) {
                const bass = ac.createOscillator();
                const bg = ac.createGain();
                bass.type = 'sine';
                bass.frequency.setValueAtTime(55, t);
                bass.frequency.exponentialRampToValueAtTime(52, t + BEAT * 2.5);
                bass.connect(bg); bg.connect(masterGain);
                bg.gain.setValueAtTime(0.058, t);
                bg.gain.exponentialRampToValueAtTime(0.001, t + BEAT * 2.6);
                bass.start(t); bass.stop(t + BEAT * 2.7);
            }
            musicNoteIndex++;
            nextNoteTime += BEAT;
        }
        musicTimeoutId = setTimeout(scheduleMelody, 80);
    };

    const startDrone = (ac) => {
        const freqs = [55, 55.38, 110, 110.55];
        const vols  = [0.030, 0.022, 0.018, 0.013];
        freqs.forEach((f, i) => {
            const osc = ac.createOscillator();
            const g = ac.createGain();
            osc.type = 'sine';
            osc.frequency.value = f;
            osc.connect(g); g.connect(masterGain);
            g.gain.setValueAtTime(0, ac.currentTime);
            g.gain.linearRampToValueAtTime(vols[i], ac.currentTime + 2.0);
            osc.start();
            droneOscs.push(osc);
            droneGains.push(g);
        });
    };

    const startMusic = () => {
        if (musicPlaying) return;
        const ac = getCtx();
        if (!ac) return;
        buildEcho(ac);
        startDrone(ac);
        musicPlaying = true;
        musicNoteIndex = 0;
        nextNoteTime = ac.currentTime + 0.20;
        scheduleMelody();
    };

    const stopMusic = () => {
        musicPlaying = false;
        if (musicTimeoutId) { clearTimeout(musicTimeoutId); musicTimeoutId = null; }
        const now = audioCtx ? audioCtx.currentTime : 0;
        droneGains.forEach(g => { try { g.gain.linearRampToValueAtTime(0, now + 0.8); } catch {} });
        droneOscs.forEach(o => { try { o.stop(now + 0.85); } catch {} });
        droneOscs = []; droneGains = [];
    };

    const dispose = () => {
        audioCtx && audioCtx.close();
        audioCtx = null;
    }

    return {
        playEatSound,
        playHitSound,
        playDeathSound,
        playMonsterDeathSound,
        playEggHatchSound,
        startMusic,
        stopMusic,
        dispose
    };
})();
