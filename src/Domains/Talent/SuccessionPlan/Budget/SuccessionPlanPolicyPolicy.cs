namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan policy policy.</summary>
public static class SuccessionPlanPolicyPolicy
{
    public static int SuccessionPlanPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 95993.0, in order.</summary>
    public static int SuccessionPlanPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95993.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SuccessionPlanPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
