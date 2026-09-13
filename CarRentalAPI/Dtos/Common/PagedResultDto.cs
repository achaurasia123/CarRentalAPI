namespace CarRentalAPI.Dtos.Common;

// Generic paged-list envelope for admin list endpoints that can grow large
// (bookings, users) - keeps the response shape consistent across controllers.
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
