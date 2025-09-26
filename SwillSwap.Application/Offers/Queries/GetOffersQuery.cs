using MediatR;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Queries;

public record GetOffersQuery(
    int Page = 1, 
    int PageSize = 10,
    string? Search = null,
    decimal? MaxBudget = null,
    bool? ShowOnlyMyOffers = null,
    string? SortBy = "id",
    bool SortDescending = false,
    string? UserId = null
) : IRequest<PagedOffersResult>;

public record PagedOffersResult(
    List<OfferDto> Offers,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);