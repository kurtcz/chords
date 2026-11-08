#!/usr/bin/env python3
"""Generate fastlane store metadata from tools/store_listing/listings_*.py.

Writes:
  fastlane/metadata/android/<play-locale>/{title,short_description,full_description}.txt
  fastlane/metadata/ios/<asc-locale>/{name,subtitle,promotional_text,keywords,description}.txt

Run from anywhere:  python3 tools/store_listing/generate.py
"""
import importlib.util
import pathlib
import shutil
import sys

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parent.parent
OUT = ROOT / "fastlane" / "metadata"

# language -> (Google Play locales, App Store Connect locales)
LOCALES = {
    "en": (["en-US", "en-GB", "en-AU", "en-CA", "en-IN"], ["en-US", "en-GB", "en-AU", "en-CA"]),
    "ar": (["ar"], ["ar-SA"]),
    "bg": (["bg"], []),
    "ca": (["ca"], ["ca"]),
    "cs": (["cs-CZ"], ["cs"]),
    "da": (["da-DK"], ["da"]),
    "de": (["de-DE"], ["de-DE"]),
    "el": (["el-GR"], ["el"]),
    "es": (["es-ES", "es-419", "es-US"], ["es-ES", "es-MX"]),
    "et": (["et"], []),
    "fi": (["fi-FI"], ["fi"]),
    "fr": (["fr-FR", "fr-CA"], ["fr-FR", "fr-CA"]),
    "he": (["iw-IL"], ["he"]),
    "hi": (["hi-IN"], ["hi"]),
    "hr": (["hr"], ["hr"]),
    "hu": (["hu-HU"], ["hu"]),
    "id": (["id"], ["id"]),
    "it": (["it-IT"], ["it"]),
    "ja": (["ja-JP"], ["ja"]),
    "ko": (["ko-KR"], ["ko"]),
    "lt": (["lt"], []),
    "lv": (["lv"], []),
    "ms": (["ms"], ["ms"]),
    "nb": (["no-NO"], ["no"]),
    "nl": (["nl-NL"], ["nl-NL"]),
    "pl": (["pl-PL"], ["pl"]),
    "pt-BR": (["pt-BR"], ["pt-BR"]),
    "pt": (["pt-PT"], ["pt-PT"]),
    "ro": (["ro"], ["ro"]),
    "ru": (["ru-RU"], ["ru"]),
    "sk": (["sk"], ["sk"]),
    "sl": (["sl"], []),
    "sr": (["sr"], []),
    "sv": (["sv-SE"], ["sv"]),
    "th": (["th"], ["th"]),
    "tr": (["tr-TR"], ["tr"]),
    "uk": (["uk"], ["uk"]),
    "vi": (["vi"], ["vi"]),
    "zh-Hans": (["zh-CN"], ["zh-Hans"]),
    "zh-Hant": (["zh-TW", "zh-HK"], ["zh-Hant"]),
}

LIMITS = {"name": 30, "subtitle": 30, "short": 80, "promo": 170, "keywords": 100, "desc": 4000}


def load():
    listings = {}
    for path in sorted(HERE.glob("listings_*.py")):
        spec = importlib.util.spec_from_file_location(path.stem, path)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        for lang, data in module.LISTINGS.items():
            if lang in listings:
                sys.exit(f"{lang} defined twice ({path.name})")
            listings[lang] = data
    return listings


def check(listings):
    errors = []
    for lang, data in listings.items():
        data.setdefault("name", "Chord Expert")
        data["desc"] = data["desc"].strip() + "\n"
        for field, limit in LIMITS.items():
            if field not in data:
                errors.append(f"{lang}: missing {field}")
                continue
            value = data[field].strip() if field != "desc" else data[field]
            size = len(value)
            if field == "keywords":
                size = len(value.encode("utf-8"))  # App Store counts keyword bytes
                if ", " in value:
                    errors.append(f"{lang}: keywords must not have spaces after commas")
            if size > limit:
                errors.append(f"{lang}: {field} is {size} > {limit}: {value[:60]}…")
    missing = set(LOCALES) - set(listings)
    unknown = set(listings) - set(LOCALES)
    errors += [f"{lang}: no text" for lang in sorted(missing)]
    errors += [f"{lang}: no locale mapping" for lang in sorted(unknown)]
    return errors


def write(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.strip() + "\n", encoding="utf-8")


def main():
    listings = load()
    errors = check(listings)
    if errors:
        print("\n".join(errors))
        sys.exit(1)
    for sub in ("android", "ios"):
        shutil.rmtree(OUT / sub, ignore_errors=True)
    for lang, (play, asc) in LOCALES.items():
        d = listings[lang]
        for loc in play:
            base = OUT / "android" / loc
            write(base / "title.txt", d["name"])
            write(base / "short_description.txt", d["short"])
            write(base / "full_description.txt", d["desc"])
        for loc in asc:
            base = OUT / "ios" / loc
            write(base / "name.txt", d["name"])
            write(base / "subtitle.txt", d["subtitle"])
            write(base / "promotional_text.txt", d["promo"])
            write(base / "keywords.txt", d["keywords"])
            write(base / "description.txt", d["desc"])
    plays = sum(len(p) for p, _ in LOCALES.values())
    ascs = sum(len(a) for _, a in LOCALES.values())
    print(f"{len(listings)} languages -> {plays} Google Play and {ascs} App Store locales in {OUT}")


if __name__ == "__main__":
    main()
