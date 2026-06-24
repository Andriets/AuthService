namespace AuthService.Web.Core.Common;

public record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    DateTimeOffset Timestamp)
{
    public bool HasNextPage => Page * PageSize < TotalCount;
}
