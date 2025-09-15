'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { runApiTests, testApiConnection, testAuthEndpoints } from '@/lib/test-api';

interface TestResult {
  success: boolean;
  message: string;
  error?: string;
  postsCount?: number;
}

export default function TestPage() {
  const [connectionResult, setConnectionResult] = useState<TestResult | null>(null);
  const [authResult, setAuthResult] = useState<TestResult | null>(null);
  const [allTestsResult, setAllTestsResult] = useState<any>(null);
  const [loading, setLoading] = useState(false);

  const handleTestConnection = async () => {
    setLoading(true);
    try {
      const result = await testApiConnection();
      setConnectionResult(result);
    } finally {
      setLoading(false);
    }
  };

  const handleTestAuth = async () => {
    setLoading(true);
    try {
      const result = await testAuthEndpoints();
      setAuthResult(result);
    } finally {
      setLoading(false);
    }
  };

  const handleRunAllTests = async () => {
    setLoading(true);
    try {
      const result = await runApiTests();
      setAllTestsResult(result);
      setConnectionResult(result.connection);
      setAuthResult(result.auth);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-4xl mx-auto">
        <div className="bg-white shadow-sm rounded-lg p-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-8">API Connection Test</h1>
          
          <div className="space-y-6">
            {/* Test Buttons */}
            <div className="flex flex-wrap gap-4">
              <Button 
                onClick={handleTestConnection}
                disabled={loading}
                variant="outline"
              >
                {loading ? 'Testing...' : 'Test API Connection'}
              </Button>
              
              <Button 
                onClick={handleTestAuth}
                disabled={loading}
                variant="outline"
              >
                {loading ? 'Testing...' : 'Test Auth Endpoints'}
              </Button>
              
              <Button 
                onClick={handleRunAllTests}
                disabled={loading}
              >
                {loading ? 'Running Tests...' : 'Run All Tests'}
              </Button>
            </div>

            {/* Environment Info */}
            <div className="bg-gray-100 p-4 rounded-lg">
              <h3 className="font-semibold text-gray-900 mb-2">Configuration</h3>
              <div className="text-sm text-gray-600 space-y-1">
                <div>API Base URL: {process.env.NEXT_PUBLIC_API_BASE_URL || 'Not set'}</div>
                <div>Site URL: {process.env.NEXT_PUBLIC_SITE_URL || 'Not set'}</div>
                <div>Environment: {process.env.NODE_ENV}</div>
              </div>
            </div>

            {/* Connection Test Result */}
            {connectionResult && (
              <div className={`p-4 rounded-lg border ${
                connectionResult.success 
                  ? 'bg-green-50 border-green-200' 
                  : 'bg-red-50 border-red-200'
              }`}>
                <h3 className={`font-semibold mb-2 ${
                  connectionResult.success ? 'text-green-900' : 'text-red-900'
                }`}>
                  API Connection Test {connectionResult.success ? '✅' : '❌'}
                </h3>
                <p className={`text-sm ${
                  connectionResult.success ? 'text-green-700' : 'text-red-700'
                }`}>
                  {connectionResult.message}
                </p>
                {connectionResult.postsCount !== undefined && (
                  <p className="text-sm text-green-700">
                    Found {connectionResult.postsCount} posts
                  </p>
                )}
                {connectionResult.error && (
                  <p className="text-sm text-red-700 mt-1">
                    Error: {connectionResult.error}
                  </p>
                )}
              </div>
            )}

            {/* Auth Test Result */}
            {authResult && (
              <div className={`p-4 rounded-lg border ${
                authResult.success 
                  ? 'bg-green-50 border-green-200' 
                  : 'bg-red-50 border-red-200'
              }`}>
                <h3 className={`font-semibold mb-2 ${
                  authResult.success ? 'text-green-900' : 'text-red-900'
                }`}>
                  Authentication Test {authResult.success ? '✅' : '❌'}
                </h3>
                <p className={`text-sm ${
                  authResult.success ? 'text-green-700' : 'text-red-700'
                }`}>
                  {authResult.message}
                </p>
                {authResult.error && (
                  <p className="text-sm text-red-700 mt-1">
                    Error: {authResult.error}
                  </p>
                )}
              </div>
            )}

            {/* All Tests Summary */}
            {allTestsResult && (
              <div className={`p-4 rounded-lg border ${
                allTestsResult.allPassed 
                  ? 'bg-blue-50 border-blue-200' 
                  : 'bg-yellow-50 border-yellow-200'
              }`}>
                <h3 className={`font-semibold mb-2 ${
                  allTestsResult.allPassed ? 'text-blue-900' : 'text-yellow-900'
                }`}>
                  Overall Test Results
                </h3>
                <div className="text-sm space-y-1">
                  <div className={allTestsResult.connection.success ? 'text-green-700' : 'text-red-700'}>
                    API Connection: {allTestsResult.connection.success ? 'Pass' : 'Fail'}
                  </div>
                  <div className={allTestsResult.auth.success ? 'text-green-700' : 'text-red-700'}>
                    Auth Endpoints: {allTestsResult.auth.success ? 'Pass' : 'Fail'}
                  </div>
                </div>
                <p className={`mt-2 text-sm font-medium ${
                  allTestsResult.allPassed ? 'text-blue-700' : 'text-yellow-700'
                }`}>
                  {allTestsResult.allPassed 
                    ? '🎉 All tests passed! Your frontend is ready to connect to the backend.' 
                    : '⚠️ Some tests failed. Check your backend API and configuration.'
                  }
                </p>
              </div>
            )}

            {/* Instructions */}
            <div className="bg-blue-50 p-4 rounded-lg">
              <h3 className="font-semibold text-blue-900 mb-2">Instructions</h3>
              <div className="text-sm text-blue-700 space-y-2">
                <p>1. Make sure your BlogAPI backend is running on https://localhost:7041</p>
                <p>2. The backend should have CORS configured to allow requests from http://localhost:3000</p>
                <p>3. Test the API connection to ensure posts endpoint is accessible</p>
                <p>4. Test authentication endpoints to verify they respond correctly</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}