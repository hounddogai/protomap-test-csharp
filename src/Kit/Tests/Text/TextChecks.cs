namespace Acme.Workforce.Kit.Tests.Text;

using Acme.Workforce.Kit.Text;

/// <summary>Checks of text code, run by the project's runner.</summary>
public static class TextChecks
{
    public static void NormalizesCaseAndSpace()
    {
        Check.Equal(Text.Normalize("  Mixed Case "), "mixed case", "normalizes case and space");
    }

    public static void SlugifiesNames()
    {
        Check.Equal(Text.Slugify(" Spring  Sale 2026 "), "spring-sale-2026", "slugifies names");
    }

    public static void TruncatesLongValues()
    {
        Check.Equal(Text.Truncate("abcdefghij", 6), "abc...", "truncates long values");
    }

    public static void KeepsShortValues()
    {
        Check.Equal(Text.Truncate("abc", 6), "abc", "keeps short values");
    }

    public static void MasksAllButTheTail()
    {
        Check.Equal(Text.MaskTail("4111111111111111", 4), "************1111", "masks all but the tail");
    }

    public static void CountsWords()
    {
        Check.Equal(Text.WordCount("  one two  three "), 3, "counts words");
    }

    public static void FitsToAWidth()
    {
        Check.Equal(Text.Fit("ab", 4), "ab  ", "fits to a width");
    }

    public static void JoinsOnlyNonEmptyParts()
    {
        Check.Equal(Text.JoinNonEmpty(new List<string> { "a", " ", "b" }, "/"), "a/b", "joined");
        Check.True(Text.ContainsAny("Late Fee", new List<string> { "fee" }), "contains fee");
    }

    public static void RunAll()
    {
        NormalizesCaseAndSpace();
        SlugifiesNames();
        TruncatesLongValues();
        KeepsShortValues();
        MasksAllButTheTail();
        CountsWords();
        FitsToAWidth();
        JoinsOnlyNonEmptyParts();
    }
}
