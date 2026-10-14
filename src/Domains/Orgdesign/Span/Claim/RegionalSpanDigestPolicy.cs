namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional span digest policy.</summary>
public static class RegionalSpanDigestPolicy
{
    public static string RegionalSpanDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeRegionalSpanDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int RegionalSpanDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a regional span digest score falls in.</summary>
    public static string ClassifyRegionalSpanDigest(double score)
    {
        if (score < 75.3)
        {
            return "ok";
        }
        if (score < 80.6)
        {
            return "elevated";
        }
        return "medium";
    }
}
