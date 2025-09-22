import { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { AuthorProfile } from '@/components/blog/author-profile';
import { ReadingProgress } from '@/components/blog/reading-progress';
import { formatDate, calculateReadingTime, stripHtml } from '@/lib/utils';
import { Post } from '@/types';
import { apiClient } from '@/lib/api';

interface PostPageProps {
  params: Promise<{
    slug: string;
  }>;
}

async function getPostBySlug(slug: string): Promise<Post | null> {
  try {
    const post = await apiClient.getPostBySlug(slug);
    return post;
  } catch (error) {
    console.error('Failed to fetch post:', error);
    return null;
  }
}

export async function generateMetadata({ params }: PostPageProps): Promise<Metadata> {
  const { slug } = await params;
  const post = await getPostBySlug(slug);
  
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
      authors: [`${post.author?.firstName} ${post.author?.lastName}`],
      tags: post.tags?.map(tag => tag.name),
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

export default async function PostPage({ params }: PostPageProps) {
  const { slug } = await params;
  const post = await getPostBySlug(slug);

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
          {post.categories && post.categories.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-4">
              {post.categories.map((category) => (
                <Link key={category.id} href={`/categories/${category.slug}`}>
                  <Badge variant="secondary" className="hover:bg-gray-200 transition-colors">
                    {category.name}
                  </Badge>
                </Link>
              ))}
            </div>
          )}

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
              {post.author && <AuthorProfile author={post.author} size="md" showBio={false} />}
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
        <div className="prose prose-lg prose-gray max-w-none prose-headings:font-bold prose-h1:text-3xl prose-h2:text-2xl prose-h3:text-xl prose-h4:text-lg prose-p:text-base prose-p:leading-7 prose-strong:font-semibold prose-em:italic prose-code:bg-gray-100 prose-code:px-1 prose-code:rounded prose-blockquote:border-l-4 prose-blockquote:border-gray-300 prose-blockquote:pl-4 prose-blockquote:italic prose-ul:list-disc prose-ol:list-decimal prose-li:my-1 prose-a:text-blue-600 prose-a:underline hover:prose-a:text-blue-800">
          <div 
            className="tiptap-content leading-relaxed"
            dangerouslySetInnerHTML={{ __html: post.content }}
            style={{
              wordBreak: 'break-word',
            }}
          />
        </div>

        {/* Tags */}
        {post.tags && post.tags.length > 0 && (
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
        {post.author && (
          <div className="mt-12 pt-8 border-t border-gray-200">
            <h3 className="text-lg font-semibold text-gray-900 mb-6">About the Author</h3>
            <AuthorProfile author={post.author} size="lg" showBio={true} />
          </div>
        )}

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