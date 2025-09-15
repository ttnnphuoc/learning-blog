import { UserRole } from './enums';

// Export enums
export * from './enums';

// Base types
export interface BaseEntity {
  id: string;
  createdAt: string;
  updatedAt: string;
}

// User types
export interface User extends BaseEntity {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  bio?: string;
  isActive: boolean;
  roles: Role[];
}

export interface Role extends BaseEntity {
  name: UserRole;
  description: string;
}

// Authentication types
export interface LoginDto {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export interface AuthResult {
  success: boolean;
  accessToken?: string;
  refreshToken?: string;
  user?: User;
  expiresAt?: string;
  error?: string;
}

export interface RefreshTokenDto {
  refreshToken: string;
}

export interface ForgotPasswordDto {
  email: string;
}

export interface ResetPasswordDto {
  token: string;
  email: string;
  password: string;
  confirmPassword: string;
}

// Post types
export interface Post extends BaseEntity {
  title: string;
  content: string;
  summary?: string;
  slug: string;
  imageUrl?: string;
  isPublished: boolean;
  publishedAt?: string;
  viewCount?: number;
  authorId: string;
  author: User;
  categories: Category[];
  tags: Tag[];
}

export interface CreatePostDto {
  title: string;
  content: string;
  summary?: string;
  slug?: string;
  imageUrl?: string;
  isPublished: boolean;
  categoryIds: string[];
  tagIds: string[];
}

export interface UpdatePostDto extends Partial<CreatePostDto> {
  id: string;
}

export interface PostDto extends Post {}

// Category types
export interface Category extends BaseEntity {
  name: string;
  slug: string;
  description?: string;
  postCount?: number;
}

export interface CategoryDto extends Category {}

// Tag types
export interface Tag extends BaseEntity {
  name: string;
  slug: string;
  postCount?: number;
}

export interface TagDto extends Tag {}

// API Response types
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  pagination: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalItems: number;
    hasNext: boolean;
    hasPrevious: boolean;
  };
}

// Query parameters
export interface PostQueryParams {
  publishedOnly?: boolean;
  categoryId?: string;
  tagId?: string;
  authorId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
}

// UI State types
export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  loading: boolean;
  error: string | null;
}

export interface PostsState {
  posts: Post[];
  currentPost: Post | null;
  loading: boolean;
  error: string | null;
  hasMore: boolean;
}