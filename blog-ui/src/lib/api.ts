import type {
  Post,
  PostDto,
  CreatePostDto,
  UpdatePostDto,
  PostQueryParams,
  Category,
  CategoryDto,
  Tag,
  TagDto,
  LoginDto,
  RegisterDto,
  AuthResult,
  RefreshTokenDto,
  ForgotPasswordDto,
  ResetPasswordDto,
  User,
  ApiResponse,
  PaginatedResponse
} from '@/types';

// API Configuration
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'https://localhost:7041';

class ApiError extends Error {
  constructor(
    message: string,
    public status?: number,
    public response?: any
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

// Token management
let authToken: string | null = null;
let refreshToken: string | null = null;

export const setAuthTokens = (accessToken: string, refresh: string) => {
  authToken = accessToken;
  refreshToken = refresh;
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refresh);
};

export const getAuthToken = (): string | null => {
  if (authToken) return authToken;
  if (typeof window !== 'undefined') {
    authToken = localStorage.getItem('accessToken');
  }
  return authToken;
};

export const getRefreshToken = (): string | null => {
  if (refreshToken) return refreshToken;
  if (typeof window !== 'undefined') {
    refreshToken = localStorage.getItem('refreshToken');
  }
  return refreshToken;
};

export const clearAuthTokens = () => {
  authToken = null;
  refreshToken = null;
  if (typeof window !== 'undefined') {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }
};

// HTTP client
class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private async request<T = any>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    
    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    };

    // Add authentication header if token exists
    const token = getAuthToken();
    if (token) {
      (config.headers as Record<string, string>)['Authorization'] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(url, config);

      // Handle 401 - try to refresh token
      if (response.status === 401 && token) {
        const refreshed = await this.refreshTokens();
        if (refreshed) {
          // Retry the original request with new token
          (config.headers as Record<string, string>)['Authorization'] = `Bearer ${getAuthToken()}`;
          const retryResponse = await fetch(url, config);
          if (!retryResponse.ok) {
            throw new ApiError(`HTTP ${retryResponse.status}`, retryResponse.status);
          }
          return await retryResponse.json();
        } else {
          // Refresh failed, clear tokens and redirect to login
          clearAuthTokens();
          if (typeof window !== 'undefined') {
            window.location.href = '/login';
          }
          throw new ApiError('Authentication failed', 401);
        }
      }

      if (!response.ok) {
        const errorText = await response.text();
        let errorMessage = `HTTP ${response.status}`;
        
        try {
          const errorJson = JSON.parse(errorText);
          errorMessage = errorJson.error || errorJson.message || errorMessage;
        } catch {
          errorMessage = errorText || errorMessage;
        }

        throw new ApiError(errorMessage, response.status);
      }

      // Handle empty responses (like 204 No Content)
      if (response.status === 204) {
        return {} as T;
      }

      return await response.json();
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      throw new ApiError(`Network error: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  }

  private async refreshTokens(): Promise<boolean> {
    const refresh = getRefreshToken();
    if (!refresh) return false;

    try {
      const result = await this.request<AuthResult>('/api/auth/refresh', {
        method: 'POST',
        body: JSON.stringify({ refreshToken: refresh }),
      });

      if (result.success && result.accessToken && result.refreshToken) {
        setAuthTokens(result.accessToken, result.refreshToken);
        return true;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
    }

    return false;
  }

  // Authentication endpoints
  async login(credentials: LoginDto): Promise<AuthResult> {
    return this.request<AuthResult>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });
  }

  async register(userData: RegisterDto): Promise<AuthResult> {
    return this.request<AuthResult>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(userData),
    });
  }

  async refreshToken(): Promise<AuthResult> {
    const refresh = getRefreshToken();
    if (!refresh) throw new ApiError('No refresh token available');

    return this.request<AuthResult>('/api/auth/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken: refresh }),
    });
  }

  async revokeToken(): Promise<void> {
    const refresh = getRefreshToken();
    if (!refresh) return;

    await this.request<void>('/api/auth/revoke', {
      method: 'POST',
      body: JSON.stringify({ refreshToken: refresh }),
    });
  }

  async validateToken(): Promise<{ valid: boolean; userId?: string; username?: string }> {
    return this.request<{ valid: boolean; userId?: string; username?: string }>('/api/auth/validate', {
      method: 'POST',
    });
  }

  async forgotPassword(email: string): Promise<{ success: boolean; message: string }> {
    return this.request<{ success: boolean; message: string }>('/api/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify({ email }),
    });
  }

  async resetPassword(resetData: ResetPasswordDto): Promise<{ success: boolean; message: string }> {
    return this.request<{ success: boolean; message: string }>('/api/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify(resetData),
    });
  }

  // Posts endpoints
  async getPosts(params: PostQueryParams = {}): Promise<PostDto[]> {
    const queryParams = new URLSearchParams();
    
    if (params.publishedOnly !== undefined) {
      queryParams.append('publishedOnly', params.publishedOnly.toString());
    }
    if (params.page) queryParams.append('page', params.page.toString());
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params.search) queryParams.append('search', params.search);

    const queryString = queryParams.toString();
    return this.request<PostDto[]>(`/api/posts${queryString ? `?${queryString}` : ''}`);
  }

  async getPost(id: string): Promise<PostDto> {
    return this.request<PostDto>(`/api/posts/${id}`);
  }

  async getPostBySlug(slug: string): Promise<PostDto> {
    return this.request<PostDto>(`/api/posts/slug/${slug}`);
  }

  async getPostsByCategory(categoryId: string): Promise<PostDto[]> {
    return this.request<PostDto[]>(`/api/posts/category/${categoryId}`);
  }

  async getPostsByTag(tagId: string): Promise<PostDto[]> {
    return this.request<PostDto[]>(`/api/posts/tag/${tagId}`);
  }

  async createPost(postData: CreatePostDto): Promise<PostDto> {
    return this.request<PostDto>('/api/posts', {
      method: 'POST',
      body: JSON.stringify(postData),
    });
  }

  async updatePost(id: string, postData: UpdatePostDto): Promise<PostDto> {
    return this.request<PostDto>(`/api/posts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(postData),
    });
  }

  async deletePost(id: string): Promise<void> {
    await this.request<void>(`/api/posts/${id}`, {
      method: 'DELETE',
    });
  }

  // Categories endpoints (assuming they exist)
  async getCategories(): Promise<CategoryDto[]> {
    return this.request<CategoryDto[]>('/api/categories');
  }

  async getCategory(id: string): Promise<CategoryDto> {
    return this.request<CategoryDto>(`/api/categories/${id}`);
  }

  // Tags endpoints (assuming they exist)
  async getTags(): Promise<TagDto[]> {
    return this.request<TagDto[]>('/api/tags');
  }

  async getTag(id: string): Promise<TagDto> {
    return this.request<TagDto>(`/api/tags/${id}`);
  }
}

// Create and export a singleton instance
export const apiClient = new ApiClient();

// Export utility functions
export { ApiError };
export default apiClient;