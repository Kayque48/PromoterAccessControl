# ============================================================
# Makefile — PromoterAccessControl (Linux / macOS)
# ============================================================

FRONTEND_PORT = 8000
BACKEND_DIR   = ControlePromotores.Api
VENV_DIR      = .venv

# ============================================================
# make install
# ============================================================
.PHONY: install
install:
	@echo ""
	@echo "-> Verificando .NET SDK..."
	@if ! command -v dotnet > /dev/null 2>&1; then \
		echo ""; \
		echo "ERRO: .NET SDK nao encontrado."; \
		echo "Instale com: sudo apt install dotnet-sdk-10.0"; \
		echo ""; \
		exit 1; \
	fi
	@echo "   .NET $$(dotnet --version) OK."
	@echo ""

	@echo "-> Verificando Python3..."
	@if ! command -v python3 > /dev/null 2>&1; then \
		echo ""; \
		echo "ERRO: Python3 nao encontrado."; \
		echo "Instale com: sudo apt install python3"; \
		echo ""; \
		exit 1; \
	fi
	@echo "   $$(python3 --version) OK."
	@echo ""

	@echo "-> Verificando MySQL..."
	@if ! command -v mysql > /dev/null 2>&1; then \
		echo ""; \
		echo "AVISO: cliente MySQL nao encontrado no PATH."; \
		echo "Instale com: sudo apt install mysql-client"; \
		echo "Certifique-se de que o servidor MySQL esta rodando antes de 'make run'."; \
		echo ""; \
	else \
		echo "   MySQL client OK."; \
	fi
	@echo ""
	@echo "   IMPORTANTE: configure a connection string em:"
	@echo "   $(BACKEND_DIR)/appsettings.json -> ConnectionStrings.MySqlConnection"
	@echo ""

	@echo "-> Criando ambiente virtual Python..."
	@if [ -d $(VENV_DIR) ]; then \
		echo "   .venv ja existe, pulando criacao."; \
	else \
		python3 -m venv $(VENV_DIR); \
		echo "   venv criado em $(VENV_DIR)/"; \
	fi
	@echo ""

	@echo "-> Restaurando pacotes NuGet..."
	@cd $(BACKEND_DIR) && dotnet restore
	@echo "   Pacotes NuGet OK."
	@echo ""

	@echo "Instalacao concluida! Rode: make run"
	@echo ""

# ============================================================
# make run
# ============================================================
.PHONY: run
run:
	@echo ""
	@echo "-> Verificando MySQL acessivel..."
	@if command -v mysqladmin > /dev/null 2>&1; then \
		if ! mysqladmin ping -h 127.0.0.1 --silent 2>/dev/null; then \
			echo ""; \
			echo "ERRO: MySQL nao responde em localhost:3306."; \
			echo "Inicie o servico: sudo systemctl start mysql"; \
			echo ""; \
			exit 1; \
		fi; \
		echo "   MySQL OK."; \
	else \
		echo "   (mysqladmin nao encontrado, pulando verificacao — o backend falhara se o banco nao estiver acessivel)"; \
	fi
	@echo ""

	@echo "-> Iniciando backend (.NET)..."
	@cd $(BACKEND_DIR) && dotnet run > ../backend.log 2>&1 &
	@printf "   Aguardando inicializacao"; \
	for i in 1 2 3 4 5 6 7 8 9 10; do \
		sleep 1; \
		printf "."; \
		if grep -q "Now listening on" ../backend.log 2>/dev/null; then break; fi; \
	done; \
	echo ""
	@if grep -q "Now listening on" backend.log 2>/dev/null; then \
		echo "   Backend OK."; \
	else \
		echo "   AVISO: backend pode ainda estar subindo. Veja backend.log para detalhes."; \
	fi
	@echo ""

	@echo "-> Iniciando frontend (porta $(FRONTEND_PORT))..."
	@$(VENV_DIR)/bin/python3 -m http.server $(FRONTEND_PORT) \
		--directory frontend > /dev/null 2>&1 &
	@sleep 1
	@echo "   Frontend OK."
	@echo ""

	@echo "============================================"
	@echo "  Projeto rodando!"
	@echo ""
	@echo "  Frontend : http://localhost:$(FRONTEND_PORT)/login.html"
	@echo "  API      : http://localhost:5297"
	@echo "  Swagger  : http://localhost:5297/swagger"
	@echo ""
	@echo "  Login: admin  |  Senha: senha123"
	@echo ""
	@echo "  Logs do backend : ./backend.log"
	@echo "  Para encerrar   : make stop"
	@echo "============================================"
	@echo ""

# ============================================================
# make stop
# ============================================================
.PHONY: stop
stop:
	@echo "-> Encerrando processos..."
	@-pkill -f "dotnet run" > /dev/null 2>&1 || true
	@-pkill -f "dotnet exec" > /dev/null 2>&1 || true
	@-pkill -f "http.server $(FRONTEND_PORT)" > /dev/null 2>&1 || true
	@rm -f backend.log
	@echo "   Processos encerrados."
	@echo ""

# ============================================================
# make clean
# ============================================================
.PHONY: clean
clean:
	@echo "-> Removendo venv e artefatos de build..."
	@rm -rf $(VENV_DIR)
	@echo "   AVISO: o banco de dados MySQL NAO e apagado automaticamente."
	@echo "   Para resetar o banco, acesse o MySQL e execute:"
	@echo "   DROP DATABASE ControlePromotores;"
	@echo "   Pronto. Rode make install para recomecar."
	@echo ""

# ============================================================
# make help
# ============================================================
.DEFAULT_GOAL := help
.PHONY: help
help:
	@echo ""
	@echo "  PromoterAccessControl -- Comandos (Linux/macOS):"
	@echo ""
	@echo "  make install  -> verifica dependencias, cria venv e restaura NuGet"
	@echo "  make run      -> verifica MySQL, sobe backend e frontend"
	@echo "  make stop     -> encerra os processos"
	@echo "  make clean    -> remove venv (banco MySQL permanece intacto)"
	@echo ""
	@echo "  Pre-requisito: MySQL rodando em localhost:3306"
	@echo "  Configure a senha em: $(BACKEND_DIR)/appsettings.json"
	@echo ""
	@echo "  No Windows: use install.bat e run.bat"
	@echo ""