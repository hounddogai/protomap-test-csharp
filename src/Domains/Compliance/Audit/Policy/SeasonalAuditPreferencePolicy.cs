namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal audit preference policy.</summary>
public static class SeasonalAuditPreferencePolicy
{
    public static List<string> MergeSeasonalAuditPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int SeasonalAuditPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double SeasonalAuditPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal audit preference score falls in.</summary>
    public static string ClassifySeasonalAuditPreference(double score)
    {
        if (score < 22.0)
        {
            return "minimal";
        }
        if (score < 69.3)
        {
            return "elevated";
        }
        if (score < 86.8)
        {
            return "medium";
        }
        return "watch";
    }
}
