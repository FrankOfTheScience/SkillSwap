'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import Image from 'next/image'
import { ArrowLeft, Edit, User as UserIcon, Mail, MapPin, Briefcase, Clock, Star } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'
import ProfileForm from '../../components/ProfileForm'
import UserDashboard from '../../components/UserDashboard'
import MyBookingsModal from '../../components/MyBookingsModal'
import DeleteConfirmModal from '../../components/DeleteConfirmModal'
import { getProfile } from '../../services/profile'
import { cancelBooking } from '../../services/booking'
import { User, Booking } from '../../types'
import ErrorDisplay from '../../components/ErrorDisplay'

export default function ProfilePage() {
  const { user, loading: authLoading } = useAuth()
  const [profile, setProfile] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isEditing, setIsEditing] = useState(false)
  
  // Modal states
  const [showBookingsModal, setShowBookingsModal] = useState(false)
  const [cancelConfirmModal, setCancelConfirmModal] = useState<{ booking: Booking; isOpen: boolean }>({ 
    booking: {} as Booking, 
    isOpen: false 
  })

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

  const handleProfileUpdate = (updatedProfile: User) => {
    setProfile(updatedProfile)
    setIsEditing(false)
  }

  const handleCancelBooking = async (bookingId: string) => {
    // For now, we need to find the booking by ID
    // In a real app, you might want to pass more data or fetch the booking details
    const mockBooking = { 
      id: bookingId,
      offerId: 'mock-offer-id',
      userId: 'mock-user-id',
      status: 'Confirmed' as const,
      amount: 0,
      commissionAmount: 0,
      createdAt: new Date().toISOString(),
      scheduledDateTime: new Date().toISOString(),
      durationInMinutes: 60,
      isOnline: true,
      offer: { title: 'Selected Booking' } 
    } as unknown as Booking;
    
    setCancelConfirmModal({ booking: mockBooking, isOpen: true })
    setShowBookingsModal(false)
  }

  const confirmCancelBooking = async () => {
    try {
      await cancelBooking(cancelConfirmModal.booking.id)
      setCancelConfirmModal({ booking: {} as Booking, isOpen: false })
      // You might want to refresh bookings or show a success message here
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to cancel booking')
      setCancelConfirmModal({ booking: {} as Booking, isOpen: false })
    }
  }

  const closeCancelConfirm = () => {
    setCancelConfirmModal({ booking: {} as Booking, isOpen: false })
    setShowBookingsModal(true) // Return to bookings modal
  }

  if (authLoading || loading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <div className="text-gray-800">Loading profile...</div>
        </div>
      </div>
    )
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-800 mb-4">Access Denied</h1>
          <p className="text-gray-600 mb-6">You need to be logged in to access your profile.</p>
          <Link 
            href="/"
            className="inline-flex items-center bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Go to Home
          </Link>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-white py-8">
        <div className="max-w-4xl mx-auto px-4">
          <Link 
            href="/"
            className="inline-flex items-center text-gray-600 hover:text-gray-800 transition-colors mb-6"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Home
          </Link>
          <ErrorDisplay 
            error={error} 
            onRetry={loadProfile}
          />
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-black">
      <div className="max-w-4xl mx-auto px-4 py-8">
        {/* Navigation Header */}
        <div className="mb-8">
          <Link 
            href="/"
            className="inline-flex items-center text-gray-400 hover:text-gray-200 transition-colors mb-6"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Home
          </Link>
          <h1 className="text-3xl font-bold text-white">My Profile</h1>
          <p className="text-gray-400 mt-2">
            Manage your personal information and preferences
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Profile Display */}
          {!isEditing ? (
            <div className="lg:col-span-3">
              <div className="bg-white rounded-lg shadow-lg border border-gray-200 p-6">
                {/* Profile Header */}
                <div className="flex items-center justify-between mb-6">
                  <div className="flex items-center space-x-4">
                    <div className="w-20 h-20 bg-gradient-to-r from-blue-500 to-indigo-600 rounded-full flex items-center justify-center">
                      {profile?.profileImageUrl ? (
                        <Image
                          src={profile.profileImageUrl}
                          alt="Profile"
                          width={80}
                          height={80}
                          className="w-full h-full rounded-full object-cover"
                        />
                      ) : (
                        <UserIcon className="w-8 h-8 text-white" />
                      )}
                    </div>
                    <div>
                      <h2 className="text-2xl font-bold text-gray-800">
                        {profile?.firstName || profile?.lastName 
                          ? `${profile?.firstName || ''} ${profile?.lastName || ''}`.trim()
                          : profile?.displayName || 'User'
                        }
                      </h2>
                      <div className="flex items-center text-gray-600 mt-1">
                        <Mail className="w-4 h-4 mr-2" />
                        {profile?.email}
                      </div>
                      {profile?.profession && (
                        <div className="flex items-center text-gray-600 mt-1">
                          <Briefcase className="w-4 h-4 mr-2" />
                          {profile.profession}
                          {profile.company && ` at ${profile.company}`}
                        </div>
                      )}
                      {(profile?.city || profile?.country) && (
                        <div className="flex items-center text-gray-600 mt-1">
                          <MapPin className="w-4 h-4 mr-2" />
                          {[profile?.city, profile?.country].filter(Boolean).join(', ')}
                        </div>
                      )}
                    </div>
                  </div>
                  <button
                    onClick={() => setIsEditing(true)}
                    className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg flex items-center"
                  >
                    <Edit className="w-4 h-4 mr-2" />
                    Edit Profile
                  </button>
                </div>

                {/* Bio Section */}
                {profile?.bio && (
                  <div className="mb-6">
                    <h3 className="text-lg font-semibold text-gray-800 mb-2">About</h3>
                    <p className="text-gray-600 leading-relaxed">{profile.bio}</p>
                  </div>
                )}

                {/* Stats Grid */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                  <div className="bg-gradient-to-br from-blue-50 to-indigo-50 p-4 rounded-lg border border-blue-100">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-blue-600 text-sm font-medium">Experience</p>
                        <p className="text-2xl font-bold text-blue-800">
                          {profile?.yearsOfExperience || 0} years
                        </p>
                      </div>
                      <Clock className="w-8 h-8 text-blue-500" />
                    </div>
                  </div>
                  
                  <div className="bg-gradient-to-br from-emerald-50 to-teal-50 p-4 rounded-lg border border-emerald-100">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-emerald-600 text-sm font-medium">Skills</p>
                        <p className="text-2xl font-bold text-emerald-800">
                          {profile?.skills?.length || 0}
                        </p>
                      </div>
                      <Star className="w-8 h-8 text-emerald-500" />
                    </div>
                  </div>
                  
                  <div className="bg-gradient-to-br from-purple-50 to-pink-50 p-4 rounded-lg border border-purple-100">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-purple-600 text-sm font-medium">Profile Completion</p>
                        <p className="text-2xl font-bold text-purple-800">
                          {profile?.profileCompletionPercentage || 0}%
                        </p>
                      </div>
                      <UserIcon className="w-8 h-8 text-purple-500" />
                    </div>
                  </div>
                </div>

                {/* Skills Section */}
                {profile?.skills && profile.skills.length > 0 && (
                  <div className="mb-6">
                    <h3 className="text-lg font-semibold text-gray-800 mb-3">Skills</h3>
                    <div className="flex flex-wrap gap-2">
                      {profile.skills.map((skill, index) => (
                        <span
                          key={index}
                          className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-blue-100 text-blue-800"
                        >
                          {skill}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

                {/* Contact Information */}
                <div>
                  <h3 className="text-lg font-semibold text-gray-800 mb-3">Contact Information</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
                    {profile?.phoneNumber && (
                      <div>
                        <span className="font-medium">Phone:</span> {profile.phoneNumber}
                      </div>
                    )}
                    {profile?.timeZone && (
                      <div>
                        <span className="font-medium">Time Zone:</span> {profile.timeZone}
                      </div>
                    )}
                    {profile?.preferredLanguage && (
                      <div>
                        <span className="font-medium">Language:</span> {profile.preferredLanguage}
                      </div>
                    )}
                    {profile?.dateOfBirth && (
                      <div>
                        <span className="font-medium">Date of Birth:</span> {new Date(profile.dateOfBirth).toLocaleDateString()}
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Dashboard Section */}
              {user && (
                <div className="mt-8">
                  <UserDashboard
                    user={user}
                  />
                </div>
              )}
            </div>
          ) : (
            /* Edit Mode */
            <div className="lg:col-span-3">
              <div className="mb-4">
                <button
                  onClick={() => setIsEditing(false)}
                  className="text-gray-600 hover:text-gray-800 transition-colors flex items-center"
                >
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  Cancel Editing
                </button>
              </div>
              <ProfileForm 
                profile={profile}
                onProfileUpdate={handleProfileUpdate}
              />
            </div>
          )}
        </div>
      </div>

      {/* Modals */}
      <MyBookingsModal
        isOpen={showBookingsModal}
        onClose={() => setShowBookingsModal(false)}
        onCancelBooking={handleCancelBooking}
        onViewOffer={() => {}} // You can implement this later if needed
      />

      <DeleteConfirmModal
        isOpen={cancelConfirmModal.isOpen}
        onClose={closeCancelConfirm}
        onConfirm={confirmCancelBooking}
        title="Cancel Booking"
        message={cancelConfirmModal.booking?.offer?.title ? 
          `Are you sure you want to cancel your booking for "${cancelConfirmModal.booking.offer.title}"? This action cannot be undone.` :
          "Are you sure you want to cancel this booking?"
        }
      />
    </div>
  )
}