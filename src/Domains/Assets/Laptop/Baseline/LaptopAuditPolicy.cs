namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop audit policy.</summary>
public static class LaptopAuditPolicy
{
    /// <summary>Names the band a laptop audit score falls in.</summary>
    public static string ClassifyLaptopAudit(double score)
    {
        if (score < 36.8)
        {
            return "high";
        }
        if (score < 69.7)
        {
            return "medium";
        }
        if (score < 72.2)
        {
            return "low";
        }
        if (score < 80.8)
        {
            return "watch";
        }
        return "critical";
    }

    public static int LaptopAuditSpan(int start, int end)
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
        return span / 60;
    }

    public static List<string> MergeLaptopAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
