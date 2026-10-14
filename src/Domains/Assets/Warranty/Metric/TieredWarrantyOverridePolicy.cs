namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered warranty override policy.</summary>
public static class TieredWarrantyOverridePolicy
{
    /// <summary>Puts a tiered warranty override code in its canonical TWO- form.</summary>
    public static string NormalizeTieredWarrantyOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TWO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TWO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredWarrantyOverrideSpan(int start, int end)
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
        return span / 86400;
    }

    public static double TieredWarrantyOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTieredWarrantyOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
