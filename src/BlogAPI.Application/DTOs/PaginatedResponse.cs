namespace BlogAPI.Application.DTOs;

public class PaginatedResponse<T>
{
    public bool Success { get; set; } = true;
    public List<T>? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public PaginationMeta Pagination { get; set; } = new();
}

public class PaginationMeta
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}