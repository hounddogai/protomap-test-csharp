namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Functions of legacy floor plan threshold policy.</summary>
public static class LegacyFloorPlanThresholdPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 24363.0, in order.</summary>
    public static int LegacyFloorPlanThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24363.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy floor plan threshold score falls in.</summary>
    public static string ClassifyLegacyFloorPlanThreshold(double score)
    {
        if (score < 52.3)
        {
            return "low";
        }
        if (score < 85.1)
        {
            return "critical";
        }
        return "watch";
    }
}
