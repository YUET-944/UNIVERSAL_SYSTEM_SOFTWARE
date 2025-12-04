param(
    [string]$AppName = "Universal Business System",
    [string]$Version = "1.0.0",
    [string]$Source = ".\PublishedApp",
    [string]$Output = ".\UniversalBusinessSystem_Setup"
)

Write-Host "Building simple installer package for $AppName $Version" -ForegroundColor Cyan

if (!(Test-Path $Source)) {
    Write-Host "Source folder '$Source' not found. Publish the app first." -ForegroundColor Red
    exit 1
}

# Prepare output directories
if (Test-Path $Output) {
    Write-Host "Removing existing output at $Output" -ForegroundColor Yellow
    Remove-Item $Output -Recurse -Force
}
New-Item -ItemType Directory -Path $Output | Out-Null

# Copy published files
Write-Host "Copying published files..." -ForegroundColor Yellow
Copy-Item -Path (Join-Path $Source '*') -Destination $Output -Recurse -Force

# Create installer batch script
$installScript = @"
@echo off
echo Installing $AppName...
echo.

REM Create installation directory
if not exist "%ProgramFiles%\$AppName" mkdir "%ProgramFiles%\$AppName"

REM Copy files
xcopy "%~dp0\*" "%ProgramFiles%\$AppName\" /E /Y /I

REM Create desktop shortcut
echo Set WshShell = WScript.CreateObject("WScript.Shell") > "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"
echo strDesktop = WshShell.SpecialFolders("Desktop") >> "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"
echo Set oShellLink = WshShell.CreateShortcut(strDesktop ^& "\$AppName.lnk") >> "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"
echo oShellLink.TargetPath = "%ProgramFiles%\$AppName\UniversalBusinessSystem.exe" >> "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"
echo oShellLink.WorkingDirectory = "%ProgramFiles%\$AppName" >> "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"
echo oShellLink.Save >> "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"
cscript //nologo "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs"

del "%temp%\create_$($AppName.Replace(' ','_'))_desktop.vbs" 2>nul

REM Create start menu shortcut
echo Set WshShell = WScript.CreateObject("WScript.Shell") > "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"
echo strStartMenu = WshShell.SpecialFolders("Programs") >> "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"
echo Set oShellLink = WshShell.CreateShortcut(strStartMenu ^& "\$AppName.lnk") >> "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"
echo oShellLink.TargetPath = "%ProgramFiles%\$AppName\UniversalBusinessSystem.exe" >> "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"
echo oShellLink.WorkingDirectory = "%ProgramFiles%\$AppName" >> "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"
echo oShellLink.Save >> "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"
cscript //nologo "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs"

del "%temp%\create_$($AppName.Replace(' ','_'))_startmenu.vbs" 2>nul

REM Create registry entry
reg add "HKCU\Software\$AppName" /v Installed /t REG_DWORD /d 1 /f >nul

echo.
echo Installation complete!
echo Files installed to %ProgramFiles%\$AppName
echo Desktop and Start Menu shortcuts created.
echo.
pause
"@

Set-Content -Path (Join-Path $Output 'Install.bat') -Value $installScript -Encoding ASCII

# Create uninstaller batch script
$uninstallScript = @"
@echo off
echo Uninstalling $AppName...
echo.

REM Remove shortcuts
del "%PUBLIC%\Desktop\$AppName.lnk" 2>nul
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\$AppName.lnk" 2>nul

REM Remove registry entry
reg delete "HKCU\Software\$AppName" /f 2>nul

REM Remove installation directory
if exist "%ProgramFiles%\$AppName" rmdir /s /q "%ProgramFiles%\$AppName"

echo.
echo Uninstallation complete!
echo.
pause
"@

Set-Content -Path (Join-Path $Output 'Uninstall.bat') -Value $uninstallScript -Encoding ASCII

# Package into ZIP
$zipPath = "$AppName`_$Version`_Installer.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Creating installer archive $zipPath" -ForegroundColor Yellow
Compress-Archive -Path (Join-Path $Output '*') -DestinationPath $zipPath -Force

Write-Host "`nInstaller bundle created:" -ForegroundColor Green
Write-Host (Resolve-Path $zipPath)
Write-Host "Extract the ZIP and run Install.bat as Administrator." -ForegroundColor Cyan
