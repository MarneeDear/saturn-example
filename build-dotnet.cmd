@echo off
cls
type build-art.txt

:: WE may want to install the dotnetsdk like this
:: choco install dotnetcore-sdk --version 2.1.403
:: choco install dotnetcore-sdk (latest version)
:: YOU NEED ELEVATED PRIVILEGES TO DO THIS SO IT MIGHT NOT WORK SO GOOD ON TEAMCITY

IF EXIST "paket.lock" (
  .paket\paket.exe restore
) ELSE (
  .paket\paket.exe install
)

if errorlevel 1 (
  exit /b %errorlevel%
)

packages\build\FAKE\tools\FAKE.exe build-dotnet.fsx %*
