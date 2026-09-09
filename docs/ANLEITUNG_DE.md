# Pulselink – Bedienung und Effektübersicht

Pulselink verbindet die Leuchtflächen eines VRChat-Avatars mit deiner Herzfrequenz oder mit Musik aus einer AudioLink-Welt. Im Wizard wählst du Avatar, Materialien und Effekt. Unterstützt werden lilToon und Poiyomi Toon; Pro ist nicht erforderlich.

## Betriebsarten

**Herzpuls:** „Welt-AudioLink“ ist aus. VRCOSC liefert BPM. Ein Effektzyklus dauert 60 / BPM Sekunden: bei 60 BPM eine Sekunde, bei 90 BPM ungefähr 0,67 Sekunden, bei 120 BPM eine halbe Sekunde. Die Animation folgt dem BPM-Tempo und ist nicht exakt mit jedem einzelnen gemessenen Herzschlag synchronisiert.

**Welt-AudioLink:** Der Toggle ist an. Eine AudioLink-Quelle steuert die Helligkeit oder den räumlichen Verlauf mit Musikdaten. Der Toggle ist kein Ein-/Aus-Schalter für den gesamten Effekt. Die frei laufenden Shader-Animationen sind keine zusätzliche dritte Pulselink-Betriebsart.

## Installation

Benötigt werden Unity 2022.3, VRChat Avatars SDK, Modular Avatar und ein passender lilToon-/Poiyomi-Shader. Importiere das Unitypackage in dein Avatarprojekt. Für Herzpuls brauchst du zusätzlich eine Sensorverbindung über VRCOSC Bluetooth Heartrate oder Pulsoid im VRCOSC-Modul. Für Musikbetrieb wird eine AudioLink-Welt oder eine Testquelle in Unity benötigt.

## Einrichten

1. Play Mode beenden. **Tools → Pulselink → Einrichtungs-Wizard** öffnen.
2. Avatar aus der Szene wählen oder aus der Hierarchie in das Auswahlfeld ziehen. Deaktivierte Avatare werden ebenfalls angezeigt. Ein Prefab-Asset muss zuerst in der Szene stehen.
3. Materialien markieren. Unter **Verwendung am Avatar** kannst du einzelne Renderer und Material-Slots wählen. Ein Material kann mehrfach am Avatar verwendet werden.
4. Pro Material einen Effekt wählen. Beschreibung und passende Optionen erscheinen darunter.
5. **Effekte einrichten** drücken, Szene speichern und Play Mode neu starten.

Gesperrte Poiyomi-Materialien benötigen zuerst eine entsperrte Kopie am Avatar. Pulselink erzeugt eigene Materialkopien, Effektsteuerung und Menü. Auswählen allein verändert den Avatar noch nicht.

## Gemeinsame Einstellungen

| Einstellung | Aufgabe |
|---|---|
| Emissionsmap / Maske | Legt fest, welche Flächen leuchten dürfen. |
| Pulsfarbe (HDR) | Farbe und Helligkeit des zusätzlichen Leuchtens. |
| AudioLink-Band | Musikbereich, beispielsweise Bass oder Treble. |
| Ruhefarbe (HDR) | Beim Farbpuls die Farbe zwischen den Pulsen. |

Ohne Ersatzmaske werden geeignete vorhandene Emissionsmaps verwendet. Fehlt eine geeignete Map, fordert der Wizard eine Auswahl an. Eine AudioLink-Steuermaske für Mask/Mask Spectrum steuert das Verhalten innerhalb der Leuchtflächen und ist von der Emissionsmap unabhängig.

## Effekte für beide Shader

### Weiches Herzblinken

Alle markierten Flächen leuchten sanft gemeinsam auf, blenden langsam aus und pausieren kurz. Im Herzpuls-Modus dauert der Anstieg 18 % des Zyklus; bis 85 % klingt der Puls ab. Im Welt-AudioLink-Modus folgt die Helligkeit dem Audioband; die feste Herzkurve stoppt. Bei Poiyomi können Grundstärke und Minimalwert ein Grundleuchten erhalten.

