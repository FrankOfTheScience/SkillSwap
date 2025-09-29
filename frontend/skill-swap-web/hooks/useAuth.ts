'use client'

import { useState, useEffect } from 'react'
import { getCurrentUser } from '../services/auth'
import { User } from '../types'

interface UseAuthReturn {
  user: User | null
  loading: boolean
  isAuthenticated: boolean
}

export function useAuth(): UseAuthReturn {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const checkAuth = () => {
      try {
        const currentUser = getCurrentUser()
        setUser(currentUser)
      } catch (error) {
        console.error('Error checking authentication:', error)
        setUser(null)
      } finally {
        setLoading(false)
      }
    }

    checkAuth()
  }, [])

  return {
    user,
    loading,
    isAuthenticated: !!user
  }
}