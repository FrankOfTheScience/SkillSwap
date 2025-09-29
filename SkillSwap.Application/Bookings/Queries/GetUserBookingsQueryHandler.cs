using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain;

namespace SkillSwap.Application.Bookings.Queries;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, PaginatedResult<Booking>>
{
    private readonly IApplicationDbContext _context;

    public GetUserBookingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Booking>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .Include(b => b.Offer)
            .Include(b => b.User)
            .Where(b => b.UserId == request.UserId)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var bookings = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Booking>
        {
            Data = bookings,
            Total = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }
}