# Pulselink Wizard

Öffnen: Tools → Pulselink → Einrichtungs-Wizard.

1. Avatar aus der Szene wählen (auch deaktivierte Avatare).
2. Materialien markieren; unter „Verwendung am Avatar“ einzelne Renderer-Slots wählen.
3. Effekt, HDR-Farbe und gegebenenfalls Masken einstellen.
4. „Effekte einrichten“ drücken, Szene speichern und Play Mode neu starten.

## Effekte
lilToon: Rim, UV, Mask, Mask Spectrum, Position sowie gleichmäßiger Emissionspuls.
Poiyomi: Strength Multiply, Strength Add und Center Out.

Die Emissionsmap begrenzt die Leuchtflächen. Ohne Ersatz werden vorhandene Maps verwendet. Eine AudioLink-Steuermaske ist davon unabhängig:
- Mask: R = Verzögerung, G = Band, B = Stärke.
- Mask Spectrum: R = Pegel, G = Frequenz, B = Stärke.
Mask Spectrum nutzt mit Welt-AudioLink das echte Frequenzspektrum. Der Herzpuls hat kein Frequenzspektrum; dafür werden die R-Pegelschwellen gegen eine Pulskurve ausgewertet. G wird dabei nicht als Frequenz verwendet.
UV bietet einen Winkel, Position einen Ursprung; die räumlichen Modi haben Ausdehnung und Versatz.
Poiyomi bietet Band und Stärke-Bereich; Multiply multipliziert die Grundstärke, Add addiert auf die Grundstärke. Center Out hat Dauer und Schwelle.

## Zurücksetzen
Auf der Avatar-Seite: „Alle Heartbeat-Effekte dieses Avatars zurücksetzen“.
Auf der Material-Seite: gewünschte Ziele auswählen und „Ausgewählte Effekte zurücksetzen“.

Zurücksetzen stellt die vor der Einrichtung gespeicherten Materialzuweisungen wieder her. Beim letzten Ziel werden die vom Setup verwendete Effektsteuerung und Menüintegration entfernt. Bereits manuell anderweitig zugewiesene Materialien werden nicht überschrieben. Erzeugte Dateien bleiben als Sicherung erhalten. Strg+Z macht das Zurücksetzen rückgängig. Szene anschließend speichern. Änderungen an einer Szene müssen wie üblich separat in ein Avatar-Prefab übernommen werden.

## Verhalten
Auswählen allein ändert den Avatar nicht. Anwenden erzeugt Materialkopien, Controller und Menü in Generated; ältere Generierungen und Originaldateien bleiben erhalten. Bestehende Einrichtungen werden aktualisiert; abgewählte Ziele werden zurückgesetzt.
Im Play Mode sind Anwenden und Zurücksetzen gesperrt. Gesperrte Poiyomi-Materialien zuerst als eigene entsperrte Kopie am Avatar zuweisen.
Mehrere Poiyomi-Slots eines Renderers benötigen denselben Effekt und dieselben Stärke-Werte sowie einen gemeinsamen freien Emissionskanal.

„Welt-AudioLink“ aus = Herzpuls; an = Welt-AudioLink. Die Umschaltung verwendet Material-Swaps.
Ohne Welt-AudioLink bleibt die gewählte Grundstärke bei Poiyomi Multiply/Add erhalten. Mit Grundstärke größer null kann der Effekt deshalb ohne AudioLink sichtbar sein. Bei den übrigen Modi bleibt die zusätzliche AudioLink-Emission ohne Signal dunkel; ursprüngliche Emission ist davon unabhängig.

## Eingänge und Voraussetzungen
BPM: VRCOSC/Heartrate/Value (Expression Int 0–255, Animator Float).
OSC: /avatar/parameters/VRCOSC/Heartrate/Value.
Bluetooth Heartrate und Pulsoid über VRCOSC verwenden diesen Eingang; jeweils nur eine Quelle senden lassen. Pulsoid wurde nicht mit einem Sensor getestet.
HBE/WorldAudioLink ist ein gespeicherter synchronisierter Bool. Namen im Wizard anpassbar.

