using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Dtos;
using SkillSwap.Domain;

namespace SkillSwap.Application.Offers.Queries;

public class GetOffersQueryHandler : IRequestHandler<GetOffersQuery, PagedOffersResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetOffersQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedOffersResult> Handle(GetOffersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Offers.AsNoTracking().AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(o => 
                o.Title.ToLower().Contains(searchLower) || 
                o.Description.ToLower().Contains(searchLower)
            );
        }

        // Apply budget filter
        if (request.MaxBudget.HasValue)
        {
            query = query.Where(o => o.Price <= request.MaxBudget.Value);
        }

        // Apply ownership filter
        if (request.ShowOnlyMyOffers.HasValue && !string.IsNullOrEmpty(request.UserId))
        {
            if (request.ShowOnlyMyOffers.Value)
            {
                // Show only user's offers
                query = query.Where(o => o.CreatedBy.ToString() == request.UserId);
            }
            else
            {
                // Show only offers NOT owned by the user
                query = query.Where(o => o.CreatedBy.ToString() != request.UserId);
            }
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDescending 
                ? query.OrderByDescending(o => o.Title) 
                : query.OrderBy(o => o.Title),
            "price" => request.SortDescending 
                ? query.OrderByDescending(o => o.Price) 
                : query.OrderBy(o => o.Price),
            "createdat" or "created" => request.SortDescending 
                ? query.OrderByDescending(o => o.Id) 
                : query.OrderBy(o => o.Id),
            _ => request.SortDescending 
                ? query.OrderByDescending(o => o.Id) 
                : query.OrderBy(o => o.Id)
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var offers = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedOffersResult(
            _mapper.Map<List<OfferDto>>(offers),
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}