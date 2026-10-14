namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict access badge assignment policy.</summary>
public static class StrictAccessBadgeAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictAccessBadgeAssignment()
    {
        var policy = Retry.DefaultPolicy("strict-access-badge-assignment");
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

    public static int StrictAccessBadgeAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a strict access badge assignment code in its canonical SABA- form.</summary>
    public static string NormalizeStrictAccessBadgeAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SABA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SABA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
