namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted rate request policy.</summary>
public static class WeightedRateRequestPolicy
{
    public static List<string> MergeWeightedRateRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a weighted rate request score falls in.</summary>
    public static string ClassifyWeightedRateRequest(double score)
    {
        if (score < 12.0)
        {
            return "low";
        }
        if (score < 74.1)
        {
            return "minimal";
        }
        if (score < 87.3)
        {
            return "ok";
        }
        return "critical";
    }

    public static string WeightedRateRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a weighted rate request code in its canonical WRR- form.</summary>
    public static string NormalizeWeightedRateRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
