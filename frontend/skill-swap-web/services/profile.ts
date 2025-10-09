import api from './api'
import { User } from '../types'

export interface UpdateProfileRequest {
  firstName?: string // max 50 chars
  lastName?: string // max 50 chars
  bio?: string // max 1000 chars
  phoneNumber?: string // must be valid phone format
  dateOfBirth?: string // ISO date string
  city?: string // max 100 chars
  country?: string // max 100 chars
  profession?: string // max 200 chars
  company?: string // max 200 chars
  yearsOfExperience?: number // 0-100 range
  skills?: string[]
  preferredLanguage?: string // max 10 chars
  timeZone?: string // max 100 chars
  emailNotifications?: boolean
  pushNotifications?: boolean
}

export interface ProfileCompletionResponse {
  percentage: number
  missingFields: string[]
  suggestions: string[]
}

export const getProfile = async (): Promise<User> => {
  const response = await api.get('/api/UserProfile')
  return response.data
}

export const updateProfile = async (data: UpdateProfileRequest): Promise<User> => {
  const response = await api.put('/api/UserProfile', data)
  return response.data
}

export const getProfileCompletion = async (): Promise<ProfileCompletionResponse> => {
  const response = await api.get('/api/UserProfile/completion')
  return response.data
}

export const uploadProfileImage = async (file: File): Promise<{ profileImageUrl: string; message: string }> => {
  const formData = new FormData()
  formData.append('Image', file) // Capital 'I' to match backend DTO
  
  const response = await api.post('/api/UserProfile/upload-image', formData)
  // Don't set Content-Type manually - let browser set it with boundary
  return response.data
}