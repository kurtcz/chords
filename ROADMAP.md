# Možné další funkce

## Doporučené pořadí

1. Ostatní funkce podle zájmu uživatelů.

## Navržené

- **Generátor pro leváky (Left-handed mode)** – ✅ svislé/vodorovné diagramy, vodorovné lze zrcadlit pro leváky. Hmatník se stupnicemi a tóny akordu se vodorovně zobrazuje i zrcadlí pro leváky.
- **Custom ladění** – ✅ Drop D, DADGAD, Half-step down (Eb), Open G/D a vlastní ladění; generátor hmatů i přehrávání zvuku podle zvoleného ladění.
- **Přepínač nástrojů** – ✅ kytara, ukulele, mandolína, baskytara, pětistrunné banjo v Open G (krátká 5. struna od 5. pražce, barré i u ostatních nástrojů; oblíbené zvlášť pro každý nástroj). Zbývá: tenorové banjo, vlastní nahrávky zvuku pro jednotlivé nástroje. Housle záměrně ne – nemají pražce.
- **Hledání akordu podle hmatu** – ✅ uživatel naklikne prsty na hmatníku zvoleného nástroje a aplikace určí název akordu (včetně obratů jako C/E a neúplných akordů).
- **Hledač akordů do tóniny (Key & Scale Finder)** – ✅ pro zvolenou tóninu a stupnici (dur, mollové, církevní módy) zobrazí diatonické akordy I–vii° nebo septakordy s přehráním; naopak z několika zadaných akordů určí nejpravděpodobnější tóniny (v mollu i s harmonickými akordy, upřednostní začátek/konec na tónice). Sdílí tóninu s kvintovým kruhem (kruh ukazuje příbuznou durovou tóninu, posun v kruhu zachová stupnici) a obě stránky na sebe odkazují. Stupnice se zobrazuje i na hmatníku a stránka doporučí nejsnadnější pozice kapodastru.
- **Generátor akordových progresí (Smart Progression Generator)** – ✅ typické progrese podle stylu (pop, rock, jazz, blues, klasika, flamenco) v libovolné tónině, volitelně se septakordy, náhodné generování podle funkční harmonie, přehrání celé progrese a výběr hmatu ke každému akordu. Plynulé přechody (voice leading) – viz níže. Progrese hraje dokola v tempu metronomu (akord na takt, úhoz na každou dobu, takt předtaktí, volitelné klikání). Zbývá: ukládání vlastních progresí.

## Další nápady

### Hraní a cvičení
- **Kapodastr** – ✅ pozice kapodastru v nastavení (pro každý nástroj), diagramy kreslí kapodastr a počítají pražce od něj, u akordu a v progresích je vidět „tvar“ k hmatání; ladička ladí bez kapodastru.
- **Ladička** – ✅ chromatická ladička přes mikrofon (detekce YIN v Chords.Core, ověřená na nahraných strunách): ručička ±50 centů, cílová struna zvoleného nástroje i vlastního ladění, automatický výběr nebo zamčení struny s přehráním referenčního tónu, kalibrace A4 430–450 Hz. Zbývá: ověřit na zařízeních (Android bez zpracování zvuku, přepínání audio session na iOS), případně režim pro neobvyklé ladění bez strun.
- **Metronom** – ✅ tempo 30–250 BPM (posuvník, ±1, tap tempo, italské označení tempa), doby v taktu 1–12, dělení doby (osminy, trioly, šestnáctiny), důraz na první dobu, blikající tečky dob; kliky syntetizované přes Web Audio s plánováním dopředu, takže drží přesný čas. Displej zůstává zapnutý, při přechodu na pozadí se zastaví. Stejné hodiny řídí přehrávání progresí. Zbývá: ověřit na zařízeních (odemčení zvuku ve WebView, souběh s hudbou jiných aplikací, zpoždění úhozu za klikem).
- **Volba způsobu přehrání** – úder dolů/nahoru, arpeggio, tempo; kvalitnější vzorky zvuku.
- **Trénink sluchu** – kvíz: poznej typ akordu (dur/moll/septakord…) nebo interval podle poslechu.
- **Kvíz na hmatníku** – poznej akord z diagramu / najdi tón na hmatníku.

