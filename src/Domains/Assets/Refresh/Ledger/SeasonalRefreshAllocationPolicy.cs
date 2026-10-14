namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal refresh allocation policy.</summary>
public static class SeasonalRefreshAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71752.0, in order.</summary>
    public static int SeasonalRefreshAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71752.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SeasonalRefreshAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a seasonal refresh allocation code in its canonical SRA- form.</summary>
    public static string NormalizeSeasonalRefreshAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
