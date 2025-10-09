// Quick test to verify SignalR connection is working
const axios = require('axios');

async function testSignalRConnection() {
    try {
        console.log('🔍 Testing SignalR connection...\n');
        
        // Step 1: Login
        console.log('1️⃣ Logging in...');
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('✅ Login successful\n');
        
        // Step 2: Test SignalR negotiate endpoint
        console.log('2️⃣ Testing SignalR negotiate endpoint...');
        try {
            const negotiateResponse = await axios.post('http://localhost:5095/hubs/dashboard/negotiate', {}, {
                headers: { 
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            console.log('✅ SignalR negotiate successful:', negotiateResponse.status);
        } catch (negotiateError) {
            console.log('❌ SignalR negotiate failed:', negotiateError.response?.status, negotiateError.response?.data || negotiateError.message);
        }
        
        // Step 3: Test CORS with preflight
        console.log('3️⃣ Testing CORS preflight...');
        try {
            const corsResponse = await axios.options('http://localhost:5095/hubs/dashboard', {
                headers: {
                    'Origin': 'http://localhost:3000',
                    'Access-Control-Request-Method': 'POST',
                    'Access-Control-Request-Headers': 'authorization'
                }
            });
            console.log('✅ CORS preflight successful:', corsResponse.status);
        } catch (corsError) {
            console.log('❌ CORS preflight failed:', corsError.response?.status || corsError.message);
        }
        
        console.log('\n🎯 MANUAL TESTING:');
        console.log('1. Open http://localhost:3000 in your browser');
        console.log('2. Navigate to the profile page');
        console.log('3. Check browser dev tools console for SignalR connection messages');
        console.log('4. The dashboard should now load without "Failed to fetch" errors');
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
    }
}

testSignalRConnection();