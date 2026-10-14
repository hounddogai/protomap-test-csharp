namespace Acme.Workforce.Engagement.ActionPlan.Policy;

/// <summary>Functions of action plan budget policy.</summary>
public static class ActionPlanBudgetPolicy
{
    /// <summary>Names the band a action plan budget score falls in.</summary>
    public static string ClassifyActionPlanBudget(double score)
    {
        if (score < 47.1)
        {
            return "medium";
        }
        if (score < 90.6)
        {
            return "elevated";
        }
        return "critical";
    }

    public static int ActionPlanBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
