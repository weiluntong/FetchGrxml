using System.Text.RegularExpressions;

namespace FetchGrxml;

/// <summary>
/// Handles directory exclusion logic
/// </summary>
public static class ExclusionMatcher
{
    public static bool ShouldExclude(string path, List<string> exclusionPatterns)
    {
        if (exclusionPatterns == null || exclusionPatterns.Count == 0)
            return false;

        foreach (var pattern in exclusionPatterns)
        {
            if (MatchesPattern(path, pattern))
                return true;
        }

        return false;
    }

    private static bool MatchesPattern(string path, string pattern)
    {
        // Support wildcard patterns
        if (pattern.Contains("*"))
        {
            return MatchesWildcard(path, pattern);
        }
        else
        {
            return MatchesExactOrPrefix(path, pattern);
        }
    }

    private static bool MatchesWildcard(string path, string pattern)
    {
        var regex = new Regex(
            "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        return regex.IsMatch(path);
    }

    private static bool MatchesExactOrPrefix(string path, string pattern)
    {
        // Exact match or path starts with pattern
        return path.Equals(pattern, StringComparison.Ordinal) ||
               path.StartsWith(pattern + "/", StringComparison.Ordinal);
    }
}
