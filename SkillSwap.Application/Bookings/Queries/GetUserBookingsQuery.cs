using MediatR;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain;

namespace SkillSwap.Application.Bookings.Queries;

public class GetUserBookingsQuery : IRequest<PaginatedResult<Booking>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}