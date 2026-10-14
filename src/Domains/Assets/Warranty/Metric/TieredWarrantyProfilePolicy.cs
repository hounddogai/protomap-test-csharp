namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered warranty profile policy.</summary>
public static class TieredWarrantyProfilePolicy
{
    public static int TieredWarrantyProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static List<string> MergeTieredWarrantyProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered warranty profile score falls in.</summary>
    public static string ClassifyTieredWarrantyProfile(double score)
    {
        if (score < 15.3)
        {
            return "high";
        }
        if (score < 17.1)
        {
            return "critical";
        }
        if (score < 21.6)
        {
            return "medium";
        }
        if (score < 28.6)
        {
            return "elevated";
        }
        return "watch";
    }
}
