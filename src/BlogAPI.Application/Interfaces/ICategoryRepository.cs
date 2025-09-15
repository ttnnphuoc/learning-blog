using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
}