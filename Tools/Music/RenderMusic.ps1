$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'TileSkyMusic.cs')
$outputDir = Join-Path $PSScriptRoot '../../Assets/Music/Original'
[System.IO.Directory]::CreateDirectory($outputDir) | Out-Null
[TileSkyMusic]::Render((Join-Path $outputDir 'Home - Cloud Garden.wav'), $false)
[TileSkyMusic]::Render((Join-Path $outputDir 'Ingame - Sky Steps.wav'), $true)
