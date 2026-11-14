# F-001 – Kernmechanik (reine Logik)

Status: umgesetzt auf Branch feature/f-001-kernmechanik, wartet auf Abnahme (Spec freigegeben 2026-11-15)
Empfohlenes Modell: Sonnet (Umsetzung), Opus (diese Spec)

## Ziel
Die Match-Simulation als reine C#-Bibliothek in `src/core/`: Bewegung, Zielen, Schießen, Treffer, Tod, Respawn und Punktestand für zwei Teams. Ohne Grafik, ohne Netzwerk, ohne Fog of War. Läuft in Tests mit simulierten Spielern.

## Grundprinzipien
- **Feste Tickrate:** 30 Ticks pro Sekunde (`dt = 1/30 s`). Alle Zeiten werden in Ticks gerechnet.
- **Ein Einstieg:** `Simulation.Step(inputs)` rechnet genau einen Tick und liefert Ereignisse (Treffer, Kill, Respawn, Matchende).
- **Eingabe pro Spieler und Tick:** `MoveX`, `MoveY` (−1..1), `AimAngle` (Radiant), `Fire` (bool), `Reload` (bool).
- **Server-Logik:** Die Simulation ist die Wahrheit. Ein Client schickt nur Eingaben.
- **Gleiche Eingaben und gleicher Seed ergeben denselben Ausgang** (auf derselben Plattform). Das macht Replays und Tests möglich.
- **Alle Werte in `content/*.json`**, nicht im Code. Die Zahlen unten sind Startwerte zum Ausprobieren.

## Welt
- Rechteckige Karte (Breite, Höhe in Metern), Hindernisse als achsparallele Rechtecke.
- Spieler sind Kreise (Radius 0,4 m). Hindernisse blockieren Spieler und Projektile.
- Je Team eine Liste von Spawnpunkten in der Kartendatei.
- Erste Testkarte: `content/maps/test_arena.json` (60 × 40 m, ca. 12 Hindernisse, 2 Spawnzonen).

## Regeln

### Bewegung
- Höchstgeschwindigkeit 5 m/s, diagonale Eingabe wird normiert.
- Rückwärts- und Seitwärtsgehen gleich schnell (einfacher Start).
- Kollision mit Hindernissen und Kartenrand durch Gleiten an der Wand; Spieler untereinander blockieren sich nicht.

### Zielen
- `AimAngle` wird sofort übernommen, keine Drehverzögerung.
- Der Schuss geht in Zielrichtung plus zufällige Streuung.

### Waffe (eine Standardwaffe für alle)
| Wert | Startwert |
|---|---|
| Schaden | 20 |
| Feuerrate | 8 Schuss/s (Cooldown 4 Ticks) |
| Streuung | ±2° (deterministisch aus Seed) |
| Magazin | 30 Schuss |
| Nachladezeit | 2,0 s (60 Ticks), Feuer unterbricht nicht, Laufen erlaubt |
| Projektilgeschwindigkeit | 60 m/s |
| Reichweite | 40 m |

### Projektile
- Echte Projektile: sie bewegen sich pro Tick vorwärts.
- Treffer-Test pro Tick als Strecke gegen Kreis und Rechtecke (kein Durchfliegen bei hoher Geschwindigkeit).
- Ein Projektil verschwindet beim ersten Treffer oder am Ende der Reichweite.
- Beschuss der eigenen Teammitglieder verursacht keinen Schaden (Friendly Fire aus).

### Leben, Tod, Respawn
- Leben 100. Sinkt es auf 0, ist der Spieler tot und der Schütze bekommt einen Kill.
- Respawn nach 2,5 s (75 Ticks) an einem zufälligen freien Spawnpunkt des eigenen Teams.
- 1,5 s Spawnschutz (kein Schaden), endet vorzeitig, sobald der Spieler schießt.
- Waffe nach Respawn voll geladen.

### Match
- Zwei Teams, Teamwechsel nur außerhalb des Matches.
- Punkte: 1 Punkt pro Kill für das Team des Schützen.
- Ende: erstes Team mit 100 Punkten oder nach 10 Minuten, dann gewinnt das Team mit mehr Punkten (sonst Unentschieden).
- Teamgröße ist nicht festgelegt; Ziel ist 16 pro Team, siehe Tests.

