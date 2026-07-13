# LZ

Linux-Prototyp für ein isometrisches 2D-Spiel in C#.

Zielbild:

- Linux-first, lauffähig mit .NET 8
- isometrische 2D-Ansicht mit Pixel-/Low-Res-Anmutung
- Diablo-ähnliche Maussteuerung: Linksklick bewegt die Figur, UI unten bleibt separat
- unteres HUD mit Lebens-/Energie-Kugeln, Aktionsleiste und Statusbereich
- prozedural gezeichnete Platzhaltergrafik, damit das Projekt sofort ohne externe Assets startet

## Starten

```bash
sudo apt install dotnet-sdk-8.0
cd src/LzGame
dotnet restore
dotnet run
```

## Steuerung

- Linksklick in die Welt: Spielerfigur läuft zum angeklickten Punkt.
- Maus über Welt: Tile-Markierung.
- `Esc`: Spiel beenden.

## Hinweis

Das Projekt kopiert keine fremden Spielassets. Es erzeugt eigene Platzhaltergrafik zur technischen Basis. Später können eigene Tiles, Figuren und UI-Grafiken ergänzt werden.
