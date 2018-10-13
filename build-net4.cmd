@echo off
cls
type build-art.txt

echo "GET BOOTSTRAPPER"
.paket\paket.bootstrapper.exe

if errorlevel 1 (
    exit /b %errorlevel%
)

echo "PAKET RESTORE"
.paket\paket.exe restore
if errorlevel 1 (
    exit /b %errorlevel%
)

:: Paket had issues with TeamCity NuGet feeds, and TeamCity doesn't use NuGet sources configured at the machine level, 
:: so always set the TeamCity source manually and then install common build package manually. 
echo "REMOVE TEAMCITY SOURCE"
packages\build\NuGet.CommandLine\tools\nuget.exe sources remove -name "TeamCity" 2>NUL
echo "ADD TEAMCITY SOURCE"
packages\build\NuGet.CommandLine\tools\nuget.exe sources add -name "TeamCity" -source https://teamcity.med.arizona.edu/httpAuth/app/nuget/v1/FeedService.svc -username "NuGetUser" -password "%TEAMCITY_NUGETUSER_PASSWORD%"
echo "GET COM.Build"
packages\build\NuGet.CommandLine\tools\nuget.exe install COM.Build -verbosity detailed -excludeversion -prerelease -OutputDirectory .\packages\build

if errorlevel 1 (
    exit /b %errorlevel%
)

:: packages\build\FAKE\tools\FAKE.exe build-net4.fsx %*