@echo off
cd /d "%~dp0"
where py >nul 2>nul
if errorlevel 1 (
  echo Python nao encontrado. Instale Python 3 e tente novamente.
  pause
  exit /b 1
)
start "" http://127.0.0.1:8765
py bridge\server.py
pause
