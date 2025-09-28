# 🔒 Development Environment Setup

Welcome to SkillSwap! This guide will help you set up your local development environment securely.

## � Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/downloads)

### 1. Clone and Setup
```bash
git clone https://github.com/FrankOfTheScience/SkillSwap.git
cd SkillSwap

# Install git hooks (recommended)
git config core.hooksPath .githooks
```

### 2. Database Setup
Create a PostgreSQL database:
```sql
CREATE DATABASE skillswap;
CREATE USER skillswap_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE skillswap TO skillswap_user;
```

### 3. Configure Application Secrets

We use secure configuration management. **No secrets are stored in the repository.**

#### Option A: .NET User Secrets (Recommended)
```bash
cd SkillSwap.Api

# Database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=skillswap;Username=skillswap_user;Password=your_password"

# JWT configuration (generate a secure key!)
dotnet user-secrets set "Jwt:Key" "your-256-bit-secret-key-here-make-it-long-and-random"

# Stripe test keys (get from https://dashboard.stripe.com - test mode)
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_your_publishable_key"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_secret_key"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_your_webhook_secret"
```

#### Option B: Environment Variables
Set these in your IDE or system environment:
```env
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=skillswap;Username=skillswap_user;Password=your_password"
Jwt__Key="your-256-bit-secret-key-here-make-it-long-and-random"
Stripe__PublishableKey="pk_test_your_publishable_key"
Stripe__SecretKey="sk_test_your_secret_key"
Stripe__WebhookSecret="whsec_your_webhook_secret"
```

### 4. Install Dependencies
```bash
# Backend dependencies
dotnet restore

# Frontend dependencies
cd frontend/skill-swap-web
npm install
```

### 5. Run the Application
```bash
# Terminal 1: Start the API (from repository root)
cd SkillSwap.Api
dotnet run

# Terminal 2: Start the frontend (from repository root)
cd frontend/skill-swap-web
npm run dev
```

The application will be available at:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5095
- **API Documentation**: http://localhost:5095/swagger

## 🔑 Getting API Keys

### Stripe Test Keys
1. Create a free account at [Stripe](https://stripe.com)
2. Go to [Dashboard > Developers > API Keys](https://dashboard.stripe.com/test/apikeys)
3. Ensure you're in **Test Mode** (top-left toggle)
4. Copy your **Publishable key** (`pk_test_...`) and **Secret key** (`sk_test_...`)

### JWT Secret Key
Generate a secure random key (256+ bits):
```bash
# Using OpenSSL
openssl rand -base64 64

# Using PowerShell
[System.Web.Security.Membership]::GeneratePassword(64, 0)

# Using Node.js
node -e "console.log(require('crypto').randomBytes(64).toString('base64'))"
```

## 🧪 Testing Your Setup

### Backend Tests
```bash
dotnet test
```

### Frontend Tests
```bash
cd frontend/skill-swap-web
npm test
```

### Full Application Test
1. **Register a new user** at http://localhost:3000
2. **Create an offer** in the dashboard
3. **Book an offer** (this will use Stripe test mode)
4. Check your Stripe Dashboard > Logs to see the API calls

## 🛡️ Security Best Practices

### ✅ Do:
- Use **User Secrets** for local development
- Keep your **Stripe keys in test mode** during development
- **Rotate credentials** regularly
- **Never commit secrets** to version control
- Use **strong, unique passwords** for your database

### ❌ Don't:
- Put secrets in code or config files
- Use production Stripe keys in development
- Commit `.env` files or configuration with secrets
- Share your JWT secret key
- Use simple or common passwords

## � Project Structure

```
SkillSwap/
├── SkillSwap.Api/              # ASP.NET Core Web API
├── SkillSwap.Application/      # Application Layer (CQRS)
├── SkillSwap.Domain/           # Domain Models
├── SkillSwap.Infrastructure/   # Database & External Services
├── SkillSwap.Tests/           # Unit & Integration Tests
├── frontend/skill-swap-web/   # Next.js Frontend
├── .github/                   # GitHub workflows & templates
└── .githooks/                 # Git hooks for quality checks
```

## � Development Workflow

### Making Changes
1. **Create a feature branch**: `git checkout -b feature/your-feature`
2. **Make your changes** with proper commit messages
3. **Run tests**: `dotnet test && npm test`
4. **Push your branch**: `git push origin feature/your-feature`
5. **Create a PR** to `development` branch
6. **Add appropriate label**: `patch`, `minor`, or `major`
7. **PR template will be applied automatically!**

### Git Hooks
Our pre-push hooks automatically check for:
- ✅ Conventional commit messages
- ✅ Test passage
- ✅ Code linting
- ✅ Secret detection
- ✅ Build success

## 🆘 Troubleshooting

### "Connection refused" errors
- Ensure PostgreSQL is running
- Check your connection string
- Verify database exists and user has permissions

### "Unauthorized" errors
- Check your JWT secret key is set
- Ensure it's at least 256 bits long
- Verify the key matches between sessions

### Stripe errors
- Ensure you're using **test mode** keys
- Check keys start with `pk_test_` and `sk_test_`
- Verify your Stripe account is in test mode

### Frontend won't connect to API
- Ensure API is running on port 5095
- Check CORS configuration in `Program.cs`
- Verify environment variables in Next.js

## 🤝 Getting Help

- 📖 **Documentation**: Check the [PR Template Guide](.github/PULL_REQUEST_TEMPLATE/README.md)
- 🐛 **Bug Reports**: Create an issue with the `bug` label
- 💡 **Feature Requests**: Create an issue with the `enhancement` label
- ❓ **Questions**: Start a discussion or comment on PRs

## 🎉 You're Ready!

Once you have everything running:
1. The API should start without errors
2. The frontend should load at http://localhost:3000
3. You should be able to register and create offers
4. Stripe test payments should work

Welcome to the SkillSwap development community! 🚀