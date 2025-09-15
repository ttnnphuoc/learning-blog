# Super Admin Setup Documentation

## 🔐 Super Admin User Created

Your BlogAPI now has a super admin user that is automatically created when the application starts.

### **Super Admin Credentials**
```
Email: admin@blogapi.com
Username: admin
Password: Admin123!@#
Role: Admin (with all permissions)
```

⚠️ **IMPORTANT**: Change the password in production!

## 🏗️ What Was Created

### **1. Default Roles**
- **Admin**: Full system access (super admin role)
- **Author**: Can create and manage own posts
- **Moderator**: Can moderate posts and manage categories/tags
- **Reader**: Can read published content and update own profile

### **2. Permissions System**
Granular permissions for all resources:
- **Posts**: create, read, update.own, update.any, delete.own, delete.any, publish
- **Users**: create, read, update.own, update.any, delete, manage.roles
- **Categories**: create, update, delete
- **Tags**: create, update, delete

### **3. Permission Assignments**
- **Admin**: ALL permissions
- **Author**: posts.create, posts.read, posts.update.own, posts.delete.own, users.update.own
- **Moderator**: All post permissions + category/tag management
- **Reader**: posts.read, users.update.own

### **4. Database Fixes**
- Existing posts without authors are automatically assigned to the super admin
- All new posts require valid AuthorId

## 🚀 Using the Super Admin

### **Login API Call**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@blogapi.com",
    "password": "Admin123!@#",
    "rememberMe": false
  }'
```

### **Response**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "user": {
    "id": "user-guid",
    "username": "admin",
    "email": "admin@blogapi.com",
    "firstName": "Super",
    "lastName": "Admin",
    "isActive": true,
    "roles": [
      {
        "id": "role-guid",
        "name": "Admin",
        "description": "System administrator with full access"
      }
    ]
  },
  "expiresAt": "2025-09-16T05:09:00.000Z"
}
```

## 🛡️ Security Features

### **What's Protected Now**
- ✅ User registration and login
- ✅ JWT token authentication
- ✅ Protected POST/PUT/DELETE endpoints
- ✅ Password hashing with BCrypt
- ✅ Refresh token mechanism
- ✅ Role-based access (basic level)

### **What's Public**
- ✅ Reading published posts
- ✅ User registration (creates users without roles)
- ✅ Authentication endpoints

## 🔧 Configuration

### **JWT Settings** (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "super-secret-jwt-key-for-blog-api-authentication-at-least-32-characters",
    "Issuer": "BlogAPI",
    "Audience": "BlogAPI-Users",
    "AccessTokenExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 30
  }
}
```

### **Database Auto-Seeding**
The seeder runs automatically on application startup and:
- Creates default roles and permissions (if they don't exist)
- Creates super admin user (if it doesn't exist)
- Assigns admin role to super admin
- Updates orphaned posts to have admin as author

## 🧪 Testing

Run the provided test script to verify everything works:
```bash
./test-super-admin.sh
```

### **Manual Tests**
1. **Login as Super Admin**
2. **Create a new post** (should work)
3. **Access protected endpoints** (should work with token)
4. **Try without token** (should fail with 401)

## 🚧 Next Steps - Phase 2

### **Role Management Features**
- Create endpoints to assign/remove roles from users
- Add role-based authorization policies
- Implement resource ownership validation
- Add user management dashboard

### **Enhanced Security**
- Email verification for new users
- Password reset functionality
- Account lockout protection
- Audit logging

### **User Experience**
- Default role assignment for new users
- User profile management
- Password change functionality

## ⚙️ Seeder Components

### **Files Created**
- `src/BlogAPI.Infrastructure/Services/DatabaseSeeder.cs` - Main seeding logic
- `src/BlogAPI.Application/Interfaces/IDatabaseSeeder.cs` - Interface
- Updated `Program.cs` - Automatic seeding on startup

### **Seeding Process**
1. ✅ Create 4 default roles
2. ✅ Create 19 granular permissions
3. ✅ Assign permissions to roles
4. ✅ Create super admin user
5. ✅ Assign admin role to super admin
6. ✅ Fix existing posts without authors

## 🔑 Important Notes

### **Production Security**
- **Change the JWT secret key**
- **Change the super admin password**
- **Use environment variables for secrets**
- **Enable HTTPS in production**
- **Consider rate limiting**

### **Database Considerations**
- Seeder is idempotent (safe to run multiple times)
- System roles cannot be deleted (IsSystemRole = true)
- Existing data is preserved and fixed automatically

Your BlogAPI now has a complete authentication foundation ready for Phase 2 enhancements!