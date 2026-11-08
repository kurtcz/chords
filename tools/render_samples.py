#!/usr/bin/env python3
"""
Renders the instrument samples of the app from SoundFonts: the General MIDI FluidR3_GM.sf2 and, for the
mandolin, Mandolino.sf2 (real mandolin recordings), both looked up in the given folder.

    python3 -I tools/render_samples.py . [instrument ...]

Without instruments it renders all of them.

Needs fluidsynth, ffmpeg and afconvert (macOS). Writes one note every STEP semitones of each
instrument's range, so the app never shifts a note by more than a semitone:
  Android: Platforms/Android/Resources/raw/<instrument>_<pitch>.ogg (Vorbis)
  iOS:     Platforms/iOS/Resources/<instrument>_<pitch>.m4a (AAC, Apple's encoder keeps the start sample-exact)
The ranges must match SampleSet in Chords/Services/SampleSet.cs.
"""
import os, struct, subprocess, sys, tempfile, wave
import numpy as np

STEP = 3
SR = 44100
# name: (SoundFont, bank, program, lowest pitch, highest pitch, seconds, decay)
# decay: time constant (s) of the fade applied after the attack; the SoundFonts' low notes barely die away
# on their own, so without it they ring at full strength until cut off.
INSTRUMENTS = {
    'guitar':   ('FluidR3_GM.sf2', 0, 25, 38, 89, 2.5, 0.7),    # Steel String Guitar
    'ukulele':  ('FluidR3_GM.sf2', 8, 24, 59, 89, 2.0, 0.6),
    'mandolin': ('Mandolino.sf2', 0, 1, 55, 94, 2.0, 0.6),      # Mandolino2
    'bass':     ('FluidR3_GM.sf2', 0, 33, 26, 65, 2.5, 0.8),    # Fingered Bass
    'banjo':    ('FluidR3_GM.sf2', 0, 105, 48, 90, 2.0, 0.6),
}
# The attack (s) the decay leaves untouched.
ATTACK = 0.1
# Loudest peak of each instrument's set, as loud as the original guitar recordings.
PEAK = 0.9
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Chords', 'Platforms')


def midi(bank, program, pitch, seconds):
    def vlq(n):
        b = [n & 0x7f]
        n >>= 7
        while n:
            b.insert(0, (n & 0x7f) | 0x80)
            n >>= 7
        return bytes(b)
    ticks = int(960 * seconds)  # 480 per quarter at 120 bpm = 960 per second
    ev = (b'\x00' + bytes([0xB0, 0, bank]) + b'\x00' + bytes([0xC0, program]) + b'\x00' + bytes([0x90, pitch, 100])
          + vlq(ticks) + bytes([0x80, pitch, 0]) + b'\x00\xff\x2f\x00')
    return b'MThd' + struct.pack('>IHHH', 6, 0, 1, 480) + b'MTrk' + struct.pack('>I', len(ev)) + ev


def render(sf2, tmp, bank, program, pitch, seconds, decay):
    mid, raw = os.path.join(tmp, 'n.mid'), os.path.join(tmp, 'n.wav')
    with open(mid, 'wb') as f:
        f.write(midi(bank, program, pitch, seconds + 1))
    subprocess.run(['fluidsynth', '-ni', '-q', '-R', '0', '-C', '0', '-g', '0.5', '-r', str(SR), '-F', raw, sf2, mid], check=True)
    with wave.open(raw) as w:
        x = np.frombuffer(w.readframes(w.getnframes()), np.int16).astype(np.float32).reshape(-1, w.getnchannels()).mean(axis=1) / 32768
    # start at the attack, so a pluck sounds the moment it is played
    start = int(np.argmax(np.abs(x) > 0.002))
    x = x[start:start + int(seconds * SR)].copy()
    t = np.arange(len(x)) / SR
    x *= np.exp(-np.maximum(0, t - ATTACK) / decay)
    fade = int(0.4 * SR)
    x[-fade:] *= np.linspace(1, 0, fade) ** 2
    return x


def write(path, x):
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype(np.int16).tobytes())


def main(folder, names):
    with tempfile.TemporaryDirectory() as tmp:
        for name in names or INSTRUMENTS:
            soundfont, bank, program, low, high, seconds, decay = INSTRUMENTS[name]
            sf2 = os.path.join(folder, soundfont)
            notes = {p: render(sf2, tmp, bank, program, p, seconds, decay) for p in range(low, high + 1, STEP)}
            gain = PEAK / max(np.max(np.abs(x)) for x in notes.values())
            for pitch, x in notes.items():
                wav = os.path.join(tmp, f'{name}_{pitch}.wav')
                write(wav, x * gain)
                subprocess.run(['ffmpeg', '-loglevel', 'error', '-y', '-i', wav, '-c:a', 'libvorbis', '-q:a', '4',
                                os.path.join(ROOT, 'Android', 'Resources', 'raw', f'{name}_{pitch}.ogg')], check=True)
                subprocess.run(['afconvert', '-f', 'm4af', '-d', 'aac', '-b', '96000', wav,
                                os.path.join(ROOT, 'iOS', 'Resources', f'{name}_{pitch}.m4a')], check=True)
            print(name, len(notes), 'notes', f'gain {gain:.2f}')


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2:])
