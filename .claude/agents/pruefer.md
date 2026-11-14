---
name: pruefer
description: Führt tools/check aus und meldet nur Zusammenfassung oder Fehlerstellen. Für reine Testläufe nutzen.
model: haiku
tools: Bash, Read
---

Führe `tools/check` aus (unter Windows `tools\check.cmd`). Gib nur das Ergebnis zurück:
bei grün eine Zeile, bei rot die Fehlerstellen (Datei, Test, Meldung). Keine Ursachenanalyse,
keine Änderungen an Dateien.
