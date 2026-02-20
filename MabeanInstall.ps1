$local = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
$mabeanDir = Join-Path $local "Mabean"
$dataDir = Join-Path $mabeanDir "data"
$keyBinPath = Join-Path $dataDir "key.bin"
$payloadsDir = Join-Path $dataDir "payloads"
$configJsonPath = Join-Path $payloadsDir "payloads.json"

$dlls = Join-Path $dataDir "Dlls"

$logs = Join-Path $dataDir "Logs"


if (-Not (Test-Path $mabeanDir)) {
    New-Item -ItemType Directory -Path $mabeanDir | Out-Null
}

if (-Not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir | Out-Null
}

if (-Not (Test-Path $payloadsDir)) {
    New-Item -ItemType Directory -Path $payloadsDir | Out-Null
}

if (-Not (Test-Path $configJsonPath)) {
    @{ Payloads = @() } | ConvertTo-Json -Compress | Out-File $configJsonPath -Encoding utf8
}

if (-Not (Test-Path $dlls)) {
    New-Item -ItemType Directory -Path $dlls | Out-Null
}

if (-Not (Test-Path $logs)) {
    New-Item -ItemType Directory -Path $logs | Out-Null
}

Copy-Item ".\MabeanScripts\Injection\1\x64\Release\1.dll" -Destination (Join-Path $dlls "1.dll")  -Force
Copy-Item ".\MabeanScripts\PrivilegeEscalation\2\x64\Release\2.dll" -Destination (Join-Path $dlls "2.dll")  -Force




