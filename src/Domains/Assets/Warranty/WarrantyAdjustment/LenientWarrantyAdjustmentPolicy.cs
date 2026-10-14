namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient warranty adjustment policy.</summary>
public static class LenientWarrantyAdjustmentPolicy
{
    public static string LenientWarrantyAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a lenient warranty adjustment code in its canonical LWA- form.</summary>
    public static string NormalizeLenientWarrantyAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 1226.0, in order.</summary>
    public static int LenientWarrantyAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1226.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
