#!/usr/bin/env python3
"""Render the Google Play feature graphic (branding/feature.svg) in every listing language.

Writes branding/store/google-play-feature/<lang>.png (1024x500, RGB) and feature.png (English).
Text comes from tools/store_listing/feature_text.py; lines that are too wide are shrunk to fit.
Needs Google Chrome (headless) and Pillow.

Run from anywhere:  python3 tools/store_listing/feature.py [lang ...]
"""
import html
import pathlib
import re
import subprocess
import sys
import tempfile

from PIL import Image

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parent.parent
sys.path.insert(0, str(HERE))
from feature_text import FEATURE, RTL  # noqa: E402

SVG = ROOT / "branding" / "feature.svg"
OUT = ROOT / "branding" / "store" / "google-play-feature"
CHROME = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
TEXT_LEFT, TEXT_RIGHT = 490, 990  # text column; the pick sits to the left of it

# Fit every <text> into the text column by shrinking its font, never growing it.
FIT_JS = f"""
for (const t of document.querySelectorAll('text')) {{
  let size = parseFloat(t.getAttribute('font-size'));
  while (t.getComputedTextLength() > {TEXT_RIGHT - TEXT_LEFT} && size > 12) {{
    size -= 1;
    t.setAttribute('font-size', size);
  }}
}}
"""


def localized_svg(lang):
    svg = SVG.read_text(encoding="utf-8")
    texts = list(re.finditer(r"<text ([^>]*)>([^<]*)</text>", svg))
    assert len(texts) == 4, "feature.svg should have title, two tagline lines and a feature line"
    lines = ["Chord Expert", *FEATURE[lang]]
    rtl = lang in RTL
    for m, line in reversed(list(zip(texts, lines))):
        attrs = m.group(1)
        if rtl:  # right-align in the same column, reading right to left
            attrs = re.sub(r'x="\d+"', f'x="{TEXT_RIGHT}"', attrs).replace(' letter-spacing="-1"', "") + ' direction="rtl"'
        svg = svg[: m.start()] + f"<text {attrs}>{html.escape(line)}</text>" + svg[m.end():]
    return re.sub(r"<!--.*?-->", "", svg, flags=re.S)


def render(lang, tmp):
    page = tmp / f"{lang}.html"
    page.write_text(
        "<!doctype html><meta charset='utf-8'>"
        "<style>html,body{margin:0;background:#000;overflow:hidden}svg{display:block}</style>"
        f"<body>{localized_svg(lang)}<script>{FIT_JS}</script></body>",
        encoding="utf-8",
    )
    shot = tmp / f"{lang}.png"
    subprocess.run(
        [CHROME, "--headless", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
         "--window-size=1024,500", f"--screenshot={shot}", page.as_uri()],
        check=True, capture_output=True,
    )
    OUT.mkdir(parents=True, exist_ok=True)
    out = OUT / f"{lang}.png"
    Image.open(shot).convert("RGB").crop((0, 0, 1024, 500)).save(out, optimize=True)
    return out


def main():
    langs = sys.argv[1:] or list(FEATURE)
    with tempfile.TemporaryDirectory() as d:
        for lang in langs:
            print(render(lang, pathlib.Path(d)).relative_to(ROOT))
    en = OUT / "en.png"
    if en.exists():
        (ROOT / "feature.png").write_bytes(en.read_bytes())


if __name__ == "__main__":
    main()
