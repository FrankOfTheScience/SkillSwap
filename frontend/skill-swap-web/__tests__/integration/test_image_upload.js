// Test profile picture upload to find exact 400 error
const axios = require('axios');
const FormData = require('form-data');
const fs = require('fs');

async function testImageUpload() {
    try {
        console.log('🔍 Testing profile picture upload 400 error...\n');
        
        // Login first
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('✅ Login successful');
        
        // Create a small test image file (1x1 pixel PNG)
        const testImageBuffer = Buffer.from([
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
            0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x01, 0x5C, 0xC2, 0x8A, 0x1B, 0x00, 0x00, 0x00, 0x00, 0x49,
            0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ]);
        
        // Test 1: Check backend DTO structure
        console.log('🔍 Testing with correct field name "Image"...');
        const formData = new FormData();
        formData.append('Image', testImageBuffer, {
            filename: 'test.png',
            contentType: 'image/png'
        });
        
        try {
            const uploadResponse = await axios.post(
                'http://localhost:5095/api/UserProfile/upload-image',
                formData,
                {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        ...formData.getHeaders()
                    }
                }
            );
            console.log('✅ Upload successful:', uploadResponse.data);
        } catch (uploadError) {
            console.log('❌ Upload failed with field "Image":');
            console.log('   Status:', uploadError.response?.status);
            console.log('   Error:', uploadError.response?.data);
            console.log('   Headers sent:', formData.getHeaders());
        }
        
        // Test 2: Try with different field name
        console.log('\n🔍 Testing with field name "image" (lowercase)...');
        const formData2 = new FormData();
        formData2.append('image', testImageBuffer, {
            filename: 'test.png',
            contentType: 'image/png'
        });
        
        try {
            const uploadResponse2 = await axios.post(
                'http://localhost:5095/api/UserProfile/upload-image',
                formData2,
                {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        ...formData2.getHeaders()
                    }
                }
            );
            console.log('✅ Upload successful with lowercase:', uploadResponse2.data);
        } catch (uploadError2) {
            console.log('❌ Upload failed with field "image":');
            console.log('   Status:', uploadError2.response?.status);
            console.log('   Error:', uploadError2.response?.data);
        }
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
    }
}

testImageUpload();