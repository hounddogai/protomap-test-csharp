namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of holiday mapping policy.</summary>
public static class HolidayMappingPolicy
{
    public static int HolidayMappingSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayMapping()
    {
        var policy = Retry.DefaultPolicy("holiday-mapping");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
