// Test script to verify create offer functionality
const axios = require('axios');

const API_BASE = 'http://localhost:5095';

async function testCreateOffer() {
    try {
        // First login to get a token
        console.log('Logging in...');
        const loginResponse = await axios.post(`${API_BASE}/api/auth/login`, {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('Login successful, token received');
        
        // Now try to create an offer
        console.log('Creating offer...');
        const offerData = {
            title: "Test Offer from Script",
            description: "This is a test offer created from a script",
            price: 75.50,
            durationInMinutes: 90,
            location: "Online",
            isOnline: true,
            requirements: "Basic knowledge required",
            category: "Technology"
        };
        
        const createResponse = await axios.post(`${API_BASE}/api/offers`, offerData, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        
        console.log('Offer created successfully:', createResponse.data);
        
    } catch (error) {
        console.error('Full error:', error);
        console.error('Error message:', error.message);
        if (error.response) {
            console.error('Status:', error.response.status);
            console.error('Response data:', error.response.data);
            console.error('Headers:', error.response.headers);
        }
    }
}

testCreateOffer();