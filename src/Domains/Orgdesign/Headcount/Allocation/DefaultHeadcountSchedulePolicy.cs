namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default headcount schedule policy.</summary>
public static class DefaultHeadcountSchedulePolicy
{
    /// <summary>Puts a default headcount schedule code in its canonical DHS- form.</summary>
    public static string NormalizeDefaultHeadcountScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DHS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DHS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultHeadcountSchedule()
    {
        var policy = Retry.DefaultPolicy("default-headcount-schedule");
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

    public static int DefaultHeadcountScheduleSpan(int start, int end)
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
        return span / 86400;
    }
}
