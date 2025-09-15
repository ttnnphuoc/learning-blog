# Authentication & Authorization Workflow

This document provides visual workflows for the authentication and authorization system implemented in Phase 1.

## 🔐 User Registration Flow

```mermaid
flowchart TD
    A[User submits registration form] --> B{Validate input data}
    B -->|Invalid| C[Return validation errors]
    B -->|Valid| D{Check if user exists}
    D -->|User exists| E[Return 'User already exists']
    D -->|User doesn't exist| F[Hash password with BCrypt]
    F --> G[Create user in database]
    G --> H[Generate JWT access token]
    H --> I[Generate refresh token]
    I --> J[Store refresh token in database]
    J --> K[Return success with tokens and user data]
    
    style A fill:#e1f5fe
    style K fill:#c8e6c9
    style C fill:#ffcdd2
    style E fill:#ffcdd2
```

## 🔑 User Login Flow

```mermaid
flowchart TD
    A[User submits login credentials] --> B{Validate input}
    B -->|Invalid| C[Return validation errors]
    B -->|Valid| D[Find user by email]
    D -->|Not found| E[Return 'Invalid credentials']
    D -->|Found| F{Verify password with BCrypt}
    F -->|Invalid| G[Increment failed attempts]
    G --> H{Check lockout threshold}
    H -->|Exceeded| I[Lock account temporarily]
    H -->|Not exceeded| E
    F -->|Valid| J{Check if account active}
    J -->|Inactive| K[Return 'Account deactivated']
    J -->|Active| L[Update last login timestamp]
    L --> M[Generate JWT access token]
    M --> N[Generate refresh token]
    N --> O[Store refresh token in database]
    O --> P[Return success with tokens and user data]
    
    style A fill:#e1f5fe
    style P fill:#c8e6c9
    style C fill:#ffcdd2
    style E fill:#ffcdd2
    style I fill:#ffcdd2
    style K fill:#ffcdd2
```

## 🔄 Token Refresh Flow

```mermaid
flowchart TD
    A[Client sends refresh token] --> B{Validate refresh token}
    B -->|Invalid format| C[Return 'Invalid token']
    B -->|Valid format| D[Find token in database]
    D -->|Not found| E[Return 'Token not found']
    D -->|Found| F{Check if token revoked}
    F -->|Revoked| G[Return 'Token revoked']
    F -->|Not revoked| H{Check if token expired}
    H -->|Expired| I[Return 'Token expired']
    H -->|Valid| J[Find associated user]
    J -->|User not found| K[Return 'User not found']
    J -->|User found| L{Check if user active}
    L -->|Inactive| M[Return 'User inactive']
    L -->|Active| N[Revoke old refresh token]
    N --> O[Generate new JWT access token]
    O --> P[Generate new refresh token]
    P --> Q[Store new refresh token]
    Q --> R[Return new tokens and user data]
    
    style A fill:#e1f5fe
    style R fill:#c8e6c9
    style C fill:#ffcdd2
    style E fill:#ffcdd2
    style G fill:#ffcdd2
    style I fill:#ffcdd2
    style K fill:#ffcdd2
    style M fill:#ffcdd2
```

## 🛡️ API Request Authorization Flow

```mermaid
flowchart TD
    A[Client makes API request] --> B{Endpoint requires auth?}
    B -->|No| C[Process request normally]
    B -->|Yes| D{Authorization header present?}
    D -->|No| E[Return 401 Unauthorized]
    D -->|Yes| F[Extract JWT token]
    F --> G{Validate JWT token}
    G -->|Invalid signature| H[Return 401 Unauthorized]
    G -->|Expired| I[Return 401 Unauthorized]
    G -->|Valid| J[Extract user claims]
    J --> K{Resource-level authorization}
    K -->|Forbidden| L[Return 403 Forbidden]
    K -->|Allowed| M[Add user context to request]
    M --> N[Process request with user context]
    N --> O[Return response]
    
    style A fill:#e1f5fe
    style C fill:#c8e6c9
    style O fill:#c8e6c9
    style E fill:#ffcdd2
    style H fill:#ffcdd2
    style I fill:#ffcdd2
    style L fill:#fff3e0
```

## 📝 Post Creation Flow (With Authentication)

```mermaid
flowchart TD
    A[User creates post] --> B[Send POST /api/posts]
    B --> C{JWT token valid?}
    C -->|No| D[Return 401 Unauthorized]
    C -->|Yes| E[Extract user ID from token]
    E --> F[Set AuthorId to current user]
    F --> G[Validate post data]
    G -->|Invalid| H[Return 400 Bad Request]
    G -->|Valid| I[Save post to database]
    I --> J[Return created post]
    
    style A fill:#e1f5fe
    style J fill:#c8e6c9
    style D fill:#ffcdd2
    style H fill:#ffcdd2
```

## 🔧 Post Update Flow (With Ownership Check)

```mermaid
flowchart TD
    A[User updates post] --> B[Send PUT /api/posts/{id}]
    B --> C{JWT token valid?}
    C -->|No| D[Return 401 Unauthorized]
    C -->|Yes| E[Extract user ID from token]
    E --> F[Find post by ID]
    F -->|Not found| G[Return 404 Not Found]
    F -->|Found| H{User is author OR admin?}
    H -->|No| I[Return 403 Forbidden]
    H -->|Yes| J[Validate update data]
    J -->|Invalid| K[Return 400 Bad Request]
    J -->|Valid| L[Update post in database]
    L --> M[Return updated post]
    
    style A fill:#e1f5fe
    style M fill:#c8e6c9
    style D fill:#ffcdd2
    style G fill:#ffcdd2
    style I fill:#fff3e0
    style K fill:#ffcdd2
```

