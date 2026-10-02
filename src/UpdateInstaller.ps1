param(
    [Parameter(Mandatory=$true)][string]$Target,
    [Parameter(Mandatory=$true)][string]$Payload,
    [Parameter(Mandatory=$true)][int]$GameProcessId,
    [Parameter(Mandatory=$true)][string]$ExpectedVersion
)
$ErrorActionPreference = 'Stop'
$job = Split-Path -Parent $Payload
$log = Join-Path $job 'result.txt'
$backup = Join-Path $job 'backup'
$changed = @()
$existing = @{}
try {
    $Target = [IO.Path]::GetFullPath($Target)
    $Payload = [IO.Path]::GetFullPath($Payload)
    if (!(Test-Path -LiteralPath (Join-Path $Target 'MiniMap_Mod.dll'))) { throw 'Ziel ist keine bestehende Mod-Installation.' }
    [xml]$oldInfo = Get-Content -LiteralPath (Join-Path $Target 'ModInfo.xml') -Raw
    if ($oldInfo.xml.Name.value -ne 'MiniMap_Mod') { throw 'Unerwartete Ziel-Mod.' }
    if ($ExpectedVersion -notmatch '^\d+\.\d+\.\d+\.\d+$') { throw 'Ungültige Version.' }
    [xml]$info = Get-Content -LiteralPath (Join-Path $Payload 'ModInfo.xml') -Raw
    if ($info.xml.Version.value -ne $ExpectedVersion -or $info.xml.Author.value -ne 'NAS4Killer') { throw 'Ungültiges Paket.' }
    $files = @('MiniMap_Mod.dll','ModInfo.xml','Config\XUi_InGame\windows.xml','Config\XUi_InGame\xui.xml','VERSION','README.md',('ReleaseNotes-'+$ExpectedVersion+'.txt'))
    foreach ($relative in $files) { if (!(Test-Path -LiteralPath (Join-Path $Payload $relative) -PathType Leaf)) { throw 'Paket unvollständig.' } }
    # Never kill the game or replace its loaded assembly.
    $game = Get-Process -Id $GameProcessId -ErrorAction SilentlyContinue
    if ($game) { Wait-Process -Id $GameProcessId -ErrorAction SilentlyContinue }
    if (Get-Process -Name '7DaysToDie' -ErrorAction SilentlyContinue) { throw 'Ein Spielprozess läuft noch. Update nicht installiert.' }
    [xml]$currentInfo = Get-Content -LiteralPath (Join-Path $Target 'ModInfo.xml') -Raw
    if ($currentInfo.xml.Name.value -ne 'MiniMap_Mod' -or [version]$currentInfo.xml.Version.value -ge [version]$ExpectedVersion) { throw 'Installation geändert oder bereits aktuell. Update abgebrochen.' }
    New-Item -ItemType Directory -Path $backup -Force | Out-Null
    # Back up every destination before changing any installed file.
    foreach ($relative in $files) {
        $destination = Join-Path $Target $relative
        $existing[$relative] = Test-Path -LiteralPath $destination
        if ($existing[$relative]) {
            $saved = Join-Path $backup $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $saved) -Force | Out-Null
            Copy-Item -LiteralPath $destination -Destination $saved
        }
    }
    foreach ($relative in $files) {
        $destination = Join-Path $Target $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        $changed += $relative
        Copy-Item -LiteralPath (Join-Path $Payload $relative) -Destination $destination -Force
        if ((Get-FileHash -LiteralPath $destination).Hash -ne (Get-FileHash -LiteralPath (Join-Path $Payload $relative)).Hash) { throw 'Installationsprüfung fehlgeschlagen.' }
    }
    [IO.File]::WriteAllText($log, ('Erfolgreich installiert: '+$ExpectedVersion), [Text.UTF8Encoding]::new($false))
} catch {
    $failure = $_.Exception.Message
    $rollbackFailed = $false
    foreach ($relative in $changed) {
        try {
            $destination = Join-Path $Target $relative
            if ($existing[$relative]) { Copy-Item -LiteralPath (Join-Path $backup $relative) -Destination $destination -Force }
            elseif (Test-Path -LiteralPath $destination) { Remove-Item -LiteralPath $destination }
        } catch { $rollbackFailed = $true }
    }
    [IO.File]::WriteAllText($log, ('Update fehlgeschlagen: '+$failure+'; Wiederherstellung fehlgeschlagen: '+$rollbackFailed), [Text.UTF8Encoding]::new($false))
    exit 1
}
