namespace Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Functions of default deduction segment policy.</summary>
public static class DefaultDeductionSegmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 35016.0, in order.</summary>
    public static int DefaultDeductionSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35016.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int DefaultDeductionSegmentSpan(int start, int end)
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
        return span / 60;
    }
}
