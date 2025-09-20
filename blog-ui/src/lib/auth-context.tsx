'use client';

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { apiClient, setAuthTokens, getAuthToken, getRefreshToken, clearAuthTokens } from './api';
import type { AuthState, User, LoginDto, RegisterDto, AuthResult } from '@/types';

interface AuthContextType extends AuthState {
  login: (credentials: LoginDto) => Promise<AuthResult>;
  register: (userData: RegisterDto) => Promise<AuthResult>;
  logout: () => void;
  refreshAuth: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [authState, setAuthState] = useState<AuthState>({
    isAuthenticated: false,
    user: null,
    token: null,
    refreshToken: null,
    loading: true,
    error: null,
  });

  const [mounted, setMounted] = useState(false);

  // Initialize auth state from localStorage
  useEffect(() => {
    setMounted(true);
    const initializeAuth = async () => {
      try {
        const token = getAuthToken();
        const refreshToken = getRefreshToken();

        if (token && refreshToken) {
          // Validate token with the server
          try {
            const validation = await apiClient.validateToken();
            if (validation.valid) {
              // Token is valid, get user info
              try {
                const user = await apiClient.getCurrentUser();
                setAuthState(prev => ({
                  ...prev,
                  isAuthenticated: true,
                  user,
                  token,
                  refreshToken,
                  loading: false,
                }));
              } catch (userError) {
                // Failed to get user info, clear tokens
                clearAuthTokens();
                setAuthState(prev => ({
                  ...prev,
                  loading: false,
                }));
              }
            } else {
              // Token invalid, clear everything
              clearAuthTokens();
              setAuthState(prev => ({
                ...prev,
                loading: false,
              }));
            }
          } catch (error) {
            // Validation failed, try to refresh
            const refreshed = await refreshAuth();
            if (!refreshed) {
              clearAuthTokens();
              setAuthState(prev => ({
                ...prev,
                loading: false,
              }));
            }
          }
        } else {
          setAuthState(prev => ({
            ...prev,
            loading: false,
          }));
        }
      } catch (error) {
        console.error('Auth initialization failed:', error);
        setAuthState(prev => ({
          ...prev,
          loading: false,
          error: error instanceof Error ? error.message : 'Authentication initialization failed',
        }));
      }
    };

    initializeAuth();
  }, []);

  const login = async (credentials: LoginDto): Promise<AuthResult> => {
    setAuthState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const result = await apiClient.login(credentials);

      if (result.success && result.accessToken && result.refreshToken) {
        setAuthTokens(result.accessToken, result.refreshToken);
        setAuthState(prev => ({
          ...prev,
          isAuthenticated: true,
          user: result.user || null,
          token: result.accessToken!,
          refreshToken: result.refreshToken!,
          loading: false,
          error: null,
        }));
      } else {
        setAuthState(prev => ({
          ...prev,
          loading: false,
          error: result.error || 'Login failed',
        }));
      }

      return result;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Login failed';
      setAuthState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage,
      }));

      return {
        success: false,
        error: errorMessage,
      };
    }
  };

  const register = async (userData: RegisterDto): Promise<AuthResult> => {
    setAuthState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const result = await apiClient.register(userData);

      if (result.success && result.accessToken && result.refreshToken) {
        setAuthTokens(result.accessToken, result.refreshToken);
        setAuthState(prev => ({
          ...prev,
          isAuthenticated: true,
          user: result.user || null,
          token: result.accessToken!,
          refreshToken: result.refreshToken!,
          loading: false,
          error: null,
        }));
      } else {
        setAuthState(prev => ({
          ...prev,
          loading: false,
          error: result.error || 'Registration failed',
        }));
      }

      return result;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Registration failed';
      setAuthState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage,
      }));

      return {
        success: false,
        error: errorMessage,
      };
    }
  };

  const logout = async () => {
    setAuthState(prev => ({ ...prev, loading: true }));

    try {
      // Revoke the refresh token on the server
      await apiClient.revokeToken();
    } catch (error) {
      console.error('Token revocation failed:', error);
    }

    // Clear local state regardless of server response
    clearAuthTokens();
    setAuthState({
      isAuthenticated: false,
      user: null,
      token: null,
      refreshToken: null,
      loading: false,
      error: null,
    });
  };

  const refreshAuth = async (): Promise<boolean> => {
    try {
      const result = await apiClient.refreshToken();

      if (result.success && result.accessToken && result.refreshToken) {
        setAuthTokens(result.accessToken, result.refreshToken);
        
        // Try to get current user data
        let user = result.user;
        if (!user) {
          try {
            user = await apiClient.getCurrentUser();
          } catch (userError) {
            console.error('Failed to get user data after refresh:', userError);
          }
        }
        
        setAuthState(prev => ({
          ...prev,
          isAuthenticated: true,
          user: user || prev.user,
          token: result.accessToken!,
          refreshToken: result.refreshToken!,
          loading: false,
          error: null,
        }));
        return true;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
    }

    return false;
  };

  const contextValue: AuthContextType = {
    ...authState,
    login,
    register,
    logout,
    refreshAuth,
  };

  // Prevent hydration issues by not rendering until mounted
  if (!mounted) {
    return (
      <AuthContext.Provider value={{
        ...authState,
        login,
        register,
        logout,
        refreshAuth,
      }}>
        {children}
      </AuthContext.Provider>
    );
  }

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default AuthContext;