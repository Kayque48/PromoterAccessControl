> 🇧🇷 [Versão em Português](README.md)

# Promoter Access Control

## 📋 Overview

An academic system for managing promoter access and duration at multiple company locations, developed as a final course project (Capstone/TCC) in Systems Analysis and Development. The project demonstrates modular architecture, relational database integration, JWT-based authentication, and report generation.

---

## 🎯 Objective

Centralize promoter access registration, automating:

- Entry and exit registration
- Automatic duration calculation
- Real-time monitoring of active promoters
- Operational metrics and report generation
- Data export for analysis

---

## 🛠️ Technology Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core (C#) |
| **Database** | MySQL (official database) / SQLite (local development support) |
| **Frontend** | HTML5, CSS3, JavaScript (vanilla) |
| **Authentication** | JWT Bearer Token |
| **ORM** | Entity Framework Core |
| **Security** | BCrypt (password hashing) |

---

## 📁 Project Structure

```
PromoterAccessControl/
├── backend/
│   └── ControlePromotores.Api/
│       ├── Controllers/          # API endpoints
│       ├── Services/             # Business logic
│       ├── Models/               # Domain entities
│       ├── DTOs/                 # Request/response contracts
│       ├── BD/                   # DbContext
│       ├── Data/                 # Data seed
│       ├── Utils/                # Utilities
│       └── Program.cs            # Application configuration
├── frontend/
│   ├── css/                      # Styles
│   ├── js/                       # Scripts (API client, logic, UI)
│   ├── img/                      # Images
│   └── *.html                    # Pages
├── database/
│   └── PromoterAccessControlDB.sql  # Official MySQL schema
└── docs/
    ├── visao-geral.md
    ├── arquitetura.md
    ├── fluxo-funcional.md
    ├── seguranca-e-lgpd.md
    └── pendencias-tecnicas.md
```

---

## ⚙️ Architecture

The system follows a **three-tier architecture**:

```
┌─────────────────────────────────────┐
│  Frontend (HTML/CSS/JS)             │
│  - User interface                   │
│  - API consumption                  │
└─────────────┬───────────────────────┘
              │ HTTP/REST
              ↓
┌─────────────────────────────────────┐
│  Backend (ASP.NET Core)             │
│  - Controllers (exposure)           │
│  - Services (business rules)        │
│  - Models (entities)                │
│  - DTOs (contracts)                 │
└─────────────┬───────────────────────┘
              │ SQL
              ↓
┌─────────────────────────────────────┐
│  Database (MySQL)                   │
│  - Data persistence                 │
│  - Relationships                    │
└─────────────────────────────────────┘
```

### Development Pattern

- **Layered separation:** Controllers → Services → Models/DTOs → DbContext
- **Authentication:** JWT with role-based access control
- **Validation:** Performed in Services and DTOs
- **Persistence:** Entity Framework Core with automatic navigation

---

## ✨ Implemented Features

### Authentication
- Login with username and password
- JWT generation and validation
- Access control by role (Admin, User)

### Data Management
- **Companies:** Create, edit, list, delete
- **Promoters:** Create, edit, list, link to company, manage exclusivity

### Time Tracking
- **Entry:** Registers the promoter's arrival time based on the linked company
- **Exit:** Registers departure time and auto-calculates duration
- **Active:** Real-time list of promoters with entry registered but no exit

### Reports & Analytics
- **Dashboard:** Operational metrics (active promoters, active companies, etc.)
- **Reports:** Filtered by date range, company, and promoter
- **Export:** Generates tabulated data file

---

## 🚀 How to Run

### Prerequisites

- **.NET SDK 10.0+** installed
- **Node.js** (optional, only to serve frontend)
- **MySQL Server** (official database) or SQLite as fallback

### 1️⃣ Prepare the Database

#### Option A: MySQL (Recommended - Official Database)

```bash
# In MySQL:
mysql -u root -p < database/PromoterAccessControlDB.sql
```

Update the connection string in `appsettings.Development.json`:
```json
"MySqlConnection": "Server=localhost;Port=3306;Database=promoter_checkin;User Id=root;Password=YOUR_PASSWORD;"
```

#### Option B: SQLite (Offline Development)

In `appsettings.Development.json`, configure:
```json
"UseSqlite": true
```

### 2️⃣ Run the Backend

```bash
cd backend/ControlePromotores.Api

# Restore dependencies
dotnet restore

# Run in development mode
dotnet run
```

Backend will be available at:
- **HTTP:** `http://localhost:5297`
- **HTTPS:** `https://localhost:7272`
- **Swagger:** `http://localhost:5297/swagger/index.html`

