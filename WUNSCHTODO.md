# Wunsch-ToDo

Noch nicht umgesetzt:

- Optional: aktivierbares Erz-Radar für Erz in der Nähe; Machbarkeit noch prüfen.
- Permanente Second-MiniMap.
- Einzelne Markertypen abschaltbar machen.
- Entfernte Marker am Kartenrand anzeigen.

## Umgesetzt

- Koordinaten einschließlich Höhe.
- Himmelsrichtungen N, W, O und S.
- Wählbares F5-Verhalten (0.0.6.1).
- Spieler und Originalkarten-Symbole (seit 0.0.6.2).
- Performance-Optimierung einschließlich Kachelupdates (0.1.0.0; vom Nutzer vorerst als erledigt bewertet).

## Später: Standalone-MiniMap

Ausgangspunkt: Version 0.0.5.3, Zweig `standalone-minimap`.

- Ziel: ausschließlich auf dem Client installierbar, auch beim Spielen auf dedizierten Servern.
- MiniMap-Fenster und Optionsmenü lokal einfügen, nachdem die Server-UI empfangen wurde.
- Vorhandene Server-Oberfläche erhalten; keine doppelten Fenster beim erneuten Verbinden.
- Lokales Spiel und Serverwechsel weiter unterstützen.
- Machbarkeit, Ladezeitpunkt und Verträglichkeit mit anderen UI-Mods zuerst prüfen.
- Noch keine Umsetzung; aktuelle Bauweise verwendet auf dem Server ergänzende UI-XML-Dateien.
