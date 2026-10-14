namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount tier policy.</summary>
public static class HeadcountTierPolicy
{
    public static string HeadcountTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double HeadcountTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a headcount tier code in its canonical HT- form.</summary>
    public static string NormalizeHeadcountTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a headcount tier score falls in.</summary>
    public static string ClassifyHeadcountTier(double score)
    {
        if (score < 28.5)
        {
            return "elevated";
        }
        if (score < 57.7)
        {
            return "medium";
        }
        if (score < 73.4)
        {
            return "low";
        }
        if (score < 84.6)
        {
            return "watch";
        }
        return "ok";
    }
}
