namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop batch policy.</summary>
public static class LaptopBatchPolicy
{
    /// <summary>Puts a laptop batch code in its canonical LB- form.</summary>
    public static string NormalizeLaptopBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LaptopBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a laptop batch score falls in.</summary>
    public static string ClassifyLaptopBatch(double score)
    {
        if (score < 33.2)
        {
            return "minimal";
        }
        if (score < 41.6)
        {
            return "ok";
        }
        if (score < 59.7)
        {
            return "medium";
        }
        return "watch";
    }
}
