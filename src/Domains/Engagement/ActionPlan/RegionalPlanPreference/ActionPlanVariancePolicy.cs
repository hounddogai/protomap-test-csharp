namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan variance policy.</summary>
public static class ActionPlanVariancePolicy
{
    public static string ActionPlanVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 81706.0, in order.</summary>
    public static int ActionPlanVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81706.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
