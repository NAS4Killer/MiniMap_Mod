# MiniMap_Mod

Client-Mod für **7 Days to Die 3.2**. Mod-Autor: **NAS4Killer**.

Version: **0.0.5.2**

## Installation

1. Spiel vollständig beenden.
2. ZIP beim [neuesten Release](https://github.com/NAS4Killer/MiniMap_Mod/releases/latest) herunterladen und entpacken.
3. Den enthaltenen Ordner `MiniMap_Mod` in den `Mods`-Ordner der Spielinstallation kopieren. Bei Bedarf `Mods` anlegen.
4. Direkt unter `Mods/MiniMap_Mod/` müssen `ModInfo.xml`, `MiniMap_Mod.dll` und der Unterordner `Config` liegen. Keine zusätzliche ZIP-Unterordnerebene verwenden.
5. Spiel ohne Easy Anti-Cheat starten, Spielstand laden und mit **F5** die Optionen öffnen.

Jeder Client, der die MiniMap nutzen möchte, installiert sie selbst. Nur eine
Kopie installieren; Updates ersetzen den vorhandenen Mod-Ordner.

## Bedienung

**F5** öffnet und schließt das Optionsmenü. Änderungen werden gespeichert.

- **Minimap:** an/aus. Im Escape-Menü wird die Karte ausgeblendet.
- **Zoomfaktor:** 0, 2, 4, 6, 8 oder 10; größere Werte zoomen näher heran.
- **Spielerpfeil:** Größe 16 bis 80, unabhängig von der Kartenhelligkeit.
- **Minimap-Größe:** 128 bis 480.
- **Rahmen:** rahmenlos, Schwarz, Weiß, Rot, Grün oder Blau.
- **Mapform:** Kreis oder Quadrat.
- **Kriegsnebel:** an zeigt unbekannte Bereiche grau; aus macht diese vollständig transparent. Unbekanntes Gelände wird dadurch nicht aufgedeckt. Die Erkundung wird beim Chunkwechsel aktualisiert.
- **Helligkeit:** 10 bis 100 Prozent, nur für die Karte.
- **Kartentransparenz:** an/aus; 0 bis 50 Prozent in 5-Prozent-Schritten. 0 Prozent bedeutet deckend.
- **Randverlauf:** an/aus. Mit Kriegsnebel am äußeren Kartenrand, ohne Kriegsnebel am Übergang zwischen erkundetem und unbekanntem Gelände.
- **Karte Norden:** an hält Norden oben; aus dreht die Karte und hält den Spielerpfeil nach oben.
- **Position:** vier Richtungsknöpfe; Schrittweite 1, 10 oder 100 Pixel. **S** speichert die Position als Standard, **R** stellt diesen Standard wieder her.
- **Version:** zeigt die installierte Mod-Version.
- **Github:** öffnet die Projektseite, wenn kein Update verfügbar ist. Beim ersten Öffnen des Menüs wird automatisch geprüft. Ein neues Release erscheint als grünes **Download + Version**, nach dem Download als grünes **Installieren + Version**.
- **Release Notes:** erscheint unter dem Updatebutton, wenn ein Update verfügbar ist, und öffnet die Releasebeschreibung genau dieser Version auf GitHub.

Andere Spieler und Symbole der originalen Karte werden noch nicht angezeigt.

## Wenn keine MiniMap erscheint

Ordnerstruktur, Start ohne Easy Anti-Cheat und Spielversion 3.2 prüfen.
Mit F5 kontrollieren, dass die MiniMap eingeschaltet ist. Doppelte
Mod-Installationen vermeiden.

## Erstes Ziel

Die vorhandene Ingame-Karte wird als kleine HUD-MiniMap wiederverwendet. Angezeigt wird nur der lokale Spieler.

## Technischer Stand

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

- Andere Spieler auf der MiniMap anzeigen.
- Symbole der originalen Karte anzeigen.

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


