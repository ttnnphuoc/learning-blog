using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlogAPI.Application.Common.Utils;

/// <summary>
/// Utility class for generating URL-friendly slugs from text
/// </summary>
public static class SlugGenerator
{
    /// <summary>
    /// Generates a URL-friendly slug from the given text
    /// </summary>
    /// <param name="text">The text to convert to a slug</param>
    /// <returns>A URL-friendly slug</returns>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text
            .ToLowerInvariant()
            .Replace(" ", "-")
            // Turkish characters
            .Replace("ş", "s")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            // Vietnamese characters
            .Replace("đ", "d")
            .Replace("Đ", "d")
            // Vietnamese a variations
            .Replace("à", "a")
            .Replace("á", "a")
            .Replace("ả", "a")
            .Replace("ã", "a")
            .Replace("ạ", "a")
            .Replace("ă", "a")
            .Replace("ằ", "a")
            .Replace("ắ", "a")
            .Replace("ẳ", "a")
            .Replace("ẵ", "a")
            .Replace("ặ", "a")
            .Replace("â", "a")
            .Replace("ầ", "a")
            .Replace("ấ", "a")
            .Replace("ẩ", "a")
            .Replace("ẫ", "a")
            .Replace("ậ", "a")
            // Vietnamese e variations
            .Replace("è", "e")
            .Replace("é", "e")
            .Replace("ẻ", "e")
            .Replace("ẽ", "e")
            .Replace("ẹ", "e")
            .Replace("ê", "e")
            .Replace("ề", "e")
            .Replace("ế", "e")
            .Replace("ể", "e")
            .Replace("ễ", "e")
            .Replace("ệ", "e")
            // Vietnamese i variations
            .Replace("ì", "i")
            .Replace("í", "i")
            .Replace("ỉ", "i")
            .Replace("ĩ", "i")
            .Replace("ị", "i")
            // Vietnamese o variations
            .Replace("ò", "o")
            .Replace("ó", "o")
            .Replace("ỏ", "o")
            .Replace("õ", "o")
            .Replace("ọ", "o")
            .Replace("ô", "o")
            .Replace("ồ", "o")
            .Replace("ố", "o")
            .Replace("ổ", "o")
            .Replace("ỗ", "o")
            .Replace("ộ", "o")
            .Replace("ơ", "o")
            .Replace("ờ", "o")
            .Replace("ớ", "o")
            .Replace("ở", "o")
            .Replace("ỡ", "o")
            .Replace("ợ", "o")
            // Vietnamese u variations
            .Replace("ù", "u")
            .Replace("ú", "u")
            .Replace("ủ", "u")
            .Replace("ũ", "u")
            .Replace("ụ", "u")
            .Replace("ư", "u")
            .Replace("ừ", "u")
            .Replace("ứ", "u")
            .Replace("ử", "u")
            .Replace("ữ", "u")
            .Replace("ự", "u")
            // Vietnamese y variations
            .Replace("ỳ", "y")
            .Replace("ý", "y")
            .Replace("ỷ", "y")
            .Replace("ỹ", "y")
            .Replace("ỵ", "y")
            .Normalize(NormalizationForm.FormD);

        var result = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                result.Append(c);
            }
        }

        return Regex.Replace(result.ToString(), @"[^a-z0-9\-]", "")
            .Replace("--", "-")
            .Trim('-');
    }

    /// <summary>
    /// Generates a unique slug by appending a number if the slug already exists
    /// </summary>
    /// <param name="text">The text to convert to a slug</param>
    /// <param name="isSlugExists">Function to check if slug already exists</param>
    /// <returns>A unique URL-friendly slug</returns>
    public static async Task<string> GenerateUniqueSlugAsync(string text, Func<string, Task<bool>> isSlugExists)
    {
        var baseSlug = GenerateSlug(text);
        var slug = baseSlug;
        var counter = 1;

        while (await isSlugExists(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}