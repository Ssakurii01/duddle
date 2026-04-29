using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace Luxodd.Game.Scripts.HelpersAndUtils
{
    public static class StringExtensions
    {
        public static string ToPascalCaseStyle(this string str)
        {
            return string.Concat(
                str.Split('_')
                    .Select(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase)
            );
        }
        
        
        private static readonly Regex LowerUpperBoundary =
            new Regex(@"(?<=[\p{Ll}\p{Nd}])(?=\p{Lu})", RegexOptions.Compiled);

        private static readonly Regex WordMatcher =
            new Regex(@"[\p{L}\p{Nd}]+", RegexOptions.Compiled);

        public static string ToPascalCase(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            
            var normalized = LowerUpperBoundary.Replace(value.Trim(), " ");
            var parts = WordMatcher.Matches(normalized).Cast<Match>().Select(m => m.Value);

            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                var lower = part.ToLowerInvariant();
                sb.Append(char.ToUpperInvariant(lower[0]));
                if (lower.Length > 1)
                    sb.Append(lower, 1, lower.Length - 1);
            }

            return sb.ToString();
        }
    }
}
