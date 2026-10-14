namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling floor plan entry policy.</summary>
public static class RollingFloorPlanEntryPolicy
{
    /// <summary>Puts a rolling floor plan entry code in its canonical RFPE- form.</summary>
    public static string NormalizeRollingFloorPlanEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RFPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RFPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RollingFloorPlanEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 69474.0, in order.</summary>
    public static int RollingFloorPlanEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69474.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling floor plan entry score falls in.</summary>
    public static string ClassifyRollingFloorPlanEntry(double score)
    {
        if (score < 2.7)
        {
            return "minimal";
        }
        if (score < 26.9)
        {
            return "watch";
        }
        if (score < 36.7)
        {
            return "ok";
        }
        return "elevated";
    }
}
