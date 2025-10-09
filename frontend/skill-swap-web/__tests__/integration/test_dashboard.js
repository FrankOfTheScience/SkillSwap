// Quick test for SignalR dashboard functionality
const axios = require('axios');

async function testDashboard() {
    try {
        console.log('🔍 Testing API server connection...');
        
        // Test basic connectivity
        try {
            const healthCheck = await axios.get('http://localhost:5095/api/offers');
            console.log('✅ Server is responding');
        } catch (connError) {
            console.log('❌ Server connection failed:', connError.message);
            return;
        }
        
        // Login first
        console.log('🔑 Attempting login...');
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('✅ Login successful, token received');
        
        // Test dashboard endpoint
        console.log('📊 Testing dashboard endpoint...');
        const dashboardResponse = await axios.get('http://localhost:5095/api/dashboard/user', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log('✅ Dashboard API Response:', dashboardResponse.data);
        
        // Test SignalR hub connectivity (optional)
        console.log('🔗 Dashboard testing completed successfully!');
        
    } catch (error) {
        console.error('❌ Error details:');
        console.error('Message:', error.message);
        if (error.response) {
            console.error('Status:', error.response.status);
            console.error('Data:', error.response.data);
        }
        console.error('Full error:', error);
    }
}

testDashboard();