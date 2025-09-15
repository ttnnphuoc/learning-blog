'use client';

import { useEffect, useState } from 'react';
import { Button } from '@/components/ui/button';
import Link from 'next/link';

interface DashboardStats {
  totalPosts: number;
  publishedPosts: number;
  draftPosts: number;
  totalUsers: number;
  totalCategories: number;
  totalTags: number;
}

export default function AdminDashboard() {
  const [stats, setStats] = useState<DashboardStats>({
    totalPosts: 0,
    publishedPosts: 0,
    draftPosts: 0,
    totalUsers: 0,
    totalCategories: 0,
    totalTags: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // TODO: Fetch real stats from API
    // For now, using mock data
    setTimeout(() => {
      setStats({
        totalPosts: 12,
        publishedPosts: 8,
        draftPosts: 4,
        totalUsers: 25,
        totalCategories: 6,
        totalTags: 15,
      });
      setLoading(false);
    }, 1000);
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
        <p className="mt-2 text-gray-600">Welcome to your admin dashboard. Manage your blog content and settings.</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 mb-8">
        <StatCard
          title="Total Posts"
          value={stats.totalPosts}
          subtitle={`${stats.publishedPosts} published, ${stats.draftPosts} drafts`}
          icon="📝"
          color="blue"
          href="/admin/posts"
        />
        <StatCard
          title="Users"
          value={stats.totalUsers}
          subtitle="Registered users"
          icon="👥"
          color="green"
          href="/admin/users"
        />
        <StatCard
          title="Categories"
          value={stats.totalCategories}
          subtitle="Content categories"
          icon="📁"
          color="purple"
          href="/admin/categories"
        />
        <StatCard
          title="Tags"
          value={stats.totalTags}
          subtitle="Content tags"
          icon="🏷️"
          color="yellow"
          href="/admin/tags"
        />
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-8">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <Button asChild className="h-auto p-4 flex flex-col items-center">
            <Link href="/admin/posts/new">
              <span className="text-2xl mb-2">📝</span>
              <span>Create New Post</span>
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-auto p-4 flex flex-col items-center">
            <Link href="/admin/categories">
              <span className="text-2xl mb-2">📁</span>
              <span>Manage Categories</span>
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-auto p-4 flex flex-col items-center">
            <Link href="/admin/users">
              <span className="text-2xl mb-2">👥</span>
              <span>Manage Users</span>
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-auto p-4 flex flex-col items-center">
            <Link href="/admin/roles">
              <span className="text-2xl mb-2">🛡️</span>
              <span>Manage Roles</span>
            </Link>
          </Button>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Recent Activity</h2>
        <div className="space-y-4">
          <ActivityItem
            action="Post created"
            item="How to Build a Modern Blog with Next.js"
            time="2 hours ago"
            icon="📝"
          />
          <ActivityItem
            action="User registered"
            item="john.doe@example.com"
            time="4 hours ago"
            icon="👤"
          />
          <ActivityItem
            action="Category created"
            item="Web Development"
            time="1 day ago"
            icon="📁"
          />
          <ActivityItem
            action="Post published"
            item="Understanding Clean Architecture"
            time="2 days ago"
            icon="✅"
          />
        </div>
      </div>
    </div>
  );
}

interface StatCardProps {
  title: string;
  value: number;
  subtitle: string;
  icon: string;
  color: 'blue' | 'green' | 'purple' | 'yellow';
  href: string;
}

function StatCard({ title, value, subtitle, icon, color, href }: StatCardProps) {
  const colorClasses = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    yellow: 'bg-yellow-50 text-yellow-600',
  };

  return (
    <Link href={href} className="block">
      <div className="bg-white rounded-lg shadow-sm p-6 hover:shadow-md transition-shadow">
        <div className="flex items-center">
          <div className={`p-3 rounded-lg ${colorClasses[color]}`}>
            <span className="text-2xl">{icon}</span>
          </div>
          <div className="ml-4">
            <p className="text-sm font-medium text-gray-600">{title}</p>
            <p className="text-2xl font-semibold text-gray-900">{value}</p>
            <p className="text-sm text-gray-500">{subtitle}</p>
          </div>
        </div>
      </div>
    </Link>
  );
}

interface ActivityItemProps {
  action: string;
  item: string;
  time: string;
  icon: string;
}

function ActivityItem({ action, item, time, icon }: ActivityItemProps) {
  return (
    <div className="flex items-center space-x-3">
      <div className="flex-shrink-0">
        <span className="text-lg">{icon}</span>
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-sm text-gray-900">
          <span className="font-medium">{action}:</span> {item}
        </p>
        <p className="text-sm text-gray-500">{time}</p>
      </div>
    </div>
  );
}