Unity 2022.3, VRChat Avatars SDK und Modular Avatar; passende lilToon-/Poiyomi-Shader.
Ausgangsversionen: lilToon 2.3.4, Poiyomi Pro 9.3.39, Modular Avatar 1.18.2.
lilToon nutzt den zweiten Emissionskanal. Bereits verfasste zweite Emissionsmaps bleiben ohne expliziten Ersatz erhalten. Andere AudioLink-Ziele werden in der generierten Kopie deaktiviert.
Poiyomi nutzt einen freien zusätzlichen Emissionskanal 1–3. Native Center-Out-Animation kann bei BPM-Änderungen in der Phase springen.
Der Setup-Component ist Editor-only; zur Laufzeit arbeiten Animator und Shader.
Alternativ kann Pulselink.prefab unter einen Avatar gezogen und im Inspector eingerichtet werden.

## Weiches Herzblinken (2026-09-08)
Neue Effektauswahl für lilToon und Poiyomi: sanfter Anstieg über 18 % des Herzzyklus, weiches Abklingen bis 85 %, anschließend eine kurze Dunkelpause. Ein Zyklus dauert 60 / BPM Sekunden; die Kurve folgt dem BPM-Tempo, nicht einzelnen gemessenen Beat-Ereignissen. Vorhandene Emissionsmaps können weiterverwendet werden. Im Welt-AudioLink-Modus steuert das gewählte Audioband die Emission; die Herzkurve läuft dort nicht weiter.

## Zusätzliche Effekte: Lauflicht, Farbpuls und Doppelherzschlag

Alle drei Optionen stehen für lilToon und Poiyomi Toon zur Verfügung; Poiyomi Pro ist nicht erforderlich.

### Lauflicht
Ein weicher Leuchtstreifen bewegt sich durch die von der Emissionsmap markierten Flächen. Unter „Richtung“ wählst du U/horizontal oder V/vertikal. Diese Richtungen beziehen sich auf die UV-Anordnung des Modells und müssen nicht der Körperrichtung entsprechen. „Streifen-Wiederholungen“ bestimmt die räumliche Wiederholung. UV-Nähte können sichtbare Unterbrechungen verursachen.
Die Phase läuft einmal pro BPM-Zyklus durch. Im Welt-AudioLink-Modus nutzt lilToon die UV-abhängige Musikverzögerung. Bei Poiyomi wird der BPM-Streifen ausgeschaltet und die markierten Flächen reagieren gemeinsam auf das Audioband.

### Farbpuls
Die markierten Leuchtflächen wechseln sanft von der „Ruhefarbe (HDR)“ zur „Pulsfarbe (HDR)“ und zurück. Der Höhepunkt liegt bei 18 % des Zyklus, bei 85 % ist die Ruhefarbe wieder erreicht. Die HDR-Werte bestimmen auch die Helligkeit: Schwarz als Ruhefarbe ergibt eine Dunkelpause, eine helle Ruhefarbe ein dauerhaftes Grundleuchten.
Im Welt-AudioLink-Modus wird die Pulsfarbe verwendet; die Musik steuert die Helligkeit. Der BPM-Farbwechsel stoppt dort.
Mehrere Farbpuls-Materialien am selben Renderer benötigen identische Farben.

### Doppelherzschlag
Zwei weiche Lichtimpulse bilden ein stilisiertes „bum-bum“: Der erste erreicht volle Stärke bei 10 % des Zyklus, der zweite 65 % Stärke bei 37 %. Ab 60 % folgt eine Dunkelpause (bei Grundstärke und Minimalwert null).
Ein BPM-Zyklus enthält zwei Lichtimpulse. Bei 60 BPM gibt es deshalb pro Sekunde ein Impulspaar. Das ist eine gestaltete Lichtkurve, keine Messung zweier Herzereignisse. Im Welt-AudioLink-Modus bestimmt das Audioband den Verlauf.

### Einrichten
Im Wizard Avatar und Materialien wählen, den gewünschten Effekt auswählen und die Beschreibung unter dem Effektmenü beachten. Farben beziehungsweise Richtung einstellen, „Effekte einrichten“ anklicken und den Play Mode neu starten. Welt-AudioLink aus = BPM-Steuerung.
Vorhandene Maps bleiben ohne ausdrücklich zugewiesenen Ersatz erhalten. Die Einstellungen können über die vorhandenen Zurücksetzen-Schaltflächen entfernt werden.

## Beschreibungen der bisherigen Effekte

### lilToon

