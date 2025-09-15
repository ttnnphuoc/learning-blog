import { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { AuthorProfile } from '@/components/blog/author-profile';
import { ReadingProgress } from '@/components/blog/reading-progress';
import { formatDate, calculateReadingTime } from '@/lib/utils';
import { Post } from '@/types';

interface PostPageProps {
  params: {
    slug: string;
  };
}

// Mock data - replace with API call
const mockPosts: Post[] = [
  {
    id: 1,
    title: 'Getting Started with Next.js and ASP.NET Core',
    content: `# Getting Started with Next.js and ASP.NET Core

This is a comprehensive guide to building modern web applications using Next.js as the frontend and ASP.NET Core as the backend.

## Why This Stack?

The combination of Next.js and ASP.NET Core provides an excellent foundation for building scalable, performant web applications. Here's why this stack is gaining popularity:

### Performance Benefits

- **Server-Side Rendering (SSR)**: Next.js provides excellent SEO and initial page load performance
- **Static Site Generation (SSG)**: Pre-built pages for lightning-fast delivery
- **API Routes**: Built-in API capabilities for seamless integration
- **Image Optimization**: Automatic image optimization and lazy loading

### Developer Experience

- **Hot Reloading**: Instant feedback during development
- **TypeScript Support**: Full type safety across the stack
- **Modern Tooling**: Built-in ESLint, testing support, and more
- **File-based Routing**: Intuitive routing system

### Backend Reliability

- **Clean Architecture**: Separation of concerns for maintainable code
- **Dependency Injection**: Built-in IoC container
- **Entity Framework Core**: Powerful ORM with LINQ support
- **Authentication & Authorization**: Robust security features

## Setting Up Your Development Environment

Let's start by setting up both the frontend and backend projects.

### Frontend Setup

First, create a new Next.js project:

\`\`\`bash
npx create-next-app@latest my-blog --typescript --tailwind
cd my-blog
npm install
\`\`\`

### Backend Setup

Create a new ASP.NET Core Web API project:

\`\`\`bash
dotnet new webapi -n BlogAPI
cd BlogAPI
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
\`\`\`

## Project Structure

Here's how we'll organize our project:

\`\`\`
my-blog/
├── frontend/          # Next.js application
│   ├── src/
│   │   ├── app/       # App Router pages
│   │   ├── components/ # Reusable components
│   │   └── lib/       # Utility functions
│   └── package.json
└── backend/           # ASP.NET Core API
    ├── Controllers/   # API endpoints
    ├── Models/        # Data models
    ├── Services/      # Business logic
    └── Program.cs
\`\`\`

## Creating the API

Let's start by creating a simple blog post API:

\`\`\`csharp
// Models/BlogPost.cs
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; }
}
\`\`\`

\`\`\`csharp
// Controllers/BlogPostsController.cs
[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetPosts()
    {
        // Implementation here
        return Ok(posts);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetPost(int id)
    {
        // Implementation here
        return Ok(post);
    }
}
\`\`\`

## Frontend Integration

Now let's create the frontend components to consume our API:

\`\`\`typescript
// lib/api.ts
export class BlogApiClient {
  private baseUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7041';
  
  async getPosts(): Promise<BlogPost[]> {
    const response = await fetch(\`\${this.baseUrl}/api/blogposts\`);
    return response.json();
  }
  
  async getPost(id: number): Promise<BlogPost> {
    const response = await fetch(\`\${this.baseUrl}/api/blogposts/\${id}\`);
    return response.json();
  }
}
\`\`\`

## CORS Configuration

Don't forget to configure CORS in your ASP.NET Core application:

\`\`\`csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("AllowNextJS");
\`\`\`

## Deployment Considerations

When deploying to production, consider:

1. **Environment Variables**: Use proper configuration management
2. **HTTPS**: Ensure all communication is encrypted
3. **Database**: Set up proper database connections
4. **Monitoring**: Implement logging and error tracking
5. **Performance**: Use caching strategies

## Conclusion

This stack provides a solid foundation for modern web development. The combination of Next.js's frontend capabilities with ASP.NET Core's robust backend features creates a powerful development experience.

In the next post, we'll dive deeper into implementing authentication and authorization across both the frontend and backend.

---

*Have questions about this setup? Feel free to reach out in the comments below or connect with me on social media.*`,
    summary: 'Learn how to build modern web applications using Next.js frontend with ASP.NET Core backend.',
    slug: 'getting-started-nextjs-aspnet-core',
    imageUrl: 'https://images.unsplash.com/photo-1517180102446-f3ece451e9d8?w=1200&h=600&fit=crop',
    isPublished: true,
    publishedAt: '2024-01-15T10:00:00Z',
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-15T10:00:00Z',
    authorId: 1,
    author: {
      id: 1,
      username: 'johndoe',
      email: 'john@example.com',
      firstName: 'John',
      lastName: 'Doe',
      bio: 'Full-stack developer passionate about modern web technologies. I love sharing knowledge and helping others learn to code.',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      roles: [{ id: 1, name: 'Author', description: 'Content Author' }]
    },
    categories: [
      { id: 1, name: 'Web Development', slug: 'web-development', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' }
    ],
    tags: [
      { id: 1, name: 'Next.js', slug: 'nextjs', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' },
      { id: 2, name: 'ASP.NET Core', slug: 'aspnet-core', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' },
      { id: 3, name: 'Tutorial', slug: 'tutorial', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' }
    ]
  }
];

export async function generateMetadata({ params }: PostPageProps): Promise<Metadata> {
  const post = mockPosts.find(p => p.slug === params.slug);
  
  if (!post) {
    return { title: 'Post Not Found' };
  }

  return {
    title: `${post.title} - Blog`,
    description: post.summary,
    openGraph: {
      title: post.title,
      description: post.summary,
      type: 'article',
      publishedTime: post.publishedAt,
      authors: [`${post.author.firstName} ${post.author.lastName}`],
      tags: post.tags.map(tag => tag.name),
      images: post.imageUrl ? [post.imageUrl] : undefined,
    },
    twitter: {
      card: 'summary_large_image',
      title: post.title,
      description: post.summary,
      images: post.imageUrl ? [post.imageUrl] : undefined,
    },
  };
}

export default function PostPage({ params }: PostPageProps) {
  const post = mockPosts.find(p => p.slug === params.slug);

  if (!post) {
    notFound();
  }

  const readingTime = calculateReadingTime(post.content);

  return (
    <>
      <ReadingProgress />
      
      <article className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Breadcrumb */}
        <nav className="mb-8">
          <ol className="flex items-center space-x-2 text-sm text-gray-500">
            <li>
              <Link href="/" className="hover:text-gray-700 transition-colors">Home</Link>
            </li>
            <li>/</li>
            <li>
              <Link href="/posts" className="hover:text-gray-700 transition-colors">Posts</Link>
            </li>
            <li>/</li>
            <li className="text-gray-900 truncate">{post.title}</li>
          </ol>
        </nav>

        {/* Post Header */}
        <header className="mb-12">
          {/* Categories */}
          <div className="flex flex-wrap gap-2 mb-4">
            {post.categories.map((category) => (
              <Link key={category.id} href={`/categories/${category.slug}`}>
                <Badge variant="secondary" className="hover:bg-gray-200 transition-colors">
                  {category.name}
                </Badge>
              </Link>
            ))}
          </div>

          {/* Title */}
          <h1 className="text-3xl font-bold text-gray-900 sm:text-4xl lg:text-5xl mb-6 leading-tight">
            {post.title}
          </h1>
          
          {/* Summary */}
          {post.summary && (
            <p className="text-xl text-gray-600 mb-8 leading-relaxed">
              {post.summary}
            </p>
          )}

          {/* Meta Info */}
          <div className="flex items-center justify-between mb-8 pb-8 border-b border-gray-200">
            <div className="flex items-center space-x-4">
              <AuthorProfile author={post.author} size="md" showBio={false} />
              <div className="hidden sm:block text-gray-300">•</div>
              <div className="text-sm text-gray-500 space-y-1">
                <div>{formatDate(post.publishedAt || post.createdAt)}</div>
                <div>{readingTime} min read</div>
              </div>
            </div>
            
            <div className="flex items-center space-x-2">
              <Button variant="outline" size="sm">
                <svg className="h-4 w-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.367 2.684 3 3 0 00-5.367-2.684z" />
                </svg>
                Share
              </Button>
              <Button variant="outline" size="sm">
                <svg className="h-4 w-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
                Save
              </Button>
            </div>
          </div>
        </header>

        {/* Featured Image */}
        {post.imageUrl && (
          <div className="mb-12">
            <img
              src={post.imageUrl}
              alt={post.title}
              className="w-full h-64 md:h-96 object-cover rounded-lg shadow-lg"
            />
          </div>
        )}

        {/* Post Content */}
        <div className="prose prose-lg prose-gray max-w-none">
          <div className="whitespace-pre-wrap leading-relaxed">
            {post.content}
          </div>
        </div>

        {/* Tags */}
        {post.tags.length > 0 && (
          <div className="mt-12 pt-8 border-t border-gray-200">
            <h3 className="text-sm font-medium text-gray-900 mb-4">Tags</h3>
            <div className="flex flex-wrap gap-2">
              {post.tags.map((tag) => (
                <Link key={tag.id} href={`/tags/${tag.slug}`}>
                  <Badge variant="outline" className="hover:bg-gray-50 transition-colors">
                    #{tag.name}
                  </Badge>
                </Link>
              ))}
            </div>
          </div>
        )}

        {/* Author Info */}
        <div className="mt-12 pt-8 border-t border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">About the Author</h3>
          <AuthorProfile author={post.author} size="lg" showBio={true} />
        </div>

        {/* Post Footer */}
        <footer className="mt-12 pt-8 border-t border-gray-200">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div className="text-sm text-gray-500">
              <p>Published on {formatDate(post.publishedAt || post.createdAt)}</p>
              {post.updatedAt !== post.createdAt && (
                <p>Last updated on {formatDate(post.updatedAt)}</p>
              )}
            </div>
            
            <div className="flex space-x-2">
              <Button variant="outline" asChild>
                <Link href="/posts">← Back to Posts</Link>
              </Button>
              <Button variant="default" asChild>
                <Link href="/">Home</Link>
              </Button>
            </div>
          </div>
        </footer>
      </article>
    </>
  );
}