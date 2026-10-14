namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered accrual threshold policy.</summary>
public static class TieredAccrualThresholdPolicy
{
    /// <summary>Puts a tiered accrual threshold code in its canonical TAT- form.</summary>
    public static string NormalizeTieredAccrualThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TAT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TAT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a tiered accrual threshold score falls in.</summary>
    public static string ClassifyTieredAccrualThreshold(double score)
    {
        if (score < 10.6)
        {
            return "high";
        }
        if (score < 54.2)
        {
            return "watch";
        }
        if (score < 77.0)
        {
            return "critical";
        }
        if (score < 94.9)
        {
            return "medium";
        }
        return "ok";
    }

    public static int TieredAccrualThresholdSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergeTieredAccrualThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
