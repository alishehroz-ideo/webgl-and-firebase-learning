# build-deck.ps1 — embed the app's real art (as data-URIs) into deck.src.html -> architecture-deck.html
# Artifacts block external images, so backgrounds + stickers are inlined. Re-run after editing deck.src.html.
Add-Type -AssemblyName System.Drawing
$here = $PSScriptRoot
$src  = Join-Path $here 'deck.src.html'
$out  = Join-Path $here 'architecture-deck.html'
$bg   = Join-Path $here '..\public\assets\backgrounds'
$obj  = Join-Path $here '..\public\assets\objects'
$shots= Join-Path $here 'shots'

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

# A real screenshot from shots\ (resized to maxW). If the file is missing, draw a
# labelled placeholder so the deck still builds and shows what to add.
function DataShot([string]$file,[string]$label,[int]$maxW){
  $path = Join-Path $shots $file
  $ms = New-Object System.IO.MemoryStream
  if(Test-Path $path){
    $img=[System.Drawing.Image]::FromFile($path)
    try{
      $w=$img.Width; $h=$img.Height
      if($w -gt $maxW){ $h=[int]([double]$h*$maxW/$w); $w=$maxW }
      $bmp=New-Object System.Drawing.Bitmap $w,$h
      $g=[System.Drawing.Graphics]::FromImage($bmp)
      $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
      $g.DrawImage($img,0,0,$w,$h); $g.Dispose()
      $bmp.Save($ms,[System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    } finally { $img.Dispose() }
  } else {
    $w=$maxW; $h=[int]($maxW*0.6)
    $bmp=New-Object System.Drawing.Bitmap $w,$h
    $g=[System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::FromArgb(246,238,223))
    $pen=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(214,196,166),3)
    $pen.DashStyle=[System.Drawing.Drawing2D.DashStyle]::Dash
    $g.DrawRectangle($pen,18,18,($w-37),($h-37))
    $sf=New-Object System.Drawing.StringFormat
    $sf.Alignment=[System.Drawing.StringAlignment]::Center
    $sf.LineAlignment=[System.Drawing.StringAlignment]::Center
    $br=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(150,128,100))
    $f1=New-Object System.Drawing.Font('Segoe UI',20,[System.Drawing.FontStyle]::Bold)
    $f2=New-Object System.Drawing.Font('Consolas',15)
    $g.DrawString($label,$f1,$br,(New-Object System.Drawing.RectangleF(0,([single]($h/2-40)),$w,40)),$sf)
    $g.DrawString("drop  shots/$file",$f2,$br,(New-Object System.Drawing.RectangleF(0,([single]($h/2+4)),$w,28)),$sf)
    $g.Dispose(); $bmp.Save($ms,[System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
  }
  'data:image/png;base64,' + [Convert]::ToBase64String($ms.ToArray())
}

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
  '__IMG_PROJECT__'  = (DataShot 'project-folder.png' 'Unity project folder' 820)
  '__IMG_EDITOR__'   = (DataShot 'editor-screen.png'  'BookLab editor screen' 1100)
  '__IMG_FIREBASE__' = (DataShot 'firebase-data.png'  'Firebase - a saved book' 1100)
}
foreach($k in $map.Keys){ $html = $html.Replace($k, $map[$k]) }
[System.IO.File]::WriteAllText($out, $html, (New-Object System.Text.UTF8Encoding($false)))
Write-Host ('built architecture-deck.html (' + [int]((Get-Item $out).Length/1KB) + ' KB)')

# Standalone document for recording locally (open in Chrome + F11 = no toolbar, no browser chrome).
$wrap = "<!doctype html>`n<html lang=`"en`">`n<head><meta charset=`"utf-8`"><meta name=`"viewport`" content=`"width=device-width,initial-scale=1`"><title>BookLab</title></head>`n<body>`n"
$standalone = $wrap + $html + "`n</body>`n</html>"
$out2 = Join-Path $here 'deck-standalone.html'
[System.IO.File]::WriteAllText($out2, $standalone, (New-Object System.Text.UTF8Encoding($false)))
Write-Host 'also wrote deck-standalone.html  (open locally + F11 to record clean)'
