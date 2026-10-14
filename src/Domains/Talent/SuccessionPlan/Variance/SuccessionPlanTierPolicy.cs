namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Functions of succession plan tier policy.</summary>
public static class SuccessionPlanTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 63031.0, in order.</summary>
    public static int SuccessionPlanTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63031.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a succession plan tier code in its canonical SPT- form.</summary>
    public static string NormalizeSuccessionPlanTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SuccessionPlanTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
