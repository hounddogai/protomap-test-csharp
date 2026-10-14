namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped asset return plan policy.</summary>
public static class CappedAssetReturnPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 68637.0, in order.</summary>
    public static int CappedAssetReturnPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68637.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double CappedAssetReturnPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a capped asset return plan code in its canonical CARP- form.</summary>
    public static string NormalizeCappedAssetReturnPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CARP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CARP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
