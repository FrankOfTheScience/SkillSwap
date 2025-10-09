// Comprehensive test script for all 4 failing flows
const axios = require('axios');

const API_BASE = 'http://localhost:5095';
const frontend_url = 'http://localhost:3000';

// Test user credentials
const testUser = {
    email: 'john.developer@skillswap.com',
    password: 'John123'
};

let authToken = '';

async function login() {
    console.log('\n🔐 Testing Login...');
    try {
        const response = await axios.post(`${API_BASE}/api/auth/login`, testUser);
        authToken = response.data.token;
        console.log('✅ Login successful');
        console.log('Token received:', authToken.substring(0, 50) + '...');
        return true;
    } catch (error) {
        console.error('❌ Login failed:', error.response?.data || error.message);
        return false;
    }
}

async function testCreateOffer() {
    console.log('\n📝 Testing Create Offer (Issue #1)...');
    try {
        const offerData = {
            title: "Automated Test Offer",
            description: "This offer was created by an automated test script",
            price: 100.00,
            durationInMinutes: 60,
            location: "Online",
            isOnline: true,
            requirements: "Basic knowledge",
            category: "Technology"
        };

        const response = await axios.post(`${API_BASE}/api/offers`, offerData, {
            headers: {
                'Authorization': `Bearer ${authToken}`,
                'Content-Type': 'application/json'
            }
        });

        console.log('✅ Create Offer SUCCESS - Status:', response.status);
        console.log('Created Offer ID:', response.data);
        return { success: true, offerId: response.data };
    } catch (error) {
        console.error('❌ Create Offer FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return { success: false };
    }
}

async function testProfileSaveNoChanges() {
    console.log('\n👤 Testing Profile Save with No Changes (Issue #2)...');
    try {
        // First get current profile
        const getResponse = await axios.get(`${API_BASE}/api/userprofile`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });

        console.log('✅ Profile retrieved successfully');
        
        // Now try to save without changes (empty request body)
        const updateResponse = await axios.put(`${API_BASE}/api/userprofile`, {}, {
            headers: {
                'Authorization': `Bearer ${authToken}`,
                'Content-Type': 'application/json'
            }
        });

        console.log('✅ Profile Save with No Changes SUCCESS - Status:', updateResponse.status);
        return { success: true };
    } catch (error) {
        console.error('❌ Profile Save with No Changes FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return { success: false };
    }
}

async function testDashboard() {
    console.log('\n📊 Testing Dashboard (Issue #3)...');
    try {
        const response = await axios.get(`${API_BASE}/api/dashboard/user`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });

        console.log('✅ Dashboard SUCCESS - Status:', response.status);
        console.log('Dashboard response:', response.data);
        return { success: true };
    } catch (error) {
        console.error('❌ Dashboard FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return { success: false };
    }
}

async function testProfilePictureUpload() {
    console.log('\n🖼️  Testing Profile Picture Upload (Issue #4)...');
    try {
        // Create a simple form data with a fake image file
        const FormData = require('form-data');
        const form = new FormData();
        
        // Create a simple buffer to simulate an image file
        const fakeImageBuffer = Buffer.from('fake-image-data');
        form.append('image', fakeImageBuffer, {
            filename: 'test-profile.jpg',
            contentType: 'image/jpeg'
        });

        const response = await axios.post(`${API_BASE}/api/userprofile/upload-image`, form, {
            headers: {
                'Authorization': `Bearer ${authToken}`,
                ...form.getHeaders()
            }
        });

        console.log('✅ Profile Picture Upload SUCCESS - Status:', response.status);
        console.log('Upload response:', response.data);
        return { success: true };
    } catch (error) {
        console.error('❌ Profile Picture Upload FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return { success: false };
    }
}

async function runAllTests() {
    console.log('🚀 Starting comprehensive tests for all 4 failing flows...');
    console.log('API Base URL:', API_BASE);
    console.log('Frontend URL:', frontend_url);
    
    // Step 1: Login
    const loginSuccess = await login();
    if (!loginSuccess) {
        console.log('\n❌ Cannot proceed without successful login');
        return;
    }

    // Step 2: Test all flows
    const results = {
        createOffer: await testCreateOffer(),
        profileSave: await testProfileSaveNoChanges(),
        dashboard: await testDashboard(),
        pictureUpload: await testProfilePictureUpload()
    };

    // Summary
    console.log('\n📋 TEST RESULTS SUMMARY:');
    console.log('========================');
    console.log(`1. Create Offer: ${results.createOffer.success ? '✅ PASS' : '❌ FAIL'}`);
    console.log(`2. Profile Save (No Changes): ${results.profileSave.success ? '✅ PASS' : '❌ FAIL'}`);
    console.log(`3. Dashboard: ${results.dashboard.success ? '✅ PASS' : '❌ FAIL'}`);
    console.log(`4. Picture Upload: ${results.pictureUpload.success ? '✅ PASS' : '❌ FAIL'}`);
    
    const passCount = Object.values(results).filter(r => r.success).length;
    console.log(`\n🎯 Overall: ${passCount}/4 tests passed`);
    
    if (passCount === 4) {
        console.log('🎉 ALL TESTS PASSED! All issues have been fixed.');
    } else {
        console.log('⚠️  Some tests failed. Issues may still exist.');
    }
}

// Run the tests
runAllTests().catch(error => {
    console.error('💥 Test runner failed:', error);
});