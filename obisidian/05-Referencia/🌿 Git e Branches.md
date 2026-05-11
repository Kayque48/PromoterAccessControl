# 🌿 Git — Branches e Fluxo

> Convenção de branches e comandos úteis para o time.

---

## Estrutura de Branches

```
main
  └── Branch estável. Só recebe merges revisados.

feature/database-integration
  └── Integração com banco de dados (EF Core, migrations)

feature/frontend-integration  ← nova branch sugerida
  └── Conexão frontend ↔ backend

feature/mysql-migration       ← nova branch sugerida
  └── Trocar SQLite por MySQL

docs/obsidian-vault           ← esta branch
  └── Documentação do projeto no Obsidian
```

---

## Fluxo de Trabalho Recomendado

```bash
# 1. Sempre atualizar antes de começar
git checkout main
git pull origin main

# 2. Criar branch para a tarefa
git checkout -b feature/nome-da-tarefa

# 3. Trabalhar, commitar frequentemente
git add .
git commit -m "feat: descrição do que foi feito"

# 4. Quando terminar, subir para o GitHub
git push origin feature/nome-da-tarefa

# 5. Abrir Pull Request no GitHub para revisão
# → main só recebe via PR, nunca push direto
```

---

## Convenção de Commits

```
feat:     nova funcionalidade
fix:      correção de bug
docs:     documentação
refactor: refatoração sem mudar comportamento
chore:    tarefas de manutenção (configs, deps)

Exemplos:
  feat: adicionar endpoint de relatório por empresa
  fix: corrigir cálculo de permanência em minutos
  docs: atualizar README com novos endpoints
  chore: migrar de SQLite para MySQL
```

---

## Comandos Úteis

```bash
# Ver status atual
git status

# Ver histórico
git log --oneline

# Ver diferenças antes de commitar
git diff

# Desfazer último commit (sem perder alterações)
git reset HEAD~1

# Atualizar branch com o que tem na main
git checkout feature/minha-branch
git merge main

# Ver todas as branches
git branch -a
```
