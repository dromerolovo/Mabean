
param(
    [switch]$UninstallDotnet,
    [switch]$UninstallVc,
    [switch]$FullUninstall
)

$IsAdmin = ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $IsAdmin) {
    Start-Process powershell `
        -Verb RunAs `
        -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`""
    exit
}

#-------------------------------------Sysmon uninstallation--------------------------------------------------

$SysmonService = Get-Service -Name "Sysmon*" -ErrorAction SilentlyContinue

if ($SysmonService) {
    & "$env:ProgramFiles\Sysmon\Sysmon64.exe" -u force
    Remove-Item -Path "$env:ProgramFiles\Sysmon" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Sysmon uninstalled" -ForegroundColor Yellow
} else {
    Write-Host "Sysmon is not installed. Skipping." -ForegroundColor Yellow
}

#-------------------------------------Remove ASR rules--------------------------------------------------

$ASRRules = @(
    "56a863a9-875e-4185-98a7-b882c64b5ce5",
    "7674ba52-37eb-4a4f-a9a1-f0f9a1619a2c",
    "d4f940ab-401b-4efc-aadc-ad5f3c50688a",
    "9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2",
    "be9ba2d9-53ea-4cdc-84e5-9b1eeee46550",
    "01443614-cd74-433a-b99e-2ecdc07bfc25",
    "5beb7efe-fd9a-4556-801d-275e5ffc04cc",
    "d3e037e1-3eb8-44c8-a917-57927947596d",
    "3b576869-a4ec-4529-8536-b80a7769e899",
    "75668c1f-73b5-4cf0-bb93-3ecf5cb7cc84",
    "26190899-1602-49e8-8b27-eb1d0a1ce869",
    "e6db77e5-3df2-4cf1-b95a-636979351e5b",
    "d1e49aac-8f56-4280-b9ba-993a6d77406c",
    "b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4",
    "92e97fa1-2edf-4476-bdd6-9dd0b4dddc7b",
    "c1db55ab-c21a-4637-bb3f-a12568109d35",
    "a8f5898e-1dc8-49a9-9878-85004b8a61e6"
)

foreach ($rule in $ASRRules) {
    Remove-MpPreference -AttackSurfaceReductionRules_Ids $rule -ErrorAction SilentlyContinue
}

Write-Host "ASR rules removed" -ForegroundColor Yellow

#-------------------------------------Revert Microsoft Defender preferences--------------------------------------------------

Set-MpPreference -PUAProtection Disabled
Set-MpPreference -EnableControlledFolderAccess Disabled
Set-MpPreference -EnableNetworkProtection Disabled
Set-MpPreference -MAPSReporting Disabled
Set-MpPreference -SubmitSamplesConsent NeverSend

Write-Host "Defender preferences reverted to defaults" -ForegroundColor Yellow

#-------------------------------------Remove .NET installation--------------------------------------------------

if($UninstallDotnet -or $FullUninstall) {
    $dotnetDir = "$env:ProgramFiles\dotnet"

    if (Test-Path $dotnetDir) {
        Remove-Item -Path $dotnetDir -Recurse -Force -ErrorAction SilentlyContinue

        $machinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
        $newPath = ($machinePath -split ";" | Where-Object { $_.TrimEnd("\") -ine $dotnetDir.TrimEnd("\") }) -join ";"
        [Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")

        Write-Host ".NET removed" -ForegroundColor Yellow
    } else {
        Write-Host ".NET install directory not found. Skipping." -ForegroundColor Yellow
    }
}



#-------------------------------------Remove VC++ Redistributable--------------------------------------------------

if($UninstallVc -or $FullUninstall) {

}



Write-Host "`nRevert complete." -ForegroundColor Cyan