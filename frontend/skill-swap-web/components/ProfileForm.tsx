'use client'

import { useState, useRef } from 'react'
import { useRouter } from 'next/navigation'
import Image from 'next/image'
import { User } from '../types'
import { updateProfile, uploadProfileImage, UpdateProfileRequest } from '../services/profile'
import Toast from './Toast'

interface ProfileFormProps {
  profile: User | null
  onProfileUpdate: (profile: User) => void
}

export default function ProfileForm({ profile, onProfileUpdate }: ProfileFormProps) {
  const router = useRouter();
  const [formData, setFormData] = useState<UpdateProfileRequest>({
    firstName: profile?.firstName || '',
    lastName: profile?.lastName || '',
    bio: profile?.bio || '',
    phoneNumber: profile?.phoneNumber || '',
    dateOfBirth: profile?.dateOfBirth || '',
    city: profile?.city || '',
    country: profile?.country || '',
    profession: profile?.profession || '',
    company: profile?.company || '',
    yearsOfExperience: profile?.yearsOfExperience || 0,
    skills: profile?.skills || [],
    preferredLanguage: profile?.preferredLanguage || 'en',
    timeZone: profile?.timeZone || '',
    emailNotifications: profile?.emailNotifications || true,
    pushNotifications: profile?.pushNotifications || true
  })
  
  const [loading, setLoading] = useState(false)
  const [uploadingImage, setUploadingImage] = useState(false)
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' | 'info' } | null>(null)
  const [newSkill, setNewSkill] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target
    
    if (type === 'checkbox') {
      const checked = (e.target as HTMLInputElement).checked
      setFormData(prev => ({ ...prev, [name]: checked }))
    } else if (type === 'number') {
      setFormData(prev => ({ ...prev, [name]: parseInt(value) || 0 }))
    } else {
      setFormData(prev => ({ ...prev, [name]: value }))
    }
  }

  const handleAddSkill = () => {
    if (newSkill.trim() && !formData.skills?.includes(newSkill.trim())) {
      setFormData(prev => ({
        ...prev,
        skills: [...(prev.skills || []), newSkill.trim()]
      }))
      setNewSkill('')
    }
  }

  const handleRemoveSkill = (skillToRemove: string) => {
    setFormData(prev => ({
      ...prev,
      skills: prev.skills?.filter(skill => skill !== skillToRemove) || []
    }))
  }

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    // Validate file type
    if (!file.type.startsWith('image/')) {
      setToast({ message: 'Please select an image file', type: 'error' })
      return
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      setToast({ message: 'Image must be less than 5MB', type: 'error' })
      return
    }

    try {
      setUploadingImage(true)
      const response = await uploadProfileImage(file)
      
      // Image upload successful - the profile image URL is updated on the backend
      // No need to call updateProfile again
      setToast({ message: response.message, type: 'success' })
      
      // Optionally refresh the profile data to get the new image URL
      // onProfileUpdate could be called here if needed
    } catch (error) {
      setToast({ 
        message: error instanceof Error ? error.message : 'Failed to upload image', 
        type: 'error' 
      })
    } finally {
      setUploadingImage(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    const hasChanges = (
      (formData.firstName || '') !== (profile?.firstName || '') ||
      (formData.lastName || '') !== (profile?.lastName || '') ||
      (formData.phoneNumber || '') !== (profile?.phoneNumber || '') ||
      (formData.bio || '') !== (profile?.bio || '') ||
      (formData.city || '') !== (profile?.city || '') ||
      (formData.country || '') !== (profile?.country || '') ||
      (formData.profession || '') !== (profile?.profession || '') ||
      (formData.company || '') !== (profile?.company || '') ||
      JSON.stringify((formData.skills || []).sort()) !== JSON.stringify((profile?.skills || []).sort())
    )
    
    if (!hasChanges) {
      setToast({ message: 'No changes to save', type: 'info' })
      return
    }

    try {
      setLoading(true)
      
      // Validate form data before sending
      const validatedData = {
        firstName: formData.firstName?.trim().substring(0, 50) || undefined,
        lastName: formData.lastName?.trim().substring(0, 50) || undefined,
        bio: formData.bio?.trim().substring(0, 1000) || undefined,
        phoneNumber: formData.phoneNumber?.trim() || undefined,
        dateOfBirth: formData.dateOfBirth || undefined,
        city: formData.city?.trim().substring(0, 100) || undefined,
        country: formData.country?.trim().substring(0, 100) || undefined,
        profession: formData.profession?.trim().substring(0, 200) || undefined,
        company: formData.company?.trim().substring(0, 200) || undefined,
        yearsOfExperience: Math.min(Math.max(formData.yearsOfExperience || 0, 0), 100),
        skills: formData.skills || [],
        preferredLanguage: formData.preferredLanguage?.substring(0, 10) || 'en',
        timeZone: formData.timeZone?.substring(0, 100) || undefined,
        emailNotifications: formData.emailNotifications,
        pushNotifications: formData.pushNotifications
      }
      
      const updatedProfile = await updateProfile(validatedData)
      onProfileUpdate(updatedProfile)
      setToast({ message: 'Profile updated successfully!', type: 'success' })
      
      // Navigate back to dashboard after successful save
      setTimeout(() => {
        router.push('/profile')
      }, 1500)
    } catch (error) {
      setToast({ 
        message: error instanceof Error ? error.message : 'Failed to update profile', 
        type: 'error' 
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="bg-white rounded-lg shadow-xl border border-gray-200">
      <form onSubmit={handleSubmit} className="p-6 space-y-6">
        {/* Profile Image Section */}
        <div className="border-b border-gray-200 pb-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Profile Picture</h3>
          <div className="flex items-center space-x-6">
            <div className="relative">
              <div className="w-24 h-24 rounded-full overflow-hidden bg-gray-200">
                {profile?.profileImageUrl ? (
                  <Image
                    src={profile.profileImageUrl}
                    alt="Profile"
                    width={96}
                    height={96}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-gray-500">
                    <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
                    </svg>
                  </div>
                )}
              </div>
              {uploadingImage && (
                <div className="absolute inset-0 bg-black bg-opacity-50 rounded-full flex items-center justify-center">
                  <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-white"></div>
                </div>
              )}
            </div>
            <div>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={handleImageUpload}
                className="hidden"
              />
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                disabled={uploadingImage}
                className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50"
              >
                {uploadingImage ? 'Uploading...' : 'Change Photo'}
              </button>
              <p className="text-sm text-gray-500 mt-1">JPG, PNG up to 5MB</p>
            </div>
          </div>
        </div>

        {/* Personal Information */}
        <div className="border-b border-gray-200 pb-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Personal Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                First Name
              </label>
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Last Name
              </label>
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Phone Number
              </label>
              <input
                type="tel"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Date of Birth
              </label>
              <input
                type="date"
                name="dateOfBirth"
                value={formData.dateOfBirth}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                City
              </label>
              <input
                type="text"
                name="city"
                value={formData.city}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Country
              </label>
              <input
                type="text"
                name="country"
                value={formData.country}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
          <div className="mt-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Bio
            </label>
            <textarea
              name="bio"
              value={formData.bio}
              onChange={handleInputChange}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Tell us about yourself..."
            />
          </div>
        </div>

        {/* Professional Information */}
        <div className="border-b border-gray-200 pb-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Professional Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Profession
              </label>
              <input
                type="text"
                name="profession"
                value={formData.profession}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Company
              </label>
              <input
                type="text"
                name="company"
                value={formData.company}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Years of Experience
              </label>
              <input
                type="number"
                name="yearsOfExperience"
                value={formData.yearsOfExperience}
                onChange={handleInputChange}
                min="0"
                max="50"
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* Skills Section */}
          <div className="mt-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Skills
            </label>
            <div className="flex flex-wrap gap-2 mb-3">
              {formData.skills?.map((skill, index) => (
                <span
                  key={index}
                  className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-blue-100 text-blue-800"
                >
                  {skill}
                  <button
                    type="button"
                    onClick={() => handleRemoveSkill(skill)}
                    className="ml-2 text-blue-600 hover:text-blue-800"
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
            <div className="flex gap-2">
              <input
                type="text"
                value={newSkill}
                onChange={(e) => setNewSkill(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddSkill())}
                placeholder="Add a skill"
                className="flex-1 px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              <button
                type="button"
                onClick={handleAddSkill}
                className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700"
              >
                Add
              </button>
            </div>
          </div>
        </div>

        {/* Preferences */}
        <div className="border-b border-gray-200 pb-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Preferences</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Preferred Language
              </label>
              <select
                name="preferredLanguage"
                value={formData.preferredLanguage}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="en">English</option>
                <option value="es">Spanish</option>
                <option value="fr">French</option>
                <option value="de">German</option>
                <option value="it">Italian</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Time Zone
              </label>
              <select
                name="timeZone"
                value={formData.timeZone}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Select a timezone...</option>
                <option value="UTC">UTC (Coordinated Universal Time)</option>
                <option value="America/New_York">EST/EDT (Eastern Time)</option>
                <option value="America/Chicago">CST/CDT (Central Time)</option>
                <option value="America/Denver">MST/MDT (Mountain Time)</option>
                <option value="America/Los_Angeles">PST/PDT (Pacific Time)</option>
                <option value="Europe/London">GMT/BST (London)</option>
                <option value="Europe/Paris">CET/CEST (Central European)</option>
                <option value="Europe/Rome">CET/CEST (Rome)</option>
                <option value="Europe/Berlin">CET/CEST (Berlin)</option>
                <option value="Europe/Madrid">CET/CEST (Madrid)</option>
                <option value="Asia/Tokyo">JST (Japan Standard Time)</option>
                <option value="Asia/Shanghai">CST (China Standard Time)</option>
                <option value="Asia/Kolkata">IST (India Standard Time)</option>
                <option value="Australia/Sydney">AEST/AEDT (Sydney)</option>
                <option value="America/Toronto">EST/EDT (Toronto)</option>
                <option value="America/Vancouver">PST/PDT (Vancouver)</option>
                <option value="Asia/Dubai">GST (Gulf Standard Time)</option>
                <option value="Europe/Moscow">MSK (Moscow Standard Time)</option>
                <option value="Africa/Cairo">EET (Eastern European Time)</option>
                <option value="Pacific/Auckland">NZST/NZDT (New Zealand)</option>
              </select>
            </div>
          </div>

          {/* Notification Preferences */}
          <div className="mt-4">
            <h4 className="text-sm font-medium text-gray-700 mb-3">Notifications</h4>
            <div className="space-y-3">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="emailNotifications"
                  checked={formData.emailNotifications}
                  onChange={handleInputChange}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Email notifications</span>
              </label>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="pushNotifications"
                  checked={formData.pushNotifications}
                  onChange={handleInputChange}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Push notifications</span>
              </label>
            </div>
          </div>
        </div>

        {/* Submit Button */}
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => router.push('/profile')}
            className="bg-gray-500 hover:bg-gray-600 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 disabled:from-gray-400 disabled:to-gray-500 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg disabled:shadow-none disabled:transform-none"
          >
            {loading ? 'Saving...' : '💾 Save Changes'}
          </button>
        </div>
      </form>

      {/* Toast Notification */}
      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          isVisible={!!toast}
          onClose={() => setToast(null)}
        />
      )}
    </div>
  )
}