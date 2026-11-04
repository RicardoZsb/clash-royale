$ErrorActionPreference='Stop'
$pidPath = Join-Path $PSScriptRoot '.local\server.pid'
if (-not (Test-Path -LiteralPath $pidPath)) { Write-Output 'No recorded backend process.'; exit 0 }
$backendPid = [int](Get-Content -LiteralPath $pidPath)
$process = Get-CimInstance Win32_Process -Filter "ProcessId=$backendPid"
$expected = Join-Path $PSScriptRoot 'server.py'
if ($process -and $process.CommandLine.Contains($expected)) { Stop-Process -Id $backendPid; Remove-Item -LiteralPath $pidPath; Write-Output 'Local backend stopped.' }
elseif ($process) { throw 'Recorded process does not match this backend; left untouched.' }
else { Remove-Item -LiteralPath $pidPath; Write-Output 'Backend already stopped.' }
