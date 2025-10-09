// Test to check if user ID matches between JWT and database
const axios = require('axios');

async function debugUserIdMismatch() {
    try {
        console.log('🔍 Debugging User ID mismatch...\n');
        
        // Login
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        
        // Decode JWT
        const base64Payload = token.split('.')[1];
        const payload = JSON.parse(Buffer.from(base64Payload, 'base64').toString());
        console.log('🎫 JWT User ID (sub claim):', payload.sub);
        
        // Get detailed profile 
        const profileResponse = await axios.get('http://localhost:5095/api/UserProfile', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        console.log('👤 UserProfile ID:', profileResponse.data.id);
        console.log('👤 UserProfile Email:', profileResponse.data.email);
        
        // Manually test dashboard service with the correct user ID
        console.log('\n🔍 Testing if user has data in database...');
        console.log('User ID from JWT:', payload.sub);
        console.log('User ID from profile:', profileResponse.data.id);
        
        // Check if they match
        if (payload.sub === profileResponse.data.id) {
            console.log('✅ User IDs match!');
        } else {
            console.log('❌ User ID mismatch detected!');
            console.log('JWT sub claim:', payload.sub);
            console.log('Profile ID:', profileResponse.data.id);
        }
        
    } catch (error) {
        console.error('❌ Error:', error.response?.data || error.message);
    }
}

debugUserIdMismatch();