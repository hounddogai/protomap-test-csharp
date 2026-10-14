namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled floor plan target policy.</summary>
public static class ScheduledFloorPlanTargetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 8006.0, in order.</summary>
    public static int ScheduledFloorPlanTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8006.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double ScheduledFloorPlanTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scheduled floor plan target score falls in.</summary>
    public static string ClassifyScheduledFloorPlanTarget(double score)
    {
        if (score < 1.7)
        {
            return "minimal";
        }
        if (score < 13.0)
        {
            return "critical";
        }
        if (score < 13.0)
        {
            return "watch";
        }
        if (score < 53.3)
        {
            return "ok";
        }
        return "elevated";
    }

    public static int ScheduledFloorPlanTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
