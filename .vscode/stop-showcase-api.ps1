$ErrorActionPreference = 'Stop'

$port = 5080
$listeners = @(Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue)

foreach ($listener in $listeners) {
    $listenerProcessId = $listener.OwningProcess
    $process = Get-CimInstance Win32_Process -Filter "ProcessId = $listenerProcessId"
    $isShowcaseApi = $process.CommandLine -match '(?i)Hs\.Showcase\.Api(\.dll)?' -or
        $process.ExecutablePath -match '(?i)Hs\.Showcase\.Api(\.exe)?$'

    if (-not $isShowcaseApi) {
        throw "Port $port is in use by process $listenerProcessId, which is not the Showcase API. It was not stopped."
    }

    Stop-Process -Id $listenerProcessId -Force
    Write-Host "Stopped the previous Showcase API process ($listenerProcessId)."
}

$timeout = [DateTime]::UtcNow.AddSeconds(5)
do {
    $remainingListeners = @(Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue)
    if ($remainingListeners.Count -eq 0) {
        break
    }

    Start-Sleep -Milliseconds 200
} while ([DateTime]::UtcNow -lt $timeout)

if ($remainingListeners.Count -gt 0) {
    throw "Port $port is still in use after stopping the previous Showcase API."
}
