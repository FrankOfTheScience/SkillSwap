namespace SkillSwap.Application.Common.Models;

public class PaginatedResult<T>
{
    public IList<T> Data { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}