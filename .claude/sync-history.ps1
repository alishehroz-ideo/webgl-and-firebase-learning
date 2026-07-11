<#
    sync-history.ps1  -  Mirror Claude Code chat + memory into the git repo.

    Claude Code stores this project's conversation transcripts and memory under
        %USERPROFILE%\.claude\projects\<derived-name>\
    where <derived-name> is the project's absolute path with every non
    alphanumeric character replaced by '-'. We mirror that folder into the
    repo's .claude-history\ so the chat travels with the code.

      save     copy  ~/.claude/projects/<derived>/  ->  repo/.claude-history/
      restore  copy  repo/.claude-history/           ->  ~/.claude/projects/<derived>/

    Copies are merge-only (never delete), so nothing is lost if a session
    exists on one machine but not the other. Run 'restore' after pulling on a
    new machine, then use Claude Code's /resume to pick up the conversation.

    NOTE: For 'restore' to land in the right place, launch Claude Code from the
    same absolute path on every machine (e.g. d:\DummyProjects\webgl and
    firebase learning). The derived folder name is computed from that path.
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('save', 'restore')]
    [string]$Direction
)

$ErrorActionPreference = 'Stop'

# Repo root = parent of this .claude folder
$repoRoot  = Split-Path -Parent $PSScriptRoot
$derived   = ($repoRoot -replace '[^a-zA-Z0-9]', '-')
$claudeDir = Join-Path $env:USERPROFILE ".claude\projects\$derived"
$histDir   = Join-Path $repoRoot ".claude-history"

function Copy-Tree($src, $dst) {
    if (-not (Test-Path $src)) { return $false }
    New-Item -ItemType Directory -Force -Path $dst | Out-Null
    $items = Get-ChildItem -Path $src -Force -ErrorAction SilentlyContinue
    if (-not $items) { return $false }
    foreach ($item in $items) {
        # Skip nothing special; both trees are small (JSONL + memory md files).
        Copy-Item -Path $item.FullName -Destination $dst -Recurse -Force -ErrorAction SilentlyContinue
    }
    return $true
}

try {
    if ($Direction -eq 'save') {
        $ok = Copy-Tree $claudeDir $histDir
        if ($ok) { Write-Host "[claude-sync] saved chat -> .claude-history" }
    }
    else {
        $ok = Copy-Tree $histDir $claudeDir
        if ($ok) { Write-Host "[claude-sync] restored chat -> $claudeDir" }
    }
}
catch {
    # Never let a sync hiccup interrupt the Claude session.
    Write-Host "[claude-sync] skipped: $($_.Exception.Message)"
}
exit 0
