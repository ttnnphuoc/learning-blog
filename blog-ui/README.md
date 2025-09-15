# Blog UI - Next.js Frontend

A modern, SEO-optimized blog frontend built with Next.js 14, TypeScript, and Tailwind CSS. This project serves as the frontend for the BlogAPI ASP.NET Core backend.

## ✨ Features

- **🚀 Next.js 14** with App Router
- **📱 Responsive Design** with Tailwind CSS
- **🔍 SEO Optimized** with proper meta tags, sitemap, and robots.txt
- **📝 TypeScript** for type safety
- **🎨 Modern UI** with clean design
- **🔗 API Integration** ready for ASP.NET Core backend
- **📊 Performance Optimized** with Next.js optimizations

## 🚀 Quick Start

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Set up environment variables:**
   ```bash
   cp .env.local.example .env.local
   ```
   
   Update the API URL in `.env.local`:
   ```
   NEXT_PUBLIC_API_BASE_URL=https://localhost:7041
   NEXT_PUBLIC_SITE_URL=http://localhost:3000
   ```

3. **Run the development server:**
   ```bash
   npm run dev
   ```

4. **Open your browser:**
   Navigate to [http://localhost:3000](http://localhost:3000)

## 📁 Project Structure

```
src/
├── app/                    # Next.js App Router pages
│   ├── globals.css        # Global styles
│   ├── layout.tsx         # Root layout
│   ├── page.tsx          # Home page
│   ├── posts/            # Blog posts pages
│   ├── robots.ts         # Robots.txt generation
│   └── sitemap.ts        # Sitemap generation
├── components/            # Reusable components
│   └── ui/               # UI components
├── lib/                  # Utility libraries
│   ├── api.ts           # API client
│   └── metadata.ts      # SEO metadata helpers
└── types/               # TypeScript type definitions
    └── index.ts         # Main types
```

## 🎯 Key Pages

- **Home (`/`)** - Landing page with hero section and features
- **Posts (`/posts`)** - Blog posts listing
- **Post Detail (`/posts/[slug]`)** - Individual post pages
- **Categories (`/categories`)** - Category listings (planned)

## 🔧 API Integration

The project includes a fully configured API client (`src/lib/api.ts`) that connects to your ASP.NET Core backend:

```typescript
import { apiClient } from '@/lib/api';

// Get all posts
const posts = await apiClient.getPosts();

// Get single post
const post = await apiClient.getPost(1);

// Authentication
const authResult = await apiClient.login({ email, password });
```

## 🎨 Styling

- **Tailwind CSS** for utility-first styling
- **Responsive design** for all screen sizes
- **Dark mode ready** (add dark mode toggle if needed)
- **Custom color scheme** configurable in `tailwind.config.js`

## 📈 SEO Features

- **Automatic sitemap generation** (`/sitemap.xml`)
- **Robots.txt** (`/robots.txt`)
- **Open Graph tags** for social media sharing
- **Twitter Card support**
- **Structured metadata** for each page
- **Canonical URLs** for better indexing

## 🛠️ Development

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint

### Environment Variables

```bash
# API Configuration
NEXT_PUBLIC_API_BASE_URL=https://localhost:7041
API_BASE_URL=https://localhost:7041

# Site Configuration
NEXT_PUBLIC_SITE_URL=http://localhost:3000

# SEO (Optional)
NEXT_PUBLIC_GOOGLE_VERIFICATION=your-google-verification-code
```

## 🚀 Deployment

### Vercel (Recommended)

1. Push your code to GitHub
2. Connect your repository to Vercel
3. Set environment variables in Vercel dashboard
4. Deploy!

### Other Platforms

The project can be deployed to any platform that supports Node.js:
- Netlify
- Railway
- Digital Ocean
- AWS Amplify

## 🔗 Backend Integration

This frontend is designed to work with the BlogAPI ASP.NET Core backend. Make sure to:

1. **Configure CORS** in your ASP.NET Core API to allow requests from your frontend domain
2. **Update API endpoints** in `src/lib/api.ts` if they differ from the default
3. **Handle authentication** by storing JWT tokens properly

## 📝 TODO

- [ ] Add search functionality
- [ ] Implement comment system
- [ ] Add user authentication UI
- [ ] Create admin dashboard
- [ ] Add markdown support for post content
- [ ] Implement infinite scroll for posts
- [ ] Add social sharing buttons
- [ ] Create newsletter signup

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.