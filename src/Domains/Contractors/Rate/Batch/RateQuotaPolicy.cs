namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rate quota policy.</summary>
public static class RateQuotaPolicy
{
    public static List<string> MergeRateQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rate quota score falls in.</summary>
    public static string ClassifyRateQuota(double score)
    {
        if (score < 84.9)
        {
            return "minimal";
        }
        if (score < 95.2)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Puts a rate quota code in its canonical RQ- form.</summary>
    public static string NormalizeRateQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
