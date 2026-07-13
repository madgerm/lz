# LZ – technische Zielrichtung

## Grundidee

LZ wird als Linux-first C#-Spiel aufgebaut. Der aktuelle Stand ist ein lauffähiger technischer Prototyp mit MonoGame DesktopGL.

## Grafikrichtung

- isometrische 2D-Welt
- düstere Wald-/Lager-/Holzzaun-Optik
- eigene Platzhaltergrafiken, prozedural im Code erzeugt
- später austauschbar durch eigene Pixel-Art-Spritesheets

## Steuerung

- Linksklick in die Welt setzt ein Bewegungsziel
- Spielfigur läuft weich zum Ziel
- Maus-Hover markiert das Tile
- UI-Klicks sollen später vom Weltklick getrennt verarbeitet werden

## Menü/HUD-Richtung

- fester unterer Bedienbereich
- Lebenskugel links
- Energiekugel rechts
- mittlere Aktionsleiste mit Slots
- dunkler Rahmenlook statt moderner Flat-UI

## Nächste sinnvolle Schritte

1. Assetsystem für eigene Spritesheets einbauen.
2. Animationen für Spieler, NPCs und Umgebung ergänzen.
3. Kollision und Wegfindung statt direkter Zielbewegung einbauen.
4. Inventar, Charakterfenster und Hauptmenü ergänzen.
5. Karteneditor oder JSON/Tiled-Import vorbereiten.
