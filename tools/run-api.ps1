$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
$iis = "C:\Program Files\IIS Express\iisexpress.exe"
$path = Join-Path $repo "src\Reinsurance.Api"
& $iis "/path:$path" "/port:5055" "/systray:false"
