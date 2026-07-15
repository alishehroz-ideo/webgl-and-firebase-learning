# build-portfolio-doc.ps1 — convert previous-work.md to Word, page break before each
# project, GameBull screenshots as a grid, links clickable, images embedded.
$ErrorActionPreference = 'Stop'
$pandoc = "C:\Users\$env:USERNAME\AppData\Local\Pandoc\pandoc.exe"
if (-not (Test-Path $pandoc)) { $pandoc = (Get-Command pandoc).Source }
$port = $PSScriptRoot
$root = Split-Path $port -Parent
$tmp  = Join-Path $port ".tmp_pw.md"
$pb = @('```{=openxml}', '<w:p><w:r><w:br w:type="page"/></w:r></w:p>', '```', '')
$o = New-Object System.Collections.Generic.List[string]
foreach ($l in (Get-Content -LiteralPath (Join-Path $port 'previous-work.md') -Encoding UTF8)) {
    if ($l -match '^##\s') { $pb | ForEach-Object { $o.Add($_) } }
    $o.Add($l)
}
Set-Content -LiteralPath $tmp -Value $o -Encoding UTF8
Push-Location $port                          # so shots/*.jpeg image paths resolve
& $pandoc '.tmp_pw.md' -o (Join-Path $root 'Ali-Shehroz-Previous-Work.docx')
Pop-Location
Remove-Item $tmp -Force
Write-Host 'built Ali-Shehroz-Previous-Work.docx'
