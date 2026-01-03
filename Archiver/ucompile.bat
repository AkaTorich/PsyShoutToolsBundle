@echo off

:: Get solution name from parameter or search in current folder
if "%~1"=="" goto search_solution
set SOLUTION_FILE=%~1
goto start_build

:search_solution
echo No parameter provided, searching for solution in current folder...
for %%f in (*.sln) do (
    set SOLUTION_FILE=%%f
    echo Found solution: %%f
    echo.
    goto start_build
)

echo ERROR: No .sln files found in current folder
echo.
echo USAGE: %~nx0 ^<solution_name.sln^>
echo        or run in folder containing .sln file
echo.
echo EXAMPLE: %~nx0 MyProject.sln
echo.
pause
exit /b 1

:start_build
set LOG_DIR=BuildLogs

:: Create logs folder if it doesn't exist
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

echo ================================
echo   Visual Studio Project Builder
echo ================================
echo.

:: Check if MSBuild is available in PATH
where msbuild >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: MSBuild not found in PATH!
    echo Make sure MSBuild is added to PATH environment variable.
    pause
    exit /b 1
)

:: Check if solution file exists
if not exist "%SOLUTION_FILE%" (
    echo ERROR: Solution file "%SOLUTION_FILE%" not found!
    echo Check the path and filename.
    pause
    exit /b 1
)

echo Starting build for solution: %SOLUTION_FILE%
echo.

:: Build Debug x86
echo [1/4] Building configuration: Debug ^| Platform: x86
msbuild "%SOLUTION_FILE%" /p:Configuration=Debug /p:Platform=x86 /m /v:minimal /flp:logfile="%LOG_DIR%\build_Debug_x86.log";verbosity=normal
if %errorlevel% equ 0 (
    echo ✓ Successfully built: Debug x86
    set /a success_count=1
) else (
    echo ✗ Build failed: Debug x86 ^(see %LOG_DIR%\build_Debug_x86.log^)
    set /a failed_count=1
)
echo.

:: Build Debug x64
echo [2/4] Building configuration: Debug ^| Platform: x64
msbuild "%SOLUTION_FILE%" /p:Configuration=Debug /p:Platform=x64 /m /v:minimal /flp:logfile="%LOG_DIR%\build_Debug_x64.log";verbosity=normal
if %errorlevel% equ 0 (
    echo ✓ Successfully built: Debug x64
    set /a success_count=%success_count%+1
) else (
    echo ✗ Build failed: Debug x64 ^(see %LOG_DIR%\build_Debug_x64.log^)
    set /a failed_count=%failed_count%+1
)
echo.

:: Build Release x86
echo [3/4] Building configuration: Release ^| Platform: x86
msbuild "%SOLUTION_FILE%" /p:Configuration=Release /p:Platform=x86 /m /v:minimal /flp:logfile="%LOG_DIR%\build_Release_x86.log";verbosity=normal
if %errorlevel% equ 0 (
    echo ✓ Successfully built: Release x86
    set /a success_count=%success_count%+1
) else (
    echo ✗ Build failed: Release x86 ^(see %LOG_DIR%\build_Release_x86.log^)
    set /a failed_count=%failed_count%+1
)
echo.

:: Build Release x64
echo [4/4] Building configuration: Release ^| Platform: x64
msbuild "%SOLUTION_FILE%" /p:Configuration=Release /p:Platform=x64 /m /v:minimal /flp:logfile="%LOG_DIR%\build_Release_x64.log";verbosity=normal
if %errorlevel% equ 0 (
    echo ✓ Successfully built: Release x64
    set /a success_count=%success_count%+1
) else (
    echo ✗ Build failed: Release x64 ^(see %LOG_DIR%\build_Release_x64.log^)
    set /a failed_count=%failed_count%+1
)
echo.

:: Display results
echo ================================
echo         BUILD RESULTS
echo ================================
echo Total configurations: 4
if not defined failed_count set failed_count=0
if not defined success_count set success_count=0
echo Successfully built:   %success_count%
echo Failed:               %failed_count%
echo.

if %failed_count% equ 0 (
    echo 🎉 All configurations built successfully!
) else (
    echo ⚠️  Some configurations failed to build.
    echo    Check logs in "%LOG_DIR%" folder
)

echo.
echo Build logs saved to folder: %LOG_DIR%
echo.

pause