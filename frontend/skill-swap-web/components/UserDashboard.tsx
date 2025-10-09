'use client';

import { useEffect, useState, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { User } from '../types';
import api from '../services/api';
import { DollarSign, Calendar, Star, Activity, TrendingUp } from 'lucide-react';
import * as signalR from '@microsoft/signalr';

interface DashboardStats {
  profileCompletion: number;
  totalOffers: number;
  activeOffers: number;
  totalEarnings: number;
  monthlyEarnings: number;
  totalSpending: number;
  monthlySpending: number;
  totalBookingsAsCustomer: number;
  totalBookingsAsProvider: number;
  completedBookingsAsCustomer: number;
  completedBookingsAsProvider: number;
  recentActivity: any[];
  upcomingAppointments: any[];
  lastUpdated: string;
}

interface UserDashboardProps {
  user: User;
  onViewBookings?: () => void;
}

export default function UserDashboard({ user, onViewBookings }: UserDashboardProps) {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const router = useRouter();

  useEffect(() => {
    initializeSignalR();
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, [user]);

  const initializeSignalR = async () => {
    try {
      setLoading(true);
      setError(null);

      // Get auth token
      const token = localStorage.getItem('token');
      if (!token) {
        setError('Authentication required');
        setLoading(false);
        return;
      }

      // Create SignalR connection
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5095'}/hubs/dashboard`, {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect()
        .build();

      connectionRef.current = connection;

      // Handle user stats updates
      connection.on('UserStatsUpdate', (userStats: any) => {
        console.log('Received user stats:', userStats);
        
        // Map the backend response to our frontend interface
        const mappedStats: DashboardStats = {
          profileCompletion: userStats.ProfileCompletion || userStats.profileCompletion || 0,
          totalOffers: userStats.TotalOffers || userStats.totalOffers || 0,
          activeOffers: userStats.ActiveOffers || userStats.activeOffers || 0,
          totalEarnings: userStats.TotalEarnings || userStats.totalEarnings || 0,
          monthlyEarnings: userStats.MonthlyEarnings || userStats.monthlyEarnings || 0,
          totalSpending: userStats.TotalSpending || userStats.totalSpending || 0,
          monthlySpending: userStats.MonthlySpending || userStats.monthlySpending || 0,
          totalBookingsAsCustomer: userStats.TotalBookingsAsCustomer || userStats.totalBookingsAsCustomer || 0,
          totalBookingsAsProvider: userStats.TotalBookingsAsProvider || userStats.totalBookingsAsProvider || 0,
          completedBookingsAsCustomer: userStats.CompletedBookingsAsCustomer || userStats.completedBookingsAsCustomer || 0,
          completedBookingsAsProvider: userStats.CompletedBookingsAsProvider || userStats.completedBookingsAsProvider || 0,
          recentActivity: userStats.RecentActivity || userStats.recentActivity || [],
          upcomingAppointments: userStats.UpcomingAppointments || userStats.upcomingAppointments || [],
          lastUpdated: new Date().toISOString()
        };
        
        setStats(mappedStats);
        setLoading(false);
        setError(null);
      });

      // Handle connection events
      connection.onclose((error) => {
        console.log('SignalR connection closed:', error);
        setError('Connection lost. Trying to reconnect...');
      });

      connection.onreconnected(() => {
        console.log('SignalR reconnected');
        setError(null);
        // Re-request stats after reconnection
        requestDashboardData();
      });

      // Start the connection
      await connection.start();
      console.log('SignalR connected');

      // Join user group and request initial data
      await connection.invoke('JoinUserDashboard');
      
      // Request dashboard data from the API (triggers SignalR update)
      await requestDashboardData();

    } catch (err) {
      console.error('SignalR initialization failed:', err);
      setError('Failed to connect to real-time updates');
      setLoading(false);
    }
  };

  const requestDashboardData = async () => {
    try {
      // Call the API endpoint which triggers SignalR update
      await api.get('/api/dashboard/user');
    } catch (err) {
      console.error('Failed to request dashboard data:', err);
      setError('Failed to load dashboard data');
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-lg border border-gray-200 p-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">📊 Dashboard</h3>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <div key={i} className="animate-pulse">
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-8 bg-gray-200 rounded w-1/2"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white rounded-lg shadow-lg border border-gray-200 p-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">📊 Dashboard</h3>
        <div className="text-center text-red-600">
          <p>{error}</p>
          <button 
            onClick={initializeSignalR}
            className="mt-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg text-sm"
          >
            Try Again
          </button>
        </div>
      </div>
    );
  }

  if (!stats) {
    return null;
  }

  return (
    <div className="bg-white rounded-lg shadow-lg border border-gray-200 p-6">
      <h3 className="text-lg font-semibold text-gray-800 mb-6">📊 Dashboard</h3>
      
      {/* Key Metrics */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-gradient-to-r from-emerald-50 to-teal-50 p-4 rounded-lg border border-emerald-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-emerald-600 text-sm font-medium">Total Offers</p>
              <p className="text-2xl font-bold text-emerald-700">{stats.totalOffers}</p>
            </div>
            <TrendingUp className="h-8 w-8 text-emerald-500" />
          </div>
        </div>

        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 p-4 rounded-lg border border-blue-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-blue-600 text-sm font-medium">Active Offers</p>
              <p className="text-2xl font-bold text-blue-700">{stats.activeOffers}</p>
            </div>
            <Activity className="h-8 w-8 text-blue-500" />
          </div>
        </div>

        <div className="bg-gradient-to-r from-purple-50 to-indigo-50 p-4 rounded-lg border border-purple-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-purple-600 text-sm font-medium">Customer Bookings</p>
              <p className="text-2xl font-bold text-purple-700">{stats.totalBookingsAsCustomer}</p>
            </div>
            <Calendar className="h-8 w-8 text-purple-500" />
          </div>
        </div>

        <div className="bg-gradient-to-r from-green-50 to-emerald-50 p-4 rounded-lg border border-green-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-green-600 text-sm font-medium">Monthly Earnings</p>
              <p className="text-2xl font-bold text-green-700">{formatCurrency(stats.monthlyEarnings)}</p>
            </div>
            <DollarSign className="h-8 w-8 text-green-500" />
          </div>
        </div>

        <div className="bg-gradient-to-r from-orange-50 to-amber-50 p-4 rounded-lg border border-orange-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-orange-600 text-sm font-medium">Provider Bookings</p>
              <p className="text-2xl font-bold text-orange-700">{stats.totalBookingsAsProvider}</p>
            </div>
            <Calendar className="h-8 w-8 text-orange-500" />
          </div>
        </div>

        <div className="bg-gradient-to-r from-yellow-50 to-amber-50 p-4 rounded-lg border border-yellow-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-yellow-600 text-sm font-medium">Profile Complete</p>
              <p className="text-2xl font-bold text-yellow-700">
                {stats.profileCompletion}%
              </p>
            </div>
            <Star className="h-8 w-8 text-yellow-500" />
          </div>
        </div>
      </div>

      {/* Refresh Data Button */}
      <div className="border-t border-gray-200 pt-4">
        <button 
          onClick={() => requestDashboardData()}
          className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-lg text-sm transition-colors"
        >
          🔄 Refresh Data
        </button>
      </div>
    </div>
  );
}