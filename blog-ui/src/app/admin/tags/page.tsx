'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { AdminTable, Column } from '@/components/admin/admin-table';
import { AdminModal } from '@/components/admin/admin-modal';
import { DeleteConfirmModal } from '@/components/admin/delete-confirm-modal';
import { Input } from '@/components/ui/input';
import { apiClient } from '@/lib/api';
import type { TagDto, CreateTagDto, UpdateTagDto } from '@/types';

export default function AdminTags() {
  const [tags, setTags] = useState<TagDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedTag, setSelectedTag] = useState<TagDto | null>(null);
  const [formData, setFormData] = useState<CreateTagDto>({
    name: '',
    slug: '',
  });
  const [formLoading, setFormLoading] = useState(false);

  const columns: Column<TagDto>[] = [
    {
      key: 'name',
      title: 'Name',
      render: (value) => (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium bg-gray-100 text-gray-800">
          #{value}
        </span>
      ),
    },
    {
      key: 'slug',
      title: 'Slug',
      render: (value) => (
        <span className="text-blue-600 font-mono text-sm">{value}</span>
      ),
    },
    {
      key: 'postCount',
      title: 'Posts',
      render: (value) => (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
          {value || 0}
        </span>
      ),
    },
    {
      key: 'createdAt',
      title: 'Created',
      render: (value) => new Date(value).toLocaleDateString(),
    },
  ];

  const fetchTags = async () => {
    try {
      setLoading(true);
      const data = await apiClient.getTags();
      setTags(data);
    } catch (error) {
      console.error('Failed to fetch tags:', error);
    } finally {
      setLoading(false);
    }
  };

  const generateSlug = (name: string) => {
    return name
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, '');
  };

  const handleCreate = () => {
    setFormData({ name: '', slug: '' });
    setIsCreateModalOpen(true);
  };

  const handleEdit = (tag: TagDto) => {
    setSelectedTag(tag);
    setFormData({
      name: tag.name,
      slug: tag.slug,
    });
    setIsEditModalOpen(true);
  };

  const handleDelete = (tag: TagDto) => {
    setSelectedTag(tag);
    setIsDeleteModalOpen(true);
  };

  const handleSubmitCreate = async () => {
    try {
      setFormLoading(true);
      const slug = formData.slug || generateSlug(formData.name);
      await apiClient.createTag({ ...formData, slug });
      setIsCreateModalOpen(false);
      fetchTags();
    } catch (error) {
      console.error('Failed to create tag:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSubmitEdit = async () => {
    if (!selectedTag) return;

    try {
      setFormLoading(true);
      const slug = formData.slug || generateSlug(formData.name);
      await apiClient.updateTag(selectedTag.id, { 
        ...formData, 
        id: selectedTag.id,
        slug 
      });
      setIsEditModalOpen(false);
      setSelectedTag(null);
      fetchTags();
    } catch (error) {
      console.error('Failed to update tag:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedTag) return;

    try {
      setFormLoading(true);
      await apiClient.deleteTag(selectedTag.id);
      setIsDeleteModalOpen(false);
      setSelectedTag(null);
      fetchTags();
    } catch (error) {
      console.error('Failed to delete tag:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormChange = useCallback((field: keyof CreateTagDto) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = e.target.value;
    setFormData(prev => {
      const newData = { ...prev, [field]: value };
      
      // Auto-generate slug when name changes and slug is empty
      if (field === 'name' && !prev.slug) {
        newData.slug = generateSlug(value);
      }
      
      return newData;
    });
  }, []);

  useEffect(() => {
    fetchTags();
  }, []);

  const TagForm = React.useMemo(() => (
    <div className="space-y-4">
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
          Name *
        </label>
        <Input
          id="name"
          value={formData.name}
          onChange={handleFormChange('name')}
          placeholder="Tag name"
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
        <p className="text-xs text-gray-500 mt-1">
          Used in URLs. Leave empty to auto-generate from name.
        </p>
      </div>
    </div>
  ), [formData, formLoading]);

  return (
    <div>
      <AdminTable
        data={tags}
        columns={columns}
        loading={loading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        title="Tags"
      />

      {/* Create Modal */}
      <AdminModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleSubmitCreate}
        title="Create Tag"
        loading={formLoading}
      >
        {TagForm}
      </AdminModal>

      {/* Edit Modal */}
      <AdminModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedTag(null);
        }}
        onSubmit={handleSubmitEdit}
        title="Edit Tag"
        loading={formLoading}
      >
        {TagForm}
      </AdminModal>

      {/* Delete Modal */}
      <DeleteConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setSelectedTag(null);
        }}
        onConfirm={handleConfirmDelete}
        title="Tag"
        message={`Are you sure you want to delete the tag "${selectedTag?.name}"? This action cannot be undone.`}
        loading={formLoading}
      />
    </div>
  );
}