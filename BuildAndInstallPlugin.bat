
dotnet build -c Release
taskkill /F /IM PowerToys.exe
taskkill /F /IM PowerToys.Runner.exe
taskkill /F /IM PowerToys.PowerLauncher.exe
del"%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\Huh\*"
robocopy /e /w:5 "%~dp0\bin\Release\net8.0-windows" "%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\Huh"
start "" "C:\Program Files\PowerToys\PowerToys.exe"