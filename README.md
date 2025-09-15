# Blog API - Clean Architecture

A RESTful API for a blog platform built with .NET 9 and Clean Architecture principles. This project serves as a learning platform where you can share knowledge about programming, languages, and various technical topics.

## 🏗️ Architecture Overview

This project implements **Clean Architecture** (also known as Onion Architecture), which provides a clear separation of concerns and makes the codebase maintainable, testable, and scalable.

### Architecture Layers

```
┌─────────────────────────────────────────┐
│              Presentation               │
│            (BlogAPI.WebAPI)             │
│     Controllers, DTOs, Middleware       │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│             Application                 │
│         (BlogAPI.Application)           │
│   Services, Interfaces, Use Cases       │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│            Infrastructure               │
│        (BlogAPI.Infrastructure)         │
│   Repositories, DbContext, External     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│               Domain                    │
│           (BlogAPI.Domain)              │
│        Entities, Value Objects          │
└─────────────────────────────────────────┘
```

## 🔄 Domain-Driven Design (DDD) Flow

This project implements DDD principles with a clear flow of data and control through the architecture layers:

### Request Flow
```
HTTP Request → Controller → Service → Repository → DbContext → Database
HTTP Response ← Controller ← Service ← Repository ← DbContext ← Database
```

### Layer Flow Details

1. **Controller Layer** (`BlogAPI.WebAPI/Controllers/`)
   - `PostsController.cs` - Handles HTTP requests and responses
   - Maps DTOs to/from domain entities
   - Delegates business logic to services

2. **Service Layer** (`BlogAPI.Application/Services/`)
   - `IPostService.cs`, `PostService.cs` - Business logic and use cases
   - Orchestrates repository calls
   - Enforces business rules and validation

3. **Repository Layer** (`BlogAPI.Infrastructure/Repositories/`)
   - `Repository.cs` - Generic repository implementation
   - `PostRepository.cs`, `CategoryRepository.cs`, `TagRepository.cs` - Specific implementations
   - Abstracts data access from business logic

4. **Data Access Layer** (`BlogAPI.Infrastructure/Data/`)
   - `BlogDbContext.cs` - Entity Framework Core context
   - Handles database operations and entity mapping

### Domain Entities (`BlogAPI.Domain/Entities/`)
- `BaseEntity.cs` - Common properties (Id, timestamps)
- `Post.cs` - Blog post aggregate root
- `Category.cs` - Post categorization
- `Tag.cs` - Post tagging

### Data Transfer Objects (`BlogAPI.Application/DTOs/`)
- `PostDto.cs`, `CategoryDto.cs`, `TagDto.cs` - API contract models
- Decouples internal domain models from external representation

### Example Flow: Creating a Post
```
1. POST /api/posts → PostsController.CreatePost()
2. Controller → PostService.CreatePostAsync(postDto)
3. Service validates business rules
4. Service → PostRepository.AddAsync(post)
5. Repository → DbContext.Posts.Add(post)
6. DbContext → Database INSERT
7. Response flows back through layers
```

### Layer Descriptions

#### 1. **Domain Layer** (`BlogAPI.Domain`)
- **Purpose**: Contains the core business entities and domain logic
- **Dependencies**: No dependencies on other layers
- **Contents**:
  - `BaseEntity`: Base class with common properties (Id, CreatedAt, UpdatedAt)
  - `Post`: Blog post entity with title, content, slug, etc.
  - `Category`: Post categorization
  - `Tag`: Post tagging system

#### 2. **Application Layer** (`BlogAPI.Application`)
- **Purpose**: Contains business logic, use cases, and abstractions
- **Dependencies**: Only depends on Domain layer
- **Contents**:
  - **Interfaces**: Repository contracts (`IPostRepository`, `ICategoryRepository`, etc.)
  - **Services**: Business logic implementation (`IPostService`, `PostService`)
  - **DTOs**: Data Transfer Objects for API communication
  - **Use Cases**: Application-specific business rules

