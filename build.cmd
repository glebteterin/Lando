@echo off
set MSBUILD=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
set MSBUILD_SCRIPT="%~dp0src\Lando.proj"
:build
echo MSBUILD: %MSBUILD%
echo MSBUILD_SCRIPT: %MSBUILD_SCRIPT%
%MSBUILD% /nologo /fl %MSBUILD_SCRIPT%
exit /b 0