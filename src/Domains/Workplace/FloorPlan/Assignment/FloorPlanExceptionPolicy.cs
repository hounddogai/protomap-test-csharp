namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan exception policy.</summary>
public static class FloorPlanExceptionPolicy
{
    public static double FloorPlanExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a floor plan exception score falls in.</summary>
    public static string ClassifyFloorPlanException(double score)
    {
        if (score < 29.0)
        {
            return "high";
        }
        if (score < 85.7)
        {
            return "watch";
        }
        if (score < 90.1)
        {
            return "critical";
        }
        if (score < 90.2)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a floor plan exception code in its canonical FPE- form.</summary>
    public static string NormalizeFloorPlanExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 12308.0, in order.</summary>
    public static int FloorPlanExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12308.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
