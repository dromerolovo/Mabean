
  param(
      [switch]$GenerateKey = $false,
      [string]$GeminiApiKey = ""
  )

$IsAdmin = ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $IsAdmin) {
      $relaunchArgs = "-ExecutionPolicy Bypass -File `"$PSCommandPath`""                                                                              
      if ($GenerateKey) { $relaunchArgs += " -GenerateKey" }
      if ($GeminiApiKey -ne "") { $relaunchArgs += " -GeminiApiKey `"$GeminiApiKey`"" }
      Start-Process powershell -Verb RunAs -ArgumentList $relaunchArgs
      exit
}

$local = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
$mabeanDir = Join-Path $local "Mabean"
$dataDir = Join-Path $mabeanDir "Data"
$keyBinPath = Join-Path $dataDir "key.bin"
$payloadsDir = Join-Path $dataDir "Payloads"
$configJsonPath = Join-Path $payloadsDir "payloads.json"

$dlls = Join-Path $dataDir "Dlls"
$executables = Join-Path $dataDir "Executables"

$logs = Join-Path $dataDir "Logs"

$sessionConfigDir = Join-Path $dataDir "SessionConfig"


if (-Not (Test-Path $mabeanDir)) {
    New-Item -ItemType Directory -Path $mabeanDir | Out-Null
}

if (-Not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir | Out-Null
}


if (-Not (Test-Path $dlls)) {
    New-Item -ItemType Directory -Path $dlls | Out-Null
}

if(-Not (Test-Path $executables)) {
    New-Item -ItemType Directory -Path $executables | Out-Null
}

if (-Not (Test-Path $logs)) {
    New-Item -ItemType Directory -Path $logs | Out-Null
}

if (-Not (Test-Path $sessionConfigDir)) {
    New-Item -ItemType Directory -Path $sessionConfigDir | Out-Null
}

if($GenerateKey) {
    if (-Not (Test-Path $payloadsDir)) {
        New-Item -ItemType Directory -Path $payloadsDir | Out-Null
    }

    if (-Not (Test-Path $configJsonPath)) {
        @{ Payloads = @() } | ConvertTo-Json -Compress | Out-File $configJsonPath -Encoding utf8
    }

    if (-Not (Test-Path $keyBinPath)) {
        $key = New-Object byte[] 32
        $rng = [System.Security.Cryptography.RNGCryptoServiceProvider]::new()
        $rng.GetBytes($key)
        [System.IO.File]::WriteAllBytes($keyBinPath, $key)
    }
} else {
    Copy-Item "$PSScriptRoot\key.bin" -Destination $keyBinPath -Force
    Copy-Item "$PSScriptRoot\Payloads" -Destination $dataDir -Recurse -Force
}


Copy-Item "$PSScriptRoot\MabeanScripts\Injection\1\x64\Release\1.dll" -Destination (Join-Path $dlls "1.dll")  -Force
Copy-Item "$PSScriptRoot\MabeanScripts\PrivilegeEscalation\2\x64\Release\2.dll" -Destination (Join-Path $dlls "2.dll")  -Force
Copy-Item "$PSScriptRoot\MabeanScripts\Persistence\3\x64\Release\3.exe" -Destination (Join-Path $executables "3.exe")  -Force
Copy-Item "$PSScriptRoot\MabeanMarker.exe" -Destination (Join-Path $dataDir "MabeanMarker.exe")  -Force

Set-MpPreference -ExclusionPath $payloadsDir

if ($GeminiApiKey -ne "") {
    [Environment]::SetEnvironmentVariable("GEMINI_API_KEY", $GeminiApiKey, "User")
}

$appDir = Join-Path $mabeanDir "App"
dotnet publish "$PSScriptRoot\Mabean\Mabean.csproj" -c Release -r win-x64 --self-contained false -o "$appDir"

$desktop = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut((Join-Path $desktop "Mabean.lnk"))
$Shortcut.TargetPath = Join-Path $appDir "Mabean.exe"
$Shortcut.WorkingDirectory = $appDir
$Shortcut.Save()

Read-Host "Press Enter to exit"