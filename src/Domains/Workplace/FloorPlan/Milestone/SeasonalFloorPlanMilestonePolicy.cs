namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal floor plan milestone policy.</summary>
public static class SeasonalFloorPlanMilestonePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalFloorPlanMilestone()
    {
        var policy = Retry.DefaultPolicy("seasonal-floor-plan-milestone");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a seasonal floor plan milestone score falls in.</summary>
    public static string ClassifySeasonalFloorPlanMilestone(double score)
    {
        if (score < 39.2)
        {
            return "high";
        }
        if (score < 75.7)
        {
            return "ok";
        }
        if (score < 80.4)
        {
            return "watch";
        }
        if (score < 85.5)
        {
            return "low";
        }
        return "critical";
    }

    public static double SeasonalFloorPlanMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 5755.0, in order.</summary>
    public static int SeasonalFloorPlanMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5755.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
