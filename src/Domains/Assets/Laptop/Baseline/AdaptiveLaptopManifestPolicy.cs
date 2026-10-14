namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive laptop manifest policy.</summary>
public static class AdaptiveLaptopManifestPolicy
{
    public static string AdaptiveLaptopManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double AdaptiveLaptopManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive laptop manifest score falls in.</summary>
    public static string ClassifyAdaptiveLaptopManifest(double score)
    {
        if (score < 31.1)
        {
            return "minimal";
        }
        if (score < 43.3)
        {
            return "ok";
        }
        if (score < 92.3)
        {
            return "medium";
        }
        if (score < 99.6)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Puts a adaptive laptop manifest code in its canonical ALM- form.</summary>
    public static string NormalizeAdaptiveLaptopManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ALM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ALM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
