$ErrorActionPreference='Stop'
$projectRoot=Split-Path (Split-Path $PSScriptRoot)
$dataPath=Join-Path $projectRoot 'DesignReference\Validation\Rewards\.local'
$pythonPath=Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
$serverPath=Join-Path $projectRoot 'Backend\server.py'
$process=Start-Process -FilePath $pythonPath -ArgumentList @('-X','utf8','-u',('"'+$serverPath+'"'),'--port','18766','--data',('"'+$dataPath+'"')) -WorkingDirectory $projectRoot -WindowStyle Hidden -RedirectStandardOutput (Join-Path $dataPath 'server.log') -RedirectStandardError (Join-Path $dataPath 'server-error.log') -PassThru
$process.Id | Set-Content -LiteralPath (Join-Path $dataPath 'server.pid')
