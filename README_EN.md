# PromoterCheckIn.Api

> 🇧🇷 [Versão em Português](README.md)

REST API built with **ASP.NET Core (.NET 10)** for managing promoter access control across companies. Allows registering check-ins and check-outs, managing companies and promoters, and authenticating users via **JWT**.

---

## Objective

PromoterCheckIn.Api aims to provide a centralized solution for access control and monitoring of promoters linked to companies. The system allows registering and validating company and promoter records, tracking check-in and check-out movements with full traceability, and ensuring that only authorized promoters can register access on permitted days.

Through secure JWT authentication and role-based access control, the system ensures each user operates only within the functionalities allowed by their permission level. It also provides a dashboard with consolidated information on promoter presence and movement history, offering real-time management insights for administrators.

The solution was built with a focus on data integrity, information security, and scalability, using modern technologies such as ASP.NET Core, Entity Framework Core, and MySQL — capable of serving companies of different sizes that require efficient control over their promoters' field activities.

---

## Technologies

| Package | Version |
|---------|---------|
| .NET | 10.0 |
| Entity Framework Core | 10.0.3 |
| Pomelo EF Core MySQL | 9.0.0 |
| ASP.NET Authentication JwtBearer | 10.0.3 |
| System.IdentityModel.Tokens.Jwt | 8.16.0 |
| BCrypt.Net-Next | 4.1.0 |

---

## Project Structure

```
PromoterCheckIn.Api/
├── BD/
│   └── PromotoresContext.cs       # DbContext and relationship configuration
├── Controllers/
│   ├── AuthController.cs          # Authentication and JWT token generation
│   ├── PromotersController.cs     # Promoter CRUD operations
│   └── AccessRecordsController.cs # Check-in and check-out registration
├── Models/
│   ├── Company.cs                 # Company entity
│   ├── Promoter.cs                # Promoter entity
│   ├── AccessRecord.cs            # Access record entity
│   ├── User.cs                    # System user entity
│   └── LoginModel.cs              # Login DTO
├── Services/
│   └── TokenService.cs            # JWT token generation
├── appsettings.json
└── Program.cs
```

---

## Entities and Relationships

```
Company (1) ──── (N) Promoter (1) ──── (N) AccessRecord
```

- A **Company** has many **Promoters**
- A **Promoter** has many **AccessRecords**
- **User** is independent — represents whoever operates the system

---

## Endpoints

### Authentication

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/auth/login` | Authenticates user and returns JWT token |

**Body:**
```json
{
  "login": "admin",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### Promoters `🔒 Requires authentication`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/promoters` | List all promoters |
| `GET` | `/api/promoters?companyId=1` | List promoters filtered by company |
| `POST` | `/api/promoters` | Create a new promoter |

---

### Access Records `🔒 Requires authentication`

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/records/checkin` | Register promoter check-in |
| `PUT` | `/api/records/checkout/{id}` | Register check-out and calculate time spent |
| `GET` | `/api/records/promoter/{promoterId}` | List all records for a promoter |

---

## Configuration

### appsettings.json

Add the database connection string and JWT secret:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=promoter_checkin;User=root;Password=yourpassword;"
  },
  "Jwt": {
    "Secret": "your_secret_key_here_minimum_32_characters"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

### Program.cs

The `Program.cs` must be configured with the required services:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<TokenService>();

// Entity Framework + MySQL
builder.Services.AddDbContext<PromotoresContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Getting Started

**1. Clone the repository**
```bash
git clone https://github.com/your-username/PromoterCheckIn.Api.git
cd PromoterCheckIn.Api
```

**2. Configure the database** in `appsettings.json` as shown above.

**3. Run the migrations**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**4. Run the application**
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5297`
- HTTPS: `https://localhost:7272`

---

## Known Issues

> The project still contains bugs that prevent compilation. Before running, fix the following:

- `[httpGet]` → `[HttpGet]` in `PromotersController.cs`
- Loose code block outside any method in `AuthController.cs`
- `[required]` → `[Required]` in `LoginModel.cs`
- `System.componentModel` → `System.ComponentModel` in `Promoter.cs`
- `AppDbContext` → `PromotoresContext` in `AccessRecordsController.cs`
- Broken query with `.include` and `.Asqueryable()` out of context in `PromotersController.cs`
- `Program.cs` does not register any services — see configuration section above

---

## License

This project is licensed under the MIT License.
