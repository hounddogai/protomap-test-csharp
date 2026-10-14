namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan metric policy.</summary>
public static class FloorPlanMetricPolicy
{
    public static double FloorPlanMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 83675.0, in order.</summary>
    public static int FloorPlanMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83675.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