**Rim**: Blickwinkelabhängiger Leuchtverlauf über die markierten Flächen und Kanten. Ausdehnung und Versatz verändern den Verlauf. Im Herzpuls-Modus läuft eine Pulskurve darüber; Welt-AudioLink steuert ihn mit dem gewählten Audioband.

**Gleichmäßiger Emissionspuls**: Alle markierten Leuchtflächen werden gleichzeitig kurz heller und wieder dunkel. Das Tempo folgt den BPM. Im Welt-AudioLink-Modus folgt die Helligkeit dem gewählten Audioband.

**UV**: Der Puls wandert entlang der UV-Koordinaten über die markierten Flächen. UV-Winkel, Ausdehnung und Versatz bestimmen Richtung und Verlauf. UV-Nähte können ihn unterbrechen. Welt-AudioLink nutzt denselben räumlichen Verlauf mit Musikdaten.

**Mask**: Eine zusätzliche Steuermaske legt fest, wann und wie stark einzelne Stellen leuchten. Im Herzpuls-Modus verschiebt sie den Puls pro Stelle; im Welt-AudioLink-Modus wählt sie zusätzlich das Audioband. Die Emissionsmap begrenzt weiterhin die Leuchtflächen.

**Mask Spectrum**: Im Welt-AudioLink-Modus reagieren Stellen der Steuermaske auf unterschiedliche Frequenzen und Pegel. Im Herzpuls-Modus werden die R-Pegelschwellen gegen eine Pulskurve ausgewertet; die Herzfrequenz liefert kein echtes Musikspektrum. Eine zusätzliche Steuermaske ist erforderlich.

**Position**: Der Leuchtverlauf richtet sich nach dem Abstand zu einem einstellbaren Ursprung im jeweiligen Renderer-Objekt. So entstehen nach außen oder innen laufende Lichtwellen. Ausdehnung und Versatz bestimmen den Verlauf; Welt-AudioLink verwendet dieselbe räumliche Zuordnung.

### Poiyomi

**Center Out**: Center Out: Ein blickwinkelabhängiger Leuchtverlauf bewegt sich über die markierte Oberfläche. Das Herzpuls-Tempo folgt den BPM. Im Welt-AudioLink-Modus bestimmen Audioband, Dauer und Intensitätsschwelle die Reaktion. Der Ursprung ist kein fester Punkt im Raum.

**Strength Multiply**: Die Grundstärke wird mit einem pulsierenden Multiplikator zwischen Min und Max verrechnet. Im Welt-AudioLink-Modus liefert das gewählte Audioband den Multiplikator. Bei Grundstärke null bleibt der Effekt dunkel; ohne AudioLink-Quelle bleibt im Welt-Modus die Grundstärke sichtbar.

**Strength Add**: Ein pulsierender Wert zwischen Min und Max wird zur Grundstärke addiert. Im Welt-AudioLink-Modus liefert das gewählte Audioband diesen Zusatz. Grundstärke null ermöglicht eine Dunkelpause; eine höhere Grundstärke sorgt für dauerhaftes Grundleuchten.


## Poiyomi-Build-Fix (2026-10-25)
Neu erzeugte Poiyomi-Materialien erhalten kurze Namen, damit beim Shader-Locking weniger lange Windows-Dateipfade entstehen. Der gemeldete LockMaterialsOnUpload-Fehler wurde damit behoben; der betroffene Avatar-Build wurde vom Tester bestätigt.
Update: Paket über die bestehende Installation importieren. Bereits eingerichtete Avatare im Wizard öffnen und erneut einrichten, damit kurze Materialnamen erzeugt werden. Vorhandene generierte Materialien werden durch den Import allein nicht umbenannt. Nachträgliche manuelle Änderungen an generierten Materialkopien vorher sichern. Extrem lange Projektpfade können weiterhin problematisch sein.


## Emissionsauswahl mit Vorschau (2026-10-25)
Schritt 3 zeigt vorhandene Emissionsmaps und Masken samt Kanal, Aktivierung, Farbe und Stärke. Eine Maske aus einem eingeschalteten Kanal wird bevorzugt empfohlen. Mit Diese Textur verwenden wird die Textur ausgewählt; die Pulsfarbe bleibt erhalten. Die Empfehlung wertet Materialeinstellungen aus, nicht den Bildinhalt. Mehrere Maps werden nicht kombiniert. Vorhandene Einstellungen werden beim Öffnen nicht überschrieben.
