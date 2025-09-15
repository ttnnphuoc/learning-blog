'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useAuth } from '@/lib/auth-context';
import { RoleHelper, UserRole, type LoginDto } from '@/types';

export default function LoginPage() {
  const { login, loading, error } = useAuth();
  const router = useRouter();
  const [formData, setFormData] = useState<LoginDto>({
    email: '',
    password: '',
    rememberMe: false,
  });
  const [formError, setFormError] = useState<string>('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    if (!formData.email || !formData.password) {
      setFormError('Please fill in all required fields.');
      return;
    }

    const result = await login(formData);
    
    if (result.success) {
      // Check if user has admin or moderator role
      const hasAdminAccess = RoleHelper.hasAdminAccess(result.user?.roles);
      
      if (hasAdminAccess) {
        router.push('/admin');
      } else {
        router.push('/');
      }
    } else {
      setFormError(result.error || 'Login failed. Please try again.');
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  return (
    <div style={{minHeight: '100vh', backgroundColor: '#f9fafb', padding: '48px 16px'}}>
      <div style={{maxWidth: '448px', margin: '0 auto'}}>
        <div style={{backgroundColor: 'white', padding: '32px 16px', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)'}}>
          <div style={{textAlign: 'center', marginBottom: '32px'}}>
            <div style={{width: '48px', height: '48px', backgroundColor: '#dbeafe', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 16px'}}>
              <span style={{color: '#2563eb', fontSize: '24px', fontWeight: 'bold'}}>🔐</span>
            </div>
            <h2 style={{fontSize: '24px', fontWeight: 'bold', color: '#111827', marginBottom: '8px'}}>
              Sign in to your account
            </h2>
            <p style={{fontSize: '14px', color: '#6b7280'}}>
              Or{' '}
              <Link href="/register" style={{color: '#2563eb', textDecoration: 'underline'}}>
                create a new account
              </Link>
            </p>
          </div>
        
          <form className="space-y-6" onSubmit={handleSubmit}>
          {(formError || error) && (
            <div className="rounded-md bg-red-50 p-4">
              <div className="text-sm text-red-700">
                {formError || error}
              </div>
            </div>
          )}
          
          <div className="space-y-4">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700">
                Email address
              </label>
              <Input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                required
                value={formData.email}
                onChange={handleChange}
                className="mt-1"
                placeholder="Enter your email"
              />
            </div>
            
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700">
                Password
              </label>
              <Input
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                required
                value={formData.password}
                onChange={handleChange}
                className="mt-1"
                placeholder="Enter your password"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <input
                  id="rememberMe"
                  name="rememberMe"
                  type="checkbox"
                  checked={formData.rememberMe}
                  onChange={handleChange}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor="rememberMe" className="ml-2 block text-sm text-gray-900">
                  Remember me
                </label>
              </div>
              
              <div className="text-sm">
                <Link href="/forgot-password" className="font-medium text-blue-600 hover:text-blue-500">
                  Forgot your password?
                </Link>
              </div>
            </div>
          </div>

          <div>
            <Button
              type="submit"
              className="w-full"
              size="lg"
              disabled={loading}
            >
              {loading ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                  Signing in...
                </>
              ) : (
                'Sign in'
              )}
            </Button>
          </div>
          </form>
        </div>
      </div>
    </div>
  );
}