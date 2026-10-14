namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant score policy.</summary>
public static class EquityGrantScorePolicy
{
    /// <summary>Names the band a equity grant score score falls in.</summary>
    public static string ClassifyEquityGrantScore(double score)
    {
        if (score < 9.5)
        {
            return "medium";
        }
        if (score < 11.0)
        {
            return "low";
        }
        return "ok";
    }

    public static List<string> MergeEquityGrantScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int EquityGrantScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
