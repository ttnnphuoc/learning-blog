'use client';

import { useAuth } from '@/lib/auth-context';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { RoleHelper, UserRole } from '@/types';

interface AdminLayoutProps {
  children: React.ReactNode;
}

export default function AdminLayout({ children }: AdminLayoutProps) {
  const { isAuthenticated, user, loading, logout } = useAuth();
  const router = useRouter();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    if (!loading && mounted) {
      if (!isAuthenticated) {
        router.push('/login');
        return;
      }
      
      // Check if user has admin/moderator role
      const hasAdminAccess = RoleHelper.hasAdminAccess(user?.roles);
      
      if (!hasAdminAccess) {
        router.push('/');
        return;
      }
    }
  }, [isAuthenticated, user, loading, mounted, router]);

  const handleLogout = () => {
    logout();
    router.push('/');
  };

  if (loading || !mounted) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!isAuthenticated || !RoleHelper.hasAdminAccess(user?.roles)) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Admin Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <Link href="/admin" className="flex items-center">
                <div className="h-8 w-8 bg-blue-600 rounded-lg flex items-center justify-center">
                  <span className="text-white font-bold text-sm">A</span>
                </div>
                <span className="ml-2 text-xl font-semibold text-gray-900">Admin Dashboard</span>
              </Link>
            </div>
            
            <div className="flex items-center space-x-4">
              <Link 
                href="/" 
                className="text-gray-500 hover:text-gray-700 text-sm font-medium"
              >
                View Site
              </Link>
              <div className="flex items-center space-x-2">
                <span className="text-sm text-gray-700">
                  {user?.firstName} {user?.lastName}
                </span>
                <Button variant="outline" size="sm" onClick={handleLogout}>
                  Logout
                </Button>
              </div>
            </div>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar Navigation */}
        <nav className="w-64 bg-white shadow-sm h-screen sticky top-0">
          <div className="p-4">
            <div className="space-y-1">
              <NavLink href="/admin" icon="📊">Dashboard</NavLink>
              <NavLink href="/admin/posts" icon="📝">Posts</NavLink>
              <NavLink href="/admin/categories" icon="📁">Categories</NavLink>
              <NavLink href="/admin/tags" icon="🏷️">Tags</NavLink>
              <NavLink href="/admin/users" icon="👥">Users</NavLink>
              <NavLink href="/admin/roles" icon="🛡️">Roles</NavLink>
              <NavLink href="/admin/permissions" icon="🔑">Permissions</NavLink>
            </div>
          </div>
        </nav>

        {/* Main Content */}
        <main className="flex-1 p-6">
          {children}
        </main>
      </div>
    </div>
  );
}

interface NavLinkProps {
  href: string;
  icon: string;
  children: React.ReactNode;
}

function NavLink({ href, icon, children }: NavLinkProps) {
  const router = useRouter();
  const isActive = typeof window !== 'undefined' && window.location.pathname === href;

  return (
    <Link
      href={href}
      className={`flex items-center px-3 py-2 rounded-md text-sm font-medium transition-colors ${
        isActive
          ? 'bg-blue-100 text-blue-700'
          : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
      }`}
    >
      <span className="mr-3">{icon}</span>
      {children}
    </Link>
  );
}