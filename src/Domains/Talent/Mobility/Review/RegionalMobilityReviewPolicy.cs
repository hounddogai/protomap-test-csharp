namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional mobility review policy.</summary>
public static class RegionalMobilityReviewPolicy
{
    public static List<string> MergeRegionalMobilityReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a regional mobility review code in its canonical RMR- form.</summary>
    public static string NormalizeRegionalMobilityReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalMobilityReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
