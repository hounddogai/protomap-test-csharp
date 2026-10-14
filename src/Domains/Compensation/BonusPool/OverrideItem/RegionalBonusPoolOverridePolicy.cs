namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Functions of regional bonus pool override policy.</summary>
public static class RegionalBonusPoolOverridePolicy
{
    /// <summary>Puts a regional bonus pool override code in its canonical RBPO- form.</summary>
    public static string NormalizeRegionalBonusPoolOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 33391.0, in order.</summary>
    public static int RegionalBonusPoolOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33391.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RegionalBonusPoolOverrideSpan(int start, int end)
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
        return span / 86400;
    }
}