### 3️⃣ Run the Frontend

Serve the frontend using any local HTTP server (e.g., http-server, Live Server on VS Code, Python http.server):

```bash
cd frontend

# Option 1: http-server (npm)
npx http-server -p 8000

# Option 2: Python
python -m http.server 8000

# Option 3: Node http-server
npm install -g http-server
http-server -p 8000
```

Frontend available at: `http://localhost:8000`

### ⚙️ Important Configurations

**appsettings.Development.json** is a **local, non-versioned configuration file**. Example structure:

```json
{
  "UseSqlite": false,
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=promoter_checkin;User Id=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters",
    "Issuer": "ControlePromotoresAPI",
    "Audience": "ControlePromotoresClient",
    "ExpirationMinutes": 1440
  }
}
```

⚠️ **IMPORTANT:** 
- Never commit real secrets (passwords, JWT keys) to the repository
- Use environment variables or local `.gitignore` files
- JWT key must have **minimum 32 characters**

---

## 📊 Main Flow

1. **Authentication:** User logs in
2. **Setup:** Admin registers companies and promoters
3. **Linking:** Promoter is linked to company (exclusive or shared)
4. **Entry:** The promoter registers arrival and the system uses the company linked to that promoter
5. **Monitoring:** System displays active promoters
6. **Exit:** Promoter registers departure (duration calculated automatically)
7. **Analysis:** Data viewable in dashboard, reports, and export

---

## 📈 Current Project Status

### ✅ Implemented & Validated

- Login with JWT authentication
- Full CRUD for companies
- Full CRUD for promoters with company linking
- Entry and exit registration with automatic duration calculation
- Active promoters query
- Reports with filters (date range, company, promoter)
- Data export
- Dashboard with basic metrics
- Input validations (CPF, CNPJ, email, etc.)

### ⚠️ Dashboard Status

The dashboard is stabilized in safe mode. Charts were temporarily disabled as a technical containment measure to prevent freezing, while the main indicators remain available.

### 🔄 Identified for Future Refinement

- Safe reactivation of charts with improved performance handling
- Advanced rate limiting and token revocation mechanisms
- Automated tests (unit and integration)
- Frontend refactoring with more modular structure
- Improvements in CNPJ validation on screens
- Refinement of exclusive vs. shared promoter logic

---

## 🔒 Security & LGPD Considerations

### Implemented

✅ JWT-based authentication  
✅ BCrypt password hashing  
✅ Role-based access control  
✅ Input validation in DTOs  

### Known Limitations

⚠️ CORS configured broadly (AllowAnyOrigin)  
⚠️ Token stored in localStorage (vulnerable to XSS)  
⚠️ No token revocation mechanism  
⚠️ Rate limiting not implemented  
⚠️ Secrets (JWT Key) in local configuration file  

### Personal Data Processed

The system handles: name, CPF, phone number, email, company association, and entry/exit records.

In a real corporate environment, the following would be required:

- Formal data retention policy
- Conscious data minimization
- Data masking in display (e.g., partially hidden CPF)
- Stricter access and export controls
- Legal basis documentation (LGPD Article 7)
- Data deletion procedures

The current project demonstrates basic security awareness, but **should not be considered production-ready** for LGPD compliance without additional improvements.

---

## 📝 Important Notes

1. **Academic Scope:** This is a capstone project with educational purposes. It reflects design decisions appropriate for demonstration, not necessarily for corporate environments.

2. **Modular Architecture:** The layered structure facilitates maintenance, testing, and future evolution.

3. **Technical Documentation:** See `docs/` for details on architecture, functional flows, and technical tasks.

4. **Sample Data:** The database is initialized with minimal data for basic functionality.

---

## 📚 Additional Documentation

- [Overview](docs/visao-geral.md) (Portuguese)
- [Technical Architecture](docs/arquitetura.md) (Portuguese)
- [Functional Flow](docs/fluxo-funcional.md) (Portuguese)
- [Security & LGPD](docs/seguranca-e-lgpd.md) (Portuguese)
- [Technical Tasks](docs/pendencias-tecnicas.md) (Portuguese)
- [Presentation Guide](docs/roteiro-de-apresentacao.md) (Portuguese)

Interactive documentation available via Swagger at `http://localhost:5297/swagger` after starting the backend.

---

## 👥 Team

- **Camila** — Backend Development
- **Kayque** — Database
- **Mateus** — Frontend Development

Capstone Project in Systems Analysis and Development
