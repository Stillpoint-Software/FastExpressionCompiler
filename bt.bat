@echo off

echo: 
echo:## Running TESTS on .NET 8.0...
echo:
dotnet run -v:minimal -f:net8.0 -c:Release -p:GeneratePackageOnBuild=false --project test/FastExpressionCompiler.TestsRunner/FastExpressionCompiler.TestsRunner.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: TESTS

echo:# Finished: ALL
echo:
exit /b 0

:error
echo:
echo:## :-( Failed with ERROR: %ERRORLEVEL%
echo:
exit /b %ERRORLEVEL%
