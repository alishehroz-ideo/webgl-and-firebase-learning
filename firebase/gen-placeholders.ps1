# Generates simple placeholder art (backgrounds + stickers) into public/assets,
# so the app's remote images live on OUR Firebase Hosting instead of a flaky 3rd party.
# Uses System.Drawing (Windows). Re-run any time to regenerate.

Add-Type -AssemblyName System.Drawing

function New-Background($path, $w, $h, $bg, $fg, $text) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear($bg)
    $font = New-Object System.Drawing.Font('Segoe UI', [int]($h / 9), [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush($fg)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $rect = New-Object System.Drawing.RectangleF(0, 0, $w, $h)
    $g.DrawString($text, $font, $brush, $rect, $sf)
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "  bg  -> $path"
}

function New-Sticker($path, $size, $fill, $text) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::Transparent)
    $pad = [int]($size * 0.08)
    $body = New-Object System.Drawing.SolidBrush($fill)
    $g.FillEllipse($body, $pad, $pad, $size - 2 * $pad, $size - 2 * $pad)
    $font = New-Object System.Drawing.Font('Segoe UI', [int]($size / 8), [System.Drawing.FontStyle]::Bold)
    $tb = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $rect = New-Object System.Drawing.RectangleF(0, 0, $size, $size)
    $g.DrawString($text, $font, $tb, $rect, $sf)
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "  obj -> $path"
}

$root   = Split-Path -Parent $PSScriptRoot
$bgDir  = Join-Path $root 'public\assets\backgrounds'
$objDir = Join-Path $root 'public\assets\objects'
New-Item -ItemType Directory -Force -Path $bgDir  | Out-Null
New-Item -ItemType Directory -Force -Path $objDir | Out-Null

New-Background (Join-Path $bgDir 'living_room.png') 1920 1080 ([System.Drawing.Color]::FromArgb(237,227,211)) ([System.Drawing.Color]::FromArgb(85,72,58)) 'Living Room'
New-Background (Join-Path $bgDir 'garden.png')      1920 1080 ([System.Drawing.Color]::FromArgb(221,239,214)) ([System.Drawing.Color]::FromArgb(54,94,59))  'Garden'

New-Sticker (Join-Path $objDir 'sofa.png') 500 ([System.Drawing.Color]::FromArgb(139,90,60))  'Sofa'
New-Sticker (Join-Path $objDir 'lamp.png') 500 ([System.Drawing.Color]::FromArgb(224,184,76)) 'Lamp'
New-Sticker (Join-Path $objDir 'tree.png') 500 ([System.Drawing.Color]::FromArgb(78,139,74))  'Tree'

Write-Host 'done.'
