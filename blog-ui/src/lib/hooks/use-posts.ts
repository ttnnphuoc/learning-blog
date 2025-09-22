'use client';

import { useState, useEffect, useCallback } from 'react';
import { apiClient } from '../api';
import type { PostDto, PostQueryParams, PostsState } from '@/types';

interface UsePostsResult extends PostsState {
  fetchPosts: (params?: PostQueryParams) => Promise<void>;
  fetchPost: (id: string) => Promise<PostDto | null>;
  fetchPostBySlug: (slug: string) => Promise<PostDto | null>;
  refreshPosts: () => Promise<void>;
}

export const usePosts = (initialParams?: PostQueryParams): UsePostsResult => {
  const [postsState, setPostsState] = useState<PostsState>({
    posts: [],
    currentPost: null,
    loading: false,
    error: null,
    hasMore: true,
  });

  const fetchPosts = useCallback(async (params: PostQueryParams = {}) => {
    setPostsState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const response = await apiClient.getPosts(params);
      const posts = response.data || [];
      setPostsState(prev => ({
        ...prev,
        posts,
        loading: false,
        hasMore: response.pagination?.hasNext || false,
      }));
    } catch (error) {
      console.error('Failed to fetch posts:', error);
      setPostsState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to fetch posts',
      }));
    }
  }, []);

  const fetchPost = useCallback(async (id: string): Promise<PostDto | null> => {
    setPostsState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const post = await apiClient.getPost(id);
      setPostsState(prev => ({
        ...prev,
        currentPost: post,
        loading: false,
      }));
      return post;
    } catch (error) {
      console.error('Failed to fetch post:', error);
      setPostsState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to fetch post',
      }));
      return null;
    }
  }, []);

  const fetchPostBySlug = useCallback(async (slug: string): Promise<PostDto | null> => {
    setPostsState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const post = await apiClient.getPostBySlug(slug);
      setPostsState(prev => ({
        ...prev,
        currentPost: post,
        loading: false,
      }));
      return post;
    } catch (error) {
      console.error('Failed to fetch post by slug:', error);
      setPostsState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to fetch post',
      }));
      return null;
    }
  }, []);

  const refreshPosts = useCallback(async () => {
    await fetchPosts(initialParams);
  }, [fetchPosts, initialParams]);

  // Initial fetch
  useEffect(() => {
    if (initialParams !== undefined) {
      fetchPosts(initialParams);
    }
  }, [fetchPosts, initialParams]);

  return {
    ...postsState,
    fetchPosts,
    fetchPost,
    fetchPostBySlug,
    refreshPosts,
  };
};

export default usePosts;