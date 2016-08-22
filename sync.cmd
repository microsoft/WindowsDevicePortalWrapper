@echo off
echo --- Fetching master branch
git fetch upstream
echo --- Merging contents of master
git merge upstream/master
set /p response=Push merged to fork (Y/N)? 
if /i "%response:~,1%" equ "Y" goto push
if /i "%response:~,1%" equ "y" goto push
goto done

:push
echo --- Pushing merged fork
git push origin
goto done

:done
echo --- Done
exit /b


   
