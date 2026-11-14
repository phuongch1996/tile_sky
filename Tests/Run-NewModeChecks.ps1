param([string]$UnityInstall = 'E:/unity/6000.6.4f1')

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $projectRoot
$outputDir = Join-Path $projectRoot 'Temp/mode-integration'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$editorData = Join-Path $UnityInstall 'Editor/Data'
$dotnet = Join-Path $editorData 'NetCoreRuntime/dotnet.exe'
$sdk = Get-ChildItem (Join-Path $editorData 'DotNetSdk/sdk') -Directory |
    Sort-Object Name -Descending | Select-Object -First 1
$compiler = Join-Path $sdk.FullName 'Roslyn/bincore/csc.dll'
$sourceRsp = Get-ChildItem 'Library/Bee/artifacts' -Recurse -Filter 'Assembly-CSharp.rsp' |
    Where-Object { $_.Directory.Name -like '*E.dag' } |
    Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if ($sourceRsp -eq $null) { throw 'Open this project in Unity first to generate its Editor compiler response file.' }
$rsp = [IO.File]::ReadAllText($sourceRsp.FullName)
$rsp = [regex]::Replace($rsp, '(?m)^-out:.*$', '-out:"Temp/mode-integration/Validation.dll"')
$rsp = [regex]::Replace($rsp, '(?m)^-refout:.*$', '-refout:"Temp/mode-integration/Validation.ref.dll"')
$newSources = @(
    'Assets/BaseProject/Scripts/GoogleTestAds.cs',
    'Assets/Scripts/Gameplay/NewModeRules.cs',
    'Assets/Scripts/Gameplay/Effects/MissingSlotTrayEffect.cs',
    'Assets/Scripts/Gameplay/LeveljsonConverter.cs',
    'Assets/Scripts/Gameplay/GameController.NewModes.cs',
    'Assets/Scripts/Gameplay/ItemTile/TileModeMarker.cs'
)
foreach ($source in $newSources) {
    if (-not $rsp.Contains('"' + $source + '"')) { $rsp += "`n" + '"' + $source + '"' }
}
$validationRsp = Join-Path $outputDir 'Validation.rsp'
[IO.File]::WriteAllText($validationRsp, $rsp)
& $dotnet $compiler ('@' + $validationRsp) 2>&1 |
    Tee-Object -FilePath (Join-Path $outputDir 'compile-output.txt') | Out-Null
if ($LASTEXITCODE -ne 0) {
    Get-Content (Join-Path $outputDir 'compile-output.txt') | Select-String 'error '
    throw 'Game C# compilation failed.'
}
Write-Output 'Game C# compilation passed using Unity compiler references (not an Editor/Play Mode run).'

$references = ($rsp -split "`n") | Where-Object { $_ -match '^-r:' } |
    ForEach-Object { [IO.Path]::GetFullPath($_.Substring(3).Trim().Trim('"')) }
$directories = @($outputDir) + @($references |
    ForEach-Object { [IO.Path]::GetDirectoryName($_) } | Select-Object -Unique)
[IO.File]::WriteAllLines((Join-Path $outputDir 'reference-dirs.txt'), [string[]]$directories)
$runtime = Get-ChildItem (Join-Path $editorData 'NetCoreRuntime/shared/Microsoft.NETCore.App') -Directory |
    Sort-Object Name -Descending | Select-Object -First 1
$runtimeRefs = foreach ($file in Get-ChildItem $runtime.FullName -Filter '*.dll') {
    try {
        $null = [Reflection.AssemblyName]::GetAssemblyName($file.FullName)
        '-r:"' + $file.FullName + '"'
    } catch { }
}
$json = $references | Where-Object { $_ -like '*Newtonsoft.Json.dll' } | Select-Object -First 1
$engine = $references | Where-Object { $_ -like '*UnityEngine.CoreModule.dll' } | Select-Object -First 1
$checksRsp = @(
    '-target:exe', '-out:"Temp/mode-integration/NewModeChecks.dll"',
    '-r:"Temp/mode-integration/Validation.dll"',
    ('-r:"' + $json + '"'), ('-r:"' + $engine + '"'), '"Tests/NewModeChecks.cs"'
) + $runtimeRefs
$checksRspPath = Join-Path $outputDir 'Checks.rsp'
[IO.File]::WriteAllLines($checksRspPath, [string[]]$checksRsp)
$runtimeConfig = @{ runtimeOptions = @{ tfm = 'net8.0'; framework = @{
    name = 'Microsoft.NETCore.App'; version = $runtime.Name
} } } | ConvertTo-Json -Depth 4
[IO.File]::WriteAllText((Join-Path $outputDir 'NewModeChecks.runtimeconfig.json'), $runtimeConfig)
& $dotnet $compiler ('@' + $checksRspPath)
if ($LASTEXITCODE -ne 0) { throw 'Check harness compilation failed.' }
& $dotnet (Join-Path $outputDir 'NewModeChecks.dll') $projectRoot
if ($LASTEXITCODE -ne 0) { throw 'Mode regression checks failed.' }

$poolRsp = @(
    '-target:exe', '-define:STANDALONE_POOL_CHECKS',
    '-out:"Temp/mode-integration/PoolManagerChecks.dll"',
    '"Tests/PoolManagerChecks.cs"', '"Assets/Scripts/Pool/PoolManager.cs"', '"Assets/Scripts/Pool/PoolObject.cs"'
) + $runtimeRefs
$poolRspPath = Join-Path $outputDir 'PoolChecks.rsp'
[IO.File]::WriteAllLines($poolRspPath, [string[]]$poolRsp)
[IO.File]::WriteAllText((Join-Path $outputDir 'PoolManagerChecks.runtimeconfig.json'), $runtimeConfig)
& $dotnet $compiler ('@' + $poolRspPath)
if ($LASTEXITCODE -ne 0) { throw 'Pool check compilation failed.' }
& $dotnet (Join-Path $outputDir 'PoolManagerChecks.dll')
if ($LASTEXITCODE -ne 0) { throw 'Pool regression checks failed.' }
