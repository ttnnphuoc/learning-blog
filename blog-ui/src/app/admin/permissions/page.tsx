'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { AdminTable, Column } from '@/components/admin/admin-table';
import { AdminModal } from '@/components/admin/admin-modal';
import { DeleteConfirmModal } from '@/components/admin/delete-confirm-modal';
import { Input } from '@/components/ui/input';
import { apiClient } from '@/lib/api';
import type { Permission, CreatePermissionDto, UpdatePermissionDto } from '@/types';

export default function AdminPermissions() {
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [loading, setLoading] = useState(true);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedPermission, setSelectedPermission] = useState<Permission | null>(null);
  const [formData, setFormData] = useState<CreatePermissionDto>({
    name: '',
    description: '',
    category: '',
  });
  const [formLoading, setFormLoading] = useState(false);

  // Permission categories for organization
  const permissionCategories = [
    'User Management',
    'Post Management', 
    'Category Management',
    'Tag Management',
    'Role & Permission Management',
    'System Administration',
    'Comment Management',
  ];

  const columns: Column<Permission>[] = [
    {
      key: 'name',
      title: 'Permission Name',
      render: (value) => (
        <span className="font-medium text-gray-900 font-mono text-sm">{value}</span>
      ),
    },
    {
      key: 'description',
      title: 'Description',
      render: (value) => (
        <span className="text-gray-600">{value}</span>
      ),
    },
    {
      key: 'category',
      title: 'Category',
      render: (value) => (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
          {value}
        </span>
      ),
    },
    {
      key: 'createdAt',
      title: 'Created',
      render: (value) => new Date(value).toLocaleDateString(),
    },
  ];

  const fetchPermissions = async () => {
    try {
      setLoading(true);
      const data = await apiClient.getPermissions();
      setPermissions(data);
    } catch (error) {
      console.error('Failed to fetch permissions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setFormData({ name: '', description: '', category: '' });
    setIsCreateModalOpen(true);
  };

  const handleEdit = (permission: Permission) => {
    setSelectedPermission(permission);
    setFormData({
      name: permission.name,
      description: permission.description,
      category: permission.category,
    });
    setIsEditModalOpen(true);
  };

  const handleDelete = (permission: Permission) => {
    setSelectedPermission(permission);
    setIsDeleteModalOpen(true);
  };

  const handleSubmitCreate = async () => {
    try {
      setFormLoading(true);
      await apiClient.createPermission(formData);
      setIsCreateModalOpen(false);
      fetchPermissions();
    } catch (error) {
      console.error('Failed to create permission:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSubmitEdit = async () => {
    if (!selectedPermission) return;

    try {
      setFormLoading(true);
      await apiClient.updatePermission(selectedPermission.id, { 
        ...formData, 
        id: selectedPermission.id
      });
      setIsEditModalOpen(false);
      setSelectedPermission(null);
      fetchPermissions();
    } catch (error) {
      console.error('Failed to update permission:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedPermission) return;

    try {
      setFormLoading(true);
      await apiClient.deletePermission(selectedPermission.id);
      setIsDeleteModalOpen(false);
      setSelectedPermission(null);
      fetchPermissions();
    } catch (error) {
      console.error('Failed to delete permission:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormChange = useCallback((field: keyof CreatePermissionDto) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const value = e.target.value;
    setFormData(prev => ({ ...prev, [field]: value }));
  }, []);

  useEffect(() => {
    fetchPermissions();
  }, []);

  const PermissionForm = React.useMemo(() => (
    <div className="space-y-4">
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
          Permission Name *
        </label>
        <Input
          id="name"
          value={formData.name}
          onChange={handleFormChange('name')}
          placeholder="e.g., create_post, delete_user"
          required
        />
        <p className="text-xs text-gray-500 mt-1">
          Use snake_case format (e.g., create_post, manage_users)
        </p>
      </div>
      
      <div>
        <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-1">
          Category *
        </label>
        <select
          id="category"
          value={formData.category}
          onChange={handleFormChange('category')}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          required
        >
          <option value="">Select a category</option>
          {permissionCategories.map((category) => (
            <option key={category} value={category}>
              {category}
            </option>
          ))}
        </select>
      </div>
      
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description *
        </label>
        <textarea
          id="description"
          value={formData.description}
          onChange={handleFormChange('description')}
          placeholder="Describe what this permission allows users to do"
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          required
        />
      </div>
    </div>
  ), [formData, formLoading]);

  // Group permissions by category for display
  const groupedPermissions = permissions.reduce((acc, permission) => {
    const category = permission.category || 'Uncategorized';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(permission);
    return acc;
  }, {} as Record<string, Permission[]>);

  return (
    <div>
      <AdminTable
        data={permissions}
        columns={columns}
        loading={loading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        title="Permissions"
      />

      {/* Create Modal */}
      <AdminModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleSubmitCreate}
        title="Create Permission"
        loading={formLoading}
      >
        {PermissionForm}
      </AdminModal>

      {/* Edit Modal */}
      <AdminModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedPermission(null);
        }}
        onSubmit={handleSubmitEdit}
        title="Edit Permission"
        loading={formLoading}
      >
        {PermissionForm}
      </AdminModal>

      {/* Delete Modal */}
      <DeleteConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setSelectedPermission(null);
        }}
        onConfirm={handleConfirmDelete}
        title="Permission"
        message={`Are you sure you want to delete the permission "${selectedPermission?.name}"? This will affect all roles that have this permission.`}
        loading={formLoading}
      />

      {/* Permission Categories Overview */}
      <div className="mt-8 bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Permissions by Category</h2>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {Object.entries(groupedPermissions).map(([category, categoryPermissions]) => (
            <div key={category} className="border rounded-lg p-4">
              <h3 className="font-medium text-gray-900 mb-2 flex items-center justify-between">
                {category}
                <span className="text-sm text-gray-500 bg-gray-100 px-2 py-1 rounded">
                  {categoryPermissions.length}
                </span>
              </h3>
              <div className="space-y-1">
                {categoryPermissions.slice(0, 5).map((permission) => (
                  <div key={permission.id} className="text-sm text-gray-600 font-mono">
                    {permission.name}
                  </div>
                ))}
                {categoryPermissions.length > 5 && (
                  <div className="text-sm text-gray-500">
                    +{categoryPermissions.length - 5} more...
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Statistics */}
      <div className="mt-8 bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Permission Statistics</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div className="text-center">
            <div className="text-2xl font-bold text-blue-600">{permissions.length}</div>
            <div className="text-sm text-gray-500">Total Permissions</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-green-600">
              {Object.keys(groupedPermissions).length}
            </div>
            <div className="text-sm text-gray-500">Categories</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-purple-600">
              {permissions.length > 0 ? Math.round(permissions.length / Math.max(Object.keys(groupedPermissions).length, 1)) : 0}
            </div>
            <div className="text-sm text-gray-500">Avg per Category</div>
          </div>
        </div>
      </div>
    </div>
  );
}