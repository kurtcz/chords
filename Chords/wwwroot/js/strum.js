// Strumming a chord diagram with a finger: each string sounds as the finger crosses it, so the strum goes the way
// and at the pace the finger does.
// - Vertical diagrams: a sideways swipe over the diagram (an up or down swipe still scrolls the page).
// - Horizontal diagrams: dragging the pick under the strings (dragging the diagram itself scrolls the page).
// The diagram's svg says where its strings lie (data-strum-axis, data-strum-strings in svg units); the component
// (Components/Shared/ChordLayoutView.razor) plays them.
window.chordStrum = (() => {
    // Pixels a finger moves before the gesture counts as a strum or a scroll.
    const threshold = 8;
    const components = new WeakMap();
    // A strum ends with a click, which would play the whole chord as a tap does.
    let swallowClickUntil = 0;
    let gesture = null;

    window.addEventListener("click", e => {
        if (performance.now() < swallowClickUntil) {
            swallowClickUntil = 0;
            e.stopPropagation();
            e.preventDefault();
        }
    }, true);

    // The pick takes the press before the page sees it, so it neither scrolls nor starts holding the diagram
    // (which marks it as a favourite).
    window.addEventListener("pointerdown", e => {
        const pick = e.target.closest && e.target.closest(".strum-pick");
        const root = pick && pick.closest(".chord-layout");

        if (!root || !components.has(root)) {
            return;
        }
        e.stopPropagation();
        e.preventDefault();
        const g = start(root, e);

        // From under the lowest string, so the first move sounds every string up to the finger.
        g.last = Math.max(...g.strings) + 1;
        g.pick = pick.querySelector(".strum-pick-body");
        g.pick.classList.add("dragging");
        g.pickHome = point(g.svg, e).y;
        begin(g, false);
        move(g, e);
    }, true);

    // The page must not scroll while the pick is dragged (pointer events alone don't stop WebKit).
    document.addEventListener("touchstart", e => {
        if (e.target.closest && e.target.closest(".strum-pick")) {
            e.preventDefault();
        }
    }, { passive: false });

    function start(root, e) {
        const svg = root.querySelector("svg[data-strum-strings]");

        gesture = {
            root, svg,
            pointer: e.pointerId,
            x: e.clientX,
            y: e.clientY,
            axis: svg.dataset.strumAxis,
            strings: svg.dataset.strumStrings.split(",").map(Number),
            strumming: false
        };
        return gesture;
    }

    function begin(g, notify) {
        g.strumming = true;
        try {
            g.root.setPointerCapture(g.pointer);
        } catch {
            // The pointer may be gone already.
        }
        if (notify) {
            call(g.root, "StrumStart");
        }
    }

    function point(svg, e) {
        return new DOMPoint(e.clientX, e.clientY).matrixTransform(svg.getScreenCTM().inverse());
    }

    function move(g, e) {
        const p = point(g.svg, e);
        const now = g.axis === "x" ? p.x : p.y;
        const from = g.last;
        // Strings between the last position and this one, in the order the finger crossed them.
        const crossed = g.strings
            .map((at, index) => ({ at, index }))
            .filter(s => (from < s.at && now >= s.at) || (from > s.at && now <= s.at))
            .sort((a, b) => now > from ? a.at - b.at : b.at - a.at);

        g.last = now;
        for (const s of crossed) {
            call(g.root, "Pluck", s.index);
        }
        if (g.pick) {
            // The pick follows the finger along the strings, from just above the highest back to its place
            // (14 svg units under the lowest string, see GuitarChordLayoutDecorator.RenderSchema).
            const top = Math.min(...g.strings) - Math.max(...g.strings) - 28;
            const offset = Math.min(4, Math.max(top, p.y - g.pickHome));

            g.pick.style.transform = `translateY(${offset}px)`;
        }
    }

    function end(e) {
        const g = gesture;

        if (!g || e.pointerId !== g.pointer) {
            return;
        }
        gesture = null;
        if (g.strumming) {
            swallowClickUntil = performance.now() + 500;
        }
        if (g.pick) {
            g.pick.classList.remove("dragging");
            g.pick.style.transform = "";
        }
    }

    function call(root, method, ...args) {
        const dotnet = components.get(root);

        dotnet && dotnet.invokeMethodAsync(method, ...args).catch(() => { });
    }

    return {
        attach(root, dotnet) {
            if (components.has(root)) {
                components.set(root, dotnet);
                return;
            }
            components.set(root, dotnet);

            root.addEventListener("pointerdown", e => {
                // Swipes across a vertical diagram only: a horizontal one is strummed with its pick.
                if (gesture || !root.querySelector("svg[data-strum-axis='x']")) {
                    return;
                }
                start(root, e);
            });
            root.addEventListener("pointermove", e => {
                const g = gesture;

                if (!g || g.root !== root || e.pointerId !== g.pointer) {
                    return;
                }
                if (!g.strumming) {
                    const dx = Math.abs(e.clientX - g.x), dy = Math.abs(e.clientY - g.y);

                    if (dy > threshold && dy >= dx) {
                        // Scrolling the page.
                        gesture = null;
                        return;
                    }
                    if (dx <= threshold) {
                        return;
                    }
                    g.last = point(g.svg, { clientX: g.x, clientY: g.y }).x;
                    begin(g, true);
                }
                move(g, e);
            });
            root.addEventListener("pointerup", end);
            root.addEventListener("pointercancel", end);
        },

        detach(root) {
            components.delete(root);
        }
    };
})();
