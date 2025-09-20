'use client';

import { useState, useEffect } from 'react';
import { AdminTable, Column } from '@/components/admin/admin-table';
import { AdminModal } from '@/components/admin/admin-modal';
import { DeleteConfirmModal } from '@/components/admin/delete-confirm-modal';
import { Input } from '@/components/ui/input';
import { apiClient } from '@/lib/api';
import { RoleHelper, UserRole } from '@/types';
import type { Role, CreateRoleDto, UpdateRoleDto, Permission } from '@/types';

export default function AdminRoles() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [loading, setLoading] = useState(true);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<Role | null>(null);
  const [formData, setFormData] = useState<CreateRoleDto>({
    name: '',
    description: '',
    permissionIds: [],
  });
  const [formLoading, setFormLoading] = useState(false);

  const columns: Column<Role>[] = [
    {
      key: 'name',
      title: 'Role Name',
      render: (value) => (
        <div className="flex items-center">
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium ${RoleHelper.getRoleColor(value as UserRole)}`}>
            {RoleHelper.getRoleDisplay(value as UserRole)}
          </span>
        </div>
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
      key: 'permissions',
      title: 'Permissions',
      render: (permissions) => (
        <div className="max-w-xs">
          {permissions && permissions.length > 0 ? (
            <div className="flex flex-wrap gap-1">
              {permissions.slice(0, 3).map((permission: any) => (
                <span
                  key={permission.id}
                  className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                >
                  {permission.name}
                </span>
              ))}
              {permissions.length > 3 && (
                <span className="text-xs text-gray-500">+{permissions.length - 3}</span>
              )}
            </div>
          ) : (
            <span className="text-gray-400 text-sm">No permissions</span>
          )}
        </div>
      ),
    },
    {
      key: 'createdAt',
      title: 'Created',
      render: (value) => new Date(value).toLocaleDateString(),
    },
  ];

  const fetchRoles = async () => {
    try {
      setLoading(true);
      const [rolesData, permissionsData] = await Promise.all([
        apiClient.getRoles(),
        apiClient.getPermissions()
      ]);
      setRoles(rolesData);
      setPermissions(permissionsData);
    } catch (error) {
      console.error('Failed to fetch roles:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setFormData({ name: '', description: '', permissionIds: [] });
    setIsCreateModalOpen(true);
  };

  const handleEdit = (role: Role) => {
    setSelectedRole(role);
    setFormData({
      name: role.name,
      description: role.description,
      permissionIds: role.permissions?.map(p => p.id) || [],
    });
    setIsEditModalOpen(true);
  };

  const handleDelete = (role: Role) => {
    setSelectedRole(role);
    setIsDeleteModalOpen(true);
  };

  const handleSubmitCreate = async () => {
    try {
      setFormLoading(true);
      await apiClient.createRole(formData);
      setIsCreateModalOpen(false);
      fetchRoles();
    } catch (error) {
      console.error('Failed to create role:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSubmitEdit = async () => {
    if (!selectedRole) return;

    try {
      setFormLoading(true);
      await apiClient.updateRole(selectedRole.id, { 
        ...formData, 
        id: selectedRole.id
      });
      setIsEditModalOpen(false);
      setSelectedRole(null);
      fetchRoles();
    } catch (error) {
      console.error('Failed to update role:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedRole) return;

    try {
      setFormLoading(true);
      await apiClient.deleteRole(selectedRole.id);
      setIsDeleteModalOpen(false);
      setSelectedRole(null);
      fetchRoles();
    } catch (error) {
      console.error('Failed to delete role:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormChange = (field: keyof CreateRoleDto) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const value = e.target.value;
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handlePermissionChange = (permissionId: string, checked: boolean) => {
    setFormData(prev => ({
      ...prev,
      permissionIds: checked 
        ? [...(prev.permissionIds || []), permissionId]
        : (prev.permissionIds || []).filter(id => id !== permissionId)
    }));
  };

  useEffect(() => {
    fetchRoles();
  }, []);

  const RoleForm = () => (
    <div className="space-y-4">
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
          Role Name *
        </label>
        <select
          id="name"
          value={formData.name}
          onChange={handleFormChange('name')}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          required
        >
          <option value="">Select a role</option>
          {RoleHelper.getAllRoles().map((role) => (
            <option key={role} value={role}>
              {RoleHelper.getRoleDisplay(role)}
            </option>
          ))}
        </select>
        <p className="text-xs text-gray-500 mt-1">
          Choose from predefined system roles.
        </p>
      </div>
      
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description *
        </label>
        <textarea
          id="description"
          value={formData.description}
          onChange={handleFormChange('description')}
          placeholder="Describe the permissions and responsibilities of this role"
          rows={4}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          required
        />
      </div>

      {/* Permissions Assignment */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Permissions
        </label>
        <div className="max-h-60 overflow-y-auto border border-gray-300 rounded-md p-3">
          {/* Group permissions by category */}
          {Object.entries(
            permissions.reduce((acc, permission) => {
              const category = permission.category || 'Uncategorized';
              if (!acc[category]) acc[category] = [];
              acc[category].push(permission);
              return acc;
            }, {} as Record<string, Permission[]>)
          ).map(([category, categoryPermissions]) => (
            <div key={category} className="mb-4">
              <h4 className="text-sm font-medium text-gray-900 mb-2 border-b pb-1">
                {category}
              </h4>
              <div className="space-y-2 ml-2">
                {categoryPermissions.map((permission) => (
                  <label key={permission.id} className="flex items-start">
                    <input
                      type="checkbox"
                      checked={(formData.permissionIds || []).includes(permission.id)}
                      onChange={(e) => handlePermissionChange(permission.id, e.target.checked)}
                      className="mt-0.5 rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
                    />
                    <div className="ml-2">
                      <span className="text-sm text-gray-900 font-mono">{permission.name}</span>
                      <p className="text-xs text-gray-500">{permission.description}</p>
                    </div>
                  </label>
                ))}
              </div>
            </div>
          ))}
        </div>
        <p className="text-xs text-gray-500 mt-1">
          Selected {formData.permissionIds?.length || 0} of {permissions.length} permissions
        </p>
      </div>

      {/* Role Information */}
      {formData.name && (
        <div className="bg-gray-50 p-4 rounded-md">
          <h4 className="text-sm font-medium text-gray-900 mb-2">Role Information</h4>
          <div className="space-y-2 text-sm text-gray-600">
            {formData.name === UserRole.ADMIN && (
              <ul className="list-disc list-inside space-y-1">
                <li>Full system access and control</li>
                <li>Can manage all users, roles, and permissions</li>
                <li>Can create, edit, and delete all content</li>
                <li>Access to admin dashboard</li>
              </ul>
            )}
            {formData.name === UserRole.MODERATOR && (
              <ul className="list-disc list-inside space-y-1">
                <li>Content moderation privileges</li>
                <li>Can review and approve posts</li>
                <li>Can manage categories and tags</li>
                <li>Access to admin dashboard</li>
              </ul>
            )}
            {formData.name === UserRole.AUTHOR && (
              <ul className="list-disc list-inside space-y-1">
                <li>Can create and edit own posts</li>
                <li>Can publish content</li>
                <li>Can manage own profile</li>
                <li>Limited admin access</li>
              </ul>
            )}
            {formData.name === UserRole.READER && (
              <ul className="list-disc list-inside space-y-1">
                <li>Can read and comment on posts</li>
                <li>Can manage own profile</li>
                <li>Basic user privileges</li>
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  );

  return (
    <div>
      <AdminTable
        data={roles}
        columns={columns}
        loading={loading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        title="Roles"
      />

      {/* Create Modal */}
      <AdminModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleSubmitCreate}
        title="Create Role"
        loading={formLoading}
        size="lg"
      >
        <RoleForm />
      </AdminModal>

      {/* Edit Modal */}
      <AdminModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedRole(null);
        }}
        onSubmit={handleSubmitEdit}
        title="Edit Role"
        loading={formLoading}
        size="lg"
      >
        <RoleForm />
      </AdminModal>

      {/* Delete Modal */}
      <DeleteConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setSelectedRole(null);
        }}
        onConfirm={handleConfirmDelete}
        title="Role"
        message={`Are you sure you want to delete the role "${selectedRole?.name}"? This will affect all users assigned to this role.`}
        loading={formLoading}
      />

      {/* Role Information Panel */}
      <div className="mt-8 bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Role Descriptions</h2>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {RoleHelper.getAllRoles().map((role) => (
            <div key={role} className="border rounded-lg p-4">
              <div className="flex items-center mb-2">
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium ${RoleHelper.getRoleColor(role)}`}>
                  {RoleHelper.getRoleDisplay(role)}
                </span>
              </div>
              <div className="text-sm text-gray-600">
                {role === UserRole.ADMIN && "Complete system administration and management capabilities."}
                {role === UserRole.MODERATOR && "Content moderation and administrative oversight."}
                {role === UserRole.AUTHOR && "Content creation and publishing privileges."}
                {role === UserRole.READER && "Basic user access and interaction rights."}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}