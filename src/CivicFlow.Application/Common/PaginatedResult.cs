namespace CivicFlow.Application.Common;

public class PaginatedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PaginatedResult<T> Create(List<T> items, int totalCount, int page, int pageSize) =>
        new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };

    public static PaginatedResult<T> Empty(int page, int pageSize) =>
        new() { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
}
