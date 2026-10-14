namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Functions of statement of work adjustment policy.</summary>
public static class StatementOfWorkAdjustmentPolicy
{
    public static int StatementOfWorkAdjustmentSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 56490.0, in order.</summary>
    public static int StatementOfWorkAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56490.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
