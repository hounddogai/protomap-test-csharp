namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted return plan override policy.</summary>
public static class WeightedReturnPlanOverridePolicy
{
    public static double WeightedReturnPlanOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a weighted return plan override code in its canonical WRPO- form.</summary>
    public static string NormalizeWeightedReturnPlanOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string WeightedReturnPlanOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
