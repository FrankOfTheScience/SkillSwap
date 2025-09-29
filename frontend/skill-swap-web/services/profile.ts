import api from './api'
import { User } from '../types'

export interface UpdateProfileRequest {
  firstName?: string
  lastName?: string
  bio?: string
  phoneNumber?: string
  dateOfBirth?: string
  city?: string
  country?: string
  profession?: string
  company?: string
  yearsOfExperience?: number
  skills?: string[]
  preferredLanguage?: string
  timeZone?: string
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
  formData.append('image', file)
  
  const response = await api.post('/api/UserProfile/upload-image', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
  return response.data
}