### Doppelherzschlag

Zwei weiche Impulse bilden ein stilisiertes „bum-bum“. Der erste erreicht volle Stärke bei 10 % des Zyklus, der zweite 65 % des Helligkeitsanstiegs bei 37 %. Ab 60 % folgt die Pause. Bei 60 BPM gibt es ein Impulspaar pro Sekunde. Das ist eine gestaltete Lichtkurve, keine Messung zweier Herzereignisse. Im Welt-AudioLink-Modus folgt die Helligkeit dem Audioband. Eine Dunkelpause setzt Grundstärke und Minimalwert null voraus.

### Farbpuls

Die Flächen wechseln sanft von der Ruhefarbe zur Pulsfarbe und zurück. Im Herzpuls-Modus wird die Pulsfarbe nach 18 % und die Ruhefarbe nach 85 % des Zyklus erreicht. Schwarz als Ruhefarbe erzeugt eine Dunkelpause; eine helle Ruhefarbe erhält Grundleuchten. HDR-Werte bestimmen ebenfalls die Helligkeit. Im Welt-AudioLink-Modus bleibt die Pulsfarbe und Musik steuert ihre Helligkeit. Farbpuls-Slots desselben Renderers benötigen identische Farben.

### Lauflicht

Ein weicher Streifen wandert durch die markierten Flächen. Eine Phasenwiederholung pro BPM-Zyklus. U/horizontal und V/vertikal beziehen sich auf die UV-Anordnung, nicht zwingend auf Körperrichtungen. Streifen-Wiederholungen verändert die räumliche Wiederholung; UV-Nähte können Unterbrechungen erzeugen.

Im Welt-AudioLink-Modus nutzt lilToon UV-abhängig verzögerte Musikdaten. Poiyomi schaltet den BPM-Streifen ab und lässt die markierten Flächen gemeinsam zur Musik leuchten.

## Weitere lilToon-Effekte

### Rim

Blickwinkelabhängiger Leuchtverlauf über Flächen und Kanten. Bei Herzpuls läuft die Pulskurve darüber, bei Welt-AudioLink das gewählte Musikband. Ausdehnung und Versatz verändern die Verteilung.

### UV

Der Leuchtverlauf wandert entlang der UV-Anordnung. Im Herzpuls-Modus im BPM-Tempo, im Musikbetrieb mit UV-abhängiger Verzögerung der Musikdaten. UV-Winkel, Ausdehnung und Versatz bestimmen Richtung und Verteilung. UV-Nähte können den Verlauf unterbrechen.

### Position

Der Abstand zu einem einstellbaren Ursprung bestimmt den Verlauf. Dadurch können nach außen oder innen laufende Wellen entstehen. Bei Herzpuls läuft die Pulskurve über diese Verteilung, bei AudioLink entsprechend verzögerte Musikdaten. Der Ursprung bezieht sich auf das jeweilige Renderer-Objekt, nicht automatisch auf die Mitte des Avatars.

### Mask

Eine zusätzliche Steuermaske legt Verhalten pro Stelle fest: Rot = Verzögerung, Grün = Audioband, Blau = Stärke. Bei Herzpuls verschiebt Rot den Puls und Blau verändert seine Stärke; die Bandwahl erzeugt keine unterschiedlichen Musiksignale. Im Musikbetrieb wählt Grün zusätzlich das Audioband. Die Emissionsmap begrenzt weiter die Leuchtflächen.

### Mask Spectrum

Rot = Pegel/Schwelle, Grün = Frequenz, Blau = Stärke. Bei Welt-AudioLink reagieren Bereiche auf tatsächliche Frequenzen und Pegel. Bei Herzpuls werden die roten Schwellen gegen eine Pulskurve ausgewertet; BPM liefert kein echtes Frequenzspektrum. Verstärkung und Breite/Schwelle passen die Reaktion an.

### Gleichmäßiger Emissionspuls

