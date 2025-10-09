// Test SignalR dashboard functionality
const axios = require('axios');
const { HubConnectionBuilder, LogLevel } = require('@microsoft/signalr');

async function testSignalRDashboard() {
    try {
        console.log('🚀 Starting SignalR Dashboard Test...\n');
        
        // Step 1: Login to get token
        console.log('1️⃣ Authenticating...');
        const loginResponse = await axios.post('http://localhost:5095/api/auth/login', {
            email: 'john.developer@skillswap.com',
            password: 'John123'
        });
        
        const token = loginResponse.data.token;
        console.log('✅ Login successful\n');
        
        // Step 2: Connect to SignalR Hub
        console.log('2️⃣ Connecting to SignalR Hub...');
        const connection = new HubConnectionBuilder()
            .withUrl('http://localhost:5095/hubs/dashboard', {
                accessTokenFactory: () => token
            })
            .configureLogging(LogLevel.Information)
            .build();

        // Handle UserStatsUpdate event
        connection.on('UserStatsUpdate', (userStats) => {
            console.log('📊 Received User Stats Update:');
            console.log('   💰 Total Earnings: $' + userStats.totalEarnings);
            console.log('   📅 Total Bookings: ' + userStats.totalBookings);
            console.log('   🎯 Active Offers: ' + userStats.activeOffers);
            console.log('   ⭐ Average Rating: ' + userStats.averageRating);
            console.log('   📈 Completion Rate: ' + userStats.completionRate + '%');
            console.log('   🔥 Recent Activity: ' + userStats.recentActivityCount);
            console.log('   📋 Upcoming Appointments: ' + userStats.upcomingAppointments);
            console.log('   🗓️ This Month Bookings: ' + userStats.thisMonthBookings);
            console.log('   💵 This Month Earnings: $' + userStats.thisMonthEarnings);
        });

        // Start connection
        await connection.start();
        console.log('✅ SignalR connection established\n');
        
        // Step 3: Join user group to receive updates
        console.log('3️⃣ Joining user group...');
        await connection.invoke('JoinUserGroup');
        console.log('✅ Joined user group\n');
        
        // Step 4: Request dashboard data via REST API (which should trigger SignalR update)
        console.log('4️⃣ Requesting dashboard data...');
        const dashboardResponse = await axios.get('http://localhost:5095/api/dashboard/user', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log('✅ Dashboard API Response:', dashboardResponse.data);
        
        // Wait for SignalR message
        console.log('\n⏳ Waiting for SignalR UserStatsUpdate...');
        
        // Keep connection alive for a few seconds to receive messages
        setTimeout(async () => {
            console.log('\n🔄 Stopping connection...');
            await connection.stop();
            console.log('✅ SignalR connection closed');
            console.log('\n🎉 SignalR Dashboard Test Complete!');
        }, 5000);
        
    } catch (error) {
        console.error('❌ Error:', error.response?.data || error.message);
        if (error.stack) {
            console.error('Stack trace:', error.stack);
        }
    }
}

testSignalRDashboard();