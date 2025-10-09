# Integration Tests Status

## ✅ Frontend Unit Tests
- **Status**: All passing (36/36 tests)
- **Command**: `npm test`
- **Coverage**: Components, services, and utilities

## 📋 Integration Tests Available

The following integration test scripts have been moved from the root directory to `__tests__/integration/`:

### Quick Tests
- `npm run test:quick` - Basic connectivity and functionality test
- `npm run test:auth` - Authentication flow testing

### Feature Tests  
- `npm run test:integration` - Comprehensive test of all major flows
- `npm run test:dashboard` - Dashboard and SignalR functionality
- `npm run test:profile` - User profile operations
- `npm run test:image-upload` - Profile picture upload functionality
- `npm run test:create-offer` - Offer creation workflow
- `npm run test:signalr` - Real-time SignalR features

### Debug Scripts
- `node __tests__/integration/check_database_data.js` - Database inspection
- `node __tests__/integration/check_john_data.js` - Test user verification  
- `node __tests__/integration/check_profile.js` - Profile validation
- `node __tests__/integration/debug_user_id.js` - User ID debugging
- `node __tests__/integration/test_profile_fixes.js` - Profile fixes validation
- `node __tests__/integration/test_signalr_cors.js` - SignalR CORS testing

## 🔧 Prerequisites for Integration Tests

1. **Backend API Server**: Must be running on `http://localhost:5095`
   ```bash
   # From repository root
   dotnet run --project SkillSwap.Api/SkillSwap.Api.csproj
   ```

2. **Test User**: Ensure test user exists with credentials:
   - Email: `john.developer@skillswap.com`
   - Password: `John123`

## 📁 Project Structure

```
frontend/skill-swap-web/
├── __tests__/
│   ├── components/         # React component tests (Jest)
│   ├── services/          # Service layer tests (Jest)  
│   ├── utils/             # Utility function tests (Jest)
│   └── integration/       # API integration tests (Node.js)
│       ├── README.md      # Integration test documentation
│       ├── test_*.js      # Test scripts
│       ├── auth_test.js   # Authentication tests
│       ├── check_*.js     # Database validation scripts
│       └── debug_*.js     # Debugging utilities
├── package.json           # Includes both Jest and integration test scripts
└── jest.config.js         # Jest configuration (excludes integration tests)
```

## 🚀 Next Steps

The test infrastructure is now properly organized with:
- ✅ Frontend unit tests passing and properly isolated
- ✅ Integration tests moved to frontend project structure  
- ✅ Clear separation between Jest tests and Node.js integration scripts
- ✅ Documentation for running different test types
- ✅ Proper npm scripts for easy test execution

All tests are ready to run when the backend API server is available.