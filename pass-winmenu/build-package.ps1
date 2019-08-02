# This scripts combines all the files necessary to run pass-winmenu as a standalone program,
# and can package them into a ZIP file as well.

param(
# Re-copy all dependencies and libraries to the package directory 
# (normally they are maintained between builds).
	[switch]$Clean,
# Combine all required files into a ZIP file.
	[switch]$Package,
# Compress the resulting zip file. Only makes sense with -Package.
	[switch]$Compress,
# Include a standalone version of GPG in the dependencies.
	[switch]$WithGpg
)

$PKGDIR="bin/Release-Packaged"
$INCLUDEDIR="$PKGDIR/lib"
$ZIPDIR="bin/"
$ZIPNAME="pass-winmenu.zip"

if($Clean){
	if(Test-Path "$PKGDIR"){
		Remove-Item -recurse "$PKGDIR"
	}
	mkdir "$PKGDIR"
}else{
	if(Test-Path "$PKGDIR/pass-winmenu.exe"){
		Remove-Item "$PKGDIR/pass-winmenu.exe"
	}
	if(Test-Path "$PKGDIR/pass-winmenu.yaml"){
		Remove-Item "$PKGDIR/pass-winmenu.yaml"
	}
}

Copy-Item -recurse "bin/Release/lib/win32" "$PKGDIR/lib/win32"
# The PDB files aren't used, so they can be removed.
Remove-Item -recurse "$PKGDIR/lib/win32/x64/*.pdb"
Remove-Item -recurse "$PKGDIR/lib/win32/x86/*.pdb"

Copy-Item "bin/Release/pass-winmenu.exe" "$PKGDIR/pass-winmenu.exe"

if($WithGpg){
	tools/7za.exe x -aos "include/GnuPG.zip" "-o$INCLUDEDIR"
	Copy-Item "embedded/default-config.yaml" "$PKGDIR/pass-winmenu.yaml"
	tools/patch.exe --no-backup-if-mismatch "$PKGDIR/pass-winmenu.yaml" "include/packaged-config.patch"
}else{
	$ZIPNAME="pass-winmenu-nogpg.zip"
	Copy-Item "embedded/default-config.yaml" "$PKGDIR/pass-winmenu.yaml"
	tools/patch.exe --no-backup-if-mismatch "$PKGDIR/pass-winmenu.yaml" "include/packaged-config-nogpg.patch"
}

if($Package){
	$ZIPPATH = "$ZIPDIR$ZIPNAME"
	if(Test-Path "$ZIPPATH"){
		Write-Output "Removing old package: $ZIPPATH"
		Remove-Item "$ZIPPATH"
	}
	$STARTDIR=$PWD
	Set-Location $ZIPDIR
	if($Compress){
		# These options seem to result in the smallest file size.
		../tools/7za.exe a -mm=Deflate -mfb=258 -mpass=15 "$ZIPNAME" "Release-Packaged/*"
	}else{
		../tools/7za.exe a "$ZIPNAME" "Release-Packaged/*"
	}
	../tools/7za.exe rn "$ZIPNAME" "Release-Packaged" "pass-winmenu"
	Set-Location $STARTDIR
}
