namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered laptop tier policy.</summary>
public static class TieredLaptopTierPolicy
{
    /// <summary>Puts a tiered laptop tier code in its canonical TLT- form.</summary>
    public static string NormalizeTieredLaptopTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TLT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TLT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredLaptopTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static string TieredLaptopTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a tiered laptop tier score falls in.</summary>
    public static string ClassifyTieredLaptopTier(double score)
    {
        if (score < 15.8)
        {
            return "ok";
        }
        if (score < 77.9)
        {
            return "watch";
        }
        return "low";
    }
}
