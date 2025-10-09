// Test profile save functionality
const axios = require('axios');

async function testProfile() {
    try {
        console.log('🔑 Logging in...');
        
        // Login first
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('✅ Login successful');
        
        // Get current profile
        console.log('👤 Getting current profile...');
        const getProfileResponse = await axios.get('http://localhost:5095/api/auth/profile', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log('✅ Current profile retrieved:', getProfileResponse.data);
        
        // Test profile update
        console.log('📝 Testing profile update...');
        const updateData = {
            displayName: 'John Updated',
            bio: 'Updated bio for testing',
            city: 'Test City',
            country: 'Test Country'
        };
        
        const updateResponse = await axios.put('http://localhost:5095/api/userprofile', updateData, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log('✅ Profile update successful:', updateResponse.data);
        
        // Test offer creation
        console.log('💼 Testing offer creation...');
        const offerData = {
            title: 'Test NodeJS Mentoring',
            description: 'Test offer for NodeJS mentoring sessions',
            category: 'Technology',
            price: 50.00,
            durationInMinutes: 60,
            isOnline: true,
            tags: ['nodejs', 'javascript', 'backend']
        };
        
        const createOfferResponse = await axios.post('http://localhost:5095/api/offers', offerData, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log('✅ Offer creation successful:', createOfferResponse.data);
        
        console.log('🎉 All tests completed successfully!');
        
    } catch (error) {
        console.error('❌ Error details:');
        console.error('Message:', error.message);
        if (error.response) {
            console.error('Status:', error.response.status);
            console.error('Data:', error.response.data);
        }
    }
}

testProfile();