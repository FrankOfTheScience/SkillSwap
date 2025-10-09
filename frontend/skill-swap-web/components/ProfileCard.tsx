'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { User } from '../types'
import { getProfileCompletion, ProfileCompletionResponse } from '../services/profile'

interface ProfileCardProps {
  user: User
}

export default function ProfileCard({ user }: ProfileCardProps) {
  const [completion, setCompletion] = useState<ProfileCompletionResponse | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    // Skip profile completion for admin users
    if (user.role === 'Admin') return
    
    loadCompletion()
  }, [user.role])

  const loadCompletion = async () => {
    try {
      setLoading(true)
      // Only load completion if user is authenticated
      const token = localStorage.getItem('token')
      if (!token) {
        console.log('No auth token found, skipping profile completion check')
        return
      }
      
      const data = await getProfileCompletion()
      setCompletion(data)
    } catch (error) {
      console.error('Error loading profile completion:', error)
      // If 401, user is not authenticated - this is expected for guest users
      if (error instanceof Error && error.message.includes('401')) {
        console.log('User not authenticated, skipping profile completion')
      }
    } finally {
      setLoading(false)
    }
  }

  const getCompletionColor = (percentage: number) => {
    if (percentage >= 80) return 'text-green-600'
    if (percentage >= 50) return 'text-yellow-600'
    return 'text-red-600'
  }

  const getProgressBarColor = (percentage: number) => {
    if (percentage >= 80) return 'bg-green-500'
    if (percentage >= 50) return 'bg-yellow-500'
    return 'bg-red-500'
  }

  // Skip profile completion for admin users
  if (user.role === 'Admin') return null

  const percentage = completion?.percentage || user.profileCompletionPercentage || 0

  if (percentage >= 80) return null // Don't show if profile is mostly complete

  return (
    <div className="mb-6 p-4 bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg border border-blue-200">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div className="relative w-12 h-12">
            <svg className="w-12 h-12 transform -rotate-90" viewBox="0 0 100 100">
              <circle
                cx="50"
                cy="50"
                r="35"
                stroke="currentColor"
                strokeWidth="8"
                fill="transparent"
                className="text-gray-200"
              />
              <circle
                cx="50"
                cy="50"
                r="35"
                stroke="currentColor"
                strokeWidth="8"
                fill="transparent"
                strokeDasharray={`${2 * Math.PI * 35}`}
                strokeDashoffset={`${2 * Math.PI * 35 * (1 - percentage / 100)}`}
                className={getProgressBarColor(percentage)}
                strokeLinecap="round"
              />
            </svg>
            <div className="absolute inset-0 flex items-center justify-center">
              <span className={`text-sm font-bold ${getCompletionColor(percentage)}`}>
                {percentage}%
              </span>
            </div>
          </div>
          <div>
            <h3 className="font-medium text-gray-900">Complete Your Profile</h3>
            <p className="text-sm text-gray-600">
              {loading 
                ? 'Checking...' 
                : completion?.suggestions?.length 
                  ? `${completion.suggestions.length} fields remaining`
                  : 'Boost your visibility'
              }
            </p>
          </div>
        </div>
        <Link
          href="/profile"
          className="bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white px-6 py-2.5 rounded-lg text-sm font-semibold transition-all duration-200 transform hover:scale-105 shadow-md hover:shadow-lg"
        >
          Complete Profile
        </Link>
      </div>
      
      {completion && completion.suggestions.length > 0 && (
        <div className="mt-3 flex flex-wrap gap-2">
          {completion.suggestions.slice(0, 3).map((suggestion, index) => (
            <span
              key={index}
              className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-blue-100 text-blue-800"
            >
              {suggestion}
            </span>
          ))}
          {completion.suggestions.length > 3 && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-gray-100 text-gray-600">
              +{completion.suggestions.length - 3} more
            </span>
          )}
        </div>
      )}
    </div>
  )
}