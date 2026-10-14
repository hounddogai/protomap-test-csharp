namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled action plan adjustment policy.</summary>
public static class ScheduledActionPlanAdjustmentPolicy
{
    public static string ScheduledActionPlanAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 25936.0, in order.</summary>
    public static int ScheduledActionPlanAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25936.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double ScheduledActionPlanAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