#### 3. **Infrastructure Layer** (`BlogAPI.Infrastructure`)
- **Purpose**: Implements data access and external services
- **Dependencies**: Depends on Application and Domain layers
- **Contents**:
  - **DbContext**: Entity Framework Core database context
  - **Repositories**: Concrete implementations of repository interfaces
  - **Configurations**: Database configurations and mappings

#### 4. **Presentation Layer** (`BlogAPI.WebAPI`)
- **Purpose**: Handles HTTP requests and responses
- **Dependencies**: Depends on Application and Infrastructure layers
- **Contents**:
  - **Controllers**: API endpoints and HTTP handling
  - **Program.cs**: Dependency injection configuration
  - **Middleware**: Cross-cutting concerns

## 🎨 Design Patterns Used

### 1. **Repository Pattern**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    // ... other CRUD operations
}
```
- **Purpose**: Abstracts data access logic
- **Benefits**: Testability, maintainability, separation of concerns
- **Implementation**: Generic base repository with specific repositories for each entity

### 2. **Dependency Injection (DI)**
```csharp
// Program.cs
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
```
- **Purpose**: Manages object dependencies and promotes loose coupling
- **Benefits**: Testability, flexibility, adherence to SOLID principles
- **Implementation**: Built-in .NET DI container

### 3. **Service Layer Pattern**
```csharp
public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    // Business logic implementation
}
```
- **Purpose**: Encapsulates business logic and use cases
- **Benefits**: Separation of business logic from data access and presentation
- **Implementation**: Service interfaces with concrete implementations

### 4. **Data Transfer Object (DTO) Pattern**
```csharp
public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    // ... other properties
}
```
- **Purpose**: Controls data flow between layers
- **Benefits**: Security, versioning, performance optimization
- **Implementation**: Separate DTOs for Create, Update, and Read operations

### 5. **Entity Framework Core (ORM Pattern)**
```csharp
public class BlogDbContext : DbContext
{
    public DbSet<Post> Posts { get; set; }
    // Configuration and relationships
}
```
- **Purpose**: Object-relational mapping for database operations
- **Benefits**: Database independence, LINQ support, change tracking
- **Implementation**: Code-first approach with fluent API configuration

### 6. **Specification Pattern** (Implicit)
- **Implementation**: Through Entity Framework LINQ expressions
- **Purpose**: Encapsulates query logic
- **Example**: `GetPublishedPostsAsync()`, `GetPostsByCategoryAsync()`

## 🛠️ Project Structure

```
BlogAPI/
├── 📁 src/
│   ├── 📁 BlogAPI.Domain/
│   │   └── 📁 Entities/
│   │       ├── BaseEntity.cs
│   │       ├── Post.cs
│   │       ├── Category.cs
│   │       └── Tag.cs
│   ├── 📁 BlogAPI.Application/
│   │   ├── 📁 DTOs/
│   │   │   ├── PostDto.cs
│   │   │   ├── CategoryDto.cs
│   │   │   └── TagDto.cs
│   │   ├── 📁 Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── IPostRepository.cs
│   │   │   ├── ICategoryRepository.cs
│   │   │   └── ITagRepository.cs
│   │   └── 📁 Services/
│   │       ├── IPostService.cs
│   │       ├── ICategoryService.cs
│   │       ├── ITagService.cs
│   │       └── PostService.cs
│   ├── 📁 BlogAPI.Infrastructure/
│   │   ├── 📁 Data/
│   │   │   └── BlogDbContext.cs
│   │   └── 📁 Repositories/
│   │       ├── Repository.cs
│   │       ├── PostRepository.cs
│   │       ├── CategoryRepository.cs
│   │       └── TagRepository.cs
│   └── 📁 BlogAPI.WebAPI/
│       ├── 📁 Controllers/
│       │   └── PostsController.cs
│       ├── Program.cs
│       └── appsettings.json
└── 📁 tests/ (Future)
```

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- PostgreSQL Server
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
```bash
git clone <your-repo-url>
cd BlogAPI
```

2. **Restore packages**
```bash
dotnet restore
```

3. **Update connection string**
Edit `src/BlogAPI.WebAPI/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=5432;Database=BlogLearning;User Id=postgres;Password=sa;Pooling=true;Maximum Pool Size=200;Command Timeout=300;"
  }
}
```

4. **Create and run migrations**
```bash
dotnet ef migrations add InitialCreate --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
dotnet ef database update --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
```

## 🔄 Database Migrations

### Adding New Columns or Tables

When you need to add new properties to entities or create new entities, follow this workflow:

#### 1. **Modify Domain Entity**
Add new properties to your entity in `BlogAPI.Domain/Entities/`:

```csharp
// Example: Adding a new column to Post entity
public class Post : BaseEntity
{
    // Existing properties...
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    // New property - this will become a new column
    public string? MetaDescription { get; set; }
    public DateTime? PublishedAt { get; set; }
}
```

#### 2. **Create Migration**
Generate a new migration file:

```bash
# From the project root directory
dotnet ef migrations add AddMetaDescriptionToPost --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
```

**Migration naming convention:**
- `Add{PropertyName}To{EntityName}` - Adding new columns
- `Create{EntityName}Table` - New entities/tables
- `Update{EntityName}{Description}` - Modifying existing columns
- `Remove{PropertyName}From{EntityName}` - Removing columns

#### 3. **Review Generated Migration**
Check the generated migration file in `src/BlogAPI.Infrastructure/Migrations/`:

```csharp
public partial class AddMetaDescriptionToPost : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "MetaDescription",
            table: "Posts",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MetaDescription",
            table: "Posts");
    }
}
```

#### 4. **Apply Migration to Database**
Update the database with the new changes:

```bash
dotnet ef database update --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
```

### Migration Commands Reference

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI

# Apply all pending migrations
dotnet ef database update --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI

# Apply specific migration
dotnet ef database update MigrationName --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI

# Remove last migration (if not applied to database)
dotnet ef migrations remove --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI

# List all migrations
dotnet ef migrations list --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI

# Generate SQL script for migrations
dotnet ef migrations script --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI

# Drop database (careful!)
dotnet ef database drop --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
```

