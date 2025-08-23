@echo off

if exist "C:\Program Files\Unity\Hub\Editor\2022.3.56f1\Editor\Unity.exe" set unityPath="C:\Program Files\Unity\Hub\Editor\2022.3.56f1\Editor\Unity.exe"
if exist "D:\Program Files\Unity\Editor\2022.3.56f1\Editor\Unity.exe" set unityPath="D:\Program Files\Unity\Editor\2022.3.56f1\Editor\Unity.exe"


start "" %unityPath% -projectPath %cd% -logFile "%cd%\Logs\CurrentLogs.log" 

powershell -Command "Get-Content 'Logs\CurrentLogs.log' -Wait"

pause