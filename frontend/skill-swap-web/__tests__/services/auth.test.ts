import { login, register, logout, getCurrentUser } from '../../services/auth'
import api from '../../services/api'

// Mock the api module
jest.mock('../../services/api')
const mockedApi = api as jest.Mocked<typeof api>

// Mock jwt-decode
jest.mock('jwt-decode', () => ({
  jwtDecode: jest.fn()
}))

import { jwtDecode } from 'jwt-decode'
const mockedJwtDecode = jwtDecode as jest.MockedFunction<typeof jwtDecode>

// Mock localStorage
const mockLocalStorage = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
}
Object.defineProperty(window, 'localStorage', {
  value: mockLocalStorage,
  writable: true,
})

describe('Auth Service', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockLocalStorage.getItem.mockClear()
    mockLocalStorage.setItem.mockClear()
    mockLocalStorage.removeItem.mockClear()
  })

  describe('login', () => {
    it('should login user and return user data', async () => {
      const mockToken = 'mock-jwt-token'
      const expectedUser = {
        id: '123',
        email: 'test@example.com',
        displayName: 'Test User',
        role: 'User' as const,
        token: mockToken
      }

      // Mock API response
      mockedApi.post.mockResolvedValueOnce({
        data: { token: mockToken }
      })

      // Mock localStorage.getItem for getCurrentUser
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)

      // Mock JWT decode
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'test@example.com',
        displayName: 'Test User',
        role: 'User'
      })

      const result = await login({
        email: 'test@example.com',
        password: 'password123'
      })

      expect(mockedApi.post).toHaveBeenCalledWith('/auth/login', {
        email: 'test@example.com',
        password: 'password123'
      })
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('token', mockToken)
      expect(result).toEqual(expectedUser)
    })

    it('should handle login failure', async () => {
      mockedApi.post.mockRejectedValueOnce(new Error('Login failed'))

      await expect(login({
        email: 'test@example.com',
        password: 'wrongpassword'
      })).rejects.toThrow('Something went wrong. Please try again.')

      expect(mockLocalStorage.setItem).not.toHaveBeenCalled()
    })
  })

  describe('register', () => {
    it('should register user successfully', async () => {
      mockedApi.post.mockResolvedValueOnce({ data: { success: true } })

      await register({
        email: 'new@example.com',
        displayName: 'New User',
        password: 'password123'
      })

      expect(mockedApi.post).toHaveBeenCalledWith('/auth/register', {
        email: 'new@example.com',
        displayName: 'New User',
        password: 'password123'
      })
    })

    it('should handle registration failure', async () => {
      mockedApi.post.mockRejectedValueOnce(new Error('Registration failed'))

      await expect(register({
        email: 'invalid@example.com',
        displayName: 'Invalid User',
        password: 'weak'
      })).rejects.toThrow('Something went wrong. Please try again.')
    })
  })

  describe('logout', () => {
    it('should remove token from localStorage', () => {
      logout()

      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('token')
    })
  })

  describe('getCurrentUser', () => {
    it('should return null when no token exists', () => {
      mockLocalStorage.getItem.mockReturnValueOnce(null)

      const result = getCurrentUser()

      expect(result).toBeNull()
    })

    it('should decode token and return user data', () => {
      const mockToken = 'mock-jwt-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'test@example.com',
        displayName: 'Test User',
        role: 'User'
      })

      const result = getCurrentUser()

      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('token')
      expect(mockedJwtDecode).toHaveBeenCalledWith(mockToken)
      expect(result).toEqual({
        id: '123',
        email: 'test@example.com',
        displayName: 'Test User',
        role: 'User',
        token: mockToken
      })
    })

    it('should handle role from different claim formats', () => {
      const mockToken = 'mock-jwt-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'admin@example.com',
        displayName: 'Admin User',
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Admin'
      })

      const result = getCurrentUser()

      expect(result?.role).toBe('Admin')
    })

    it('should handle roles array', () => {
      const mockToken = 'mock-jwt-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'user@example.com',
        displayName: 'User',
        roles: ['User', 'Moderator']
      })

      const result = getCurrentUser()

      expect(result?.role).toBe('User')
    })

    it('should default to User role when no role found', () => {
      const mockToken = 'mock-jwt-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'user@example.com',
        displayName: 'User'
      })

      const result = getCurrentUser()

      expect(result?.role).toBe('User')
    })

    it('should return null when token decode fails', () => {
      const mockToken = 'invalid-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockImplementationOnce(() => {
        throw new Error('Invalid token')
      })

      // Mock console.error to avoid test output
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation()

      const result = getCurrentUser()

      expect(result).toBeNull()
      expect(consoleSpy).toHaveBeenCalledWith('Error decoding token:', expect.any(Error))
      
      consoleSpy.mockRestore()
    })

    it('should use fallback for missing displayName', () => {
      const mockToken = 'mock-jwt-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'test@example.com',
        name: 'Alternative Name'
      })

      const result = getCurrentUser()

      expect(result?.displayName).toBe('Alternative Name')
    })

    it('should use email as fallback for displayName', () => {
      const mockToken = 'mock-jwt-token'
      mockLocalStorage.getItem.mockReturnValueOnce(mockToken)
      
      mockedJwtDecode.mockReturnValueOnce({
        sub: '123',
        email: 'test@example.com'
      })

      const result = getCurrentUser()

      expect(result?.displayName).toBe('test@example.com')
    })
  })
})