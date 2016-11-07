REM "..\tools\gitlink\GitLink.exe" ..\ -u https://github.com/huoshan12345/FxUtility.DataStructures -c release

@ECHO OFF
SET /P VERSION_SUFFIX=Please enter version-suffix (can be left empty): 
dotnet "pack" "..\src\FxUtility.DataStructuresCSharp" -c "Release" -o "." --version-suffix "%VERSION_SUFFIX%"
pause
