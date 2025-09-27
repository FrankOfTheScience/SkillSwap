# 🔒 Secure Development Setup Guide

## ⚠️ IMPORTANT SECURITY NOTICE

**ALL SENSITIVE CONFIGURATION HAS BEEN REMOVED FROM THE REPOSITORY**

This repository now uses secure configuration management. Follow this guide to set up your development environment.

## 🔧 Required Configuration

### Database Configuration
- PostgreSQL connection string with credentials
- Recommended: `Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=YOUR_PASSWORD`

### JWT Authentication
- Secret key for token signing (minimum 256 bits)
- Issuer: `SkillSwap`
- Audience: `SkillSwapUsers`

### Stripe Integration
- Test publishable key (`pk_test_...`)
- Test secret key (`sk_test_...`)
- Webhook secret (`whsec_...`) - for future webhook implementation

## 🛡️ Setup Methods

### Method 1: .NET User Secrets (Recommended for Development)

```bash
cd SkillSwap.Api

# Database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=YOUR_DB_PASSWORD"

# JWT configuration
dotnet user-secrets set "Jwt:Key" "YOUR_VERY_LONG_AND_SECURE_JWT_SECRET_KEY_HERE_MINIMUM_256_BITS"

# Stripe configuration
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_STRIPE_SECRET_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_WEBHOOK_SECRET_WHEN_READY"
```

### Method 2: Environment Variables

Set these environment variables in your system or IDE:

```env
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=YOUR_DB_PASSWORD"
Jwt__Key="YOUR_VERY_LONG_AND_SECURE_JWT_SECRET_KEY_HERE_MINIMUM_256_BITS"
Stripe__PublishableKey="pk_test_YOUR_STRIPE_PUBLISHABLE_KEY"
Stripe__SecretKey="sk_test_YOUR_STRIPE_SECRET_KEY"
Stripe__WebhookSecret="whsec_YOUR_WEBHOOK_SECRET"
```

### Method 3: Local Configuration File (Not Recommended)

If you must use a local config file, create `appsettings.Development.local.json` (this file is git-ignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "YOUR_VERY_LONG_AND_SECURE_JWT_SECRET_KEY_HERE"
  },
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_KEY",
    "SecretKey": "sk_test_YOUR_KEY",
    "WebhookSecret": "whsec_YOUR_SECRET"
  }
}
```

## 🔑 Getting Your Stripe Keys

1. Visit [Stripe Dashboard](https://dashboard.stripe.com)
2. Ensure you're in **Test Mode** (toggle in top-left)
3. Navigate to **Developers > API Keys**
4. Copy your test keys (they start with `pk_test_` and `sk_test_`)

## ✅ Verification Steps

1. **Start the API server:**
   ```bash
   cd SkillSwap.Api
   dotnet run
   ```
   
2. **Check the console output for:**
   - No configuration errors
   - Database connection successful
   - Server listening on `http://localhost:5095`

3. **Start the frontend:**
   ```bash
   cd frontend/skill-swap-web
   npm run dev
   ```

4. **Test the integration:**
   - Open `http://localhost:3002`
   - Register a new user
   - Try creating a booking
   - Check Stripe Dashboard > Logs for API calls

## 🛡️ Security Best Practices Implemented

- ✅ **No secrets in repository** - All sensitive data removed from git history
- ✅ **Git ignore rules** - Prevents accidental commits of sensitive files
- ✅ **User Secrets support** - Secure development configuration
- ✅ **Environment variable support** - Production-ready configuration
- ✅ **Clear documentation** - Easy setup for new developers
- ✅ **Placeholder values** - Configuration files show structure without exposing secrets

## 🚨 What Was Removed

For security, the following sensitive information has been permanently removed from the repository:

- Database passwords
- JWT secret keys  
- Stripe API keys (test and live)
- Any other credentials or secrets

## 📝 Notes for New Developers

1. **Never commit secrets** to the repository
2. **Use User Secrets** for local development
3. **Use Azure Key Vault** or similar for production
4. **Rotate credentials** regularly
5. **Test with Stripe test mode** only during development

## 🔄 Next Steps

Once you have the basic setup working:
1. Implement Stripe webhook processing
2. Add payment confirmation flow  
3. Implement booking status updates
4. Set up production deployment with secure configuration

---

**⚠️ Remember: This setup ensures that no sensitive data is ever committed to version control, keeping your application secure.**