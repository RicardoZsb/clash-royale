$ErrorActionPreference='Stop'
$projectRoot=Split-Path (Split-Path $PSScriptRoot)
$dataPath=Join-Path $projectRoot 'DesignReference\Validation\Rewards\.local'
$pidPath=Join-Path $dataPath 'server.pid'
if(Test-Path -LiteralPath $pidPath){
 $qaProcessId=[int](Get-Content -LiteralPath $pidPath)
 $qaProcess=Get-CimInstance Win32_Process -Filter "ProcessId=$qaProcessId"
 if($qaProcess -and $qaProcess.CommandLine.Contains($dataPath) -and $qaProcess.CommandLine.Contains('--port 18766')){Stop-Process -Id $qaProcessId;Remove-Item -LiteralPath $pidPath}
 elseif($qaProcess){throw 'PID no longer belongs to reward QA.'}
 else{Remove-Item -LiteralPath $pidPath}
}
