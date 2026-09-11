param(
    [string]$Filter,
    [switch]$Integration
)

$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
$vstest = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$adapter = Join-Path $repo "src\packages\NUnit3TestAdapter.4.5.0\build\net462"
$arguments = @(
    (Join-Path $repo "src\Reinsurance.Tests\bin\Debug\Reinsurance.Tests.dll"),
    "/TestAdapterPath:$adapter",
    "/Logger:console"
)
if ($Filter)
{
    $arguments += "/TestCaseFilter:$Filter"
}
elseif (-not $Integration)
{
    $arguments += "/TestCaseFilter:TestCategory!=Integration"
}
& $vstest $arguments
exit $LASTEXITCODE
