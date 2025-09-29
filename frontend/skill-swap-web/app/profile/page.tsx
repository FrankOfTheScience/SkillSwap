'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '../../hooks/useAuth'
import ProfileForm from '../../components/ProfileForm'
import ProfileCompletion from '../../components/ProfileCompletion'
import { getProfile } from '../../services/profile'
import { User } from '../../types'
import ErrorDisplay from '../../components/ErrorDisplay'

export default function ProfilePage() {
  const { user, loading: authLoading } = useAuth()
  const [profile, setProfile] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!authLoading && user) {
      loadProfile()
    }
  }, [authLoading, user])

  const loadProfile = async () => {
    try {
      setLoading(true)
      setError(null)
      const profileData = await getProfile()
      setProfile(profileData)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load profile')
    } finally {
      setLoading(false)
    }
  }

  if (authLoading || loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Access Denied</h1>
          <p className="text-gray-600">Please log in to view your profile.</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4">
          <ErrorDisplay 
            error={error} 
            onRetry={loadProfile}
          />
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">My Profile</h1>
          <p className="text-gray-600 mt-2">
            Manage your personal information and preferences
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Profile Completion Sidebar */}
          <div className="lg:col-span-1">
            <ProfileCompletion 
              profile={profile}
            />
          </div>

          {/* Main Profile Form */}
          <div className="lg:col-span-2">
            <ProfileForm 
              profile={profile}
              onProfileUpdate={(updatedProfile: User) => {
                setProfile(updatedProfile)
              }}
            />
          </div>
        </div>
      </div>
    </div>
  )
}