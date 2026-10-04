# MiniMap_Mod

Client-Mod für **7 Days to Die 3.2**. Mod-Autor: **NAS4Killer**.

Version: **0.1.0.2**

## Installation

1. Spiel vollständig beenden.
2. ZIP beim [neuesten Release](https://github.com/NAS4Killer/MiniMap_Mod/releases/latest) herunterladen und entpacken.
3. Den enthaltenen Ordner `MiniMap_Mod` in den `Mods`-Ordner der Spielinstallation kopieren. Bei Bedarf `Mods` anlegen.
4. Direkt unter `Mods/MiniMap_Mod/` müssen `ModInfo.xml`, `MiniMap_Mod.dll` und der Unterordner `Config` liegen. Keine zusätzliche ZIP-Unterordnerebene verwenden.
5. Spiel ohne Easy Anti-Cheat starten, Spielstand laden und mit **F5** die Optionen öffnen.

Jeder Client, der die MiniMap nutzen möchte, installiert sie selbst. Nur eine
Kopie installieren; Updates ersetzen den vorhandenen Mod-Ordner.

## Bedienung

**F5-Verhalten** ist im Menü wählbar: „Direkt ins Menü“ öffnet und schließt die Optionen mit F5 (Standard). „Ein/Aus + Doppel-F5“ blendet mit einmal F5 die Minimap ein/aus und öffnet mit zweimal F5 innerhalb von 0,3 Sekunden die Optionen. Der einzelne Tastendruck wird deshalb erst nach dieser kurzen Wartezeit ausgeführt. Bei geöffneten Optionen schließt F5 das Menü. **ESC** schließt es ebenfalls. Änderungen werden gespeichert.

- **Minimap:** an/aus. Im Escape-Menü wird die Karte ausgeblendet.
- **Zoomfaktor:** 0, 2, 4, 6, 8 oder 10; größere Werte zoomen näher heran.
- **Spielerpfeil:** Größe 16 bis 80, unabhängig von der Kartenhelligkeit.
- **Minimap-Größe:** 128 bis 480.
- **Rahmen:** fünf direkte Farbbuttons für Schwarz, Weiß, Grün, Rot und Rahmenlos. Ein blauer Buttonrahmen markiert die Auswahl. Der Rahmenlos-Button hat einen neutral grauen Inhalt.
- **Mapform:** Kreis oder Quadrat.
- **Kriegsnebel:** an zeigt unbekannte Bereiche grau; aus macht diese vollständig transparent. Unbekanntes Gelände wird dadurch nicht aufgedeckt. Die Erkundung wird beim Chunkwechsel aktualisiert.
- **Helligkeit:** 10 bis 100 Prozent, nur für die Karte.
- **Kartentransparenz:** an/aus; 1 bis 10 Prozent in 1-Prozent-Schritten. Aus bedeutet vollständig deckend. Alte Prozentwerte werden beim Laden auf den neuen Bereich begrenzt.
- **Randverlauf:** an/aus. Mit Kriegsnebel am äußeren Kartenrand, ohne Kriegsnebel am Übergang zwischen erkundetem und unbekanntem Gelände.
- **Karte Norden:** an hält Norden oben; aus dreht die Karte und hält den Spielerpfeil nach oben.
- **Koordinaten:** an/aus; eine Zeile über oder unter der Karte. X und Y zeigen die Kartenposition, Z die Höhe in Metern.
- **N–S–O–W:** an/aus; Himmelsrichtungen am Kartenrand. Bei drehender Karte folgen ihre Positionen der Kartenausrichtung.
- **Position:** vier Richtungsknöpfe mit ▲ ▼ ◄ ►; Schrittweite 1, 10 oder 100 Pixel. Ein blauer Buttonrahmen markiert die Schrittweite. **S** speichert die Position als Standard, **R** stellt diesen Standard wieder her. Beim Überfahren von R oder S erscheint ein Erklärungstext unter GitHub.
- **Version:** zeigt die installierte Mod-Version.
- **Github:** öffnet die Projektseite, wenn kein Update verfügbar ist. Beim ersten Öffnen des Menüs wird automatisch geprüft. Ein neues Release erscheint als grünes **Download + Version**, nach dem Download als grünes **Installieren + Version**.
- **Release Notes:** erscheint unter dem Updatebutton, wenn ein Update verfügbar ist, und öffnet die Releasebeschreibung genau dieser Version auf GitHub.

Die Minimap zeigt die vom Spiel für die Originalkarte bereitgestellten Marker, einschließlich anderer sichtbarer Spieler, Wegpunkte und weiterer Kartensymbole. Die Sichtbarkeitsregeln der Originalkarte bleiben erhalten: versteckte oder vom Spiel nicht bereitgestellte Spieler werden nicht aufgedeckt. Marker werden laufend aktualisiert, folgen Zoom und Kartendrehung und erscheinen nur innerhalb der Minimap. Namen werden in dieser ersten Umsetzung nicht eingeblendet.

Die sichtbare Kategorie **Sonstiges** enthält Karte Norden, Himmelsrichtungen,
Randverlauf und Kriegsnebel. Alle Bedienzeilen haben dieselbe Gesamtbreite;
die Positionstasten und die Schrittweitentasten sind innerhalb ihrer Zeile gleich breit.

## Wenn keine MiniMap erscheint

Ordnerstruktur, Start ohne Easy Anti-Cheat und Spielversion 3.2 prüfen.
Mit F5 kontrollieren, dass die MiniMap eingeschaltet ist. Doppelte
Mod-Installationen vermeiden.

## Erstes Ziel

Die vorhandene Ingame-Karte wird als kleine HUD-MiniMap wiederverwendet. Angezeigt wird nur der lokale Spieler.

## Technischer Stand

- Die Erkundungsmaske bleibt unabhängig von der Kriegsnebel-Option erhalten. Kriegsnebel schaltet nur zwischen grauer und transparenter Darstellung unbekannter Bereiche um; der Randverlauf bleibt eine eigene Option.

- Teilaktualisierung: bei normalen Chunkwechseln werden nur geänderte 64×64-Pixel-Kacheln gelesen, bearbeitet und per GPU-Kopie übertragen. Der Gelände-Randverlauf liest 12 Pixel Umgebung mit, um Kachelnähte zu vermeiden. Ohne GPU-Kopierunterstützung bleibt eine vollständige Übertragung als Rückfall erhalten. Laden, Einstellungswechsel und vollständige Neuzeichnungen nutzen weiterhin die verteilte Gesamtverarbeitung.

- Performance: Farbverarbeitung nutzt vorberechnete Tabellen. Farben und Gelände-Randverlauf werden mit einem Arbeits- und Zeitbudget auf mehrere Frames verteilt; die alte vollständige Darstellung bleibt bis zum Abschluss erhalten. Chunkwechsel aktualisieren neue Kartenstreifen und die direkte Umgebung statt automatisch die gesamte Karte. Bei Minimap AUS pausieren Kartenberechnung und Marker. Die temporären Laufzeitmessungen und PERF-Logmeldungen sind entfernt.

- `XUiC_MapArea` ist erweiterbar und besitzt bereits Kartentextur, lokalen Spieler und Zentrierungslogik.
- Fuer die erste Version ist keine Server-Mod erforderlich.
- Ein erster HUD-Prototyp verwendet `XUiC_MapArea`, zentriert auf den lokalen Spieler.

## Bauen

```powershell
dotnet build .\src\MiniMapMod.csproj -p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die"
```

Die erzeugte DLL wird nach `bin/Debug/netstandard2.1/` geschrieben.

## Versionierung

Das Projekt verwendet immer vier Stellen nach dem Schema `A.B.C.D`. Die Version steht im Code und im Commit-Betreff.
Bei jeder weiteren Aenderung wird mindestens die letzte Stelle `D` erhoeht.

## Zukunfts-ToDo

- Einzelne Markertypen abschaltbar machen.
- Entfernte Marker am Kartenrand anzeigen.
- Permanente zweite Minimap.
- Standalone-Minimap ohne Serverinstallation.
- Zuschaltbares Erz-Radar.

## Updates

Beim ersten Öffnen des Optionsmenüs wird das neueste stabile GitHub-Release abgefragt, ohne
Anmeldung und ohne Zugangsdaten. Ein neueres Release erscheint mit seiner
Versionsnummer. Gleiche oder ältere Releases werden nicht installiert.

**Download + Version** lädt das Paket herunter und prüft Größe, SHA-256-Prüfsumme,
Ordnerstruktur, enthaltene Dateien, Mod-Autor und Version. Anschließend
**Installieren + Version** anklicken. Der Button zeigt wieder **Github**,
darunter rot **Spiel neu starten**. Dann das Spiel vollständig beenden.
Ein unsichtbarer Windows-Helfer wartet auf das Spielende, sichert die bisherigen
Dateien und installiert anschließend das Update. Das Spiel wird weder beendet
noch neu gestartet. Bei Schreibfehlern versucht der Helfer, die alten Dateien
wiederherzustellen. Administratorrechte werden nicht automatisch angefordert.

Sicherungen und das Ergebnis `result.txt` liegen im jeweiligen Auftragsordner
unter `%APPDATA%/7DaysToDie/MiniMap_Updates/`. Bei einem Installationsfehler dort
nachsehen; alternativ die ZIP vom neuesten Release manuell installieren.
Die persönlichen Einstellungen in `%APPDATA%/7DaysToDie/MiniMap_Mod.cfg`
bleiben erhalten. Die automatische Installation ist für Windows vorgesehen.


