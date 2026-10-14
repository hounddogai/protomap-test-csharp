namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched timesheet tier policy.</summary>
public static class BatchedTimesheetTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 59971.0, in order.</summary>
    public static int BatchedTimesheetTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59971.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTimesheetTier()
    {
        var policy = Retry.DefaultPolicy("batched-timesheet-tier");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