### Migration Best Practices

1. **Always review migrations** before applying them
2. **Test migrations** on a development database first
3. **Backup production database** before applying migrations
4. **Use descriptive names** for migrations
5. **Don't edit applied migrations** - create new ones instead
6. **Add data seeding** in migrations when needed:

```csharp
// In migration Up() method
migrationBuilder.InsertData(
    table: "Categories",
    columns: new[] { "Id", "Name", "CreatedAt" },
    values: new object[] { Guid.NewGuid(), "Technology", DateTime.UtcNow });
```

### Example: Adding a New Entity

1. **Create new entity in Domain**:
```csharp
// BlogAPI.Domain/Entities/Comment.cs
public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
}
```

2. **Add to DbContext**:
```csharp
// BlogAPI.Infrastructure/Data/BlogDbContext.cs
public DbSet<Comment> Comments { get; set; }
```

3. **Create and apply migration**:
```bash
dotnet ef migrations add CreateCommentsTable --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
dotnet ef database update --project src/BlogAPI.Infrastructure --startup-project src/BlogAPI.WebAPI
```

5. **Run the application**
```bash
dotnet run --project src/BlogAPI.WebAPI
```

## 🐳 Docker Setup

### Quick Start with Docker Compose (Recommended)

1. **Build and run the entire stack**
```bash
docker-compose up --build
```

2. **Run in background (detached mode)**
```bash
docker-compose up -d
```

3. **View logs**
```bash
docker-compose logs blogapi
```

4. **Stop services**
```bash
docker-compose down
```

