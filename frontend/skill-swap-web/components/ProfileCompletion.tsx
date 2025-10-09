'use client'

import { useEffect, useState } from 'react'
import { User } from '../types'
import { getProfileCompletion, ProfileCompletionResponse } from '../services/profile'

interface ProfileCompletionProps {
  profile: User | null
}

export default function ProfileCompletion({ profile }: ProfileCompletionProps) {
  const [completion, setCompletion] = useState<ProfileCompletionResponse | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (profile) {
      loadCompletion()
    }
  }, [profile])

  const loadCompletion = async () => {
    try {
      setLoading(true)
      const data = await getProfileCompletion()
      setCompletion(data)
    } catch (error) {
      console.error('Error loading profile completion:', error)
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

  if (loading && !completion) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <div className="animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
          <div className="h-20 bg-gray-200 rounded mb-4"></div>
          <div className="space-y-2">
            <div className="h-3 bg-gray-200 rounded"></div>
            <div className="h-3 bg-gray-200 rounded w-5/6"></div>
          </div>
        </div>
      </div>
    )
  }

  const percentage = completion?.percentage || profile?.profileCompletionPercentage || 0

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Profile Completion
        </h3>
        
        {/* Progress Circle */}
        <div className="flex items-center justify-center mb-6">
          <div className="relative w-24 h-24">
            <svg className="w-24 h-24 transform -rotate-90" viewBox="0 0 100 100">
              <circle
                cx="50"
                cy="50"
                r="40"
                stroke="currentColor"
                strokeWidth="8"
                fill="transparent"
                className="text-gray-200"
              />
              <circle
                cx="50"
                cy="50"
                r="40"
                stroke="currentColor"
                strokeWidth="8"
                fill="transparent"
                strokeDasharray={`${2 * Math.PI * 40}`}
                strokeDashoffset={`${2 * Math.PI * 40 * (1 - percentage / 100)}`}
                className={getProgressBarColor(percentage)}
                strokeLinecap="round"
              />
            </svg>
            <div className="absolute inset-0 flex items-center justify-center">
              <span className={`text-2xl font-bold ${getCompletionColor(percentage)}`}>
                {percentage}%
              </span>
            </div>
          </div>
        </div>

        {/* Completion Status */}
        <div className="text-center mb-6">
          <p className="text-sm text-gray-600">
            {percentage === 100
              ? 'Your profile is complete!'
              : `${completion?.missingFields?.length || 0} fields remaining`
            }
          </p>
        </div>

        {/* Suggestions */}
        {completion && completion.suggestions.length > 0 && (
          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-3">
              Complete your profile:
            </h4>
            <ul className="space-y-2">
              {completion.suggestions.slice(0, 5).map((suggestion, index) => (
                <li key={index} className="flex items-start">
                  <div className="flex-shrink-0 w-1.5 h-1.5 bg-blue-500 rounded-full mt-2 mr-3"></div>
                  <span className="text-sm text-gray-700">{suggestion}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Profile Benefits */}
        {percentage < 100 && (
          <div className="mt-6 p-4 bg-blue-50 rounded-lg">
            <h4 className="text-sm font-medium text-blue-900 mb-2">
              Why complete your profile?
            </h4>
            <ul className="text-sm text-blue-800 space-y-1">
              <li>• Get more booking requests</li>
              <li>• Build trust with other users</li>
              <li>• Unlock premium features</li>
            </ul>
          </div>
        )}
      </div>
    </div>
  )
}