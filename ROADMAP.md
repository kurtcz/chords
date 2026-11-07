# Možné další funkce

## Doporučené pořadí

1. Ostatní funkce podle zájmu uživatelů.

## Navržené

- **Generátor pro leváky (Left-handed mode)** – ✅ svislé/vodorovné diagramy, vodorovné lze zrcadlit pro leváky. Hmatník se stupnicemi a tóny akordu se vodorovně zobrazuje i zrcadlí pro leváky.
- **Custom ladění** – ✅ Drop D, DADGAD, Half-step down (Eb), Open G/D a vlastní ladění; generátor hmatů i přehrávání zvuku podle zvoleného ladění.
- **Přepínač nástrojů** – ✅ kytara, ukulele, mandolína, baskytara, pětistrunné banjo v Open G (krátká 5. struna od 5. pražce, barré i u ostatních nástrojů; oblíbené zvlášť pro každý nástroj). Zbývá: tenorové banjo, vlastní nahrávky zvuku pro jednotlivé nástroje. Housle záměrně ne – nemají pražce.
- **Hledání akordu podle hmatu** – ✅ uživatel naklikne prsty na hmatníku zvoleného nástroje a aplikace určí název akordu (včetně obratů jako C/E a neúplných akordů).
- **Hledač akordů do tóniny (Key & Scale Finder)** – ✅ pro zvolenou tóninu a stupnici (dur, mollové, církevní módy) zobrazí diatonické akordy I–vii° nebo septakordy s přehráním; naopak z několika zadaných akordů určí nejpravděpodobnější tóniny (v mollu i s harmonickými akordy, upřednostní začátek/konec na tónice). Sdílí tóninu s kvintovým kruhem (kruh ukazuje příbuznou durovou tóninu, posun v kruhu zachová stupnici) a obě stránky na sebe odkazují. Stupnice se zobrazuje i na hmatníku a stránka doporučí nejsnadnější pozice kapodastru.
- **Generátor akordových progresí (Smart Progression Generator)** – ✅ typické progrese podle stylu (pop, rock, jazz, blues, klasika, flamenco) v libovolné tónině, volitelně se septakordy, náhodné generování podle funkční harmonie, přehrání celé progrese a výběr hmatu ke každému akordu. Zbývá: voice leading, tempo/metronom, ukládání vlastních progresí.

## Další nápady

### Hraní a cvičení
- **Kapodastr** – ✅ pozice kapodastru v nastavení (pro každý nástroj), diagramy kreslí kapodastr a počítají pražce od něj, u akordu a v progresích je vidět „tvar“ k hmatání; ladička ladí bez kapodastru.
- **Ladička** – ✅ chromatická ladička přes mikrofon (detekce YIN v Chords.Core, ověřená na nahraných strunách): ručička ±50 centů, cílová struna zvoleného nástroje i vlastního ladění, automatický výběr nebo zamčení struny s přehráním referenčního tónu, kalibrace A4 430–450 Hz. Zbývá: ověřit na zařízeních (Android bez zpracování zvuku, přepínání audio session na iOS), případně režim pro neobvyklé ladění bez strun.
- **Metronom** – jednoduchý metronom, případně propojený s přehráváním progresí.
- **Volba způsobu přehrání** – úder dolů/nahoru, arpeggio, tempo; kvalitnější vzorky zvuku.
- **Trénink sluchu** – kvíz: poznej typ akordu (dur/moll/septakord…) nebo interval podle poslechu.
- **Kvíz na hmatníku** – poznej akord z diagramu / najdi tón na hmatníku.

### Zobrazení
- **Stupnice a arpeggia na hmatníku** – ✅ celá stupnice (stránka tóniny) a tóny akordu (stránka akordu) přes hmatník, popisky intervaly nebo názvy tónů, tónika zvýrazněná, ťuknutí přehraje tón; respektuje kapodastr, levorukost a krátkou strunu banja.
- **Prstoklad v diagramech** – ✅ čísla prstů (1–4) v diagramech, vypočtená nejlevnějším rozložením prstů (barré, natažení). Zbývá: možnost zobrazit v diagramu místo názvů tónů intervaly.
- **Hodnocení obtížnosti hmatu** – ✅ skóre podle počtu prstů, rozpětí, natažení malíčku, barré a tlumených strun (s pravidly každého nástroje), úrovně snadný/střední/těžký; řazení od nejsnadnějšího, přepínač „Jen snadné hmaty“ (u akordu bez snadného hmatu ukáže nejsnadnější dostupné). Obtížnost se zobrazuje přímo pod diagramem.
- **Plynulé přechody (voice leading)** – v progresi vybírat hmaty, které jsou na hmatníku blízko sebe.
- **Inverze a slash akordy** – např. C/G, D/F#; hledání hmatů s daným basovým tónem.
- **Tmavý režim** a lepší rozvržení na tabletu / na šířku – ✅ motiv podle systému, světlý nebo tmavý (nastavení), tmavé barvy i v diagramech, hmatníku, kruhu a ladičce, systémové lišty podle motivu. Na tabletu čitelný sloupec, nastavení ve dvou sloupcích, větší kruh a ladička; na šířku ladička a hledání podle hmatu ve dvou sloupcích, na nízkém displeji menší okraje.

### Organizace a sdílení
- **Skupiny oblíbených** – oblíbené akordy po skupinách (např. podle písně), řazení.
- **Sdílení a export** – export diagramů jako obrázek/PDF, sdílení oblíbených nebo progrese.
- **Synchronizace** – záloha oblíbených a nastavení (iCloud / Google Drive).
- **Další jazyky** – ✅ 64 jazyků (evropské, turečtina, asijské, arabština/perština/hebrejština/urdština zprava doleva). Zbývá: kontrola strojových překladů rodilými mluvčími. Další jazyk = nový `Strings.<kód>.resx` + položka v `Localizer.Languages`.

### Pokročilé
- **Rozpoznání akordu z mikrofonu** – zahraj akord a aplikace určí jeho název.
- **Widget „akord dne“** pro Android/iOS.

## Záměrně ne

- **Zpěvník / katalog písní** – texty písní jsou chráněné (licence OSA/SOZA nebo vydavatelů), ruční zadávání písní uživatele neláká a zpěvníkových webů a aplikací je dost. Aplikace se drží akordů, teorie a ladičky.
