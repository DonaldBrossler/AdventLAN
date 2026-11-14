# Vision – Projekt AdventLAN (Arbeitstitel)

Status: freigegeben (2026-11-14)

## Kernidee
Top-down-Team-Shooter auf einer geschlossenen Arena-Karte. Zwei Teams treten gegeneinander an.
Mechanik angelehnt an Escape from Duckov: Top-down-Ansicht, Zielen mit der Maus, Fog of War.

## Spielgefühl (3 Säulen)
1. Top-down-Perspektive
2. Präzises Zielen mit der Maus
3. Fog of War: Man sieht nur, was die Figur sehen kann (Sichtkegel, Hindernisse). Gegner können von der Seite oder von hinten kommen.

## Zielgruppe und Plattform
Spieler von Team-Shootern, die Taktik und Übersicht mögen. Windows. Steam später.

## Meilenstein
Vorstellung auf einer LAN-Party in 10 Monaten (ab 2026-11-14). Grundspiel nach 6 Monaten, danach 2 Monate Puffer, dann Feinschliff und Aufbau für die LAN.
Betrieb im lokalen Netz: ein Host mit Server, kein Steam nötig.

## Start-Umfang (MVP)
- Eine Karte, Team Deathmatch, Respawn nach 2–3 Sekunden
- Alle starten mit gleicher Ausrüstung
- Bots als Gegner und Mitspieler
- Lobby mit freier Teamwahl; Bots weist der Host per Klick einem Team zu
- Knopf „Balancing“, nur für den Host: verteilt Spieler nach Rating auf die Teams (nur manuell, nie automatisch)
- Rating je Spieler, vom Host als Textdatei gespeichert, nach jedem Match angepasst; neue Spieler starten mit Standardwert

## Teamgröße
Ziel für die LAN: 8–16 pro Team. Mehr nur, wenn die Lasttests es zeigen.

## Später
- Online-PvP mit großen Teams (Steam)
- Loot und Upgrades in der Arena (Rüstung, Waffen)
- Weitere Modi: Bankräuber (Geld zu einer Zone tragen), Capture the Flag u. a.
- Weitere, voneinander getrennte Karten
- Koop-PvE gegen Bots

## Nicht-Ziele (vorerst)
Fortschritt außerhalb der Matches außer dem Rating, Stash, Kampagne, Fahrzeuge.

## Grundsätze der Technik
- Server entscheidet über Sichtbarkeit (Fog of War), damit Cheater nichts durch Wände sehen.
- Spiellogik liegt in `src/core/` ohne Engine-Abhängigkeit und ist ohne Grafik testbar.
