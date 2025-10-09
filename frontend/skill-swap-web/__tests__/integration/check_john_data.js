// Test to verify John Developer has data in the database
const axios = require('axios');

async function checkJohnDeveloperData() {
    try {
        console.log('🔍 Checking John Developer data in database...\n');
        
        // Login
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('✅ Login successful');
        
        // Decode JWT to see user ID
        const base64Payload = token.split('.')[1];
        const payload = JSON.parse(Buffer.from(base64Payload, 'base64').toString());
        console.log('🔍 JWT Payload:', {
            sub: payload.sub,
            nameid: payload.nameid,
            unique_name: payload.unique_name,
            email: payload.email
        });
        
        // Get profile to see user data
        const profileResponse = await axios.get('http://localhost:5095/api/auth/profile', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        console.log('👤 Profile data:', profileResponse.data);
        
        // Get offers
        try {
            const offersResponse = await axios.get('http://localhost:5095/api/offers', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            console.log('💼 Total offers:', offersResponse.data?.length || 'No data');
            if (offersResponse.data && Array.isArray(offersResponse.data)) {
                console.log('💼 John\'s offers:', offersResponse.data.filter(o => o.createdBy === payload.sub));
            }
        } catch (offerError) {
            console.log('❌ Offers error:', offerError.response?.status, offerError.response?.data);
        }
        
        // Get bookings  
        try {
            const bookingsResponse = await axios.get('http://localhost:5095/api/bookings', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            console.log('📅 Total bookings:', bookingsResponse.data?.length || 'No data');
        } catch (bookingError) {
            console.log('❌ Bookings error:', bookingError.response?.status, bookingError.response?.data);
        }
        
        // Test dashboard API directly
        const dashboardResponse = await axios.get('http://localhost:5095/api/dashboard/user', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        console.log('📊 Dashboard API response:', dashboardResponse.data);
        
    } catch (error) {
        console.error('❌ Error:', error.response?.data || error.message);
    }
}

checkJohnDeveloperData();