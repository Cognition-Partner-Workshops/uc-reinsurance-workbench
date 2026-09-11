$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
$nuget = (Get-Command nuget.exe).Source
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
& $nuget restore (Join-Path $repo "src\ReinsuranceWorkbench.sln")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $msbuild (Join-Path $repo "src\ReinsuranceWorkbench.sln") /t:Build /p:Configuration=Debug /v:minimal
exit $LASTEXITCODE
