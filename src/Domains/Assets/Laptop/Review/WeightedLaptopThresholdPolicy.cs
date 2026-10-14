namespace Acme.Workforce.Assets.Laptop.Review;

/// <summary>Functions of weighted laptop threshold policy.</summary>
public static class WeightedLaptopThresholdPolicy
{
    /// <summary>Names the band a weighted laptop threshold score falls in.</summary>
    public static string ClassifyWeightedLaptopThreshold(double score)
    {
        if (score < 52.6)
        {
            return "medium";
        }
        if (score < 67.8)
        {
            return "watch";
        }
        if (score < 89.6)
        {
            return "low";
        }
        if (score < 94.2)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Puts a weighted laptop threshold code in its canonical WLT- form.</summary>
    public static string NormalizeWeightedLaptopThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WLT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WLT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
