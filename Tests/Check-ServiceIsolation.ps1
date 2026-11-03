$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Read-ProjectFile([string]$path) {
    return [IO.File]::ReadAllText((Join-Path $projectRoot $path))
}
function Assert-Condition([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

$settings = Read-ProjectFile 'ProjectSettings/ProjectSettings.asset'
Assert-Condition ($settings -match '(?m)^  cloudProjectId:\s*\r?$') 'A Unity Cloud project is still linked.'
Assert-Condition ($settings -match '(?m)^  organizationId:\s*\r?$') 'A Unity Cloud organization is still linked.'
Assert-Condition ($settings -notmatch 'com\.DefaultCompany\.(TileGO|2D-URP)') 'An inherited platform application identifier remains.'
$environment = (Read-ProjectFile 'ProjectSettings/Packages/com.unity.services.core/Settings.json') | ConvertFrom-Json
Assert-Condition ([string]::IsNullOrEmpty($environment.EnvironmentId)) 'An inherited UGS environment is still linked.'
Write-Output 'PASS: Cloud project, organization and environment detached; platform IDs cleaned.'

$catalog = (Read-ProjectFile 'Assets/Resources/IAPProductCatalog.json') | ConvertFrom-Json
Assert-Condition (-not $catalog.enableCodelessAutoInitialization) 'IAP automatic initialization remains enabled.'
Assert-Condition (-not $catalog.enableUnityGamingServicesAutoInitialization) 'UGS automatic initialization remains enabled.'
foreach ($product in $catalog.products) {
    Assert-Condition ($product.id.StartsWith('tilespace_')) "Inherited catalog product: $($product.id)"
}
Assert-Condition ((Read-ProjectFile 'Assets/Scripts/Manager/IAPManager.cs') -match 'PurchasingEnabled => false;') 'Purchases are enabled without a new store configuration.'

$iapGuids = '7b79b5624d944058bd39aad2b90fb275|36a5f511b0d8142cba0aeb92d0b264c9|8f7dd963c1a8b427b96975fa211478a1'
$components = 0
foreach ($root in @('Assets/Prefabs','Assets/BaseProject/Scenes')) {
    foreach ($file in Get-ChildItem (Join-Path $projectRoot $root) -Recurse -File | Where-Object Extension -In @('.prefab','.unity')) {
        $source = [IO.File]::ReadAllText($file.FullName)
        Assert-Condition ($source -notmatch 'productId: tile(evo|up|go)_') "Inherited product remains in $($file.FullName)"
        foreach ($block in [regex]::Matches($source, '(?ms)^--- !u!114 &[^\r\n]+\r?\n.*?(?=^---|\z)')) {
            if ($block.Value -match "m_Script:.*guid: ($iapGuids)") {
                $components++
                Assert-Condition ($block.Value -match 'm_Enabled: 0') "An IAP component can auto-start in $($file.FullName)"
            }
        }
    }
}
Assert-Condition ($components -gt 0) 'No serialized IAP components were inspected.'
Write-Output "PASS: catalog auto-init disabled; $components purchase/listener components disabled."

foreach ($root in @('Assets/Scripts','Assets/BaseProject/Scripts')) {
    foreach ($file in Get-ChildItem (Join-Path $projectRoot $root) -Recurse -Filter '*.cs') {
        $source = [IO.File]::ReadAllText($file.FullName)
        $source = [regex]::Replace($source, '(?s)/\*.*?\*/', '')
        $source = [regex]::Replace($source, '(?m)^\s*//.*$', '')
        Assert-Condition ($source -notmatch 'UnityServices\.InitializeAsync|LeaderboardsService\.Instance|AuthenticationService\.Instance|FirebaseApp\.DefaultInstance|CheckAndFixDependenciesAsync') "Remote backend startup/call remains in $($file.FullName)"
        Assert-Condition ($source -notmatch '"tile(evo|go|up)_[^"]*"') "An inherited service/product identifier remains in $($file.FullName)"
    }
}
Write-Output 'PASS: game scripts contain no Unity/Firebase backend initialization or leaderboard/account requests.'

$connect = Read-ProjectFile 'ProjectSettings/UnityConnectSettings.asset'
Assert-Condition ($connect -match '(?s)UnityAnalyticsSettings:.*?m_Enabled: 0.*?m_InitializeOnStartup: 0') 'Unity Analytics can auto-start.'
$firebaseConfigs = @(Get-ChildItem (Join-Path $projectRoot 'Assets') -Recurse -File -Include google-services.json,google-services-desktop.json,GoogleService-Info.plist)
Assert-Condition ($firebaseConfigs.Count -eq 0) 'A Firebase app configuration remains under Assets.'
$manifest = Read-ProjectFile 'Assets/Plugins/Android/AndroidManifest.xml'
Assert-Condition ($manifest -match 'FirebaseInitProvider[^>]*tools:node="remove"') 'Firebase Android automatic startup is not blocked.'
Assert-Condition ($manifest -match 'firebase_analytics_collection_deactivated" android:value="true"') 'Firebase Analytics is not deactivated.'
Write-Output 'PASS: Unity Analytics disabled; Firebase app configurations absent and Android auto-init blocked.'

$ads = Read-ProjectFile 'Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset'
Assert-Condition ($ads -match 'adMobAndroidAppId: ca-app-pub-3940256099942544~') 'AdMob Android is not configured with a demo app ID.'
Assert-Condition ($ads -match 'adMobIOSAppId: ca-app-pub-3940256099942544~') 'AdMob iOS is not configured with a demo app ID.'
$max = Read-ProjectFile 'Assets/MaxSdk/Resources/AppLovinSettings.asset'
Assert-Condition ($max -match '(?m)^  sdkKey:\s*\r?$') 'MAX still has an SDK key.'
Write-Output 'PASS: AdMob demo app IDs retained; MAX has no SDK key.'
