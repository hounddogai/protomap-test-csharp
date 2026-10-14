namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty tier policy.</summary>
public static class WarrantyTierPolicy
{
    /// <summary>Puts a warranty tier code in its canonical WT- form.</summary>
    public static string NormalizeWarrantyTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double WarrantyTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 3.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int WarrantyTierSpan(int start, int end)
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
}
