namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling vendor quota policy.</summary>
public static class RollingVendorQuotaPolicy
{
    public static List<string> MergeRollingVendorQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rolling vendor quota score falls in.</summary>
    public static string ClassifyRollingVendorQuota(double score)
    {
        if (score < 19.5)
        {
            return "ok";
        }
        if (score < 29.8)
        {
            return "minimal";
        }
        if (score < 49.5)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Puts a rolling vendor quota code in its canonical RVQ- form.</summary>
    public static string NormalizeRollingVendorQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RVQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RVQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RollingVendorQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
