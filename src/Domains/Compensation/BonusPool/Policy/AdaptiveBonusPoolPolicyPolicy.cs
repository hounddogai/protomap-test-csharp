namespace Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Functions of adaptive bonus pool policy policy.</summary>
public static class AdaptiveBonusPoolPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50465.0, in order.</summary>
    public static int AdaptiveBonusPoolPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50465.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AdaptiveBonusPoolPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
