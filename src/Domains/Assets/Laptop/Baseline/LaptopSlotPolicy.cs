namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop slot policy.</summary>
public static class LaptopSlotPolicy
{
    /// <summary>Puts a laptop slot code in its canonical LS- form.</summary>
    public static string NormalizeLaptopSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LaptopSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a laptop slot score falls in.</summary>
    public static string ClassifyLaptopSlot(double score)
    {
        if (score < 46.4)
        {
            return "elevated";
        }
        if (score < 62.8)
        {
            return "watch";
        }
        return "low";
    }

    public static double LaptopSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
