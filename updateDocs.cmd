ECHO OFF

if "%1" equ "" ( 
echo Must provide master repo path
exit /b
)

del *.html
del *.png
del *.css
del *.js
rmdir /S /Q search
rmdir /S /Q Samples\*\bin
rmdir /S /Q Samples\*\obj
rmdir /S /Q WindowsDevicePortalWrapper\*\bin
rmdir /S /Q WindowsDevicePortalWrapper\*\obj

pushd %1
doxygen docconfig.txt
popd
move %1\html\* .
move %1\html\search search
git add *
git commit -m "Update code documentation via UpdateDocs.cmd script"
REM git push
