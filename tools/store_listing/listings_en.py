# Source text of the store listings. Every language is a dict with:
#   name      App Store name / Google Play title (max 30), defaults to "Chord Expert"
#   subtitle  App Store subtitle (max 30)
#   short     Google Play short description (max 80)
#   promo     App Store promotional text (max 170)
#   keywords  App Store keywords, comma separated (max 100)
#   desc      Full description for both stores (max 4000)

LISTINGS = {
    "en": dict(
        subtitle="Chords, Tuner & Music Theory",
        short="Chords, tuner, metronome and music theory for guitar, ukulele, banjo & mandolin",
        promo="Every chord in every position, worked out from music theory. Smart progressions, key finder, tuner and metronome for guitar, ukulele, mandolin, bass and banjo. No ads.",
        keywords="guitar,ukulele,tuner,metronome,progression,circle of fifths,scale,capo,bass,banjo,mandolin,key",
        desc="""Chord Expert brings chords, music theory, a tuner and a metronome together for guitar, ukulele, mandolin, bass and banjo – in one app that works completely offline, with no ads, no account and no tracking.

WHY CHORD EXPERT
• Worked out from music theory, not a database – every chord is built from its notes and the shapes are calculated for your instrument, tuning and capo, so no chord or position is ever missing
• Smooth progressions – shapes are chosen across the whole progression so your hand moves as little as possible
• Works both ways – tap a shape to get the chord name, enter a few chords to get the key
• Real instrument sounds – hear every chord strummed or picked
• Private by design – the app has no internet access at all

CHORDS
• Diagrams for any chord in every position along the neck
• Difficulty rating and finger numbers for every shape, with an option to show only easy ones
• Inversions and slash chords such as C/E or D/F#
• Find a chord by shape, including inversions and incomplete chords
• Favorites for each instrument

THEORY
• Keys and scales: diatonic triads and seventh chords for major, minor, modes, pentatonic, blues and more
• Find the most likely key from the chords of a song
• Interactive circle of fifths
• Scales and chord tones on the fretboard, with intervals or note names
• Easiest capo positions for any key

PROGRESSIONS
• Typical progressions for pop, rock, jazz, blues, classical and flamenco in any key
• Random progressions built on functional harmony
• Play along in a loop with the metronome

TOOLS
• Chromatic tuner with reference tones and A4 calibration (430–450 Hz)
• Alternate and custom tunings: Drop D, DADGAD, Open G, half step down and your own
• Metronome with tap tempo, time signatures, subdivisions and accents

MADE FOR YOU
• Left-handed mode
• Dark mode and tablet layout
• English, German (H) or Latin (Do Re Mi) note names
• Available in more than 60 languages
""",
    ),
}
