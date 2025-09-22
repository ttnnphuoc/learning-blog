'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { AdminTable, Column } from '@/components/admin/admin-table';
import { AdminModal } from '@/components/admin/admin-modal';
import { DeleteConfirmModal } from '@/components/admin/delete-confirm-modal';
import { Input } from '@/components/ui/input';
import { RichTextEditor } from '@/components/ui/rich-text-editor';
import { apiClient } from '@/lib/api';
import { useAuth } from '@/lib/auth-context';
import type { PostDto, CreatePostDto, UpdatePostDto, CategoryDto, TagDto } from '@/types';

export default function AdminPosts() {
  const { user } = useAuth();
  const [posts, setPosts] = useState<PostDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize] = useState(10);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedPost, setSelectedPost] = useState<PostDto | null>(null);
  const [formData, setFormData] = useState<CreatePostDto>({
    title: '',
    content: '',
    summary: '',
    slug: '',
    imageUrl: '',
    isPublished: false,
    categoryIds: [],
    tagIds: [],
  });
  const [formLoading, setFormLoading] = useState(false);

  const columns: Column<PostDto>[] = [
    {
      key: 'title',
      title: 'Title',
      render: (value, record) => (
        <div className="max-w-xs">
          <div className="font-medium text-gray-900 truncate">{value}</div>
          {record.summary && (
            <div className="text-sm text-gray-500 truncate">{record.summary}</div>
          )}
        </div>
      ),
    },
    {
      key: 'author',
      title: 'Author',
      render: (author) => (
        <span className="text-gray-900">{author.firstName} {author.lastName}</span>
      ),
    },
    {
      key: 'isPublished',
      title: 'Status',
      render: (value, record) => (
        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
          value ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
        }`}>
          {value ? 'Published' : 'Draft'}
          {value && record.publishedAt && (
            <span className="ml-1 text-xs">
              ({new Date(record.publishedAt).toLocaleDateString()})
            </span>
          )}
        </span>
      ),
    },
    {
      key: 'categories',
      title: 'Categories',
      render: (categories) => (
        <div className="flex flex-wrap gap-1 max-w-xs">
          {categories.slice(0, 2).map((category: any) => (
            <span
              key={category.id}
              className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
            >
              {category.name}
            </span>
          ))}
          {categories.length > 2 && (
            <span className="text-xs text-gray-500">+{categories.length - 2}</span>
          )}
        </div>
      ),
    },
    {
      key: 'tags',
      title: 'Tags',
      render: (tags) => (
        <div className="flex flex-wrap gap-1 max-w-xs">
          {tags.slice(0, 3).map((tag: any) => (
            <span
              key={tag.id}
              className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
            >
              #{tag.name}
            </span>
          ))}
          {tags.length > 3 && (
            <span className="text-xs text-gray-500">+{tags.length - 3}</span>
          )}
        </div>
      ),
    },
    {
      key: 'viewCount',
      title: 'Views',
      render: (value) => (
        <span className="text-gray-600">{value || 0}</span>
      ),
    },
    {
      key: 'createdAt',
      title: 'Created',
      render: (value) => new Date(value).toLocaleDateString(),
    },
  ];

  const fetchPosts = async (page: number = currentPage) => {
    try {
      setLoading(true);
      const [postsResponse, categoriesData, tagsData] = await Promise.all([
        apiClient.getPosts({ 
          publishedOnly: false, 
          page, 
          pageSize 
        }),
        apiClient.getCategories(),
        apiClient.getTags()
      ]);
      setPosts(postsResponse.data || []);
      setTotalPages(postsResponse.pagination?.totalPages || 1);
      setCurrentPage(postsResponse.pagination?.currentPage || 1);
      setCategories(categoriesData);
      setTags(tagsData);
    } catch (error) {
      console.error('Failed to fetch posts:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPosts();
  }, []);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    fetchPosts(page);
  };

  const generateSlug = (title: string) => {
    return title
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, '');
  };

  const handleCreate = () => {
    setFormData({
      title: '',
      content: '',
      summary: '',
      slug: '',
      imageUrl: '',
      isPublished: false,
      categoryIds: [],
      tagIds: [],
    });
    setIsCreateModalOpen(true);
  };

  const handleEdit = (post: PostDto) => {
    setSelectedPost(post);
    setFormData({
      title: post.title,
      content: post.content,
      summary: post.summary || '',
      slug: post.slug,
      imageUrl: post.imageUrl || '',
      isPublished: post.isPublished,
      categoryIds: post.categories.map(cat => cat.id),
      tagIds: post.tags.map(tag => tag.id),
    });
    setIsEditModalOpen(true);
  };

  const handleDelete = (post: PostDto) => {
    setSelectedPost(post);
    setIsDeleteModalOpen(true);
  };

  const handleSubmitCreate = async () => {
    try {
      setFormLoading(true);
      const slug = formData.slug || generateSlug(formData.title);
      await apiClient.createPost({ ...formData, slug });
      setIsCreateModalOpen(false);
      fetchPosts(1);
      setCurrentPage(1);
    } catch (error) {
      console.error('Failed to create post:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSubmitEdit = async () => {
    if (!selectedPost) return;

    try {
      setFormLoading(true);
      const slug = formData.slug || generateSlug(formData.title);
      await apiClient.updatePost(selectedPost.id, { 
        ...formData, 
        id: selectedPost.id,
        slug 
      });
      setIsEditModalOpen(false);
      setSelectedPost(null);
      fetchPosts(currentPage);
    } catch (error) {
      console.error('Failed to update post:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedPost) return;

    try {
      setFormLoading(true);
      await apiClient.deletePost(selectedPost.id);
      setIsDeleteModalOpen(false);
      setSelectedPost(null);
      
      // If current page becomes empty after deletion, go to previous page
      if (posts.length === 1 && currentPage > 1) {
        const newPage = currentPage - 1;
        setCurrentPage(newPage);
        fetchPosts(newPage);
      } else {
        fetchPosts(currentPage);
      }
    } catch (error) {
      console.error('Failed to delete post:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormChange = useCallback((field: keyof CreatePostDto) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const value = e.target.type === 'checkbox' ? (e.target as HTMLInputElement).checked : e.target.value;
    setFormData(prev => {
      const newData = { ...prev, [field]: value };
      
      // Auto-generate slug when title changes and slug is empty
      if (field === 'title') {
        newData.slug = generateSlug(value as string);
      }
      
      return newData;
    });
  }, []);

  const handleCategoryChange = (categoryId: string, checked: boolean) => {
    setFormData(prev => ({
      ...prev,
      categoryIds: checked 
        ? [...prev.categoryIds, categoryId]
        : prev.categoryIds.filter(id => id !== categoryId)
    }));
  };

  const handleTagChange = (tagId: string, checked: boolean) => {
    setFormData(prev => ({
      ...prev,
      tagIds: checked 
        ? [...prev.tagIds, tagId]
        : prev.tagIds.filter(id => id !== tagId)
    }));
  };

  const PostForm = React.useMemo(() => (
    <div className="space-y-6">
      <div className="grid grid-cols-2 gap-4">
        <div className="col-span-2">
          <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-1">
            Title *
          </label>
          <Input
            id="title"
            value={formData.title}
            onChange={handleFormChange('title')}
            placeholder="Post title"
            required
          />
        </div>
        
        <div>
          <label htmlFor="slug" className="block text-sm font-medium text-gray-700 mb-1">
            Slug
          </label>
          <Input
            id="slug"
            value={formData.slug}
            onChange={handleFormChange('slug')}
            placeholder="URL-friendly slug (auto-generated if empty)"
          />
        </div>
        
        <div>
          <label htmlFor="imageUrl" className="block text-sm font-medium text-gray-700 mb-1">
            Featured Image URL
          </label>
          <Input
            id="imageUrl"
            value={formData.imageUrl}
            onChange={handleFormChange('imageUrl')}
            placeholder="https://example.com/image.jpg"
          />
        </div>
      </div>
      
      <div>
        <label htmlFor="summary" className="block text-sm font-medium text-gray-700 mb-1">
          Summary
        </label>
        <textarea
          id="summary"
          value={formData.summary}
          onChange={handleFormChange('summary')}
          placeholder="Brief summary of the post"
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>
      
      <div>
        <label htmlFor="content" className="block text-sm font-medium text-gray-700 mb-1">
          Content *
        </label>
        <RichTextEditor
          content={formData.content}
          onChange={(content) => setFormData(prev => ({ ...prev, content }))}
          placeholder="Write your post content here..."
          className="w-full"
        />
      </div>
      
      <div className="grid grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Categories
          </label>
          <div className="space-y-2 max-h-40 overflow-y-auto">
            {categories.map((category) => (
              <label key={category.id} className="flex items-center">
                <input
                  type="checkbox"
                  checked={formData.categoryIds.includes(category.id)}
                  onChange={(e) => handleCategoryChange(category.id, e.target.checked)}
                  className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
                />
                <span className="ml-2 text-sm text-gray-700">{category.name}</span>
              </label>
            ))}
          </div>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Tags
          </label>
          <div className="space-y-2 max-h-40 overflow-y-auto">
            {tags.map((tag) => (
              <label key={tag.id} className="flex items-center">
                <input
                  type="checkbox"
                  checked={formData.tagIds.includes(tag.id)}
                  onChange={(e) => handleTagChange(tag.id, e.target.checked)}
                  className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
                />
                <span className="ml-2 text-sm text-gray-700">#{tag.name}</span>
              </label>
            ))}
          </div>
        </div>
      </div>
      
      <div className="flex items-center">
        <input
          id="isPublished"
          type="checkbox"
          checked={formData.isPublished}
          onChange={handleFormChange('isPublished')}
          className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
        />
        <label htmlFor="isPublished" className="ml-2 text-sm text-gray-700">
          Publish immediately
        </label>
      </div>
    </div>
  ), [formData, formLoading, categories, tags]);

  return (
    <div>
      <AdminTable
        data={posts}
        columns={columns}
        loading={loading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        title="Posts"
        pagination={{
          currentPage,
          totalPages,
          onPageChange: handlePageChange,
        }}
      />

      {/* Create Modal */}
      <AdminModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleSubmitCreate}
        title="Create Post"
        loading={formLoading}
        size="xl"
      >
        {PostForm}
      </AdminModal>

      {/* Edit Modal */}
      <AdminModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedPost(null);
        }}
        onSubmit={handleSubmitEdit}
        title="Edit Post"
        loading={formLoading}
        size="xl"
      >
        {PostForm}
      </AdminModal>

      {/* Delete Modal */}
      <DeleteConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setSelectedPost(null);
        }}
        onConfirm={handleConfirmDelete}
        title="Post"
        message={`Are you sure you want to delete the post "${selectedPost?.title}"? This action cannot be undone.`}
        loading={formLoading}
      />
    </div>
  );
}