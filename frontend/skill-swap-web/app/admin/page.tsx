'use client';

import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { 
  Users, 
  DollarSign, 
  Calendar, 
  TrendingUp, 
  ShoppingBag, 
  Star,
  Activity,
  AlertCircle,
  CheckCircle
} from 'lucide-react';
import { useSignalR } from '@/hooks/useSignalR';

interface AdminStats {
  totalUsers: number;
  newUsersThisMonth: number;
  newUsersThisWeek: number;
  newUsersToday: number;
  totalOffers: number;
  activeOffers: number;
  newOffersThisMonth: number;
  totalBookings: number;
  completedBookings: number;
  pendingBookings: number;
  bookingsThisMonth: number;
  bookingsThisWeek: number;
  bookingsToday: number;
  totalRevenue: number;
  totalCommissions: number;
  revenueThisMonth: number;
  commissionsThisMonth: number;
  recentUsers: Array<{
    id: string;
    displayName: string;
    email: string;
    createdAt: string;
    profileCompletionPercentage: number;
  }>;
  recentBookings: Array<{
    id: number;
    status: string;
    amount: number;
    createdAt: string;
    scheduledDateTime?: string;
    offerTitle: string;
    customerName: string;
  }>;
  dailyBookings: Array<{
    date: string;
    count: number;
    revenue: number;
  }>;
  lastUpdated: string;
}

interface NewBookingNotification {
  id: number;
  amount: number;
  createdAt: string;
  offerTitle: string;
  userId: string;
}

interface NewUserNotification {
  id: string;
  displayName: string;
  email: string;
  createdAt: string;
}

export default function AdminDashboard() {
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notifications, setNotifications] = useState<Array<{
    id: string;
    type: 'booking' | 'user';
    message: string;
    timestamp: string;
  }>>([]);

  // SignalR connection for real-time updates
  const { connection, connectionState } = useSignalR('/hubs/dashboard', {
    onConnected: async () => {
      try {
        // Join admin dashboard group
        await connection?.invoke('JoinAdminDashboard');
        console.log('Joined admin dashboard group');
      } catch (err) {
        console.error('Failed to join admin dashboard group:', err);
      }
    },
    onDisconnected: () => {
      console.log('Disconnected from dashboard hub');
    }
  });

  // Listen for SignalR updates
  useEffect(() => {
    if (!connection) return;

    const handleAdminStatsUpdate = (newStats: AdminStats) => {
      console.log('Received admin stats update:', newStats);
      setStats(newStats);
      setLoading(false);
    };

    const handleNewBookingNotification = (notification: NewBookingNotification) => {
      const newNotification = {
        id: `booking-${notification.id}-${Date.now()}`,
        type: 'booking' as const,
        message: `New booking: ${notification.offerTitle} - ${formatCurrency(notification.amount)}`,
        timestamp: new Date().toISOString()
      };
      setNotifications(prev => [newNotification, ...prev.slice(0, 9)]); // Keep last 10
    };

    const handleNewUserNotification = (notification: NewUserNotification) => {
      const newNotification = {
        id: `user-${notification.id}-${Date.now()}`,
        type: 'user' as const,
        message: `New user registered: ${notification.displayName}`,
        timestamp: new Date().toISOString()
      };
      setNotifications(prev => [newNotification, ...prev.slice(0, 9)]); // Keep last 10
    };

    connection.on('AdminStatsUpdate', handleAdminStatsUpdate);
    connection.on('NewBookingNotification', handleNewBookingNotification);
    connection.on('NewUserNotification', handleNewUserNotification);

    return () => {
      connection.off('AdminStatsUpdate', handleAdminStatsUpdate);
      connection.off('NewBookingNotification', handleNewBookingNotification);
      connection.off('NewUserNotification', handleNewUserNotification);
    };
  }, [connection]);

  // Fetch initial dashboard data
  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      
      const response = await fetch('/api/dashboard/admin', {
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
      
      await fetch('/api/dashboard/admin/refresh', {
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

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat('en-US').format(num);
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold">Admin Dashboard</h1>
        </div>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4, 5, 6].map((i) => (
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
        <h1 className="text-3xl font-bold">Admin Dashboard</h1>
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

      {/* Live Notifications */}
      {notifications.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              Live Activity
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2 max-h-32 overflow-y-auto">
              {notifications.map((notification) => (
                <div key={notification.id} className="flex items-center gap-2 text-sm">
                  {notification.type === 'booking' ? (
                    <ShoppingBag className="h-4 w-4 text-green-600" />
                  ) : (
                    <Users className="h-4 w-4 text-blue-600" />
                  )}
                  <span>{notification.message}</span>
                  <span className="text-xs text-gray-500 ml-auto">
                    {formatDate(notification.timestamp)}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Key Metrics */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Users</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.totalUsers)}</div>
            <p className="text-xs text-muted-foreground">
              +{stats.newUsersThisMonth} this month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Revenue</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatCurrency(stats.totalRevenue)}</div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(stats.revenueThisMonth)} this month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Offers</CardTitle>
            <Star className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.activeOffers)}</div>
            <p className="text-xs text-muted-foreground">
              of {formatNumber(stats.totalOffers)} total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Bookings</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.totalBookings)}</div>
            <p className="text-xs text-muted-foreground">
              {stats.bookingsThisMonth} this month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Commission Revenue</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatCurrency(stats.totalCommissions)}</div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(stats.commissionsThisMonth)} this month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending Bookings</CardTitle>
            <AlertCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.pendingBookings)}</div>
            <p className="text-xs text-muted-foreground">
              Awaiting completion
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Completed Bookings</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.completedBookings)}</div>
            <p className="text-xs text-muted-foreground">
              {((stats.completedBookings / stats.totalBookings) * 100).toFixed(1)}% completion rate
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Today&apos;s Activity</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.bookingsToday}</div>
            <p className="text-xs text-muted-foreground">
              {stats.newUsersToday} new users
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Recent Activity */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Recent Users */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              Recent Users
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.recentUsers.length === 0 ? (
                <p className="text-sm text-muted-foreground">No recent users</p>
              ) : (
                stats.recentUsers.map((user) => (
                  <div key={user.id} className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{user.displayName}</p>
                      <p className="text-xs text-muted-foreground">{user.email}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">{formatDate(user.createdAt)}</p>
                      <p className="text-xs text-muted-foreground">
                        {user.profileCompletionPercentage}% profile
                      </p>
                    </div>
                  </div>
                ))
              )}
            </div>
          </CardContent>
        </Card>

        {/* Recent Bookings */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Recent Bookings
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.recentBookings.length === 0 ? (
                <p className="text-sm text-muted-foreground">No recent bookings</p>
              ) : (
                stats.recentBookings.map((booking) => (
                  <div key={booking.id} className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{booking.offerTitle}</p>
                      <p className="text-xs text-muted-foreground">
                        by {booking.customerName}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">{formatCurrency(booking.amount)}</p>
                      <p className="text-xs text-muted-foreground">
                        {booking.status} • {formatDate(booking.createdAt)}
                      </p>
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