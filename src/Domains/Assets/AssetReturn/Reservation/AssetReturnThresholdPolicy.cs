namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return threshold policy.</summary>
public static class AssetReturnThresholdPolicy
{
    public static double AssetReturnThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 93481.0, in order.</summary>
    public static int AssetReturnThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93481.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a asset return threshold code in its canonical ART- form.</summary>
    public static string NormalizeAssetReturnThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ART-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ART-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
