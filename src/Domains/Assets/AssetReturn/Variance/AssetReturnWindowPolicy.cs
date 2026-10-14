namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return window policy.</summary>
public static class AssetReturnWindowPolicy
{
    public static string AssetReturnWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a asset return window score falls in.</summary>
    public static string ClassifyAssetReturnWindow(double score)
    {
        if (score < 72.8)
        {
            return "medium";
        }
        if (score < 76.4)
        {
            return "ok";
        }
        if (score < 80.6)
        {
            return "minimal";
        }
        if (score < 96.4)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Puts a asset return window code in its canonical ARW- form.</summary>
    public static string NormalizeAssetReturnWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AssetReturnWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
