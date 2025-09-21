'use client';

import { useState, useEffect } from 'react';
import { Metadata } from 'next';
import { PostCard } from '@/components/blog/post-card';
import { SearchBar } from '@/components/blog/search-bar';
import { CategoryFilter } from '@/components/blog/category-filter';
import { Pagination } from '@/components/blog/pagination';
import { Button } from '@/components/ui/button';
import { Post, Category } from '@/types';
import { apiClient } from '@/lib/api';

// Mock data - replace with API calls
const mockPosts: Post[] = [
  {
    id: 1,
    title: 'Getting Started with Next.js and ASP.NET Core',
    content: 'This is a comprehensive guide to building modern web applications...',
    summary: 'Learn how to build modern web applications using Next.js frontend with ASP.NET Core backend.',
    slug: 'getting-started-nextjs-aspnet-core',
    imageUrl: 'https://images.unsplash.com/photo-1517180102446-f3ece451e9d8?w=800&h=600&fit=crop',
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
      bio: 'Full-stack developer passionate about modern web technologies.',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      roles: []
    },
    categories: [
      { id: 1, name: 'Web Development', slug: 'web-development', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' }
    ],
    tags: [
      { id: 1, name: 'Next.js', slug: 'nextjs', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' },
      { id: 2, name: 'ASP.NET Core', slug: 'aspnet-core', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' }
    ]
  },
  {
    id: 2,
    title: 'Understanding Clean Architecture',
    content: 'Clean Architecture is a software design philosophy that separates the elements of a design into ring levels...',
    summary: 'Explore the principles of clean architecture and how to implement it in your projects.',
    slug: 'understanding-clean-architecture',
    imageUrl: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&h=600&fit=crop',
    isPublished: true,
    publishedAt: '2024-01-10T10:00:00Z',
    createdAt: '2024-01-10T10:00:00Z',
    updatedAt: '2024-01-10T10:00:00Z',
    authorId: 2,
    author: {
      id: 2,
      username: 'janesmith',
      email: 'jane@example.com',
      firstName: 'Jane',
      lastName: 'Smith',
      bio: 'Software architect with 10+ years of experience in enterprise applications.',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      roles: []
    },
    categories: [
      { id: 2, name: 'Architecture', slug: 'architecture', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' }
    ],
    tags: [
      { id: 3, name: 'Clean Architecture', slug: 'clean-architecture', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' },
      { id: 4, name: 'Design Patterns', slug: 'design-patterns', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-01T00:00:00Z' }
    ]
  },
  // Add more mock posts...
];


export default function PostsPage() {
  const [posts] = useState<Post[]>(mockPosts);
  const [filteredPosts, setFilteredPosts] = useState<Post[]>(mockPosts);
  const [categories, setCategories] = useState<Category[]>([]);
  const [categoriesLoading, setCategoriesLoading] = useState(true);
  const [categoriesError, setCategoriesError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

  const postsPerPage = 6;
  const totalPages = Math.ceil(filteredPosts.length / postsPerPage);
  const startIndex = (currentPage - 1) * postsPerPage;
  const currentPosts = filteredPosts.slice(startIndex, startIndex + postsPerPage);

  // Load categories from backend
  useEffect(() => {
    const loadCategories = async () => {
      try {
        setCategoriesLoading(true);
        setCategoriesError(null);
        const data = await apiClient.getCategories();
        setCategories(data);
      } catch (error) {
        console.error('Failed to load categories:', error);
        setCategoriesError('Failed to load categories');
      } finally {
        setCategoriesLoading(false);
      }
    };

    loadCategories();
  }, []);

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    filterPosts(query, selectedCategories);
    setCurrentPage(1);
  };

  const handleCategoryChange = (categoryIds: string[]) => {
    setSelectedCategories(categoryIds);
    filterPosts(searchQuery, categoryIds);
    setCurrentPage(1);
  };

  const filterPosts = (query: string, categoryIds: string[]) => {
    let filtered = posts;

    if (query) {
      filtered = filtered.filter(post =>
        post.title.toLowerCase().includes(query.toLowerCase()) ||
        post.summary?.toLowerCase().includes(query.toLowerCase()) ||
        post.content.toLowerCase().includes(query.toLowerCase())
      );
    }

    if (categoryIds.length > 0) {
      filtered = filtered.filter(post =>
        post.categories.some(cat => categoryIds.includes(cat.id.toString()))
      );
    }

    setFilteredPosts(filtered);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="text-center mb-12">
        <h1 className="text-4xl font-bold text-gray-900 sm:text-5xl">Blog Posts</h1>
        <p className="mt-4 max-w-2xl mx-auto text-xl text-gray-600">
          Discover our latest articles and insights on web development, architecture, and more
        </p>
      </div>

      {/* Search and Filters */}
      <div className="mb-8 space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <SearchBar onSearch={handleSearch} />
          
          <div className="flex items-center gap-2">
            <span className="text-sm text-gray-600">View:</span>
            <Button
              variant={viewMode === 'grid' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setViewMode('grid')}
            >
              <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
              </svg>
              Grid
            </Button>
            <Button
              variant={viewMode === 'list' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setViewMode('list')}
            >
              <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 10h16M4 14h16M4 18h16" />
              </svg>
              List
            </Button>
          </div>
        </div>

        <CategoryFilter
          categories={categories}
          selectedCategories={selectedCategories}
          onCategoryChange={handleCategoryChange}
          loading={categoriesLoading}
          error={categoriesError}
        />
      </div>

      {/* Results Summary */}
      <div className="mb-6 flex items-center justify-between">
        <p className="text-sm text-gray-600">
          Showing {startIndex + 1}-{Math.min(startIndex + postsPerPage, filteredPosts.length)} of {filteredPosts.length} posts
        </p>
      </div>

      {/* Posts Grid/List */}
      {currentPosts.length > 0 ? (
        <div className={`mb-12 ${
          viewMode === 'grid' 
            ? 'grid gap-8 md:grid-cols-2 lg:grid-cols-3' 
            : 'space-y-6'
        }`}>
          {currentPosts.map((post) => (
            <PostCard key={post.id} post={post} />
          ))}
        </div>
      ) : (
        <div className="text-center py-12">
          <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.172 16.172a4 4 0 015.656 0M9 12h6m-6-4h6m2 5.291A7.962 7.962 0 0112 15c-2.34 0-4.511.799-6.255 2.145a10.02 10.02 0 0012.51 0z" />
          </svg>
          <h3 className="mt-2 text-sm font-medium text-gray-900">No posts found</h3>
          <p className="mt-1 text-sm text-gray-500">
            Try adjusting your search criteria or browse all posts.
          </p>
          <div className="mt-6">
            <Button onClick={() => {
              setSearchQuery('');
              setSelectedCategories([]);
              setFilteredPosts(posts);
              setCurrentPage(1);
            }}>
              Clear Filters
            </Button>
          </div>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      )}
    </div>
  );
}