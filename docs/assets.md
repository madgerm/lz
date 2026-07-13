# Kostenlose Assets für LZ

Für die spätere Grafik- und Audioausstattung sollen bevorzugt Assets mit klarer, dauerhaft dokumentierter Lizenz verwendet werden.

## Bevorzugte Quelle: Kenney

Kenney stellt seine Assets auf den offiziellen Assetseiten unter CC0 bereit. Sie dürfen auch kommerziell verwendet und verändert werden; eine Namensnennung ist nicht erforderlich.

Geeignete Kategorien:

- 2D- und Pixel-Tiles für Gelände, Gebäude und Dekoration
- UI-Pakete für Schaltflächen, Rahmen und Symbole
- Audio-Pakete für Klicks, Treffer, Schritte und Umgebung
- Dungeon-, Fantasy- und RPG-Pakete als Stilreferenz

Quelle: https://kenney.nl/assets
Lizenzhinweis: https://kenney.nl/support

## Weitere Quelle: OpenGameArt

OpenGameArt bietet Tiles, Figuren, Musik und Soundeffekte unter unterschiedlichen freien Lizenzen an. Vor jeder Übernahme muss die Lizenz des konkreten Downloads geprüft werden.

Bevorzugte Lizenzen:

1. CC0 / Public Domain
2. CC BY, wenn der Urheber sauber in `CREDITS.md` genannt wird

CC BY-SA, GPL oder gemischte Pakete sollen nur nach bewusster Prüfung übernommen werden, weil daraus zusätzliche Pflichten entstehen können.

Quelle: https://opengameart.org/

## Geplante Ordnerstruktur

```text
src/LzGame/Content/
├── Audio/
├── Characters/
├── Enemies/
├── Tiles/
└── UI/
```

Zu jedem übernommenen Paket gehören:

- Originalname des Pakets
- Name des Urhebers
- Downloadquelle
- Lizenz
- Datum des Downloads
- unveränderte Lizenzdatei des Pakets

Diese Angaben werden zusätzlich in einer späteren `CREDITS.md` gesammelt.

## Aktueller Stand

Im aktuellen Prototyp sind noch keine fremden Dateien eingebettet. Die vorhandenen Tiles, Figuren und kurzen Sounds entstehen zur Laufzeit aus eigenem Programmcode. Dadurch lässt sich das Spiel bereits testen, bevor ein endgültiger, zusammenpassender Grafikstil ausgewählt wird.
