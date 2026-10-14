namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual adjustment rule policy.</summary>
public static class ManualAdjustmentRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 45916.0, in order.</summary>
    public static int ManualAdjustmentRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45916.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ManualAdjustmentRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a manual adjustment rule code in its canonical MAR- form.</summary>
    public static string NormalizeManualAdjustmentRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ManualAdjustmentRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
