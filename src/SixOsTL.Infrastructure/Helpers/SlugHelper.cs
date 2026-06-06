using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SixOsTL.Infrastructure.Helpers
{
    public static class SlugHelper
    {
        private static readonly Regex SlugRegex = new("[^a-z0-9]+", RegexOptions.Compiled);

        public static string ToSlug(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Trim().ToLowerInvariant();        
            input = input.Replace("đ", "d"); // xử lý tiếng Việt      
            var normalized = input.Normalize(NormalizationForm.FormD); // bỏ dấu unicode
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            var result = sb.ToString().Normalize(NormalizationForm.FormC);         
            result = SlugRegex.Replace(result, "-"); // replace ký tự đặc biệt bằng "-"            
            result = result.Trim('-'); // remove "-" dư            
            result = Regex.Replace(result, "-{2,}", "-"); // gộp nhiều dấu "-"
            return result;
        }
    }
}
