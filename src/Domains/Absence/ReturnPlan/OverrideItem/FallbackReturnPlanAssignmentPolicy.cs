namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback return plan assignment policy.</summary>
public static class FallbackReturnPlanAssignmentPolicy
{
    public static int FallbackReturnPlanAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 58741.0, in order.</summary>
    public static int FallbackReturnPlanAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58741.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FallbackReturnPlanAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a fallback return plan assignment score falls in.</summary>
    public static string ClassifyFallbackReturnPlanAssignment(double score)
    {
        if (score < 13.5)
        {
            return "elevated";
        }
        if (score < 36.4)
        {
            return "high";
        }
        if (score < 75.7)
        {
            return "low";
        }
        if (score < 96.9)
        {
            return "critical";
        }
        return "ok";
    }
}