## 🔒 Complete Authentication Architecture Flow

```mermaid
graph TB
    subgraph "Client Layer"
        A[Web App/Mobile App]
        B[API Requests]
    end
    
    subgraph "Presentation Layer"
        C[AuthController]
        D[PostsController]
        E[JWT Middleware]
    end
    
    subgraph "Application Layer"
        F[AuthService]
        G[UserService]
        H[PostService]
    end
    
    subgraph "Infrastructure Layer"
        I[UserRepository]
        J[RefreshTokenRepository]
        K[PostRepository]
    end
    
    subgraph "Database"
        L[(Users)]
        M[(RefreshTokens)]
        N[(Posts)]
        O[(Roles)]
        P[(Permissions)]
    end
    
    A --> B
    B --> E
    E --> C
    E --> D
    C --> F
    C --> G
    D --> H
    F --> I
    F --> J
    G --> I
    H --> K
    I --> L
    J --> M
    K --> N
    I --> O
    I --> P
    
    style A fill:#e3f2fd
    style E fill:#fff3e0
    style F fill:#f3e5f5
    style G fill:#f3e5f5
    style H fill:#f3e5f5
    style L fill:#e8f5e8
    style M fill:#e8f5e8
    style N fill:#e8f5e8
```

## 🚀 JWT Token Structure

```mermaid
graph LR
    subgraph "JWT Token"
        A[Header] --> B[Payload] --> C[Signature]
    end
    
    subgraph "Header Content"
        D[{"alg": "HS256"<br/>"typ": "JWT"}]
    end
    
    subgraph "Payload Content"
        E[{"sub": "user-id"<br/>"email": "user@email.com"<br/>"name": "username"<br/>"roles": ["Author"]<br/>"exp": 1234567890<br/>"iss": "BlogAPI"<br/>"aud": "BlogAPI-Users"}]
    end
    
    subgraph "Signature"
        F[HMACSHA256(base64(header) + "." + base64(payload), secret)]
    end
    
    A --> D
    B --> E
    C --> F
```

## 📊 Database Relationships

```mermaid
erDiagram
    Users ||--o{ Posts : "authors"
    Users ||--o{ UserRoles : "has"
    Users ||--o{ RefreshTokens : "owns"
    Roles ||--o{ UserRoles : "assigned_to"
    Roles ||--o{ RolePermissions : "has"
    Permissions ||--o{ RolePermissions : "belongs_to"
    Categories ||--o{ Posts : "contains"
    Posts }o--o{ Tags : "tagged_with"
    
    Users {
        uuid id PK
        string username UK
        string email UK
        string password_hash
        string first_name
        string last_name
        boolean is_active
        datetime created_at
    }
    
    Posts {
        uuid id PK
        uuid author_id FK
        uuid category_id FK
        string title
        text content
        string slug UK
        boolean is_published
        datetime created_at
    }
    
    Roles {
        uuid id PK
        string name UK
        string description
        boolean is_system_role
    }
    
    RefreshTokens {
        uuid id PK
        uuid user_id FK
        string token UK
        datetime expires_at
        boolean is_revoked
    }
```

## 🔐 Security Layers

```mermaid
graph TD
    A[API Request] --> B[HTTPS Layer]
    B --> C[JWT Middleware]
    C --> D[Authentication Check]
    D --> E[Authorization Check]
    E --> F[Resource Access]
    
    subgraph "Security Validations"
        G[Token Signature]
        H[Token Expiration]
        I[User Active Status]
        J[Role Permissions]
        K[Resource Ownership]
    end
    
    D --> G
    D --> H
    D --> I
    E --> J
    E --> K
    
    style B fill:#ffebee
    style C fill:#e8f5e8
    style D fill:#e3f2fd
    style E fill:#fff3e0
    style F fill:#f3e5f5
```

## 🎯 Phase 2 Future Authorization Flow

```mermaid
flowchart TD
    A[User Action Request] --> B{Authenticated?}
    B -->|No| C[Return 401]
    B -->|Yes| D[Get User Roles]
    D --> E[Get Role Permissions]
    E --> F{Has Required Permission?}
    F -->|No| G[Return 403]
    F -->|Yes| H{Resource Owner Check}
    H -->|Not Owner & Not Admin| I[Return 403]
    H -->|Owner OR Admin| J[Process Request]
    
    subgraph "Permission Examples"
        K[posts.create]
        L[posts.update.own]
        M[posts.update.any]
        N[posts.delete.own]
        O[posts.delete.any]
        P[users.manage]
    end
    
    style A fill:#e1f5fe
    style J fill:#c8e6c9
    style C fill:#ffcdd2
    style G fill:#fff3e0
    style I fill:#fff3e0
```

---

## 🔧 Implementation Notes

### Current Phase 1 Status ✅
- **Authentication**: Fully implemented with JWT tokens
- **Basic Authorization**: Implemented for write operations
- **User Management**: Registration, login, token refresh
- **Security**: BCrypt password hashing, token validation

### Phase 2 Roadmap 🚧
- **Role-based Authorization**: Implement permission checks
- **Resource Ownership**: Detailed ownership validation
- **Admin Panel**: User and role management endpoints
- **Advanced Security**: Account lockout, audit logging

### API Endpoints Currently Available
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/revoke` - Token revocation
- `POST /api/auth/validate` - Token validation
- `GET /api/posts` - Public read access
- `POST /api/posts` - Authenticated write access
- `PUT /api/posts/{id}` - Authenticated update access
- `DELETE /api/posts/{id}` - Authenticated delete access