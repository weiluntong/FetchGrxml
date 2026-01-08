using Xunit;

namespace FetchGrxml.Tests;

public class ExclusionMatcherTests
{
    [Fact]
    public void ShouldExclude_WithWildcardPattern_MatchesCorrectly()
    {
        var patterns = new List<string> { "*/logs/*" };
        
        Assert.True(ExclusionMatcher.ShouldExclude("/app/logs/debug", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/system/logs/error.log", patterns));
        Assert.False(ExclusionMatcher.ShouldExclude("/app/data/logs", patterns));
        Assert.False(ExclusionMatcher.ShouldExclude("/logs", patterns));
    }

    [Fact]
    public void ShouldExclude_WithPrefixPattern_MatchesExactAndSubpaths()
    {
        var patterns = new List<string> { "/temp" };
        
        Assert.True(ExclusionMatcher.ShouldExclude("/temp", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/temp/subfolder", patterns));
        Assert.False(ExclusionMatcher.ShouldExclude("/temporary", patterns));
    }

    [Fact]
    public void ShouldExclude_WithPrefixPattern_MatchesAllSubdirectories()
    {
        var patterns = new List<string> { "/backup" };
        
        Assert.True(ExclusionMatcher.ShouldExclude("/backup", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/backup/2024", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/backup/old/data", patterns));
        Assert.False(ExclusionMatcher.ShouldExclude("/backups", patterns));
    }

    [Fact]
    public void ShouldExclude_WithMultiplePatterns_MatchesAny()
    {
        var patterns = new List<string> { "/temp", "*/logs/*", "*.tmp" };
        
        Assert.True(ExclusionMatcher.ShouldExclude("/temp", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/app/logs/debug", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/data/cache.tmp", patterns));
    }

    [Fact]
    public void ShouldExclude_WithEmptyPatternList_ReturnsFalse()
    {
        var patterns = new List<string>();
        
        Assert.False(ExclusionMatcher.ShouldExclude("/any/path", patterns));
    }

    [Fact]
    public void ShouldExclude_WithWildcardAtEnd_MatchesPrefix()
    {
        var patterns = new List<string> { "/temp*" };
        
        Assert.True(ExclusionMatcher.ShouldExclude("/temp", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/temporary", patterns));
        Assert.True(ExclusionMatcher.ShouldExclude("/temp123", patterns));
        Assert.False(ExclusionMatcher.ShouldExclude("/data/temp", patterns));
    }
}
