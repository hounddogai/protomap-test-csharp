namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback action plan segment policy.</summary>
public static class FallbackActionPlanSegmentPolicy
{
    public static double FallbackActionPlanSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback action plan segment code in its canonical FAPS- form.</summary>
    public static string NormalizeFallbackActionPlanSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FAPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FAPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 73276.0, in order.</summary>
    public static int FallbackActionPlanSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73276.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a fallback action plan segment score falls in.</summary>
    public static string ClassifyFallbackActionPlanSegment(double score)
    {
        if (score < 17.5)
        {
            return "elevated";
        }
        if (score < 22.7)
        {
            return "critical";
        }
        if (score < 52.0)
        {
            return "ok";
        }
        if (score < 97.7)
        {
            return "low";
        }
        return "high";
    }
}
