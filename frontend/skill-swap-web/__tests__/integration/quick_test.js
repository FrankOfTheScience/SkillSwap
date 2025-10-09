// Quick test to register a user and then test flows
const axios = require('axios');

const API_BASE = 'http://localhost:5095';

async function registerTestUser() {
    console.log('🔧 Registering test user...');
    try {
        const userData = {
            email: 'test@skillswap.com',
            password: 'Test123!',
            displayName: 'Test User'
        };

        const response = await axios.post(`${API_BASE}/api/auth/register`, userData);
        console.log('✅ Registration successful');
        return response.data.token;
    } catch (error) {
        console.error('❌ Registration failed:', error.response?.data || error.message);
        return null;
    }
}

async function loginTestUser() {
    console.log('🔐 Logging in test user...');
    try {
        const userData = {
            email: 'test@skillswap.com',
            password: 'Test123!'
        };

        const response = await axios.post(`${API_BASE}/api/auth/login`, userData);
        console.log('✅ Login successful');
        return response.data.token;
    } catch (error) {
        console.error('❌ Login failed:', error.response?.data || error.message);
        return null;
    }
}

async function testCreateOffer(token) {
    console.log('\n📝 Testing Create Offer...');
    try {
        const offerData = {
            title: "Test Offer",
            description: "Automated test offer",
            price: 50.00,
            durationInMinutes: 60,
            location: "Online",
            isOnline: true,
            requirements: "None",
            category: "Testing"
        };

        const response = await axios.post(`${API_BASE}/api/offers`, offerData, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        console.log('✅ Create Offer SUCCESS - Status:', response.status);
        console.log('Offer ID:', response.data);
        return true;
    } catch (error) {
        console.error('❌ Create Offer FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return false;
    }
}

async function testProfileUpdate(token) {
    console.log('\n👤 Testing Profile Save with No Changes...');
    try {
        const response = await axios.put(`${API_BASE}/api/userprofile`, {}, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        console.log('✅ Profile Save SUCCESS - Status:', response.status);
        return true;
    } catch (error) {
        console.error('❌ Profile Save FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return false;
    }
}

async function testDashboard(token) {
    console.log('\n📊 Testing Dashboard...');
    try {
        const response = await axios.get(`${API_BASE}/api/dashboard/user`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        console.log('✅ Dashboard SUCCESS - Status:', response.status);
        return true;
    } catch (error) {
        console.error('❌ Dashboard FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return false;
    }
}

async function testPictureUpload(token) {
    console.log('\n🖼️  Testing Profile Picture Upload...');
    try {
        const FormData = require('form-data');
        const form = new FormData();
        
        const fakeImageBuffer = Buffer.from('fake-image-data');
        form.append('image', fakeImageBuffer, {
            filename: 'test.jpg',
            contentType: 'image/jpeg'
        });

        const response = await axios.post(`${API_BASE}/api/userprofile/upload-image`, form, {
            headers: {
                'Authorization': `Bearer ${token}`,
                ...form.getHeaders()
            }
        });

        console.log('✅ Picture Upload SUCCESS - Status:', response.status);
        return true;
    } catch (error) {
        console.error('❌ Picture Upload FAILED');
        console.error('Status:', error.response?.status);
        console.error('Error:', error.response?.data || error.message);
        return false;
    }
}

async function runTests() {
    console.log('🚀 Starting E2E Tests for Fixed Issues...\n');

    // Try to register, if that fails try to login
    let token = await registerTestUser();
    if (!token) {
        token = await loginTestUser();
    }

    if (!token) {
        console.log('❌ Cannot proceed without authentication');
        return;
    }

    console.log('\n🎯 Running all 4 issue tests...');

    const results = {
        createOffer: await testCreateOffer(token),
        profileUpdate: await testProfileUpdate(token),
        dashboard: await testDashboard(token),
        pictureUpload: await testPictureUpload(token)
    };

    console.log('\n📋 FINAL RESULTS:');
    console.log('==================');
    console.log(`✨ Issue #1 - Create Offer: ${results.createOffer ? '✅ FIXED' : '❌ STILL BROKEN'}`);
    console.log(`✨ Issue #2 - Profile Save: ${results.profileUpdate ? '✅ FIXED' : '❌ STILL BROKEN'}`);
    console.log(`✨ Issue #3 - Dashboard: ${results.dashboard ? '✅ FIXED' : '❌ STILL BROKEN'}`);
    console.log(`✨ Issue #4 - Picture Upload: ${results.pictureUpload ? '✅ FIXED' : '❌ STILL BROKEN'}`);

    const fixedCount = Object.values(results).filter(Boolean).length;
    console.log(`\n🎉 SUMMARY: ${fixedCount}/4 issues have been FIXED!`);

    if (fixedCount === 4) {
        console.log('🏆 ALL ISSUES RESOLVED! Authentication fix was successful!');
    }
}

runTests().catch(console.error);