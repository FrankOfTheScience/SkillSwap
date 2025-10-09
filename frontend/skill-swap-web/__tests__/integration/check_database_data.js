// Test to verify if offers and bookings exist for John's current user ID
const axios = require('axios');

async function checkDatabaseForJohnData() {
    try {
        console.log('🔍 Checking database for John\'s actual data...\n');
        
        // Get all offers to see what's in the database
        try {
            const allOffersResponse = await axios.get('http://localhost:5095/api/offers');
            const offersData = allOffersResponse.data;
            console.log('📋 Total offers in database:', Array.isArray(offersData) ? offersData.length : 'No array data');
            console.log('📋 Raw offers response:', typeof offersData, offersData);
            
            if (Array.isArray(offersData) && offersData.length > 0) {
                console.log('💼 Sample offer creators:');
                offersData.slice(0, 3).forEach((offer, index) => {
                    console.log(`  ${index + 1}. ${offer.title} - Created by: ${offer.createdBy}`);
                });
            }
        } catch (offersError) {
            console.log('❌ Failed to get offers:', offersError.response?.status, offersError.response?.data);
        }
        
        // Login as John
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        const base64Payload = token.split('.')[1];
        const payload = JSON.parse(Buffer.from(base64Payload, 'base64').toString());
        const johnUserId = payload.sub;
        
        console.log(`\n👤 John's User ID: ${johnUserId}`);
        
        // Check if any offers belong to John
        let johnOffers = [];
        try {
            const allOffersResponse = await axios.get('http://localhost:5095/api/offers');
            const offersData = allOffersResponse.data;
            if (Array.isArray(offersData)) {
                johnOffers = offersData.filter(offer => offer.createdBy === johnUserId);
            }
        } catch (err) {
            console.log('❌ Could not check John\'s offers');
        }
        
        console.log(`💼 John's offers: ${johnOffers.length}`);
        
        if (johnOffers.length > 0) {
            johnOffers.forEach(offer => {
                console.log(`  - ${offer.title} (${offer.price})`);
            });
        }
        
        // Test by creating a new offer for John to see if dashboard updates
        console.log('\n🔧 Creating test offer for John...');
        try {
            const newOfferResponse = await axios.post('http://localhost:5095/api/offers', {
                title: 'Test Dashboard Offer',
                description: 'Test offer to check if dashboard updates',
                category: 'Technology',
                price: 25.00,
                durationInMinutes: 60,
                isOnline: true,
                tags: ['test', 'dashboard']
            }, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            
            console.log('✅ Test offer created:', newOfferResponse.data);
            
            // Now trigger dashboard update
            const dashboardResponse = await axios.get('http://localhost:5095/api/dashboard/user', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            console.log('📊 Dashboard API triggered:', dashboardResponse.data);
            
        } catch (offerError) {
            console.log('❌ Failed to create test offer:', offerError.response?.data || offerError.message);
        }
        
    } catch (error) {
        console.error('❌ Error:', error.response?.data || error.message);
    }
}

checkDatabaseForJohnData();