### Zobrazení
- **Stupnice a arpeggia na hmatníku** – ✅ celá stupnice (stránka tóniny) a tóny akordu (stránka akordu) přes hmatník, popisky intervaly nebo názvy tónů, tónika zvýrazněná, ťuknutí přehraje tón; respektuje kapodastr, levorukost a krátkou strunu banja.
- **Prstoklad v diagramech** – ✅ čísla prstů (1–4) v diagramech, vypočtená nejlevnějším rozložením prstů (barré, natažení). Zbývá: možnost zobrazit v diagramu místo názvů tónů intervaly.
- **Hodnocení obtížnosti hmatu** – ✅ skóre podle počtu prstů, rozpětí, natažení malíčku, barré a tlumených strun (s pravidly každého nástroje), úrovně snadný/střední/těžký; na stejném pražci řazení od nejsnadnějšího, přepínač „Jen snadné hmaty“ (u akordu bez snadného hmatu ukáže nejsnadnější dostupné). Obtížnost se zobrazuje přímo pod diagramem.
- **Plynulé přechody (voice leading)** – ✅ přepínač v progresích (výchozí zapnuto): hmaty celé progrese se volí najednou (dynamické programování přes akordy, i přechod z posledního na první), aby ruka skákala co nejméně a prsty zůstávaly na místě, a přitom byly hmaty snadné, úplné, s tónikou v basu a co nejblíž u nultého pražce (oblíbené mají přednost). Hmat zvolený ručně šipkami se připíchne 📌 a ostatní se přizpůsobí kolem něj; ťuknutím na 📌 se uvolní. Zbývá: případně zobrazit mezi diagramy, o kolik pražců se ruka posune a které prsty zůstávají.
- **Inverze a slash akordy** – ✅ u akordu volba basového tónu: libovolný (výchozí – všechny hmaty, obraty mají pod diagramem značku jako C/G), základní tón (jen hmaty bez obratů), nebo konkrétní tón akordu (C/E) i mimo akord (D/C) – generátor pak nabízí jen hmaty s tímto tónem nejníž (podle výšky, ne podle struny – u ukulele s re-entrant laděním). Hmaty se řadí od nultého pražce výš, na stejném pražci od nejsnadnějšího. Slash akordy se parsují (D/F#, 6/9 zůstává akordem), transponují, ukládají do oblíbených a hledání podle hmatu na ně odkazuje. V progresích zápis V/7, vi/5, I/3 (bas na stupni stupnice) a nové progrese se sestupným basem (I V/7 vi IV, Pachelbelův bas, i i/7 VI V); voice leading penalizuje jen bas, který akord nežádá.
- **Tmavý režim** a lepší rozvržení na tabletu / na šířku – ✅ motiv podle systému, světlý nebo tmavý (nastavení), tmavé barvy i v diagramech, hmatníku, kruhu a ladičce, systémové lišty podle motivu. Na tabletu čitelný sloupec, nastavení ve dvou sloupcích, větší kruh a ladička; na šířku ladička a hledání podle hmatu ve dvou sloupcích, na nízkém displeji menší okraje.

### Organizace a sdílení
- **Skupiny oblíbených** – oblíbené akordy po skupinách (např. podle písně), řazení.
- **Sdílení a export** – export diagramů jako obrázek/PDF, sdílení oblíbených nebo progrese.
- **Synchronizace** – záloha oblíbených a nastavení (iCloud / Google Drive).
- **Další jazyky** – ✅ 64 jazyků (evropské, turečtina, asijské, arabština/perština/hebrejština/urdština zprava doleva). Zbývá: kontrola strojových překladů rodilými mluvčími. Další jazyk = nový `Strings.<kód>.resx` + položka v `Localizer.Languages`.

### Pokročilé
- **Rozpoznání akordu z mikrofonu** – zahraj akord a aplikace určí jeho název.
- **Widget „akord dne“** pro Android/iOS.

## Technické

- **Optimalizace DEX pro Google Play (Android)** – Play od února 2027 vyžaduje u aplikací s více než 10 MB nekomprimovaného DEX kódu aspoň 25 % optimalizace, obfuskace i zmenšení (jinak hrozí nižší viditelnost a varování v obchodě). ✅ Release build zmenšuje DEX přes R8 (`AndroidLinkTool=r8`, soubor mapování pro stack trace): 16,0 MB → 8,9 MB, takže jsme pod hranicí (rezerva jen ~1 MB – před vydáním kontrolovat velikost DEX v .aab). Obfuskace zůstává ~0 %, protože .NET Android SDK vždy předává R8 `-dontobfuscate` ([dotnet/android#12535](https://github.com/dotnet/android/issues/12535)). Zbývá: přejít na .NET 11 SDK a kompatibilní MAUI/Android workloady (ideálně po prvním servisním vydání), přepnout na CoreCLR a zapnout obfuskaci (`AndroidR8ObfuscationMode=runtime-remapping`, `AndroidTypeMapImplementation=trimmable`); důkladně otestovat Blazor WebView a zvuk v reálném čase (ladička, metronom).

## Záměrně ne

- **Zpěvník / katalog písní** – texty písní jsou chráněné (licence OSA/SOZA nebo vydavatelů), ruční zadávání písní uživatele neláká a zpěvníkových webů a aplikací je dost. Aplikace se drží akordů, teorie a ladičky.
