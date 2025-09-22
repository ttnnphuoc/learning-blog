namespace BlogAPI.Application.DTOs;

public class PostQueryParams : PaginationRequest
{
    public bool? PublishedOnly { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public Guid? AuthorId { get; set; }
}