<#
    sync.ps1  -  One-command sync of code + Claude Code chat between machines.

    Workflow (office <-> home), using the SAME folder path on both machines:

        End of a session:   .\sync.ps1 push      # capture chat, commit, push
        Start on other PC:  .\sync.ps1 pull      # pull, restore chat, then /resume

    'push'  ->  mirror chat into .claude-history, commit everything, push
    'pull'  ->  pull latest, then restore chat into ~/.claude so /resume works

    This is fully manual: nothing is committed or pushed unless you run it.
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('push', 'pull')]
    [string]$Action
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if ($Action -eq 'push') {
    Write-Host "==> Capturing Claude chat into .claude-history ..."
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\.claude\sync-history.ps1" save

    Write-Host "==> Staging changes ..."
    git add -A

    git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) {
        $stamp = Get-Date -Format 'yyyy-MM-dd HH:mm'
        git commit -m "sync: $stamp"
        Write-Host "==> Pushing ..."
        git push
    }
    else {
        Write-Host "Nothing to commit. Working tree clean."
    }
}
else {
    Write-Host "==> Pulling latest ..."
    git pull
    Write-Host "==> Restoring Claude chat into ~/.claude ..."
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\.claude\sync-history.ps1" restore
    Write-Host ""
    Write-Host "Done. Open Claude Code here and run /resume to continue the conversation."
}
