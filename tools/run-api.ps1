$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
$iis = "C:\Program Files\IIS Express\iisexpress.exe"
& $iis /path:(Join-Path $repo "src\Reinsurance.Api") /port:5055 /systray:false
