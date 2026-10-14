namespace Acme.Workforce.Absence.Accrual.Group;

/// <summary>Functions of weighted accrual group policy.</summary>
public static class WeightedAccrualGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99402.0, in order.</summary>
    public static int WeightedAccrualGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99402.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedAccrualGroupSpan(int start, int end)
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
        return span / 86400;
    }
}
