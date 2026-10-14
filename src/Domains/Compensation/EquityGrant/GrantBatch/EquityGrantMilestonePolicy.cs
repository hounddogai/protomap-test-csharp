namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant milestone policy.</summary>
public static class EquityGrantMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 32668.0, in order.</summary>
    public static int EquityGrantMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32668.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double EquityGrantMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a equity grant milestone code in its canonical EGM- form.</summary>
    public static string NormalizeEquityGrantMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
