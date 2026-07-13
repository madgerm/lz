# LZ

Linux-Prototyp für ein isometrisches 2D-Spiel in C#.

Aktueller Stand:

- Linux-first, lauffähig mit .NET 8 und MonoGame DesktopGL
- isometrische 60×60-Karte mit weich folgender Kamera
- Diablo-ähnliche Maussteuerung
- unteres HUD mit Lebens-/Energie-Kugeln und Aktionsleiste
- Pausemenü mit Sound, Vollbild und Spiel beenden
- prozedural erzeugte Klick- und Treffergeräusche ohne externe Audiodateien
- einfache Zombie-Gegner mit Verfolgung, Schaden, Lebensbalken und Respawn
- prozedural gezeichnete Platzhaltergrafik, damit das Projekt sofort ohne externe Assets startet

## Aktualisieren und starten

```bash
cd ~/linux-zombie/lz
git pull origin main
cd src/LzGame
dotnet restore
dotnet run
```

## Steuerung

- Linksklick in die Welt: Spielerfigur läuft zum angeklickten Punkt.
- Linksklick auf einen nahen Gegner: Gegner angreifen.
- Maus über Welt: Tile-Markierung.
- `Esc`: Pausemenü öffnen oder schließen.
- Menü: Pfeiltasten oder `W`/`S`, Auswahl mit `Enter` oder Mausklick.

## Kostenlose Assets

Geeignete freie Assetquellen und die Regeln für ihre spätere Einbindung stehen in [`docs/assets.md`](docs/assets.md).

## Hinweis

Der aktuelle Build kopiert keine fremden Spielassets. Grafik und Sounds werden zunächst im Code erzeugt. Dadurch bleibt der Prototyp direkt startbar, während später ausgewählte CC0-Tiles, Figuren, Animationen und Sounds sauber eingebunden werden können.
