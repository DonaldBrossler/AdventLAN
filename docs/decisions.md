# Entscheidungslog

| Datum | Entscheidung | Grund |
|---|---|---|
| 2026-11-14 | Vision freigegeben (Top-down-Team-Shooter, TDM, Respawn 2–3 s) | Donald |
| 2026-11-14 | Balancing nur manuell, nur durch den Host | Teams sollen nicht ständig wechseln |
| 2026-11-14 | Ziel: LAN-Party in 10 Monaten, Betrieb im LAN ohne Steam | Meilenstein von Donald |
| 2026-11-14 | Engine: Godot 4 mit C#; Core als reine C#-Bibliothek (.NET Standard 2.1) | MIT-Lizenz, Dedicated-Server-Export, Core testbar ohne Grafik, Wechsel zu Unity bleibt möglich |
| 2026-11-14 | Netzwerk über LiteNetLib mit eigenem Server, nicht über Godots eingebautes Multiplayer | 32+ Spieler, volle Kontrolle über die Simulation |
| 2026-11-14 | Tests: eigener minimaler Test-Runner ohne NuGet-Pakete (Setup-Umgebung hatte keinen NuGet-Zugang); Wechsel auf xUnit jederzeit möglich | Weniger Abhängigkeiten, lokal verifizierbar |
