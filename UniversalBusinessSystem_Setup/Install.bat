@echo off
echo Installing Universal Business System...
echo.

REM Create installation directory
if not exist "%ProgramFiles%\Universal Business System" mkdir "%ProgramFiles%\Universal Business System"

REM Copy files
xcopy "%~dp0\*" "%ProgramFiles%\Universal Business System\" /E /Y /I

REM Create desktop shortcut
echo Set WshShell = WScript.CreateObject("WScript.Shell") > "%temp%\create_Universal_Business_System_desktop.vbs"
echo strDesktop = WshShell.SpecialFolders("Desktop") >> "%temp%\create_Universal_Business_System_desktop.vbs"
echo Set oShellLink = WshShell.CreateShortcut(strDesktop ^& "\Universal Business System.lnk") >> "%temp%\create_Universal_Business_System_desktop.vbs"
echo oShellLink.TargetPath = "%ProgramFiles%\Universal Business System\UniversalBusinessSystem.exe" >> "%temp%\create_Universal_Business_System_desktop.vbs"
echo oShellLink.WorkingDirectory = "%ProgramFiles%\Universal Business System" >> "%temp%\create_Universal_Business_System_desktop.vbs"
echo oShellLink.Save >> "%temp%\create_Universal_Business_System_desktop.vbs"
cscript //nologo "%temp%\create_Universal_Business_System_desktop.vbs"

del "%temp%\create_Universal_Business_System_desktop.vbs" 2>nul

REM Create start menu shortcut
echo Set WshShell = WScript.CreateObject("WScript.Shell") > "%temp%\create_Universal_Business_System_startmenu.vbs"
echo strStartMenu = WshShell.SpecialFolders("Programs") >> "%temp%\create_Universal_Business_System_startmenu.vbs"
echo Set oShellLink = WshShell.CreateShortcut(strStartMenu ^& "\Universal Business System.lnk") >> "%temp%\create_Universal_Business_System_startmenu.vbs"
echo oShellLink.TargetPath = "%ProgramFiles%\Universal Business System\UniversalBusinessSystem.exe" >> "%temp%\create_Universal_Business_System_startmenu.vbs"
echo oShellLink.WorkingDirectory = "%ProgramFiles%\Universal Business System" >> "%temp%\create_Universal_Business_System_startmenu.vbs"
echo oShellLink.Save >> "%temp%\create_Universal_Business_System_startmenu.vbs"
cscript //nologo "%temp%\create_Universal_Business_System_startmenu.vbs"

del "%temp%\create_Universal_Business_System_startmenu.vbs" 2>nul

REM Create registry entry
reg add "HKCU\Software\Universal Business System" /v Installed /t REG_DWORD /d 1 /f >nul

echo.
echo Installation complete!
echo Files installed to %ProgramFiles%\Universal Business System
echo Desktop and Start Menu shortcuts created.
echo.
pause
