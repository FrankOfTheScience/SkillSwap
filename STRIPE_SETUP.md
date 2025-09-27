# Stripe Configuration Setup

## 🔧 Development Setup

To run the application locally with Stripe integration, you need to add your Stripe test keys to the configuration files.

### Step 1: Get Your Stripe Test Keys
1. Go to [Stripe Dashboard](https://dashboard.stripe.com)
2. Make sure you're in **Test mode** (toggle in top-left)
3. Go to **Developers > API keys**
4. Copy your **Publishable key** (starts with `pk_test_`)
5. Copy your **Secret key** (starts with `sk_test_`)

### Step 2: Update Configuration Files

Replace the placeholder values in these files:

**SkillSwap.Api/appsettings.Development.json:**
```json
{
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY_HERE",
    "SecretKey": "sk_test_YOUR_SECRET_KEY_HERE",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE"
  }
}
```

**SkillSwap.Api/appsettings.json:**
```json
{
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY_HERE", 
    "SecretKey": "sk_test_YOUR_SECRET_KEY_HERE",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE"
  }
}
```

### Step 3: Webhook Configuration (Future Implementation)
For webhook secrets:
1. Go to **Developers > Webhooks** in Stripe Dashboard
2. Add endpoint: `https://yourdomain.com/api/webhooks/stripe`
3. Select events: `checkout.session.completed`, `payment_intent.succeeded`
4. Copy the webhook signing secret (starts with `whsec_`)

## ⚠️ Security Notes

- **Never commit real API keys to version control**
- Use environment variables for production
- Keep test keys separate from live keys
- Rotate keys regularly for security

## 🚀 Verification

After adding your keys:
1. Start the API server: `dotnet run`
2. Start the frontend: `npm run dev` 
3. Test booking creation through the UI
4. Check Stripe Dashboard > Logs for API calls

## ✅ Features Implemented

- ✅ Stripe checkout session creation
- ✅ Payment amount calculation with 10% commission
- ✅ Database booking record creation  
- ✅ User authentication integration
- ✅ Error handling and logging
- 🔄 Webhook processing (next phase)
- 🔄 Payment confirmation flow (next phase)