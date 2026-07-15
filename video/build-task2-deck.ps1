# build-task2-deck.ps1 — embed Task 2 screenshots (as data-URIs) into task2-deck.src.html
# -> task2-deck.html (+ task2-deck-standalone.html for clean local F11 recording).
Add-Type -AssemblyName System.Drawing
$here  = $PSScriptRoot
$src   = Join-Path $here 'task2-deck.src.html'
$out   = Join-Path $here 'task2-deck.html'
$shots = Join-Path $here 'shots'

# A real screenshot from shots\ (resized to maxW). If missing, draw a labelled placeholder.
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
    $sf.Alignment=[System.Drawing.StringAlignment]::Center; $sf.LineAlignment=[System.Drawing.StringAlignment]::Center
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
  '__T2_SEARCH__'   = (DataShot 'task2-search.png'   'Search screen' 1100)
  '__T2_FIREBASE__' = (DataShot 'task2-firebase.png' 'Messy CoverInfo in Firebase' 1100)
  '__T2_FOLDER__'   = (DataShot 'task2-folder.png'   'Features / Search folder' 820)
}
foreach($k in $map.Keys){ $html = $html.Replace($k, $map[$k]) }
[System.IO.File]::WriteAllText($out, $html, (New-Object System.Text.UTF8Encoding($false)))
Write-Host ('built task2-deck.html (' + [int]((Get-Item $out).Length/1KB) + ' KB)')

$wrap = "<!doctype html>`n<html lang=`"en`">`n<head><meta charset=`"utf-8`"><meta name=`"viewport`" content=`"width=device-width,initial-scale=1`"><title>BookLab Task 2</title></head>`n<body>`n"
$standalone = $wrap + $html + "`n</body>`n</html>"
[System.IO.File]::WriteAllText((Join-Path $here 'task2-deck-standalone.html'), $standalone, (New-Object System.Text.UTF8Encoding($false)))
Write-Host 'also wrote task2-deck-standalone.html  (open locally + F11 to record clean)'
