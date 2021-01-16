$hasdeps = $true
if (!(Get-Command inkscape)) {
	echo "Make sure Inkscape is installed and accessible from your PATH"
	$hasdeps = $false
}
if (!(Get-Command magick)) {
	echo "Make sure ImageMagick is installed and accessible from your PATH"
	$hasdeps = $false
}
if (!$hasdeps) {
	exit
}

$workdir = "./workdir"
if (!(Test-Path $workdir)) {
	New-Item -ItemType Directory $workdir
}

# Percentages below are based on the default icon size of notification area
# icons, which is 16x16. Based on the scaling level used by Windows, the
# appropriate icon size is selected. The larger sizes may also be used by
# Windows Explorer.
$resolutions = 
	"16", # 100%
	"20", # 125%
	"24", # 150%
	"28", # 175%
	"32", # 200%
	"36", # 225%
	"40", # 250%
	"48", # 300%
	"64" # 400%

$iconstyles = @{
	plain = "key";
	ahead = "key;orb-green;arrow-up";
	behind = "key;orb-blue;arrow-down";
	diverged = "key;orb-yellow";
}

foreach ($style in $iconstyles.GetEnumerator()) {
	Write-Output ""
	Write-Output "Generating bitmaps (style: $($style.Name))"
	Write-Output "------------------"
	
	foreach ($resolution in $resolutions) {
		inkscape `
			-w $resolution `
			-h $resolution `
			--export-area-page `
			--export-filename="$workdir/pass-winmenu-$($style.Name)-${resolution}x${resolution}.png" `
			icon-$($style.Name).svg
			#--export-id="$($style.Value)" `
			#--export-id-only `
	}
	
	Write-Output ""
	Write-Output "Building icon file (style: $($style.Name))"
	Write-Output "------------------"
	
	$icofile = "$workdir/pass-winmenu-$($style.Name).ico"

	Write-Output "Generating $icofile"
	$filenames = $resolutions | Foreach-Object { "$workdir/pass-winmenu-$($style.Name)-${_}x${_}.png" }
	magick $filenames $icofile
	
	Write-Output "Copying $icofile to ../pass-winmenu/embedded"
	Copy-Item -Path $icofile -Destination ../pass-winmenu/embedded
}

