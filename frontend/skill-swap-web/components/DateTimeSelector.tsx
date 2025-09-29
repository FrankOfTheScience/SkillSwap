'use client'

import { useState, useEffect, useCallback } from 'react'
import { Offer } from '../types'
import { getAvailableSlots, AvailableSlot } from '../services/availability'

interface DateTimeSelectorProps {
  offer: Offer
  selectedDateTime: string | null
  onDateTimeSelect: (dateTime: string, durationInMinutes: number) => void
  onClose: () => void
}

export default function DateTimeSelector({ 
  offer, 
  selectedDateTime, 
  onDateTimeSelect, 
  onClose 
}: DateTimeSelectorProps) {
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedWeek, setSelectedWeek] = useState(0) // 0 = current week, 1 = next week, etc.
  const [error, setError] = useState<string | null>(null)

  const getWeekDates = (weekOffset: number) => {
    const today = new Date()
    const startOfWeek = new Date(today)
    startOfWeek.setDate(today.getDate() - today.getDay() + (weekOffset * 7))
    
    const endOfWeek = new Date(startOfWeek)
    endOfWeek.setDate(startOfWeek.getDate() + 6)
    
    return { startOfWeek, endOfWeek }
  }

  const loadAvailableSlots = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      
      const { startOfWeek, endOfWeek } = getWeekDates(selectedWeek)
      const slots = await getAvailableSlots(
        offer.id,
        startOfWeek.toISOString().split('T')[0],
        endOfWeek.toISOString().split('T')[0]
      )
      
      setAvailableSlots(slots)
    } catch (err) {
      console.error('Error loading available slots:', err)
      setError('Failed to load available times. Please try again.')
    } finally {
      setLoading(false)
    }
  }, [selectedWeek, offer.id])

  useEffect(() => {
    loadAvailableSlots()
  }, [loadAvailableSlots])

  const formatDate = (dateString: string) => {
    const date = new Date(dateString)
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'short',
      day: 'numeric'
    })
  }

  const formatTime = (dateString: string) => {
    const date = new Date(dateString)
    return date.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    })
  }

  const groupSlotsByDate = (slots: AvailableSlot[]) => {
    const groups: { [date: string]: AvailableSlot[] } = {}
    
    slots.forEach(slot => {
      const dateKey = new Date(slot.dateTime).toDateString()
      if (!groups[dateKey]) {
        groups[dateKey] = []
      }
      groups[dateKey].push(slot)
    })
    
    return groups
  }

  const getWeekLabel = (weekOffset: number) => {
    if (weekOffset === 0) return 'This Week'
    if (weekOffset === 1) return 'Next Week'
    return `Week of ${getWeekDates(weekOffset).startOfWeek.toLocaleDateString()}`
  }

  const groupedSlots = groupSlotsByDate(availableSlots)
  const sortedDates = Object.keys(groupedSlots).sort((a, b) => 
    new Date(a).getTime() - new Date(b).getTime()
  )

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-lg font-semibold text-gray-900">
                Select Date & Time
              </h3>
              <p className="text-gray-600 mt-1">
                {offer.title} • {offer.durationInMinutes} minutes • ${offer.price}
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        <div className="p-6">
          {/* Week Navigation */}
          <div className="flex items-center justify-between mb-6">
            <button
              onClick={() => setSelectedWeek(Math.max(0, selectedWeek - 1))}
              disabled={selectedWeek === 0}
              className="flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
              Previous
            </button>
            
            <h4 className="text-lg font-medium text-gray-900">
              {getWeekLabel(selectedWeek)}
            </h4>
            
            <button
              onClick={() => setSelectedWeek(selectedWeek + 1)}
              disabled={selectedWeek >= 3} // Limit to 4 weeks ahead
              className="flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
              <svg className="w-4 h-4 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </div>

          {/* Loading State */}
          {loading && (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
              <p className="text-gray-600 mt-4">Loading available times...</p>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="text-center py-12">
              <div className="text-red-500 mb-4">
                <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                </svg>
              </div>
              <p className="text-red-600 mb-4">{error}</p>
              <button
                onClick={loadAvailableSlots}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              >
                Try Again
              </button>
            </div>
          )}

          {/* Available Slots */}
          {!loading && !error && (
            <div className="space-y-6">
              {sortedDates.length === 0 ? (
                <div className="text-center py-12">
                  <div className="text-gray-400 mb-4">
                    <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <p className="text-gray-600 text-lg">No available times this week</p>
                  <p className="text-gray-500 text-sm mt-2">Try selecting a different week</p>
                </div>
              ) : (
                sortedDates.map(dateKey => (
                  <div key={dateKey} className="border border-gray-200 rounded-lg p-4">
                    <h5 className="text-md font-medium text-gray-900 mb-3">
                      {formatDate(groupedSlots[dateKey][0].dateTime)}
                    </h5>
                    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-2">
                      {groupedSlots[dateKey]
                        .sort((a, b) => new Date(a.dateTime).getTime() - new Date(b.dateTime).getTime())
                        .map((slot, index) => (
                          <button
                            key={index}
                            onClick={() => onDateTimeSelect(slot.dateTime, slot.durationInMinutes)}
                            className={`px-3 py-2 text-sm font-medium rounded-md border transition-colors ${
                              selectedDateTime === slot.dateTime
                                ? 'bg-blue-600 text-white border-blue-600'
                                : 'bg-white text-gray-700 border-gray-300 hover:bg-blue-50 hover:border-blue-300'
                            }`}
                          >
                            {formatTime(slot.dateTime)}
                          </button>
                        ))
                      }
                    </div>
                  </div>
                ))
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        {selectedDateTime && (
          <div className="border-t border-gray-200 p-6 bg-gray-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Selected:</p>
                <p className="font-medium text-gray-900">
                  {formatDate(selectedDateTime)} at {formatTime(selectedDateTime)}
                </p>
              </div>
              <div className="flex gap-3">
                <button
                  onClick={onClose}
                  className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  onClick={() => {
                    const slot = availableSlots.find(s => s.dateTime === selectedDateTime)
                    if (slot) {
                      onDateTimeSelect(selectedDateTime, slot.durationInMinutes)
                    }
                  }}
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Continue to Booking
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}