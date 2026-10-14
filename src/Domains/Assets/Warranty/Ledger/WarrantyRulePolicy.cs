namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty rule policy.</summary>
public static class WarrantyRulePolicy
{
    /// <summary>Puts a warranty rule code in its canonical WR- form.</summary>
    public static string NormalizeWarrantyRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double WarrantyRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
