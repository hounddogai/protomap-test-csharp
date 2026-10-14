namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Functions of adaptive bonus pool estimate policy.</summary>
public static class AdaptiveBonusPoolEstimatePolicy
{
    public static int AdaptiveBonusPoolEstimateSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a adaptive bonus pool estimate code in its canonical ABPE- form.</summary>
    public static string NormalizeAdaptiveBonusPoolEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 4584.0, in order.</summary>
    public static int AdaptiveBonusPoolEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4584.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
