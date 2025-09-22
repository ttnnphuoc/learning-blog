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


export default function PostsPage() {
  const [posts, setPosts] = useState<Post[]>([]);
  const [filteredPosts, setFilteredPosts] = useState<Post[]>([]);
  const [postsLoading, setPostsLoading] = useState(true);
  const [postsError, setPostsError] = useState<string | null>(null);
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

  // Load posts from backend
  useEffect(() => {
    const loadPosts = async () => {
      try {
        setPostsLoading(true);
        setPostsError(null);
        const response = await apiClient.getPosts({ publishedOnly: true });
        const data = response.data || [];
        setPosts(data);
        setFilteredPosts(data);
      } catch (error) {
        console.error('Failed to load posts:', error);
        setPostsError('Failed to load posts');
      } finally {
        setPostsLoading(false);
      }
    };

    loadPosts();
  }, []);

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
      {postsError ? (
        <div className="text-center py-12">
          <div className="text-red-600 bg-red-50 p-6 rounded-lg max-w-md mx-auto">
            <svg className="mx-auto h-12 w-12 text-red-400 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
            <h3 className="text-lg font-semibold text-red-800 mb-2">Failed to Load Posts</h3>
            <p className="text-red-700">{postsError}</p>
          </div>
        </div>
      ) : postsLoading ? (
        <div className={`mb-12 ${
          viewMode === 'grid' 
            ? 'grid gap-8 md:grid-cols-2 lg:grid-cols-3' 
            : 'space-y-6'
        }`}>
          {[...Array(6)].map((_, index) => (
            <div key={index} className="bg-white rounded-lg shadow-md overflow-hidden animate-pulse">
              <div className="h-48 bg-gray-200"></div>
              <div className="p-6">
                <div className="h-4 bg-gray-200 rounded mb-2"></div>
                <div className="h-4 bg-gray-200 rounded mb-4 w-3/4"></div>
                <div className="h-3 bg-gray-200 rounded mb-2"></div>
                <div className="h-3 bg-gray-200 rounded mb-2"></div>
                <div className="h-3 bg-gray-200 rounded w-1/2"></div>
              </div>
            </div>
          ))}
        </div>
      ) : currentPosts.length > 0 ? (
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