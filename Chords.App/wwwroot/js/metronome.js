// Metronome: clicks synthesised with Web Audio and scheduled on the audio clock a little ahead of time, so they keep
// exact time however busy the page is. The same clock drives the progressions (Pages/Progressions.razor): the page
// hears of every beat and strums its chord on it.
// - Pages/Metronome.razor shows the beats with dots inside the element it passes; js lights them as each beat sounds.
// - Web Audio may only start on a tap. A tap on an element with data-audio-unlock wakes the audio before the page
//   (which runs in .NET, after the tap is over) starts the metronome.
window.metronome = (() => {
    // How far ahead the clicks are scheduled (s), and how often the scheduler looks (ms).
    const lookahead = 0.15;
    const interval = 25;
    let context = null;
    let run = null;

    function audio() {
        if (!context) {
            const Context = window.AudioContext || window.webkitAudioContext;

            if (!Context) {
                return null;
            }
            context = new Context({ latencyHint: "interactive" });
        }
        return context;
    }

    function unlock() {
        try {
            // iOS: sound also with the ringer switched off, as the app's own notes do.
            if (navigator.audioSession) {
                navigator.audioSession.type = "playback";
            }
        } catch {
            // Not supported.
        }
        const c = audio();

        if (c && c.state !== "running") {
            c.resume().catch(() => { });
            // Older WebKit starts the audio only once something plays within the tap.
            const silence = c.createBufferSource();

            silence.buffer = c.createBuffer(1, 1, 22050);
            silence.connect(c.destination);
            silence.start(0);
        }
    }

    for (const type of ["pointerdown", "touchend", "click"]) {
        window.addEventListener(type, e => {
            if (e.target.closest && e.target.closest("[data-audio-unlock]")) {
                unlock();
            }
        }, true);
    }

    // Stop when the app goes to the background: timers pause there and the beat would stumble.
    document.addEventListener("visibilitychange", () => {
        if (document.hidden && run) {
            const dotnet = run.dotnet;

            stop();
            dotnet && dotnet.invokeMethodAsync("OnMetronomeStopped").catch(() => { });
        }
    });

    // A short sine blip: higher and louder on the accented beat, softer between the beats.
    function click(c, time, kind, volume) {
        const [frequency, gain] = kind === "accent" ? [1760, 1] : kind === "beat" ? [1175, 0.7] : [880, 0.35];
        const osc = c.createOscillator();
        const env = c.createGain();

        osc.frequency.value = frequency;
        env.gain.setValueAtTime(0.0001, time);
        env.gain.exponentialRampToValueAtTime(gain * volume, time + 0.002);
        env.gain.exponentialRampToValueAtTime(0.0001, time + 0.05);
        osc.connect(env);
        env.connect(c.destination);
        osc.start(time);
        osc.stop(time + 0.06);
    }

    function now(r) {
        return r.audio ? r.context.currentTime : performance.now() / 1000;
    }

    function schedule() {
        const r = run;

        if (!r) {
            return;
        }
        // The page was frozen for a while: carry on from now rather than rush through the beats missed.
        if (r.next < now(r) - 0.2) {
            r.next = now(r) + 0.05;
        }
        while (r.next < now(r) + lookahead) {
            const o = r.options;
            const beat = r.sub === 0;
            const countIn = r.bar < (o.countIn || 0);

            if (r.audio && (o.click || countIn)) {
                const kind = !beat ? "sub" : r.beat === 0 && o.accent ? "accent" : "beat";

                click(r.context, r.next, kind, o.volume ?? 1);
            }
            if (beat) {
                notify(r, r.next, r.bar, r.beat);
            }
            r.sub++;
            if (r.sub >= Math.max(1, o.subdivision || 1)) {
                r.sub = 0;
                r.beat++;
                if (r.beat >= o.beats) {
                    r.beat = 0;
                    r.bar++;
                }
            }
            r.next += 60 / o.bpm / Math.max(1, o.subdivision || 1);
        }
    }

    // Tells the page of a beat when it is heard (the clicks are scheduled ahead of it).
    function notify(r, time, bar, beat) {
        const latency = r.audio ? (r.context.outputLatency || r.context.baseLatency || 0) : 0;
        const delay = Math.max(0, (time - now(r) + latency) * 1000);
        const id = setTimeout(() => {
            r.pending.delete(id);
            if (run !== r) {
                return;
            }
            if (r.dots) {
                r.dots.querySelectorAll(".metronome-dot").forEach((dot, i) => dot.classList.toggle("on", i === beat));
            }
            if (r.options.notify && r.dotnet) {
                r.dotnet.invokeMethodAsync("OnMetronomeBeat", bar, beat).catch(() => { });
            }
        }, delay);

        r.pending.add(id);
    }

    function stop() {
        const r = run;

        if (!r) {
            return;
        }
        run = null;
        clearInterval(r.timer);
        r.pending.forEach(id => clearTimeout(id));
        if (r.dots) {
            r.dots.querySelectorAll(".metronome-dot.on").forEach(dot => dot.classList.remove("on"));
        }
    }

    return {
        // options: bpm, beats (per bar), subdivision (clicks per beat), accent (first beat), click (sound the beats;
        // the count-in always sounds), countIn (bars), notify (call OnMetronomeBeat(bar, beat) on the page), volume.
        start(dotnet, dots, options) {
            stop();
            const c = audio();
            const playing = !!c && c.state === "running";
            const r = {
                dotnet, dots, options,
                context: c,
                // Without sound (the audio could not start) the beats still come, on the page's clock.
                audio: playing,
                bar: 0, beat: 0, sub: 0,
                pending: new Set()
            };

            r.next = now(r) + 0.05;
            run = r;
            schedule();
            r.timer = setInterval(schedule, interval);
            return playing;
        },

        // Takes effect from the next click.
        update(options) {
            if (run) {
                run.options = options;
                if (run.beat >= options.beats) {
                    run.beat = 0;
                    run.bar++;
                }
                if (run.sub >= Math.max(1, options.subdivision || 1)) {
                    run.sub = 0;
                }
            }
        },

        stop
    };
})();
