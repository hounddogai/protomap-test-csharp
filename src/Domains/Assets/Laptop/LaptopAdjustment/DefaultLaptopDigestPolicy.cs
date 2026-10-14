namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Functions of default laptop digest policy.</summary>
public static class DefaultLaptopDigestPolicy
{
    /// <summary>Puts a default laptop digest code in its canonical DLD- form.</summary>
    public static string NormalizeDefaultLaptopDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DLD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DLD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default laptop digest score falls in.</summary>
    public static string ClassifyDefaultLaptopDigest(double score)
    {
        if (score < 41.6)
        {
            return "ok";
        }
        if (score < 64.5)
        {
            return "medium";
        }
        if (score < 74.8)
        {
            return "high";
        }
        if (score < 90.6)
        {
            return "minimal";
        }
        return "elevated";
    }
}
