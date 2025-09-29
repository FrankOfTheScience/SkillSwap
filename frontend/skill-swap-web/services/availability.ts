import api from './api'

export interface OfferAvailability {
  id: number
  offerId: number
  dayOfWeek: number // 0 = Sunday, 1 = Monday, etc.
  startTime: string // HH:mm format
  endTime: string // HH:mm format
  isAvailable: boolean
}

export interface CreateAvailabilityRequest {
  dayOfWeek: number // 0 = Sunday, 1 = Monday, etc.
  startTime: string // HH:mm format
  endTime: string // HH:mm format
}

export interface UpdateAvailabilityRequest {
  dayOfWeek?: number
  startTime?: string
  endTime?: string
  isAvailable?: boolean
}

export interface AvailableSlot {
  dateTime: string
  durationInMinutes: number
  availabilityId: number
}

export const getOfferAvailability = async (offerId: number): Promise<OfferAvailability[]> => {
  const response = await api.get(`/api/OfferAvailability/${offerId}`)
  return response.data
}

export const createAvailability = async (offerId: number, data: CreateAvailabilityRequest): Promise<OfferAvailability> => {
  const response = await api.post(`/api/OfferAvailability/${offerId}`, data)
  return response.data
}

export const updateAvailability = async (availabilityId: number, data: UpdateAvailabilityRequest): Promise<OfferAvailability> => {
  const response = await api.put(`/api/OfferAvailability/${availabilityId}`, data)
  return response.data
}

export const deleteAvailability = async (availabilityId: number): Promise<void> => {
  await api.delete(`/api/OfferAvailability/${availabilityId}`)
}

export const getAvailableSlots = async (
  offerId: number, 
  startDate: string, 
  endDate: string
): Promise<AvailableSlot[]> => {
  const response = await api.get(`/api/OfferAvailability/${offerId}/available-slots`, {
    params: { startDate, endDate }
  })
  return response.data
}