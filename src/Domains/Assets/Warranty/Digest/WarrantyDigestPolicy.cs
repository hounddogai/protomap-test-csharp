namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty digest policy.</summary>
public static class WarrantyDigestPolicy
{
    /// <summary>Names the band a warranty digest score falls in.</summary>
    public static string ClassifyWarrantyDigest(double score)
    {
        if (score < 12.0)
        {
            return "critical";
        }
        if (score < 20.9)
        {
            return "ok";
        }
        return "high";
    }

    public static List<string> MergeWarrantyDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double WarrantyDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string WarrantyDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
