@echo off
rem Prueft das Projekt: Build und Tests. Gibt nur Zusammenfassung oder Fehlerstellen aus.
cd /d "%~dp0.."

dotnet --list-sdks 2>nul | findstr /b "8." >nul
if errorlevel 1 (
  echo FEHLER: .NET SDK 8 nicht gefunden. Installieren: winget install Microsoft.DotNet.SDK.8
  exit /b 2
)

dotnet build tests\AdventLAN.Core.Tests -c Release -nologo -v quiet > "%TEMP%\adventlan_build.log" 2>&1
if errorlevel 1 (
  echo BUILD FEHLGESCHLAGEN
  findstr /i "error Fehler" "%TEMP%\adventlan_build.log"
  exit /b 1
)

dotnet run --project tests\AdventLAN.Core.Tests -c Release --no-build > "%TEMP%\adventlan_test.log" 2>&1
type "%TEMP%\adventlan_test.log"
findstr /c:" 0 fehlgeschlagen" "%TEMP%\adventlan_test.log" >nul
if errorlevel 1 (
  echo check: ROT
  exit /b 1
)
echo check: gruen
exit /b 0
