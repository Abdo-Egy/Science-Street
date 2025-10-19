
@echo off
rem ------------------------------------------------------------
rem  CreateAllCodeWithPath.bat
rem  Scans the current folder and all sub‑folders for *.cs files
rem  and writes:
rem      <FullPath>
rem      "<file‑content>"
rem  into a single file called my_all_code.txt
rem ------------------------------------------------------------

setlocal EnableDelayedExpansion

rem ----------  CONFIGURATION  ----------
set "OUTFILE=my_all_code.txt"
rem ---------------------------------------

rem  Make sure the output file starts empty
> "%OUTFILE%" (
    rem  nothing – just truncate the file
)

rem  Count how many .cs files we will process
set /a TOTAL=0
for /R %%F in (*.cs) do (
    set /a TOTAL+=1
)

rem  If there are no .cs files, bail out
if %TOTAL%==0 (
    echo No *.cs files found in the current folder tree.
    pause
    exit /b
)

echo Found %TOTAL% *.cs files.
echo -----------------------------------------

rem  Process each file
set /a CUR=0
for /R %%F in (*.cs) do (
    set /a CUR+=1

    rem  Show progress
    echo Processing !CUR! of %TOTAL%: %%~fF

    rem  Write the full path
    echo %%~fF >> "%OUTFILE%"

    rem  Write the opening quote
    echo ^" >> "%OUTFILE%"

    rem  Dump the file content verbatim
    type "%%~fF" >> "%OUTFILE%"

    rem  Write the closing quote
    echo ^" >> "%OUTFILE%"

    rem  Blank line between files
    echo. >> "%OUTFILE%"
)

echo -----------------------------------------
echo Done – all code is in "%OUTFILE%"
echo Press any key to close this window.
pause >nul