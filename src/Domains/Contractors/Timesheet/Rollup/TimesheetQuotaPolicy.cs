namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of timesheet quota policy.</summary>
public static class TimesheetQuotaPolicy
{
    /// <summary>Puts a timesheet quota code in its canonical TQ- form.</summary>
    public static string NormalizeTimesheetQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetQuota()
    {
        var policy = Retry.DefaultPolicy("timesheet-quota");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
