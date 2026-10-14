namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled refresh review policy.</summary>
public static class ScheduledRefreshReviewPolicy
{
    /// <summary>Names the band a scheduled refresh review score falls in.</summary>
    public static string ClassifyScheduledRefreshReview(double score)
    {
        if (score < 4.2)
        {
            return "low";
        }
        if (score < 49.0)
        {
            return "critical";
        }
        if (score < 60.6)
        {
            return "ok";
        }
        if (score < 94.8)
        {
            return "elevated";
        }
        return "high";
    }

    public static List<string> MergeScheduledRefreshReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled refresh review code in its canonical SRR- form.</summary>
    public static string NormalizeScheduledRefreshReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
