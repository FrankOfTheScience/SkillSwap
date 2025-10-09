# Integration Tests

This directory contains integration tests that test the SkillSwap API endpoints from a client perspective. These tests require the backend API server to be running.

## Prerequisites

1. **Backend Server**: Ensure the SkillSwap API is running on `http://localhost:5095`
   ```bash
   cd ../../..  # Go to root directory
   dotnet run --project SkillSwap.Api/SkillSwap.Api.csproj
   ```

2. **Test User**: The tests use a test user with credentials:
   - Email: `john.developer@skillswap.com`
   - Password: `John123`

## Available Test Scripts

Run these commands from the frontend directory (`skill-swap-web/`):

### Comprehensive Tests
- `npm run test:integration` - Runs all major flows (login, create offer, profile, dashboard)
- `npm run test:quick` - Quick connectivity and basic functionality test

### Specific Feature Tests
- `npm run test:auth` - Authentication flow testing
- `npm run test:dashboard` - Dashboard and SignalR functionality
- `npm run test:profile` - User profile operations
- `npm run test:image-upload` - Profile picture upload functionality
- `npm run test:create-offer` - Offer creation workflow
- `npm run test:signalr` - Real-time SignalR features

### Debugging Tests
- `node __tests__/integration/check_database_data.js` - Database state inspection
- `node __tests__/integration/check_john_data.js` - Test user data verification
- `node __tests__/integration/check_profile.js` - Profile data validation
- `node __tests__/integration/debug_user_id.js` - User ID debugging
- `node __tests__/integration/test_profile_fixes.js` - Profile bug fixes validation
- `node __tests__/integration/test_signalr_cors.js` - SignalR CORS testing

## Test Categories

### Unit Tests (Jest)
Regular React component and utility function tests:
```bash
npm test                 # Run all unit tests
npm run test:watch      # Run tests in watch mode
npm run test:coverage   # Run with coverage report
```

### Integration Tests (Node.js)
API endpoint and full workflow tests:
```bash
npm run test:integration  # Run main integration test suite
```

## Notes

- Integration tests use `axios` to make HTTP requests directly to the API
- Tests are designed to be run independently or as part of a suite
- Some tests create test data that may need cleanup
- SignalR tests verify real-time functionality
- Image upload tests use generated test images

## Troubleshooting

1. **Connection Errors**: Ensure the backend API is running on port 5095
2. **Authentication Errors**: Verify the test user exists in the database
3. **Permission Errors**: Check that the test user has appropriate permissions
4. **Port Conflicts**: Ensure no other services are using the required ports