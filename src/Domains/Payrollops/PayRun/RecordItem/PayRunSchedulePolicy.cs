namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run schedule policy.</summary>
public static class PayRunSchedulePolicy
{
    public static int PayRunScheduleSpan(int start, int end)
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

    /// <summary>Puts a pay run schedule code in its canonical PRS- form.</summary>
    public static string NormalizePayRunScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunSchedule()
    {
        var policy = Retry.DefaultPolicy("pay-run-schedule");
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
