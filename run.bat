@echo off
chcp 65001 > NUL
echo.
echo ============================================
echo   PromoterAccessControl -- Iniciando
echo   Windows
echo ============================================
echo.

:: ----------------------------------------
:: Verificar se install.bat foi executado
:: ----------------------------------------
if not exist .venv (
    echo ERRO: Ambiente virtual nao encontrado.
    echo Execute install.bat primeiro.
    echo.
    pause
    exit /b 1
)

if not exist ControlePromotores.Api\obj (
    echo ERRO: Pacotes NuGet nao restaurados.
    echo Execute install.bat primeiro.
    echo.
    pause
    exit /b 1
)

:: ----------------------------------------
:: Verificar MySQL acessivel
:: ----------------------------------------
echo -^> Verificando MySQL em localhost:3306...
mysqladmin ping -h 127.0.0.1 --silent > NUL 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo AVISO: nao foi possivel confirmar que o MySQL esta rodando.
    echo Certifique-se de que o servico MySQL esta ativo antes de continuar.
    echo Verifique tambem a connection string em:
    echo    ControlePromotores.Api\appsettings.json
    echo.
    choice /C SN /M "Continuar mesmo assim?"
    if %ERRORLEVEL% equ 2 (
        echo Operacao cancelada.
        pause
        exit /b 1
    )
) else (
    echo    MySQL OK.
)
echo.

:: ----------------------------------------
:: Iniciar backend em janela separada
:: ----------------------------------------
echo -^> Iniciando backend (.NET)...
start "PromoterAccessControl -- Backend" cmd /k ^
    "cd ControlePromotores.Api && dotnet run"

echo    Aguardando inicializacao (5s)...
timeout /t 5 /nobreak > NUL
echo    Backend OK.
echo.

:: ----------------------------------------
:: Iniciar frontend em janela separada
:: ----------------------------------------
echo -^> Iniciando frontend (porta 8000)...
start "PromoterAccessControl -- Frontend" cmd /k ^
    ".venv\Scripts\python -m http.server 8000 --directory frontend"

timeout /t 1 /nobreak > NUL
echo    Frontend OK.
echo.

:: ----------------------------------------
:: Informacoes de acesso
:: ----------------------------------------
echo ============================================
echo   Projeto rodando!
echo.
echo   Frontend : http://localhost:8000/Login.html
echo   API      : http://localhost:5297
echo   Swagger  : http://localhost:5297/swagger
echo.
echo   Banco    : MySQL localhost:3306
echo   Database : ControlePromotores
echo.
echo   Login: admin  ^|  Senha: senha123
echo.
echo   Para encerrar: feche as janelas
echo   "Backend" e "Frontend" que abriram.
echo ============================================
echo.
pause
