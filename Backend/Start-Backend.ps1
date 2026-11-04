param([int]$Port = 18765)
$ErrorActionPreference = 'Stop'
$dataDirectory = Join-Path $PSScriptRoot '.local'
New-Item -ItemType Directory -Path $dataDirectory -Force | Out-Null
try {
    $health = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/health" -TimeoutSec 2
    if ($health.service -eq 'ClashRoyaleV54') { Write-Output "Backend already running: http://127.0.0.1:$Port"; exit 0 }
} catch { }
$pythonPath = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
if (-not (Test-Path -LiteralPath $pythonPath)) { $pythonPath = (Get-Command python -ErrorAction Stop).Source }
$serverPath = Join-Path $PSScriptRoot 'server.py'
$process = Start-Process -FilePath $pythonPath -ArgumentList @('-X','utf8','-u',('"' + $serverPath + '"'),'--port',"$Port") -WorkingDirectory $PSScriptRoot -WindowStyle Hidden -RedirectStandardOutput (Join-Path $dataDirectory 'server.log') -RedirectStandardError (Join-Path $dataDirectory 'server-error.log') -PassThru
$process.Id | Set-Content -LiteralPath (Join-Path $dataDirectory 'server.pid') -Encoding utf8
for ($attempt = 0; $attempt -lt 20; $attempt++) {
    Start-Sleep -Milliseconds 250
    try { $health = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/health" -TimeoutSec 1; if ($health.service -eq 'ClashRoyaleV54') { Write-Output "Backend ready: http://127.0.0.1:$Port"; exit 0 } } catch { }
    if ($process.HasExited) { Get-Content -LiteralPath (Join-Path $dataDirectory 'server-error.log'); throw 'Backend exited before becoming ready.' }
}
throw 'Backend readiness timed out; inspect .local/server-error.log.'
