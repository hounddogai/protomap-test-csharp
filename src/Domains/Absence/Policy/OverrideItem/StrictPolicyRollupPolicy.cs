namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict policy rollup policy.</summary>
public static class StrictPolicyRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72704.0, in order.</summary>
    public static int StrictPolicyRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72704.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double StrictPolicyRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int StrictPolicyRollupSpan(int start, int end)
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

    /// <summary>Puts a strict policy rollup code in its canonical SPR- form.</summary>
    public static string NormalizeStrictPolicyRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
