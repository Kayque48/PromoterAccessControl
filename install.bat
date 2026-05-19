@echo off
chcp 65001 > NUL
echo.
echo ============================================
echo   PromoterAccessControl -- Instalacao
echo   Windows
echo ============================================
echo.

:: ----------------------------------------
:: Verificar .NET SDK
:: ----------------------------------------
echo -^> Verificando .NET SDK...
dotnet --version > NUL 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo ERRO: .NET SDK nao encontrado.
    echo Baixe em: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)
for /f "tokens=*" %%v in ('dotnet --version') do echo    .NET %%v OK.
echo.

:: ----------------------------------------
:: Verificar Python
:: ----------------------------------------
echo -^> Verificando Python...
python --version > NUL 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo ERRO: Python nao encontrado.
    echo Baixe em: https://www.python.org/downloads/
    echo Marque a opcao "Add Python to PATH" durante a instalacao.
    echo.
    pause
    exit /b 1
)
for /f "tokens=*" %%v in ('python --version') do echo    %%v OK.
echo.

:: ----------------------------------------
:: Verificar MySQL
:: ----------------------------------------
echo -^> Verificando MySQL...
mysql --version > NUL 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo AVISO: cliente MySQL nao encontrado no PATH.
    echo Baixe o MySQL Community Server em: https://dev.mysql.com/downloads/mysql/
    echo Certifique-se de que o servico MySQL esta rodando antes de executar run.bat.
    echo.
) else (
    for /f "tokens=*" %%v in ('mysql --version') do echo    %%v OK.
)
echo.
echo    IMPORTANTE: configure a connection string antes de rodar o projeto:
echo    ControlePromotores.Api\appsettings.json
echo    -^> ConnectionStrings.MySqlConnection
echo    (ajuste User e Password conforme seu ambiente)
echo.

:: ----------------------------------------
:: Criar ambiente virtual Python
:: ----------------------------------------
echo -^> Criando ambiente virtual Python...
if exist .venv (
    echo    .venv ja existe, pulando criacao.
) else (
    python -m venv .venv
    echo    venv criado em .venv\
)
echo.

:: ----------------------------------------
:: Restaurar pacotes NuGet
:: ----------------------------------------
echo -^> Restaurando pacotes NuGet...
cd ControlePromotores.Api
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo.
    echo ERRO: Falha ao restaurar pacotes NuGet.
    echo.
    pause
    exit /b 1
)
cd ..
echo    Pacotes NuGet OK.
echo.

echo ============================================
echo   Instalacao concluida!
echo   Agora execute: run.bat
echo ============================================
echo.
pause