The application will be available at `http://localhost:8080` with SQL Server automatically configured.

### Manual Docker Commands

1. **Build the Docker image**
```bash
docker build -t blog-api .
```

2. **Run the container**
```bash
docker run -p 8080:8080 blog-api
```

3. **Run with environment variables**
```bash
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;Port=5432;Database=BlogLearning;User Id=postgres;Password=sa;" \
  blog-api
```

### Docker Services

The docker-compose setup includes:

- **BlogAPI**: Main web API service (port 8080)
- **PostgreSQL**: Database service (port 5432)
- **Networks**: Isolated network for service communication
- **Volumes**: Persistent storage for database data

### Environment Variables

Key environment variables for Docker deployment:

```yaml
ASPNETCORE_ENVIRONMENT: Development|Production
ConnectionStrings__DefaultConnection: Database connection string
ASPNETCORE_URLS: http://+:8080
```

### Health Check

The Docker container includes a health check endpoint that monitors the application status:

```bash
# Check container health
docker ps
# Look for "healthy" status in the STATUS column
```

## 📡 API Endpoints

### Posts
- `GET /api/posts` - Get all posts
- `GET /api/posts?publishedOnly=true` - Get published posts only
- `GET /api/posts/{id}` - Get post by ID
- `GET /api/posts/slug/{slug}` - Get post by slug
- `GET /api/posts/category/{categoryId}` - Get posts by category
- `GET /api/posts/tag/{tagId}` - Get posts by tag
- `POST /api/posts` - Create new post
- `PUT /api/posts/{id}` - Update post
- `DELETE /api/posts/{id}` - Delete post

### Example Request Body (Create Post)
```json
{
  "title": "Learning C# SOLID Principles",
  "content": "Content about SOLID principles...",
  "summary": "A comprehensive guide to SOLID principles",
  "slug": "learning-csharp-solid-principles",
  "isPublished": true,
  "featuredImage": "https://example.com/image.jpg",
  "readTimeMinutes": 10,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tagIds": ["tag-id-1", "tag-id-2"]
}
```

## 🏅 SOLID Principles Implementation

### Single Responsibility Principle (SRP)
- Each class has one reason to change
- `PostRepository` only handles data access
- `PostService` only handles business logic
- `PostsController` only handles HTTP concerns

### Open/Closed Principle (OCP)
- Classes are open for extension, closed for modification
- New repository implementations can be added without changing existing code
- Generic repository pattern allows extension

### Liskov Substitution Principle (LSP)
- Derived classes can replace base classes
- All repository implementations are interchangeable through their interfaces

### Interface Segregation Principle (ISP)
- Clients depend only on interfaces they use
- Specific repository interfaces (`IPostRepository`) extend generic interface

### Dependency Inversion Principle (DIP)
- High-level modules don't depend on low-level modules
- Controllers depend on service abstractions, not concrete implementations
- Services depend on repository abstractions

## 🧪 Benefits of This Architecture

1. **Testability**: Easy to unit test with mocked dependencies
2. **Maintainability**: Clear separation of concerns
3. **Scalability**: Easy to add new features without affecting existing code
4. **Flexibility**: Easy to change data sources or add new presentation layers
5. **Reusability**: Business logic can be reused across different applications

## 🔮 Future Enhancements

- [x] Add authentication and authorization
- [ ] Implement CQRS pattern
- [ ] Add caching layer
- [ ] Implement logging and monitoring
- [ ] Add unit and integration tests
- [ ] Implement API versioning
- [ ] Add search functionality
- [ ] Implement file upload for images
- [ ] Add email notifications
- [ ] Implement rate limiting

## 📚 Learning Resources

This project demonstrates several important concepts:
- Clean Architecture
- Domain-Driven Design (DDD)
- Repository Pattern
- Service Layer Pattern
- Dependency Injection
- Entity Framework Core
- RESTful API Design
- SOLID Principles

Perfect for learning and understanding modern .NET development practices!