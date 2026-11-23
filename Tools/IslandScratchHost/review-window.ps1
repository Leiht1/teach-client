param([string]$PlayerPath="Builds/IslandScratchTrial/IslandScratchTrial.exe")
$ErrorActionPreference='Stop'
$projectRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location $PSScriptRoot
try {
 & 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' /nologo /target:winexe /out:../../Logs/WindowReviewParent.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll WindowReviewParent.cs
 if($LASTEXITCODE -ne 0){throw 'Review harness compilation failed'}
} finally {Pop-Location}
Push-Location $projectRoot
try {
 $player=Join-Path $projectRoot $PlayerPath
 $log=Join-Path $projectRoot 'Logs/ScratchPlayerWindow.log'
 $review=Start-Process (Join-Path $projectRoot 'Logs/WindowReviewParent.exe') -ArgumentList ('"'+$player+'" "'+$log+'"') -WindowStyle Hidden -PassThru
 if(!$review.WaitForExit(65000)){throw 'Hidden player review timed out'}
 if($review.ExitCode -ne 0){throw (Get-Content 'Logs/ScratchPlayerWindowReview.txt' -Raw)}
 Get-Content 'Logs/ScratchPlayerWindowReview.txt'
} finally {Pop-Location}
