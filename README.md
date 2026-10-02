# MiniMap_Mod

Client-Mod fuer 7 Days to Die 3.2.

Version: **0.0.4.2**

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

Im F5-Menü zeigt die Mod ihre Version. UPDATE SUCHEN öffnet das neueste
GitHub-Release im Browser. ZIP herunterladen, Spiel beenden und den enthaltenen
Ordner MiniMap_Mod im Mods-Ordner ersetzen. Die persönlichen Einstellungen
bleiben in der separaten MiniMap_Mod.cfg erhalten.


