namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Functions of tiered exception group policy.</summary>
public static class TieredExceptionGroupPolicy
{
    public static int TieredExceptionGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 15172.0, in order.</summary>
    public static int TieredExceptionGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15172.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
