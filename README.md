# Pulselink

Herzpuls- und AudioLink-Effekte für VRChat-Avatare mit **lilToon oder Poiyomi Toon**. Enthält einen Unity-Wizard zur Auswahl von Avatar, Materialien und Effekten sowie zum Zurücksetzen der Einrichtung. Poiyomi Pro ist nicht erforderlich.

**Testversion:** Die Einrichtung wurde in Unity geprüft; weitere sichtbare Tests in VRChat sind erwünscht.

## Download und Einstieg

1. [Pulselink-Testpaket herunterladen](Downloads/Pulselink_2026-10-25_EmissionWizard.unitypackage) (auf GitHub bei Bedarf „Download raw file“ wählen).
2. In ein Unity-Avatarprojekt mit VRChat Avatars SDK, Modular Avatar und passendem Shader importieren.
3. **Tools → Pulselink → Einrichtungs-Wizard** öffnen.
4. Avatar aus der Szene und Materialien wählen, Effekt einstellen und **Effekte einrichten** anklicken.
5. Szene speichern und Play Mode zum Test neu starten.

Die vollständige [Bedienungsanleitung mit allen Effekten](docs/ANLEITUNG_DE.md) erklärt Herzpuls, Welt-AudioLink, Masken und Zurücksetzen.

## Effekte

| Beide Shader | Zusätzlich lilToon | Zusätzlich Poiyomi |
|---|---|---|
| Weiches Herzblinken | Rim | Center Out |
| Doppelherzschlag | UV | Strength Multiply |
| Farbpuls | Position | Strength Add |
| Lauflicht | Mask | |
| | Mask Spectrum | |
| | Gleichmäßiger Emissionspuls | |

## Signal

- **Welt-AudioLink aus:** Herzfrequenz über `VRCOSC/Heartrate/Value`.
- **Welt-AudioLink an:** Musik aus einer AudioLink-Quelle.
- Standard-Toggle: `HBE/WorldAudioLink`.
- VRCOSC Bluetooth Heartrate und Pulsoid über VRCOSC können denselben BPM-Eingang nutzen. Jeweils nur eine Quelle senden lassen.

Der Rhythmus wird aus BPM berechnet, nicht durch einzelne gemessene Beat-Ereignisse ausgelöst. Pulsoid wurde nicht mit einem Pulsoid-Sensor getestet.

## Voraussetzungen und Teststand

Unity 2022.3; VRChat Avatars SDK; Modular Avatar; lilToon oder Poiyomi Toon. Geprüfte Ausgangsversionen: Unity 2022.3.22f1, SDK 3.10.4, Modular Avatar 1.18.2, lilToon 2.3.4, Poiyomi Toon 9.2.69 und Pro 9.3.39.

Geprüft wurden unter anderem Material-/Animator-Erzeugung, Einstellungen der Betriebsarten, Kurven und Wiederherstellung der Materialien. Ein separater Importtest der früheren Paketversion bestand mit lilToon. Die spätere erweiterte Version wurde im Entwicklungsprojekt geprüft; ein vollständiger VRChat-Test aller Kombinationen steht aus.

## Repository-Inhalt

- `Assets/Pulselink`: Quellcode, Editor-Wizard, Prefab, Hinweise und Unity-Metadaten.
- `Downloads`: exportiertes Unitypackage für Tester.
- `docs`: vollständige deutsche Anleitung und Testhinweise.

Dies ist ein Unity-Asset, kein vollständiges Unity-Projekt. Avatar-Dateien, Shader, SDK und Modular Avatar werden separat benötigt. Generierte Testmaterialien und lokale Backups sind nicht Bestandteil dieses Repositories.

## Feedback

Bitte bei Fehlern Shader und Version, Effekt, Herzpuls-/AudioLink-Modus, kurze Reproduktionsschritte und gegebenenfalls einen Screenshot angeben. Keine Sensor-Zugangsdaten oder Tokens posten.

## Poiyomi-Build-Fix vom 25.10.2026

Kurze Namen für erzeugte Poiyomi-Materialien beheben den gemeldeten Windows-Pfadfehler bei LockMaterialsOnUpload. Der betroffene Avatar-Build wurde erfolgreich erneut getestet.

Update: Paket importieren und bestehende Einrichtung im Wizard erneut ausführen. Der Import allein benennt alte Materialkopien nicht um. Manuelle Änderungen an generierten Kopien vorher sichern. Extrem lange Projektpfade können weiterhin problematisch sein.

## Emissionsauswahl mit Vorschau vom 25.10.2026

Der Wizard zeigt zugewiesene Emissionsmaps und Masken mit Vorschau, Kanal, Aktivierung, Farbe und Stärke. Eine passende Textur wird anhand der Materialeinstellungen empfohlen und kann mit Diese Textur verwenden ausgewählt werden. Pulsfarbe und bestehende Einrichtungswerte bleiben beim Öffnen erhalten. Enthält auch den Poiyomi-Build-Fix.

Update: Unitypackage über die vorhandene Installation importieren, Kompilierung abwarten und den Wizard neu öffnen. Auswahl und Übernahme wurden mit lilToon und Poiyomi geprüft.