## Nicht enthalten
Fog of War (F-002), Netzwerk (F-003), Lobby und Teamwahl (F-004), Rating und Balancing (F-005), Bot-KI (F-006), Loot, Rüstung, mehrere Waffen, Grafik und Sound.

## Tests und Prüfung
Alle Tests laufen mit `tools/check` ohne Grafik.
1. **Einzeltests:** Bewegung normiert, Wandkollision, Cooldown, Magazin und Nachladen, Streuung im Rahmen, Schaden, Kill zählt für den richtigen Schützen, Friendly Fire ohne Schaden, Spawnschutz, Respawn nach 75 Ticks, Matchende bei Punktelimit und Zeitlimit.
2. **Kein Durchfliegen:** Projektil mit 60 m/s trifft auch eine dünne Wand und einen Spieler, der quer zur Flugbahn steht.
3. **Simulation:** 2 × 16 zufällig steuernde Spieler spielen 10 Minuten Spielzeit. Prüft nach jedem Tick: Spieler nie in Hindernissen, Leben zwischen 0 und 100, Magazin nie negativ, Summe der Punkte gleich Summe der Kills.
4. **Determinismus:** Dieselbe Simulation mit gleichem Seed zweimal, die Endstände (Hash) sind identisch.
5. **Lasttest:** Dieselbe Simulation mit 2 × 32 Spielern. Gemessen wird die Rechenzeit pro Tick. Richtwert: im Mittel unter 2 ms. Das ist eine Messung, kein Bestehen-oder-Durchfallen, sie liefert Zahlen für die Netzwerk-Entscheidung.

## Aufwand
M. Etwa 10–12 Stunden Arbeit in mehreren Schritten: (1) Datentypen und Karte, (2) Bewegung und Kollision, (3) Waffe und Projektile, (4) Leben, Respawn und Match, (5) Simulationstests und Lasttest.

## Offene Entscheidungen (bitte bestätigen oder ändern)
1. Projektile mit Flugzeit statt Sofort-Treffer (empfohlen: mehr Ausweichen und Vorhalten, passt zum Mausziel, kostet etwas mehr Rechenzeit).
2. Magazin und Nachladen statt unbegrenzter Munition (empfohlen: bessere Spieltempo-Kontrolle).
3. Friendly Fire aus.
4. Matchende bei 100 Punkten oder 10 Minuten.

## Umsetzungsnotizen (2026-11-15)
Kleine Klarstellungen, die die Spec offen ließ:
- Ein leeres Magazin lädt automatisch nach (Ablauf wie bei der Nachladetaste, 60 Ticks).
- Projektile fliegen durch Spieler mit Spawnschutz hindurch, sie werden nicht verbraucht. Gleiches gilt für Teammitglieder.
- Spawnschutz zählt 45 Ticks ab dem Spawn-Tick. Wer schießt, verliert ihn sofort.
- Der Matchstart gibt allen Spielern Spawnschutz, ebenso jeder Respawn.
- Spawnpunkte: bevorzugt einer ohne lebenden Spieler im Umkreis von 1 m, sonst ein zufälliger.
- JSON wird im eigenen Projekt `src/core/AdventLAN.Content` geladen (net8.0, System.Text.Json), weil .NET Standard 2.1 kein JSON mitbringt. Der Core selbst bleibt frei von Paketen.
- Die Lastmessung ist ein eigenes Programm: `dotnet run --project tools/SimBench -c Release`. Sie ist nicht Teil von `tools/check`.

Ergebnis der Lastmessung (Cloud-Rechner mit 2 Kernen, nur Simulation, ohne Netzwerk und Fog of War):
| Spieler | Ø pro Tick | Maximum |
|---|---|---|
| 2 × 8 | 0,005 ms | 1,08 ms |
| 2 × 16 | 0,004 ms | 0,16 ms |
| 2 × 32 | 0,010 ms | 0,60 ms |
Die Spiellogik ist damit weit unter dem Richtwert (2 ms). Netzwerk und Sichtberechnung werden den Takt bestimmen, nicht diese Regeln.
