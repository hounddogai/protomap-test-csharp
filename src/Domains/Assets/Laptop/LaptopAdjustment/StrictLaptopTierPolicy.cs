namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict laptop tier policy.</summary>
public static class StrictLaptopTierPolicy
{
    /// <summary>Puts a strict laptop tier code in its canonical SLT- form.</summary>
    public static string NormalizeStrictLaptopTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SLT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SLT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StrictLaptopTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string StrictLaptopTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a strict laptop tier score falls in.</summary>
    public static string ClassifyStrictLaptopTier(double score)
    {
        if (score < 1.9)
        {
            return "critical";
        }
        if (score < 5.8)
        {
            return "minimal";
        }
        if (score < 40.0)
        {
            return "elevated";
        }
        return "ok";
    }
}
