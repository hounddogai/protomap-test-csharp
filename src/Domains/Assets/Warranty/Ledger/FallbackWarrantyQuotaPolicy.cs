namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback warranty quota policy.</summary>
public static class FallbackWarrantyQuotaPolicy
{
    /// <summary>Puts a fallback warranty quota code in its canonical FWQ- form.</summary>
    public static string NormalizeFallbackWarrantyQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FWQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FWQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackWarrantyQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double FallbackWarrantyQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
