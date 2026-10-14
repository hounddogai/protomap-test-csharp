namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending timesheet metric policy.</summary>
public static class PendingTimesheetMetricPolicy
{
    public static int PendingTimesheetMetricSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingTimesheetMetric()
    {
        var policy = Retry.DefaultPolicy("pending-timesheet-metric");
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
}
