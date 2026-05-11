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

	@echo "-> Criando ambiente virtual Python..."
	@python3 -m venv $(VENV_DIR)
	@echo "   venv criado em $(VENV_DIR)/"
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
	@echo "-> Iniciando backend (.NET)..."
	@cd $(BACKEND_DIR) && dotnet run &
	@echo "   Aguardando inicializacao..."
	@sleep 4

	@echo "-> Iniciando frontend (porta $(FRONTEND_PORT))..."
	@$(VENV_DIR)/bin/python3 -m http.server $(FRONTEND_PORT) \
		--directory frontend > /dev/null 2>&1 &
	@sleep 1

	@echo ""
	@echo "Projeto rodando!"
	@echo ""
	@echo "   Frontend : http://localhost:$(FRONTEND_PORT)/Login.html"
	@echo "   API      : http://localhost:5297"
	@echo "   Swagger  : http://localhost:5297/swagger"
	@echo ""
	@echo "   Login: admin  |  Senha: senha123"
	@echo ""
	@echo "   Para encerrar: make stop"
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
	@echo "   Processos encerrados."
	@echo ""

# ============================================================
# make clean
# ============================================================
.PHONY: clean
clean:
	@echo "-> Removendo venv e banco local..."
	@rm -rf $(VENV_DIR)
	@rm -f $(BACKEND_DIR)/database.db \
	       $(BACKEND_DIR)/database.db-shm \
	       $(BACKEND_DIR)/database.db-wal
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
	@echo "  make install  -> instala dependencias e cria venv"
	@echo "  make run      -> sobe backend e frontend"
	@echo "  make stop     -> encerra os processos"
	@echo "  make clean    -> remove venv e banco local"
	@echo ""
	@echo "  No Windows: use install.bat e run.bat"
	@echo ""