Alle markierten Flächen leuchten gemeinsam kurz auf. Ein kurzer Puls pro BPM-Zyklus; zeitlich stärker begrenzt als weiches Herzblinken. Im Musikbetrieb folgen die Flächen gemeinsam dem Audioband.

## Weitere Poiyomi-Effekte

### Strength Multiply

Die Grundstärke wird mit einem Faktor zwischen Min und Max multipliziert. Bei Herzpuls durch die Pulskurve, im Welt-AudioLink-Modus durch das Audioband. Beispiel: Grundstärke 2 und Faktor 0–1 ergeben Emission 0–2. Grundstärke null bleibt dunkel. Ohne AudioLink-Quelle kann im Welt-Modus die Grundstärke sichtbar bleiben.

### Strength Add

Ein Wert zwischen Min und Max wird zur Grundstärke addiert. Bei Herzpuls steigt der Zusatz an und klingt ab; bei AudioLink steuert das Audioband den Zusatz. Beispiel: Grundstärke 1 und Zusatz 0–2 ergeben Emission 1–3. Für eine Dunkelpause müssen Grundstärke und Minimalwert null sein.

### Center Out

Ein fließender, blickwinkelabhängiger Verlauf über der Oberfläche. Im Herzpuls-Modus folgt die native Bewegung dem BPM-Tempo; bei BPM-Änderungen kann ihre Phase springen. Im Welt-AudioLink-Modus beeinflussen Band, Dauer und Intensitätsschwelle die Reaktion. Kein fester Mittelpunkt im Raum; für einen räumlichen Ursprung ist bei lilToon Position vorgesehen.

## Testen

Für Herzpuls Sensor verbinden, passendes VRCOSC-Modul aktivieren und prüfen, ob es BPM empfängt. Play Mode mit Gesture Manager starten, Welt-AudioLink ausschalten.

Standardparameter: `VRCOSC/Heartrate/Value`. OSC-Adresse: `/avatar/parameters/VRCOSC/Heartrate/Value`. Bluetooth Heartrate und Pulsoid über VRCOSC können denselben Eingang nutzen; nur eine Quelle gleichzeitig senden lassen. Unter Signal und Menü können Namen angepasst werden. Standard-Toggle: `HBE/WorldAudioLink`.

Für Musik eine AudioLink-Testquelle oder Welt verwenden, Welt-AudioLink einschalten und Musik abspielen. Gesture Manager allein liefert keine Musikdaten.

## Änderungen und Zurücksetzen

Wizard erneut öffnen und denselben Avatar wählen. Die bestehende Einrichtung wird geladen. Einstellungen ändern, erneut einrichten, speichern und Play Mode neu starten.

- In Schritt 1: **Alle Heartbeat-Effekte dieses Avatars zurücksetzen**.
- In Schritt 2: betreffende Materialien/Slots markieren und **Ausgewählte Effekte zurücksetzen**.

Zurücksetzen stellt die vor der Einrichtung gespeicherten Materialzuweisungen wieder her. Beim letzten Ziel wird auch die Pulselink-Steuerung entfernt. Andere Avataränderungen werden nicht zurückgesetzt. Erzeugte Dateien bleiben als Sicherung; Strg+Z macht das Zurücksetzen rückgängig. Szene speichern und Änderungen gegebenenfalls zusätzlich in das Avatar-Prefab übernehmen.

## Häufige Probleme

- Dauerhaftes Leuchten: ursprüngliche Grundemission, Ruhefarbe, Grundstärke und Minimalwerte prüfen.
- Falsche Flächen: Material-Slots und Emissionsmap kontrollieren; bei Lauflicht UV-Anordnung beachten.
- Material nicht auswählbar: Shader-Unterstützung und Poiyomi-Sperre prüfen.
- Unterschiedliche Effekte am selben Renderer: animierte Eigenschaften können mehrere Slots betreffen. Der Wizard meldet erkannte Konflikte; Poiyomi-Slots benötigen passende gemeinsame Einstellungen und einen freien Emissionskanal.
- Pulsoid: gemeinsamer VRCOSC-Eingang vorbereitet, noch kein Test mit Pulsoid-Sensor.
