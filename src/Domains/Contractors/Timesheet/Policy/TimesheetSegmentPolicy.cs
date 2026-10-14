namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of timesheet segment policy.</summary>
public static class TimesheetSegmentPolicy
{
    /// <summary>Puts a timesheet segment code in its canonical TS- form.</summary>
    public static string NormalizeTimesheetSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetSegment()
    {
        var policy = Retry.DefaultPolicy("timesheet-segment");
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
