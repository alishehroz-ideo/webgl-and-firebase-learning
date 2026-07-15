# build-handover-docs.ps1 — convert the handover markdowns to Word .docx, inserting a
# page break before each "## " section so topics don't cram together. Images embed
# automatically (a .docx is a zip; images land in word/media/ inside it).
$ErrorActionPreference = 'Stop'
$pandoc = "C:\Users\$env:USERNAME\AppData\Local\Pandoc\pandoc.exe"
if (-not (Test-Path $pandoc)) { $pandoc = (Get-Command pandoc).Source }
$notes = $PSScriptRoot
$root  = Split-Path $notes -Parent
$pb = @('```{=openxml}', '<w:p><w:r><w:br w:type="page"/></w:r></w:p>', '```', '')

function Build($srcName, $outName) {
    $src = Join-Path $notes $srcName
    $tmp = Join-Path $notes (".tmp_" + $srcName)
    $o = New-Object System.Collections.Generic.List[string]
    foreach ($l in (Get-Content -LiteralPath $src -Encoding UTF8)) {
        if ($l -match '^##\s') { $pb | ForEach-Object { $o.Add($_) } }
        $o.Add($l)
    }
    Set-Content -LiteralPath $tmp -Value $o -Encoding UTF8
    Push-Location $notes                      # so ../video/shots/*.png image paths resolve
    & $pandoc ([IO.Path]::GetFileName($tmp)) -o (Join-Path $root $outName) --toc --toc-depth=2
    Pop-Location
    Remove-Item $tmp -Force
}

Build 'handover.md'       'BookLab-Task1-Handover.docx'
Build 'handover-task2.md' 'BookLab-Task2-Handover.docx'
Write-Host "built both handover docs (page breaks between sections)"
