'use client';

import { Metadata } from 'next';
import Link from 'next/link';
import { useEffect, useState } from 'react';
import { Button } from '@/components/ui/button';
import { PostCard } from '@/components/blog/post-card';
import { usePosts } from '@/lib/hooks/use-posts';
import { useAuth } from '@/lib/auth-context';
import { PostDto } from '@/types';
 import { RoleHelper } from '@/types';
export default function HomePage() {
  const { posts, loading, error, fetchPosts } = usePosts();
  const { isAuthenticated, user } = useAuth();
  const [featuredPosts, setFeaturedPosts] = useState<PostDto[]>([]);

  useEffect(() => {
    // Fetch published posts only for the home page
    fetchPosts({ publishedOnly: true, pageSize: 4 });
  }, [fetchPosts]);

  useEffect(() => {
    // Set featured posts from fetched posts
    if (posts.length > 0) {
      setFeaturedPosts(posts.slice(0, 2));
    }
  }, [posts]);

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-blue-50 via-white to-purple-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <div className="text-center">
            <h1 className="text-4xl tracking-tight font-bold text-gray-900 sm:text-6xl md:text-7xl">
              <span className="block">Welcome to</span>
              <span className="block text-blue-600">Our Blog</span>
            </h1>
            <p className="mt-6 max-w-2xl mx-auto text-lg text-gray-600 sm:text-xl">
              Discover insightful articles, tutorials, and thoughts on technology, development, and more. 
              Join our community of developers and learners.
            </p>
            <div className="mt-8 flex flex-col sm:flex-row gap-4 justify-center">
              <Button asChild size="lg">
                <Link href="/posts">Read Latest Posts</Link>
              </Button>
              <Button variant="outline" size="lg" asChild>
                <Link href="/categories">Browse Categories</Link>
              </Button>
              {isAuthenticated && RoleHelper.canCreateContent(user?.roles) && (
                <Button variant="secondary" size="lg" asChild>
                  <Link href="/admin">Admin Dashboard</Link>
                </Button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Featured Posts */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-gray-900 sm:text-4xl">Featured Posts</h2>
          <p className="mt-4 text-lg text-gray-600">Our most popular and recent articles</p>
        </div>
        
        {loading ? (
          <div className="flex justify-center items-center py-8">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        ) : error ? (
          <div className="text-center py-8">
            <p className="text-red-600 mb-4">Failed to load posts: {error}</p>
            <Button variant="outline" onClick={() => fetchPosts({ publishedOnly: true, pageSize: 4 })}>
              Try Again
            </Button>
          </div>
        ) : featuredPosts.length > 0 ? (
          <>
            <div className="grid gap-8 lg:grid-cols-2">
              {featuredPosts.map((post, index) => (
                <PostCard 
                  key={post.id} 
                  post={post} 
                  featured={index === 0}
                />
              ))}
            </div>
            
            <div className="text-center mt-12">
              <Button variant="outline" size="lg" asChild>
                <Link href="/posts">View All Posts</Link>
              </Button>
            </div>
          </>
        ) : (
          <div className="text-center py-8">
            <p className="text-gray-600 mb-4">No posts available yet.</p>
            <Button variant="outline" asChild>
              <Link href="/posts">Check back later</Link>
            </Button>
          </div>
        )}
      </div>

      {/* Features Section */}
      <div className="bg-gray-50 py-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900">Why Choose Our Blog?</h2>
            <p className="mt-4 text-lg text-gray-600">Built with modern technologies for the best experience</p>
          </div>
          
          <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
            <div className="text-center group">
              <div className="flex items-center justify-center h-16 w-16 rounded-full bg-blue-100 text-blue-600 mx-auto group-hover:bg-blue-600 group-hover:text-white transition-colors">
                <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h3 className="mt-6 text-xl font-semibold text-gray-900">Fast Performance</h3>
              <p className="mt-4 text-gray-600">
                Built with Next.js for optimal performance, SEO, and lightning-fast page loads.
              </p>
            </div>

            <div className="text-center group">
              <div className="flex items-center justify-center h-16 w-16 rounded-full bg-green-100 text-green-600 mx-auto group-hover:bg-green-600 group-hover:text-white transition-colors">
                <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="mt-6 text-xl font-semibold text-gray-900">Reliable Backend</h3>
              <p className="mt-4 text-gray-600">
                Powered by ASP.NET Core with clean architecture and robust security features.
              </p>
            </div>

            <div className="text-center group">
              <div className="flex items-center justify-center h-16 w-16 rounded-full bg-purple-100 text-purple-600 mx-auto group-hover:bg-purple-600 group-hover:text-white transition-colors">
                <svg className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
              </div>
              <h3 className="mt-6 text-xl font-semibold text-gray-900">Modern Design</h3>
              <p className="mt-4 text-gray-600">
                Clean, responsive design with professional components and accessibility in mind.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="bg-blue-600 py-16">
        <div className="max-w-4xl mx-auto text-center px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-white sm:text-4xl">
            Ready to Start Reading?
          </h2>
          <p className="mt-4 text-xl text-blue-100">
            Join thousands of developers who stay updated with our latest articles and tutorials.
          </p>
          <div className="mt-8 flex flex-col sm:flex-row gap-4 justify-center">
            <Button size="lg" variant="secondary" asChild>
              <Link href="/posts">Browse All Posts</Link>
            </Button>
            <Button size="lg" variant="outline" className="border-white text-white hover:bg-white hover:text-blue-600" asChild>
              <Link href="/categories">Explore Topics</Link>
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}