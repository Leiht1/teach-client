$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sdk=Join-Path $root 'Logs/WebView2Sdk/package'
if(!(Test-Path $sdk)){New-Item (Split-Path $sdk) -ItemType Directory -Force | Out-Null; Invoke-WebRequest 'https://www.nuget.org/api/v2/package/Microsoft.Web.WebView2/1.0.3537.50' -OutFile (Join-Path (Split-Path $sdk) 'sdk.zip');Expand-Archive (Join-Path (Split-Path $sdk) 'sdk.zip') $sdk}
$out=Join-Path $root 'Assets/StreamingAssets/IslandSystem/Host'
New-Item $out -ItemType Directory -Force | Out-Null
Copy-Item "$sdk/lib/net462/Microsoft.Web.WebView2.Core.dll","$sdk/lib/net462/Microsoft.Web.WebView2.WinForms.dll","$sdk/runtimes/win-x64/native/WebView2Loader.dll","$sdk/LICENSE.txt" $out
Push-Location $PSScriptRoot
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' /nologo /target:winexe /platform:x64 /out:"$out/IslandScratchHost.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:"$out/Microsoft.Web.WebView2.Core.dll" /reference:"$out/Microsoft.Web.WebView2.WinForms.dll" Host.cs
if($LASTEXITCODE -ne 0){throw 'Host build failed'}

Pop-Location
