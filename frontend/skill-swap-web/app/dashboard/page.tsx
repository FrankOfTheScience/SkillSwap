'use client';

import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Button } from '@/components/ui/button';
import { TrendingUp, DollarSign, Calendar, User, Star, Activity } from 'lucide-react';
import { useSignalR } from '@/hooks/useSignalR';

interface UserStats {
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
  recentActivity: Array<{
    id: number;
    status: string;
    amount: number;
    createdAt: string;
    scheduledDateTime?: string;
    offerTitle: string;
    type: 'Customer' | 'Provider';
  }>;
  upcomingAppointments: Array<{
    id: number;
    scheduledDateTime: string;
    durationInMinutes: number;
    location: string;
    isOnline: boolean;
    offerTitle: string;
    type: 'Customer' | 'Provider';
  }>;
  lastUpdated: string;
}

export default function UserDashboard() {
  const [stats, setStats] = useState<UserStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // SignalR connection for real-time updates
  const { connection, connectionState } = useSignalR('/hubs/dashboard', {
    onConnected: async () => {
      try {
        // Join user dashboard group
        await connection?.invoke('JoinUserDashboard');
        console.log('Joined user dashboard group');
      } catch (err) {
        console.error('Failed to join user dashboard group:', err);
      }
    },
    onDisconnected: () => {
      console.log('Disconnected from dashboard hub');
    }
  });

  // Listen for SignalR updates
  useEffect(() => {
    if (!connection) return;

    const handleUserStatsUpdate = (newStats: UserStats) => {
      console.log('Received user stats update:', newStats);
      setStats(newStats);
      setLoading(false);
    };

    connection.on('UserStatsUpdate', handleUserStatsUpdate);

    return () => {
      connection.off('UserStatsUpdate', handleUserStatsUpdate);
    };
  }, [connection]);

  // Fetch initial dashboard data
  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      
      const response = await fetch('/api/dashboard/user', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error('Failed to fetch dashboard data');
      }

      // The API returns a message indicating data will be sent via SignalR
      // The actual data will come through the SignalR connection
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const refreshDashboard = async () => {
    try {
      const token = localStorage.getItem('token');
      
      await fetch('/api/dashboard/user/refresh', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });
    } catch (err) {
      console.error('Failed to refresh dashboard:', err);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount / 100); // Convert from cents
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold">My Dashboard</h1>
        </div>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <Card key={i} className="animate-pulse">
              <CardHeader className="pb-2">
                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
              </CardHeader>
              <CardContent>
                <div className="h-8 bg-gray-200 rounded w-1/2"></div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-4">Error Loading Dashboard</h1>
          <p className="text-gray-600 mb-4">{error}</p>
          <Button onClick={fetchDashboardData}>Try Again</Button>
        </div>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">No Dashboard Data</h1>
          <p className="text-gray-600 mb-4">Unable to load dashboard statistics.</p>
          <Button onClick={fetchDashboardData}>Refresh</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">My Dashboard</h1>
        <div className="flex items-center gap-2">
          <div className={`w-2 h-2 rounded-full ${connectionState === 'Connected' ? 'bg-green-500' : 'bg-red-500'}`}></div>
          <span className="text-sm text-gray-600">
            {connectionState === 'Connected' ? 'Live' : 'Disconnected'}
          </span>
          <Button onClick={refreshDashboard} variant="outline" size="sm">
            Refresh
          </Button>
        </div>
      </div>

      {/* Key Metrics */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Profile Completion</CardTitle>
            <User className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.profileCompletion}%</div>
            <Progress value={stats.profileCompletion} className="mt-2" />
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Earnings</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatCurrency(stats.totalEarnings)}</div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(stats.monthlyEarnings)} this month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Offers</CardTitle>
            <Star className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.activeOffers}</div>
            <p className="text-xs text-muted-foreground">
              of {stats.totalOffers} total offers
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Completed Bookings</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {stats.completedBookingsAsProvider + stats.completedBookingsAsCustomer}
            </div>
            <p className="text-xs text-muted-foreground">
              {stats.completedBookingsAsProvider} as provider, {stats.completedBookingsAsCustomer} as customer
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Charts and Details */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Recent Activity */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              Recent Activity
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.recentActivity.length === 0 ? (
                <p className="text-sm text-muted-foreground">No recent activity</p>
              ) : (
                stats.recentActivity.map((activity) => (
                  <div key={activity.id} className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{activity.offerTitle}</p>
                      <p className="text-xs text-muted-foreground">
                        {activity.type} • {formatDate(activity.createdAt)}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">{formatCurrency(activity.amount)}</p>
                      <p className="text-xs text-muted-foreground">{activity.status}</p>
                    </div>
                  </div>
                ))
              )}
            </div>
          </CardContent>
        </Card>

        {/* Upcoming Appointments */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Upcoming Appointments
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.upcomingAppointments.length === 0 ? (
                <p className="text-sm text-muted-foreground">No upcoming appointments</p>
              ) : (
                stats.upcomingAppointments.map((appointment) => (
                  <div key={appointment.id} className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{appointment.offerTitle}</p>
                      <p className="text-xs text-muted-foreground">
                        {appointment.type} • {appointment.durationInMinutes} min
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {appointment.isOnline ? 'Online' : appointment.location}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">{formatDate(appointment.scheduledDateTime)}</p>
                    </div>
                  </div>
                ))
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Footer */}
      <div className="text-center text-sm text-muted-foreground">
        Last updated: {formatDate(stats.lastUpdated)}
      </div>
    </div>
  );
}