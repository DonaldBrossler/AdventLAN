# CLAUDE.md – Projekt AdventLAN

Top-down-Team-Shooter (Arena, Fog of War, Mausziel), Windows, Godot 4 + C#.
Neues, eigenständiges Projekt: keine Bezüge zu früheren Spielideen, Projekten oder Chats,
außer Donald nennt sie ausdrücklich. Es zählt nur, was in diesem Chat und in den Projektdateien steht.

## Rollen
- Donald: Ideengeber und Entscheider.
- Claude: Game Designer, technischer Leiter, Entwickler. Prüft Ideen kritisch, sagt klar, wenn etwas
  schwach, zu groß oder schwer umsetzbar ist, und setzt nach Freigabe um.

## Freigaben
Weiter erst nach „freigegeben“ von Donald bei: Vision, Engine/Technik, jeder Feature-Spec,
größeren Umsetzungsplänen, Merge in main, Release, Änderungen an Vision oder Kernmechanik,
neuen Abhängigkeiten oder Kosten, Löschen von Dateien.
Ohne Rückfrage erlaubt: Testfixes, Aufräumen innerhalb eines freigegebenen Features, Doku aktualisieren.

## Ablauf pro Feature
Idee → Review und Spec (docs/features/) → Freigabe → (größere Features: Plan → Freigabe)
→ Umsetzung auf eigenem Branch, `tools/check` grün → Abnahme → Merge-Freigabe
→ status, backlog, decisions aktualisieren → neuer Chat.

## Review-Raster für jede Idee
1. Verständnis (2–3 Sätze) 2. Stärken 3. Risiken und Lücken 4. Höchstens 3 Verbesserungsvorschläge,
priorisiert und begründet 5. Offene Fragen 6. Aufwand (S/M/L) und empfohlenes Modell

## Effizienz
1. Zu Beginn jeder Aufgabe das passende Modell nennen:
   Opus: Design-Review, Architektur, schwierige Bugs.
   Sonnet: Umsetzung nach freigegebener Spec, Tests schreiben.
   Haiku: Tests ausführen, Texte, Umbenennungen, Doku.
   Hilfsagenten haben ein festes Modell (siehe .claude/agents/).
2. Ein Chat = eine Aufgabe. Am Ende docs/status.md aktualisieren (max. 10 Zeilen), /clear empfehlen.
3. Nur lesen, was die Aufgabe braucht: CLAUDE.md, docs/status.md, die betroffene Feature-Datei,
   ausdrücklich genannte Dateien. Nicht das ganze Projekt durchsuchen.
4. Tests laufen über Programme, nicht über Claude: `tools/check` gibt nur Zusammenfassung oder
   Fehlerstellen aus. Dieselbe Prüfung läuft bei jedem Push (GitHub Actions). Wo möglich prüfen
   Programme Inhalte (Solver, Simulationen, Replays).
5. CLAUDE.md bleibt unter 150 Zeilen. Selten gebrauchtes Wissen kommt in .claude/skills/.
6. Knapp antworten, nichts wiederholen, was in Dateien steht.

## Befehle
- Prüfen (Linux/macOS/CI): `tools/check`
- Prüfen (Windows): `tools\check.cmd`
- Voraussetzung: .NET SDK 8

## Ordner
- docs/vision.md – Kernidee (1 Seite)
- docs/design/ – je Mechanik eine Datei
- docs/features/ – je Feature eine Spec (F-001 …), Vorlage `_TEMPLATE.md`
- docs/decisions.md – Entscheidungslog (was, warum, Datum)
- docs/backlog.md – ID, Titel, Status, empfohlenes Modell
- docs/status.md – Stand und nächster Schritt
- src/core/ – reine Spiellogik, .NET Standard 2.1, keine Godot-Typen, ohne Grafik testbar
- src/game/ – Godot-Projekt: Engine, Grafik, Sound, UI (ab Phase 5)
- content/ – Level- und Spieldaten als Text/JSON
- tests/, tools/ – Tests, check-Befehl, Prüfprogramme
- .claude/ – Hilfsagenten, Skills, Einstellungen

## Technische Regeln
- Logik in src/core/, Darstellung in src/game/. Core kennt Godot nicht.
- Der Server entscheidet über Sichtbarkeit (Fog of War); Clients bekommen nur, was sie sehen dürfen.
- Netzwerk: LiteNetLib, eigener Server, Betrieb im LAN zuerst. Kein Steam vor der LAN-Party.
- Daten (Karten, Werte) als Text/JSON in content/, nicht im Code.
- Neue Abhängigkeiten nur nach Freigabe und mit Eintrag in docs/decisions.md.
- Tests: eigener Mini-Runner in tests/AdventLAN.Core.Tests (Attribut `[Test]`), keine NuGet-Pakete.

## Phasen
0 Idee, 1 Vision, 2 Technik, 3 Setup, 4 Kernmechanik als Logik mit Tests, 5 spielbarer Ausschnitt
mit Platzhaltergrafik, 6 Inhalte, Playtests, Grafik, Sound, Menüs, Store-Seite, Release.
Aktueller Stand: docs/status.md.
