import { getErrorMessage, showErrorToast } from '../../utils/errorHandler'
import { AxiosError } from 'axios'

describe('errorHandler', () => {
  describe('getErrorMessage', () => {
    it('should return backend error message when available', () => {
      const axiosError: AxiosError = {
        name: 'AxiosError',
        message: 'Request failed',
        isAxiosError: true,
        response: {
          data: {
            message: 'Invalid request. Please check your input and try again.',
            type: 'error'
          },
          status: 400,
          statusText: 'Bad Request',
          headers: {},
          config: {} as any
        },
        config: {} as any,
        toJSON: () => ({})
      }

      const result = getErrorMessage(axiosError)

      expect(result).toBe('Invalid request. Please check your input and try again.')
    })

    it('should return appropriate message for 400 status code', () => {
      const axiosError: AxiosError = {
        name: 'AxiosError',
        message: 'Request failed',
        isAxiosError: true,
        response: {
          data: {},
          status: 400,
          statusText: 'Bad Request',
          headers: {},
          config: {} as any
        },
        config: {} as any,
        toJSON: () => ({})
      }

      const result = getErrorMessage(axiosError)

      expect(result).toBe('Invalid request. Please check your input and try again.')
    })

    it('should return appropriate message for 401 status code', () => {
      const axiosError: AxiosError = {
        name: 'AxiosError',
        message: 'Request failed',
        isAxiosError: true,
        response: {
          data: {},
          status: 401,
          statusText: 'Unauthorized',
          headers: {},
          config: {} as any
        },
        config: {} as any,
        toJSON: () => ({})
      }

      const result = getErrorMessage(axiosError)

      expect(result).toBe('You are not authorized to perform this action.')
    })

    it('should return appropriate message for 404 status code', () => {
      const axiosError: AxiosError = {
        name: 'AxiosError',
        message: 'Request failed',
        isAxiosError: true,
        response: {
          data: {},
          status: 404,
          statusText: 'Not Found',
          headers: {},
          config: {} as any
        },
        config: {} as any,
        toJSON: () => ({})
      }

      const result = getErrorMessage(axiosError)

      expect(result).toBe('The requested resource was not found.')
    })

    it('should return appropriate message for 500 status code', () => {
      const axiosError: AxiosError = {
        name: 'AxiosError',
        message: 'Request failed',
        isAxiosError: true,
        response: {
          data: {},
          status: 500,
          statusText: 'Internal Server Error',
          headers: {},
          config: {} as any
        },
        config: {} as any,
        toJSON: () => ({})
      }

      const result = getErrorMessage(axiosError)

      expect(result).toBe('Something went wrong on our end. Please try again later.')
    })

    it('should return network error message for network issues', () => {
      const networkError = {
        message: 'Network Error'
      }

      const result = getErrorMessage(networkError)

      expect(result).toBe('Unable to connect to the server. Please check your internet connection and try again.')
    })

    it('should return default message for unknown errors', () => {
      const unknownError = {}

      const result = getErrorMessage(unknownError)

      expect(result).toBe('Something went wrong. Please try again.')
    })

    it('should handle undefined/null errors', () => {
      expect(getErrorMessage(null)).toBe('Something went wrong. Please try again.')
      expect(getErrorMessage(undefined)).toBe('Something went wrong. Please try again.')
    })
  })

  describe('showErrorToast', () => {
    let consoleSpy: jest.SpyInstance

    beforeEach(() => {
      consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {})
    })

    afterEach(() => {
      consoleSpy.mockRestore()
    })

    it('should return error message and log error', () => {
      const error = new Error('Test error')

      const result = showErrorToast(error)

      expect(result).toBe('Something went wrong. Please try again.')
      expect(consoleSpy).toHaveBeenCalledWith('Error occurred:', error)
    })

    it('should use default message when provided', () => {
      const error = new Error('Test error')
      const defaultMessage = 'Custom default message'

      const result = showErrorToast(error, defaultMessage)

      expect(result).toBe(defaultMessage)
      expect(consoleSpy).toHaveBeenCalledWith('Error occurred:', error)
    })

    it('should handle axios errors with backend messages', () => {
      const axiosError: AxiosError = {
        name: 'AxiosError',
        message: 'Request failed',
        isAxiosError: true,
        response: {
          data: {
            message: 'Backend error message',
            type: 'error'
          },
          status: 400,
          statusText: 'Bad Request',
          headers: {},
          config: {} as any
        },
        config: {} as any,
        toJSON: () => ({})
      }

      const result = showErrorToast(axiosError)

      expect(result).toBe('Backend error message')
      expect(consoleSpy).toHaveBeenCalledWith('Error occurred:', axiosError)
    })
  })
})