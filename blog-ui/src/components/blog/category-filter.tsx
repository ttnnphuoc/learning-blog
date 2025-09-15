'use client';

import { useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Category } from '@/types';

interface CategoryFilterProps {
  categories: Category[];
  selectedCategories: string[];
  onCategoryChange: (categoryIds: string[]) => void;
}

export function CategoryFilter({ categories, selectedCategories, onCategoryChange }: CategoryFilterProps) {
  const [showAll, setShowAll] = useState(false);
  const displayCategories = showAll ? categories : categories.slice(0, 6);

  const toggleCategory = (categoryId: string) => {
    const newSelected = selectedCategories.includes(categoryId)
      ? selectedCategories.filter(id => id !== categoryId)
      : [...selectedCategories, categoryId];
    
    onCategoryChange(newSelected);
  };

  const clearAll = () => {
    onCategoryChange([]);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">Categories</h3>
        {selectedCategories.length > 0 && (
          <Button variant="ghost" size="sm" onClick={clearAll}>
            Clear all
          </Button>
        )}
      </div>
      
      <div className="flex flex-wrap gap-2">
        {displayCategories.map((category) => {
          const isSelected = selectedCategories.includes(category.id.toString());
          return (
            <button
              key={category.id}
              onClick={() => toggleCategory(category.id.toString())}
              className="transition-transform hover:scale-105"
            >
              <Badge
                variant={isSelected ? "default" : "outline"}
                className="cursor-pointer hover:shadow-sm"
              >
                {category.name}
              </Badge>
            </button>
          );
        })}
        
        {categories.length > 6 && (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowAll(!showAll)}
            className="h-auto p-1 text-xs"
          >
            {showAll ? 'Show less' : `+${categories.length - 6} more`}
          </Button>
        )}
      </div>
      
      {selectedCategories.length > 0 && (
        <div className="text-sm text-gray-600">
          {selectedCategories.length} {selectedCategories.length === 1 ? 'category' : 'categories'} selected
        </div>
      )}
    </div>
  );
}