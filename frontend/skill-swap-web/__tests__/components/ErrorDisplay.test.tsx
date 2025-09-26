import { render, screen, fireEvent } from '@testing-library/react'
import ErrorDisplay from '../../components/ErrorDisplay'

describe('ErrorDisplay', () => {
  const defaultProps = {
    error: 'Something went wrong',
  }

  it('should render error message', () => {
    render(<ErrorDisplay {...defaultProps} />)

    expect(screen.getByRole('heading', { name: 'Something went wrong' })).toBeInTheDocument()
    expect(screen.getByText('Something went wrong', { selector: 'p' })).toBeInTheDocument()
  })

  it('should render default title when not provided', () => {
    render(<ErrorDisplay {...defaultProps} />)

    expect(screen.getByRole('heading', { name: 'Something went wrong' })).toBeInTheDocument()
  })

  it('should render custom title when provided', () => {
    render(<ErrorDisplay {...defaultProps} title="Custom Error Title" />)

    expect(screen.getByRole('heading', { name: 'Custom Error Title' })).toBeInTheDocument()
  })

  it('should render retry button when onRetry is provided', () => {
    const mockRetry = jest.fn()
    render(<ErrorDisplay {...defaultProps} onRetry={mockRetry} />)

    const retryButton = screen.getByText('Try again')
    expect(retryButton).toBeInTheDocument()
  })

  it('should call onRetry when retry button is clicked', () => {
    const mockRetry = jest.fn()
    render(<ErrorDisplay {...defaultProps} onRetry={mockRetry} />)

    const retryButton = screen.getByText('Try again')
    fireEvent.click(retryButton)

    expect(mockRetry).toHaveBeenCalledTimes(1)
  })

  it('should not render retry button when onRetry is not provided', () => {
    render(<ErrorDisplay {...defaultProps} />)

    expect(screen.queryByText('Try again')).not.toBeInTheDocument()
  })

  it('should render custom retry text when provided', () => {
    const mockRetry = jest.fn()
    render(
      <ErrorDisplay 
        {...defaultProps} 
        onRetry={mockRetry} 
        retryText="Retry Operation" 
      />
    )

    expect(screen.getByText('Retry Operation')).toBeInTheDocument()
  })

  it('should have correct CSS classes for styling', () => {
    render(<ErrorDisplay {...defaultProps} />)

    const container = screen.getByRole('heading', { name: 'Something went wrong' }).closest('div')?.parentElement
    expect(container).toHaveClass('bg-red-50', 'border-red-200', 'rounded-lg')
  })

  it('should display warning emoji', () => {
    render(<ErrorDisplay {...defaultProps} />)

    expect(screen.getByText('⚠️')).toBeInTheDocument()
  })

  it('should handle long error messages', () => {
    const longError = 'This is a very long error message that should be displayed properly in the error component without breaking the layout or causing any issues with the user interface'
    
    render(<ErrorDisplay error={longError} />)

    expect(screen.getByText(longError)).toBeInTheDocument()
  })

  it('should handle empty error message', () => {
    render(<ErrorDisplay error="" />)

    expect(screen.getByRole('heading', { name: 'Something went wrong' })).toBeInTheDocument()
    // The error message paragraph should exist but may be empty
    const errorParagraph = screen.getByRole('heading').parentElement?.querySelector('p')
    expect(errorParagraph).toBeInTheDocument()
  })

  it('should handle special characters in error message', () => {
    const errorWithSpecialChars = 'Error: {"status": 500, "message": "Server Error"}'
    
    render(<ErrorDisplay error={errorWithSpecialChars} />)

    expect(screen.getByText(errorWithSpecialChars)).toBeInTheDocument()
  })
})