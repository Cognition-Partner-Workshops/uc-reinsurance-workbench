$ErrorActionPreference = "Stop"
$password = if ($env:MSSQL_SA_PASSWORD) { $env:MSSQL_SA_PASSWORD } else { "Reins3ure!Dev" }
$existing = docker ps -aq --filter "name=^reinsurance-sql$"
if ($existing) { docker rm -f reinsurance-sql | Out-Null }
docker run -d --name reinsurance-sql -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=$password" -e MSSQL_PID=Developer -p 14330:1433 mcr.microsoft.com/mssql/server:2022-latest | Out-Null
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$sqlcmd = (Get-Command sqlcmd.exe -ErrorAction SilentlyContinue).Source
if (-not $sqlcmd) { $sqlcmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source }
for ($i = 0; $i -lt 60; $i++) {
  Start-Sleep -Seconds 2
  if ($sqlcmd) {
    & $sqlcmd -S "localhost,14330" -U sa -P $password -C -Q "SELECT 1" 2>$null
    if ($LASTEXITCODE -eq 0) { break }
  }
}
if (-not $sqlcmd -or $LASTEXITCODE -ne 0) { throw "SQL Server did not become ready." }
& $sqlcmd -S "localhost,14330" -U sa -P $password -C -Q "IF DB_ID(N'ReinsuranceWorkbench') IS NULL CREATE DATABASE ReinsuranceWorkbench"
exit $LASTEXITCODE
