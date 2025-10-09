// Quick test to check profile image URL
const axios = require('axios');

async function checkProfile() {
    try {
        // Login first
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('Login successful');
        
        // Get profile
        const profileResponse = await axios.get('http://localhost:5095/api/UserProfile', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log('Profile Image URL:', profileResponse.data.profileImageUrl);
        console.log('Display Name:', profileResponse.data.displayName);
        
    } catch (error) {
        console.error('Error:', error.message);
        if (error.response) {
            console.error('Response:', error.response.data);
        }
    }
}

checkProfile();