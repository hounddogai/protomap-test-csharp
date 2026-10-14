namespace Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Functions of scheduled garnishment plan policy.</summary>
public static class ScheduledGarnishmentPlanPolicy
{
    public static int ScheduledGarnishmentPlanSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 48406.0, in order.</summary>
    public static int ScheduledGarnishmentPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48406.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
