namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return budget policy.</summary>
public static class AssetReturnBudgetPolicy
{
    public static int AssetReturnBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 61417.0, in order.</summary>
    public static int AssetReturnBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61417.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a asset return budget code in its canonical ARB- form.</summary>
    public static string NormalizeAssetReturnBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AssetReturnBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
