// Labels that shrink to fit: an element with data-autofit keeps its label (the <span> inside it) on one line and
// makes the font smaller when a translation is too long for it. Only below minScale does the label wrap instead.
// The page needs no code: labels are fitted whenever the page changes (Blazor renders) or an element resizes.
(() => {
    const minScale = 0.6;
    // Text, width and font size each label was last fitted for: refitted only when one of them changes.
    const fitted = new WeakMap();
    const observed = new WeakSet();
    const resizes = window.ResizeObserver ? new ResizeObserver(fitAll) : null;

    // Width inside the padding and border, with the fractions clientWidth rounds away.
    function inner(element) {
        const style = getComputedStyle(element);

        return element.getBoundingClientRect().width
            - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight)
            - parseFloat(style.borderLeftWidth) - parseFloat(style.borderRightWidth);
    }

    function fit(element) {
        const label = element.querySelector(":scope > span");

        if (!label) {
            return;
        }
        if (resizes && !observed.has(element)) {
            observed.add(element);
            resizes.observe(element);
        }
        // A pixel to spare: glyphs may paint a little past the width they measure.
        const available = inner(element) - 1;

        if (available <= 0) {
            return;
        }
        const last = fitted.get(element);

        if (last && last.text === label.textContent && Math.abs(last.available - available) < 0.5
            && last.size === label.style.fontSize) {
            return;
        }
        label.style.fontSize = "";
        label.style.whiteSpace = "";
        label.style.maxWidth = "";
        const base = parseFloat(getComputedStyle(label).fontSize);
        let size = base;

        // Measured again after each step: the system font size (Android) does not always scale text in proportion.
        for (let i = 0; i < 5; i++) {
            const width = label.getBoundingClientRect().width;

            if (width <= available) {
                break;
            }
            size = Math.floor(size * available / width * 10) / 10 - (i > 0 ? 0.5 : 0);
            if (size < base * minScale) {
                label.style.fontSize = base * minScale + "px";
                label.style.whiteSpace = "normal";
                label.style.maxWidth = "100%";
                break;
            }
            label.style.fontSize = size + "px";
        }
        fitted.set(element, { text: label.textContent, available, size: label.style.fontSize });
    }

    let pending = false;

    function fitAll() {
        if (pending) {
            return;
        }
        pending = true;
        requestAnimationFrame(() => {
            pending = false;
            document.querySelectorAll("[data-autofit]").forEach(fit);
        });
    }

    new MutationObserver(fitAll).observe(document.documentElement, { childList: true, subtree: true, characterData: true });
    window.addEventListener("resize", fitAll);
    window.addEventListener("load", fitAll);
    document.fonts && document.fonts.ready.then(fitAll);
})();
