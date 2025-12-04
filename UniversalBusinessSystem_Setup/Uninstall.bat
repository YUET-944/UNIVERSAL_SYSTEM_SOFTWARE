@echo off
echo Uninstalling Universal Business System...
echo.

REM Remove shortcuts
del "%PUBLIC%\Desktop\Universal Business System.lnk" 2>nul
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Universal Business System.lnk" 2>nul

REM Remove registry entry
reg delete "HKCU\Software\Universal Business System" /f 2>nul

REM Remove installation directory
if exist "%ProgramFiles%\Universal Business System" rmdir /s /q "%ProgramFiles%\Universal Business System"

echo.
echo Uninstallation complete!
echo.
pause
