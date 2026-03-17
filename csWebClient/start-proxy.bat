@echo off
REM Quick Start for Chat Server Web Client
REM This script helps set up and run the Node.js proxy server

cls
echo.
echo ====================================================
echo   Chat Server Web Client - Quick Start
echo ====================================================
echo.

REM Check if Node.js is installed
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Node.js is not installed or not in PATH
    echo.
    echo Please install Node.js from: https://nodejs.org/
    echo.
    pause
    exit /b 1
)

echo [OK] Node.js found
node --version

REM Check if npm is installed
where npm >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] npm is not installed
    pause
    exit /b 1
)

echo [OK] npm found
npm --version
echo.

REM Check if dependencies are installed
if not exist "node_modules" (
    echo [INFO] Installing dependencies...
    call npm install
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to install dependencies
        pause
        exit /b 1
    )
    echo [OK] Dependencies installed
) else (
    echo [OK] Dependencies already installed
)

echo.
echo ====================================================
echo   Starting WebSocket Proxy Server
echo ====================================================
echo.
echo Make sure csServer2 is running on localhost:8888
echo.
echo Proxy will listen on: ws://localhost:9090
echo.
echo Press Ctrl+C to stop the proxy
echo.

call npm start
