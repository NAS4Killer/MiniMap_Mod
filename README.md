# MiniMap_Mod

Client-Mod fuer 7 Days to Die 3.2.

Version: **0.0.2.0**

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


