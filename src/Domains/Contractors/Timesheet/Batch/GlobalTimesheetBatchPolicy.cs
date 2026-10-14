namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global timesheet batch policy.</summary>
public static class GlobalTimesheetBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalTimesheetBatch()
    {
        var policy = Retry.DefaultPolicy("global-timesheet-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a global timesheet batch code in its canonical GTB- form.</summary>
    public static string NormalizeGlobalTimesheetBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GTB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GTB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
