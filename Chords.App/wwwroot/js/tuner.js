// Tuner needle: the page sends the angle of every reading (50 a second); a critically damped spring turns the
// needle towards it, so it moves smoothly without lagging behind the string.
// It steps 30 times a second on a timer rather than on every display frame: on a 120 Hz phone each frame redraws the
// whole WebView, and the needle never comes to rest while a note sounds.
window.tunerNeedle = (() => {
    // Angular frequency of the spring (1/s): the needle covers 60 % of a change in 50 ms, 90 % in 100 ms.
    const omega = 40;
    const interval = 1000 / 30;
    // Smaller moves (degrees) are not drawn: they are under a pixel at the needle's tip.
    const minMove = 0.1;
    const states = new WeakMap();

    function draw(element, state, angle) {
        state.drawn = angle;
        element.style.transform = `rotate(${angle}deg)`;
    }

    function frame(element, state) {
        const time = performance.now();
        const dt = Math.min(0.1, (time - state.time) / 1000);

        state.time = time;
        // Critically damped spring, integrated in small steps so it stays stable on slow frames.
        for (let t = 0; t < dt; t += 0.004) {
            const h = Math.min(0.004, dt - t);
            const force = omega * omega * (state.target - state.angle) - 2 * omega * state.velocity;

            state.velocity += force * h;
            state.angle += state.velocity * h;
        }
        if (Math.abs(state.target - state.angle) < 0.01 && Math.abs(state.velocity) < 0.01) {
            state.angle = state.target;
            state.velocity = 0;
            state.running = false;
        }
        if (!state.running || Math.abs(state.angle - state.drawn) >= minMove) {
            draw(element, state, state.angle);
        }
        if (state.running) {
            setTimeout(() => frame(element, state), interval);
        }
    }

    return {
        // angle in degrees; null when no note sounds (the needle hides and next shows up at the new angle).
        target(element, angle) {
            if (!element) {
                return;
            }
            let state = states.get(element);

            if (angle === null || angle === undefined) {
                element.style.visibility = "hidden";
                if (state) {
                    state.shown = false;
                }
                return;
            }
            if (!state) {
                state = { angle, velocity: 0, target: angle, running: false, shown: false, time: 0, drawn: angle };
                states.set(element, state);
            }
            if (!state.shown) {
                state.angle = angle;
                state.velocity = 0;
                state.shown = true;
                draw(element, state, angle);
                element.style.visibility = "visible";
            }
            state.target = angle;
            if (!state.running) {
                state.running = true;
                state.time = performance.now();
                setTimeout(() => frame(element, state), interval);
            }
        }
    };
})();
