namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary succession plan rollup policy.</summary>
public static class PrimarySuccessionPlanRollupPolicy
{
    public static double PrimarySuccessionPlanRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 35876.0, in order.</summary>
    public static int PrimarySuccessionPlanRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35876.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary succession plan rollup code in its canonical PSPR- form.</summary>
    public static string NormalizePrimarySuccessionPlanRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
