# build-deck.ps1 — embed the app's real art (as data-URIs) into deck.src.html -> architecture-deck.html
# Artifacts block external images, so backgrounds + stickers are inlined. Re-run after editing deck.src.html.
Add-Type -AssemblyName System.Drawing
$here = $PSScriptRoot
$src  = Join-Path $here 'deck.src.html'
$out  = Join-Path $here 'architecture-deck.html'
$bg   = Join-Path $here '..\public\assets\backgrounds'
$obj  = Join-Path $here '..\public\assets\objects'

function DataJpg([string]$path,[int]$maxW,[int]$quality){
  $img = [System.Drawing.Image]::FromFile($path)
  try{
    $w=$img.Width; $h=$img.Height
    if($w -gt $maxW){ $h=[int]([double]$h*$maxW/$w); $w=$maxW }
    $bmp = New-Object System.Drawing.Bitmap $w,$h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.DrawImage($img,0,0,$w,$h); $g.Dispose()
    $enc = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object {$_.FormatID -eq [System.Drawing.Imaging.ImageFormat]::Jpeg.Guid}
    $ep = New-Object System.Drawing.Imaging.EncoderParameters 1
    $ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality,[long]$quality)
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms,$enc,$ep); $bmp.Dispose()
    'data:image/jpeg;base64,' + [Convert]::ToBase64String($ms.ToArray())
  } finally { $img.Dispose() }
}
function DataPng([string]$path){ 'data:image/png;base64,' + [Convert]::ToBase64String([System.IO.File]::ReadAllBytes($path)) }

$html = [System.IO.File]::ReadAllText($src)
$map = [ordered]@{
  '__COVER_BG__'  = (DataJpg (Join-Path $bg 'desert-sunset-bg.png') 1200 72)
  '__WRAP_BG__'   = (DataJpg (Join-Path $bg 'ocean-bg.png') 1200 72)
  '__ST_CAMEL__'  = (DataPng (Join-Path $obj 'camel.png'))
  '__ST_PALM__'   = (DataPng (Join-Path $obj 'palm.png'))
  '__ST_SUN__'    = (DataPng (Join-Path $obj 'sun.png'))
  '__ST_BOAT__'   = (DataPng (Join-Path $obj 'boat.png'))
  '__ST_TURTLE__' = (DataPng (Join-Path $obj 'turtle.png'))
  '__ST_STAR__'   = (DataPng (Join-Path $obj 'star.png'))
}
foreach($k in $map.Keys){ $html = $html.Replace($k, $map[$k]) }
[System.IO.File]::WriteAllText($out, $html, (New-Object System.Text.UTF8Encoding($false)))
Write-Host ('built architecture-deck.html (' + [int]((Get-Item $out).Length/1KB) + ' KB)')
