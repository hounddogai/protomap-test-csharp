namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered position bundle policy.</summary>
public static class TieredPositionBundlePolicy
{
    /// <summary>Puts a tiered position bundle code in its canonical TPB- form.</summary>
    public static string NormalizeTieredPositionBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 36360.0, in order.</summary>
    public static int TieredPositionBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36360.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string TieredPositionBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
