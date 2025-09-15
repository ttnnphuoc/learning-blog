// User Roles Enum
export enum UserRole {
  ADMIN = 'Admin',
  MODERATOR = 'Moderator', 
  AUTHOR = 'Author',
  READER = 'Reader'
}

// Helper functions for role checking
export class RoleHelper {
  /**
   * Check if user has admin access (Admin or Moderator)
   */
  static hasAdminAccess(roles: { name: string }[] = []): boolean {
    return roles.some(role => 
      role.name === UserRole.ADMIN || role.name === UserRole.MODERATOR
    );
  }

  /**
   * Check if user can create content (Admin, Moderator, or Author)
   */
  static canCreateContent(roles: { name: string }[] = []): boolean {
    return roles.some(role => 
      role.name === UserRole.ADMIN || 
      role.name === UserRole.MODERATOR || 
      role.name === UserRole.AUTHOR
    );
  }

  /**
   * Check if user has specific role
   */
  static hasRole(roles: { name: string }[] = [], role: UserRole): boolean {
    return roles.some(r => r.name === role);
  }

  /**
   * Check if user is admin
   */
  static isAdmin(roles: { name: string }[] = []): boolean {
    return this.hasRole(roles, UserRole.ADMIN);
  }

  /**
   * Check if user is moderator
   */
  static isModerator(roles: { name: string }[] = []): boolean {
    return this.hasRole(roles, UserRole.MODERATOR);
  }

  /**
   * Check if user is author
   */
  static isAuthor(roles: { name: string }[] = []): boolean {
    return this.hasRole(roles, UserRole.AUTHOR);
  }

  /**
   * Check if user is reader
   */
  static isReader(roles: { name: string }[] = []): boolean {
    return this.hasRole(roles, UserRole.READER);
  }

  /**
   * Get user's highest role priority
   */
  static getHighestRole(roles: { name: string }[] = []): UserRole | null {
    if (this.isAdmin(roles)) return UserRole.ADMIN;
    if (this.isModerator(roles)) return UserRole.MODERATOR;
    if (this.isAuthor(roles)) return UserRole.AUTHOR;
    if (this.isReader(roles)) return UserRole.READER;
    return null;
  }

  /**
   * Get all available roles
   */
  static getAllRoles(): UserRole[] {
    return [UserRole.ADMIN, UserRole.MODERATOR, UserRole.AUTHOR, UserRole.READER];
  }

  /**
   * Get role display name with emoji
   */
  static getRoleDisplay(role: UserRole): string {
    switch (role) {
      case UserRole.ADMIN:
        return '👑 Admin';
      case UserRole.MODERATOR:
        return '🛡️ Moderator';
      case UserRole.AUTHOR:
        return '✍️ Author';
      case UserRole.READER:
        return '👁️ Reader';
      default:
        return role;
    }
  }

  /**
   * Get role color class for UI
   */
  static getRoleColor(role: UserRole): string {
    switch (role) {
      case UserRole.ADMIN:
        return 'bg-red-100 text-red-800';
      case UserRole.MODERATOR:
        return 'bg-blue-100 text-blue-800';
      case UserRole.AUTHOR:
        return 'bg-green-100 text-green-800';
      case UserRole.READER:
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }
}