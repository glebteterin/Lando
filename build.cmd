@echo off
set MSBUILD=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
set MSBUILD_SCRIPT="%~dp0src\Lando.proj"

set MSBUILD_ARGUMENTS=
:next
if (%1)==() goto build
if (%1)==(debug) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:Configuration=Debug
	shift
	goto next
)

if (%1)==(release) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:Configuration=Release
	shift
	goto next
)


:build
echo MSBUILD: %MSBUILD%
echo MSBUILD_SCRIPT: %MSBUILD_SCRIPT%
echo MSBUILD_ARGUMENTS: %MSBUILD_ARGUMENTS%
%MSBUILD% /nologo /fl %MSBUILD_SCRIPT% %MSBUILD_ARGUMENTS%
exit /b 0