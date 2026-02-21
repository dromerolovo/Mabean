$IsAdmin = ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $IsAdmin) {
    Start-Process powershell `
        -Verb RunAs `
        -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`""
    exit
}

#-------------------------------------Sysmon installation--------------------------------------------------

$IsSysmonInstalled = [bool](Get-Service -Name "Sysmon*" -ErrorAction SilentlyContinue)

if(!$IsSysmonInstalled) {
    Invoke-WebRequest -Uri "https://download.sysinternals.com/files/Sysmon.zip" -OutFile "$env:TEMP\Sysmon.zip"

    Expand-Archive -Path "$env:TEMP\Sysmon.zip" -DestinationPath "$env:ProgramFiles\Sysmon" -Force

    Copy-Item -Path ".\sysmonconfig-export.xml" -Destination "$env:ProgramFiles\Sysmon"

    & "$env:ProgramFiles\Sysmon\Sysmon64.exe" -accepteula -i "$env:ProgramFiles\Sysmon\sysmonconfig-export.xml"

    #For changing the configuration file run
    #sysmon -c C:\configs\sysmonconfig.xml

    Write-Host "Sysmon installed" -ForegroundColor Green
} else {
    Write-Host "Sysmon is already installed. Skipping instalation" -ForegroundColor Green
}

#-------------------------------------Set ASR rules to audit mode--------------------------------------------------
#https://learn.microsoft.com/en-us/defender-endpoint/attack-surface-reduction-rules-reference#/asr-rule-to-guid-matrix
$ASRRules = @(
    "56a863a9-875e-4185-98a7-b882c64b5ce5", # Block abuse of exploited vulnerable signed drivers
    "7674ba52-37eb-4a4f-a9a1-f0f9a1619a2c", # Block Adobe Reader from creating child processes
    "d4f940ab-401b-4efc-aadc-ad5f3c50688a", # Block all Office applications from creating child processes
    "9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2", # Block credential stealing from lsass.exe
    "be9ba2d9-53ea-4cdc-84e5-9b1eeee46550", # Block executable content from email client and webmail
    "01443614-cd74-433a-b99e-2ecdc07bfc25", # Block executable files from running unless they meet prevalence, age, or trusted list criteria
    "5beb7efe-fd9a-4556-801d-275e5ffc04cc", # Block execution of potentially obfuscated scripts
    "d3e037e1-3eb8-44c8-a917-57927947596d", # Block JavaScript or VBScript from launching downloaded executable content
    "3b576869-a4ec-4529-8536-b80a7769e899", # Block Office applications from creating executable content
    "75668c1f-73b5-4cf0-bb93-3ecf5cb7cc84", # Block Office applications from injecting code into other processes
    "26190899-1602-49e8-8b27-eb1d0a1ce869", # Block Office communication application from creating child processes
    "e6db77e5-3df2-4cf1-b95a-636979351e5b", # Block persistence through WMI event subscription
    "d1e49aac-8f56-4280-b9ba-993a6d77406c", # Block process creations originating from PSExec and WMI commands
    "b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4", # Block untrusted and unsigned processes that run from USB
    "92e97fa1-2edf-4476-bdd6-9dd0b4dddc7b", # Block Win32 API calls from Office macros
    "c1db55ab-c21a-4637-bb3f-a12568109d35", # Use advanced protection against ransomware
    "a8f5898e-1dc8-49a9-9878-85004b8a61e6"  # Block Webshell creation for Servers (if applicable)
)

foreach ($rule in $ASRRules) {
    Add-MpPreference -AttackSurfaceReductionRules_Ids $rule -AttackSurfaceReductionRules_Actions AuditMode
}

Write-Host "Set ASR rules to Audit mode done" -ForegroundColor Green

#-------------------------------------Set other Microsoft Defender components to Audit mode--------------------------------------------------
Set-MpPreference -PUAProtection AuditMode
Set-MpPreference -EnableControlledFolderAccess AuditMode
Set-MpPreference -EnableNetworkProtection AuditMode
Set-MpPreference -MAPSReporting Advanced
Set-MpPreference -SubmitSamplesConsent SendSafeSamples

Write-Host "Set MpPreference done" -ForegroundColor Green

#--------------------------------------Dotnet installation---------------------------------------------------#
$IsDotnetInstalled = [bool](Get-Command dotnet -ErrorAction SilentlyContinue)
if(!$IsDotnetInstalled) {
    $installScript = "$env:TEMP\dotnet-install.ps1"
    $dotnetDir = "$env:ProgramFiles\dotnet"
    Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
    & $installScript -Channel "10.0" -InstallDir $dotnetDir

    $machinePath=[Environment]::GetEnvironmentVariable("Path","Machine")
    if(-not ($machinePath -split ";" | Where-Object { $_.TrimEnd("\") -ieq $dotnetDir.TrimEnd("\") }))
    { 
        [Environment]::SetEnvironmentVariable("Path",$machinePath+";"+$dotnetDir,"Machine") 
    }

    Write-Host "Dotnet installed successfully" -ForegroundColor Green
} else {
    Write-Host "Dotnet is already installed. Skipping instalation" -ForegroundColor Green
}

#-------------------------------------Visual C++ Redistributable installation---------------------------------------------------
$vcRedistKey = "HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" 
$installed = (Test-Path $vcRedistKey) -and ((Get-ItemProperty $vcRedistKey -ErrorAction SilentlyContinue).Installed -eq 1)
  if (-not $installed) {
      $installer = "$env:TEMP\vc_redist.x64.exe"
      Invoke-WebRequest -Uri "https://aka.ms/vs/17/release/vc_redist.x64.exe" -OutFile $installer -UseBasicParsing
      Start-Process -FilePath $installer -ArgumentList "/install", "/quiet", "/norestart" -Wait
      Remove-Item $installer -Force
      Write-Host "VC++ Redistributable x64 installed" -ForegroundColor Green
  } else {
      Write-Host "VC++ Redistributable x64 already installed. Skipping" -ForegroundColor Green
  }
