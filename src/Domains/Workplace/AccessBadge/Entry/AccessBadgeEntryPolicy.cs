namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge entry policy.</summary>
public static class AccessBadgeEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeEntry()
    {
        var policy = Retry.DefaultPolicy("access-badge-entry");
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

    public static int AccessBadgeEntrySpan(int start, int end)
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
        return span / 3600;
    }
}
