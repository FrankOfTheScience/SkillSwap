# SkillSwap - Local Development Setup

## 🔐 Security Configuration

For security reasons, sensitive configuration files are not included in the repository. You need to create local configuration files with your own secrets.

### Required Configuration Files

#### 1. API Configuration

Copy the template and add your secrets:

```bash
cp SkillSwap.Api/appsettings.Development.template.json SkillSwap.Api/appsettings.Development.json
```

Then update `SkillSwap.Api/appsettings.Development.json` with:

- **Database Password**: Replace `[POSTGRES_PASSWORD]` with your PostgreSQL password
- **JWT Secret Key**: Replace `[JWT_SECRET_KEY]` with a strong secret key (minimum 32 characters)

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=your_actual_password"
  },
  "Jwt": {
    "Key": "your-very-long-and-secure-jwt-secret-key-here-at-least-32-chars",
    "Issuer": "SkillSwap",
    "Audience": "SkillSwapUsers",
    "ExpiryInMinutes": 60
  }
}
```

### 🚨 Important Security Notes

- **NEVER** commit `appsettings.Development.json` to git
- **NEVER** commit any file containing passwords, API keys, or secrets
- The `.gitignore` file prevents accidental commits of sensitive files
- Use GitHub Secrets for production deployment

### 🗄️ Database Setup

1. Install PostgreSQL
2. Create database: `CREATE DATABASE skillswap;`
3. Update connection string in your local `appsettings.Development.json`
4. Run migrations: `dotnet ef database update --startup-project SkillSwap.Api`

### 🚀 Running the Application

#### Backend (API)

```bash
cd SkillSwap.Api
dotnet run
# API will be available at http://localhost:5095
```

#### Frontend

```bash
cd frontend/skill-swap-web
npm install
npm run dev
# Frontend will be available at http://localhost:3000
```

### 🔑 Test Accounts

After running the database migrations, these test accounts will be available:

- **Admin**: `admin@skillswap.com` / `Admin123`
- **User**: `user@skillswap.com` / `User123`
- **Guest**: `guest@skillswap.com` / `Guest123`

---

⚠️ **Security Reminder**: Keep your secrets secure and never commit them to version control!