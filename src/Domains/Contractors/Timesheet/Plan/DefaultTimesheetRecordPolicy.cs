namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default timesheet record policy.</summary>
public static class DefaultTimesheetRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 45179.0, in order.</summary>
    public static int DefaultTimesheetRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45179.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultTimesheetRecord()
    {
        var policy = Retry.DefaultPolicy("default-timesheet-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a default timesheet record score falls in.</summary>
    public static string ClassifyDefaultTimesheetRecord(double score)
    {
        if (score < 33.2)
        {
            return "critical";
        }
        if (score < 39.0)
        {
            return "high";
        }
        return "elevated";
    }
}
