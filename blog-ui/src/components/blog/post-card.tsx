import Link from 'next/link';
import { formatDate, calculateReadingTime, getPlainTextSummary } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import { Post } from '@/types';

interface PostCardProps {
  post: Post;
  featured?: boolean;
}

export function PostCard({ post, featured = false }: PostCardProps) {
  const readingTime = calculateReadingTime(post.content);

  if (featured) {
    return (
      <article className="group relative isolate flex flex-col justify-end overflow-hidden rounded-2xl bg-gray-900 px-8 pb-8 pt-80 sm:pt-48 lg:pt-80">
        <img
          src={post.imageUrl || `https://images.unsplash.com/photo-1499750310107-5fef28a66643?w=800&h=600&fit=crop`}
          alt={post.title}
          className="absolute inset-0 -z-10 h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
        <div className="absolute inset-0 -z-10 bg-gradient-to-t from-gray-900 via-gray-900/40" />
        <div className="absolute inset-0 -z-10 rounded-2xl ring-1 ring-inset ring-gray-900/10" />

        <div className="flex flex-wrap items-center gap-y-1 overflow-hidden text-sm leading-6 text-gray-300">
          <time dateTime={post.publishedAt} className="mr-8">
            {formatDate(post.publishedAt || post.createdAt)}
          </time>
          <div className="-ml-4 flex items-center gap-x-4">
            <svg viewBox="0 0 2 2" className="-ml-0.5 h-0.5 w-0.5 flex-none fill-white/50">
              <circle cx={1} cy={1} r={1} />
            </svg>
            <span>{readingTime} min read</span>
          </div>
        </div>
        
        <h3 className="mt-3 text-lg font-semibold leading-6 text-white">
          <Link href={`/posts/${post.slug}`}>
            <span className="absolute inset-0" />
            {post.title}
          </Link>
        </h3>
        
        <p className="mt-2 text-sm leading-6 text-gray-300 line-clamp-2">
          {post.summary}
        </p>

        <div className="mt-4 flex items-center gap-x-4">
          <Avatar className="h-6 w-6">
            <AvatarImage
              src={post.author.avatarUrl}
              alt={`${post.author.firstName} ${post.author.lastName}`}
            />
            <AvatarFallback>
              {post.author.firstName?.[0]}{post.author.lastName?.[0]}
            </AvatarFallback>
          </Avatar>
          <div className="text-sm leading-6">
            <p className="font-semibold text-white">
              {post.author.firstName} {post.author.lastName}
            </p>
          </div>
        </div>
      </article>
    );
  }

  return (
    <article className="group relative bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow duration-200">
      {post.imageUrl && (
        <div className="aspect-[16/9] overflow-hidden">
          <img
            src={post.imageUrl}
            alt={post.title}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        </div>
      )}
      
      <div className="p-6">
        <div className="flex items-center gap-x-4 text-xs text-gray-500">
          <time dateTime={post.publishedAt}>
            {formatDate(post.publishedAt || post.createdAt)}
          </time>
          <span>{readingTime} min read</span>
        </div>
        
        <div className="group relative">
          <h3 className="mt-3 text-lg font-semibold leading-6 text-gray-900 group-hover:text-blue-600">
            <Link href={`/posts/${post.slug}`}>
              <span className="absolute inset-0" />
              {post.title}
            </Link>
          </h3>
          <p className="mt-2 line-clamp-3 text-sm leading-6 text-gray-600">
            {post.summary || getPlainTextSummary(post.content, 150)}
          </p>
        </div>
        
        <div className="mt-4 flex flex-wrap gap-2">
          {post.categories.slice(0, 2).map((category) => (
            <Badge key={category.id} variant="secondary" className="text-xs">
              {category.name}
            </Badge>
          ))}
        </div>
        
        <div className="mt-4 flex items-center gap-x-4">
          <Avatar className="h-8 w-8">
            <AvatarImage
              src={post.author.avatarUrl}
              alt={`${post.author.firstName} ${post.author.lastName}`}
            />
            <AvatarFallback>
              {post.author.firstName?.[0]}{post.author.lastName?.[0]}
            </AvatarFallback>
          </Avatar>
          <div className="text-sm leading-6">
            <p className="font-semibold text-gray-900">
              {post.author.firstName} {post.author.lastName}
            </p>
            <p className="text-gray-600">@{post.author.username}</p>
          </div>
        </div>
      </div>
    </article>
  );
}