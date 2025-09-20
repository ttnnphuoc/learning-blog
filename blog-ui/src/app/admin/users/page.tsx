'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { AdminTable, Column } from '@/components/admin/admin-table';
import { AdminModal } from '@/components/admin/admin-modal';
import { DeleteConfirmModal } from '@/components/admin/delete-confirm-modal';
import { Input } from '@/components/ui/input';
import { apiClient } from '@/lib/api';
import { useAuth } from '@/lib/auth-context';
import { RoleHelper, UserRole } from '@/types';
import type { User, CreateUserDto, UpdateUserDto, Role } from '@/types';

export default function AdminUsers() {
  const { user } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<CreateUserDto>({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    bio: '',
    roleIds: [],
  });
  const [formLoading, setFormLoading] = useState(false);

  const columns: Column<User>[] = [
    {
      key: 'username',
      title: 'Username',
      render: (value) => (
        <span className="font-medium text-gray-900">{value}</span>
      ),
    },
    {
      key: 'email',
      title: 'Email',
      render: (value) => (
        <span className="text-gray-600">{value}</span>
      ),
    },
    {
      key: 'firstName',
      title: 'Name',
      render: (value, record) => (
        <span className="text-gray-900">{record.firstName} {record.lastName}</span>
      ),
    },
    {
      key: 'roles',
      title: 'Roles',
      render: (roles: Role[]) => (
        <div className="flex flex-wrap gap-1">
          {roles.map((role) => (
            <span
              key={role.id}
              className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${RoleHelper.getRoleColor(role.name as UserRole)}`}
            >
              {RoleHelper.getRoleDisplay(role.name as UserRole)}
            </span>
          ))}
        </div>
      ),
    },
    {
      key: 'isActive',
      title: 'Status',
      render: (value) => (
        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
          value ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
        }`}>
          {value ? 'Active' : 'Inactive'}
        </span>
      ),
    },
    {
      key: 'createdAt',
      title: 'Created',
      render: (value) => new Date(value).toLocaleDateString(),
    },
  ];

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const [usersData, rolesData] = await Promise.all([
        apiClient.getUsers(),
        apiClient.getRoles()
      ]);
      setUsers(usersData);
      setRoles(rolesData);
    } catch (error) {
      console.error('Failed to fetch users:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  // Admin-only access control
  if (!user || !RoleHelper.isAdmin(user.roles)) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <div className="text-6xl mb-4">🚫</div>
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Access Denied</h2>
          <p className="text-gray-600">You need Admin privileges to access this page.</p>
        </div>
      </div>
    );
  }

  const handleCreate = () => {
    setFormData({
      username: '',
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      bio: '',
      roleIds: [],
    });
    setIsCreateModalOpen(true);
  };

  const handleEdit = (user: User) => {
    setSelectedUser(user);
    setFormData({
      username: user.username,
      email: user.email,
      password: '', // Don't pre-fill password for security
      firstName: user.firstName,
      lastName: user.lastName,
      bio: user.bio || '',
      roleIds: user.roles.map(role => role.id),
    });
    setIsEditModalOpen(true);
  };

  const handleDelete = (user: User) => {
    setSelectedUser(user);
    setIsDeleteModalOpen(true);
  };

  const handleSubmitCreate = async () => {
    try {
      setFormLoading(true);
      await apiClient.createUser(formData);
      setIsCreateModalOpen(false);
      fetchUsers();
    } catch (error) {
      console.error('Failed to create user:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSubmitEdit = async () => {
    if (!selectedUser) return;

    try {
      setFormLoading(true);
      const updateData: UpdateUserDto = {
        id: selectedUser.id,
        username: formData.username,
        email: formData.email,
        firstName: formData.firstName,
        lastName: formData.lastName,
        bio: formData.bio,
        roleIds: formData.roleIds,
      };
      
      // Only include password if it's provided
      if (formData.password) {
        updateData.password = formData.password;
      }
      
      await apiClient.updateUser(selectedUser.id, updateData);
      setIsEditModalOpen(false);
      setSelectedUser(null);
      fetchUsers();
    } catch (error) {
      console.error('Failed to update user:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!selectedUser) return;

    try {
      setFormLoading(true);
      await apiClient.deleteUser(selectedUser.id);
      setIsDeleteModalOpen(false);
      setSelectedUser(null);
      fetchUsers();
    } catch (error) {
      console.error('Failed to delete user:', error);
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormChange = useCallback((field: keyof CreateUserDto) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const value = e.target.value;
    setFormData(prev => ({ ...prev, [field]: value }));
  }, []);

  const handleRoleChange = (roleId: string, checked: boolean) => {
    setFormData(prev => ({
      ...prev,
      roleIds: checked 
        ? [...prev.roleIds, roleId]
        : prev.roleIds.filter(id => id !== roleId)
    }));
  };

  const UserForm = React.useMemo(() => (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-1">
            First Name *
          </label>
          <Input
            id="firstName"
            value={formData.firstName}
            onChange={handleFormChange('firstName')}
            placeholder="First name"
            required
          />
        </div>
        
        <div>
          <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-1">
            Last Name *
          </label>
          <Input
            id="lastName"
            value={formData.lastName}
            onChange={handleFormChange('lastName')}
            placeholder="Last name"
            required
          />
        </div>
      </div>
      
      <div>
        <label htmlFor="username" className="block text-sm font-medium text-gray-700 mb-1">
          Username *
        </label>
        <Input
          id="username"
          value={formData.username}
          onChange={handleFormChange('username')}
          placeholder="Username"
          required
        />
      </div>
      
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
          Email *
        </label>
        <Input
          id="email"
          type="email"
          value={formData.email}
          onChange={handleFormChange('email')}
          placeholder="Email address"
          required
        />
      </div>
      
      <div>
        <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
          Password {!selectedUser && '*'}
        </label>
        <Input
          id="password"
          type="password"
          value={formData.password}
          onChange={handleFormChange('password')}
          placeholder={selectedUser ? "Leave empty to keep current password" : "Password"}
          required={!selectedUser}
        />
      </div>
      
      <div>
        <label htmlFor="bio" className="block text-sm font-medium text-gray-700 mb-1">
          Bio
        </label>
        <textarea
          id="bio"
          value={formData.bio}
          onChange={handleFormChange('bio')}
          placeholder="User bio"
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>
      
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Roles *
        </label>
        <div className="space-y-2">
          {roles.map((role) => (
            <label key={role.id} className="flex items-center">
              <input
                type="checkbox"
                checked={formData.roleIds.includes(role.id)}
                onChange={(e) => handleRoleChange(role.id, e.target.checked)}
                className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
              />
              <span className="ml-2 text-sm text-gray-700">
                {RoleHelper.getRoleDisplay(role.name as UserRole)} - {role.description}
              </span>
            </label>
          ))}
        </div>
      </div>
    </div>
  ), [formData, formLoading, roles]);

  return (
    <div>
      <AdminTable
        data={users}
        columns={columns}
        loading={loading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        title="Users"
      />

      {/* Create Modal */}
      <AdminModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleSubmitCreate}
        title="Create User"
        loading={formLoading}
        size="lg"
      >
        {UserForm}
      </AdminModal>

      {/* Edit Modal */}
      <AdminModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedUser(null);
        }}
        onSubmit={handleSubmitEdit}
        title="Edit User"
        loading={formLoading}
        size="lg"
      >
        {UserForm}
      </AdminModal>

      {/* Delete Modal */}
      <DeleteConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setSelectedUser(null);
        }}
        onConfirm={handleConfirmDelete}
        title="User"
        message={`Are you sure you want to delete the user "${selectedUser?.username}"? This action cannot be undone.`}
        loading={formLoading}
      />
    </div>
  );
}