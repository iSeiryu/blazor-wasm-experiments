window.neonSnakeAudio = (() => {
    const AudioCtx = window.AudioContext || window.webkitAudioContext;
    let audioCtx = null;
    let masterGain = null;

    const getCtx = () => {
        if (!AudioCtx) return null;

        if (!audioCtx) {
            audioCtx = new AudioCtx();
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

    audioCtx = getCtx();

    return {
        playEatSound,
        playHitSound,
        playDeathSound
    };
})();
