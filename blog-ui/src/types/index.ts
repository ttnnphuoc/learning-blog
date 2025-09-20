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
  permissions?: Permission[];
}

// Permission types
export interface Permission extends BaseEntity {
  name: string;
  description: string;
  category: string; // e.g., 'User Management', 'Post Management', etc.
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

export interface CreateCategoryDto {
  name: string;
  description?: string;
  slug?: string;
}

export interface UpdateCategoryDto extends Partial<CreateCategoryDto> {
  id: string;
}

export interface CategoryDto extends Category {}

// Tag types
export interface Tag extends BaseEntity {
  name: string;
  slug: string;
  postCount?: number;
}

export interface CreateTagDto {
  name: string;
  slug?: string;
}

export interface UpdateTagDto extends Partial<CreateTagDto> {
  id: string;
}

export interface TagDto extends Tag {}

// User DTOs
export interface CreateUserDto {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  bio?: string;
  roleIds: string[];
}

export interface UpdateUserDto extends Partial<Omit<CreateUserDto, 'password'>> {
  id: string;
  password?: string;
}

// Role DTOs
export interface CreateRoleDto {
  name: string;
  description: string;
  permissionIds?: string[];
}

export interface UpdateRoleDto extends Partial<CreateRoleDto> {
  id: string;
}

// Permission DTOs
export interface CreatePermissionDto {
  name: string;
  description: string;
  category: string;
}

export interface UpdatePermissionDto extends Partial<CreatePermissionDto> {
  id: string;
}

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