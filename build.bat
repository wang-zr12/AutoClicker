@echo off
echo ========================================
echo Building AutoClicker.exe...
echo ========================================

REM Check for csc.exe (C# compiler)
where csc.exe >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Locating C# compiler...
    set CSC_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
    if not exist "!CSC_PATH!" (
        set CSC_PATH=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe
    )
    if not exist "!CSC_PATH!" (
        echo ERROR: C# compiler not found!
        echo Please install .NET Framework 4.0 or higher
        pause
        exit /b 1
    )
) else (
    set CSC_PATH=csc.exe
)

REM Compile
echo Compiling...
"%CSC_PATH%" /target:winexe /out:AutoClicker.exe /optimize+ /win32icon:NONE AutoClicker.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo SUCCESS! AutoClicker.exe created!
    echo ========================================
    echo.
    echo You can now run AutoClicker.exe
    echo.
) else (
    echo.
    echo ========================================
    echo Build FAILED!
    echo ========================================
    echo.
)

pause
