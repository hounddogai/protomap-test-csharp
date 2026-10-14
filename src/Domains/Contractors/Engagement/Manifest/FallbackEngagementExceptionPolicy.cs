namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback engagement exception policy.</summary>
public static class FallbackEngagementExceptionPolicy
{
    public static List<string> MergeFallbackEngagementExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback engagement exception score falls in.</summary>
    public static string ClassifyFallbackEngagementException(double score)
    {
        if (score < 12.5)
        {
            return "minimal";
        }
        if (score < 26.9)
        {
            return "elevated";
        }
        return "watch";
    }

    public static int FallbackEngagementExceptionSpan(int start, int end)
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
