@echo off
setlocal
cd /d "%~dp0"
echo ==========================================
echo       JVPLEY 1.3 - CRIADOR DE EXE
echo ==========================================
where py >nul 2>nul
if errorlevel 1 (
  echo Python nao encontrado. Instale Python 3 para Windows e tente novamente.
  pause
  exit /b 1
)
echo.
echo Instalando ou atualizando PyInstaller...
py -m pip install --upgrade pyinstaller
if errorlevel 1 (
  echo Nao foi possivel instalar o PyInstaller.
  pause
  exit /b 1
)
if not exist "bridge\__init__.py" type nul > "bridge\__init__.py"
echo.
echo Gerando JVPLEY-1.3.exe...
py -m PyInstaller --noconfirm --clean --onedir --windowed --name JVPLEY-1.3 --add-data "web;web" --add-data "bridge;bridge" launcher.py
if errorlevel 1 (
  echo.
  echo A criacao do EXE falhou. Confira as mensagens acima.
  pause
  exit /b 1
)
echo.
echo PRONTO! O programa esta em:
echo dist\JVPLEY-1.3\JVPLEY-1.3.exe
echo.
echo Copie a pasta JVPLEY-1.3 inteira para manter os arquivos necessarios.
pause
