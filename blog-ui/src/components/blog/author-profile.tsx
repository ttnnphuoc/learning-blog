import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { User } from '@/types';

interface AuthorProfileProps {
  author: User;
  postCount?: number;
  showBio?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

export function AuthorProfile({ 
  author, 
  postCount, 
  showBio = true, 
  size = 'md' 
}: AuthorProfileProps) {
  const avatarSizes = {
    sm: 'h-8 w-8',
    md: 'h-12 w-12',
    lg: 'h-16 w-16'
  };

  const textSizes = {
    sm: 'text-sm',
    md: 'text-base',
    lg: 'text-lg'
  };

  return (
    <div className="flex items-start space-x-4">
      <Avatar className={avatarSizes[size]}>
        <AvatarImage
          src={author.avatarUrl || `https://ui-avatars.com/api/?name=${author.firstName}+${author.lastName}&background=3B82F6&color=fff`}
          alt={`${author.firstName} ${author.lastName}`}
        />
        <AvatarFallback>
          {author.firstName?.[0]}{author.lastName?.[0]}
        </AvatarFallback>
      </Avatar>
      
      <div className="flex-1 min-w-0">
        <div className="flex items-center space-x-2">
          <h3 className={`font-semibold text-gray-900 ${textSizes[size]}`}>
            {author.firstName} {author.lastName}
          </h3>
          {author.roles?.length > 0 && (
            <Badge variant="secondary" className="text-xs">
              {author.roles[0].name}
            </Badge>
          )}
        </div>
        
        <p className="text-sm text-gray-600">@{author.username}</p>
        
        {postCount !== undefined && (
          <p className="text-xs text-gray-500 mt-1">
            {postCount} {postCount === 1 ? 'post' : 'posts'}
          </p>
        )}
        
        {showBio && author.bio && size !== 'sm' && (
          <p className="text-sm text-gray-700 mt-2 line-clamp-2">
            {author.bio}
          </p>
        )}
        
        {size === 'lg' && (
          <div className="mt-4 flex space-x-2">
            <Button variant="outline" size="sm">
              Follow
            </Button>
            <Button variant="ghost" size="sm">
              View Profile
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}