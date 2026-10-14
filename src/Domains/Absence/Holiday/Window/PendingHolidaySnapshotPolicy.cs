namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending holiday snapshot policy.</summary>
public static class PendingHolidaySnapshotPolicy
{
    public static int PendingHolidaySnapshotSpan(int start, int end)
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

    /// <summary>Puts a pending holiday snapshot code in its canonical PHS- form.</summary>
    public static string NormalizePendingHolidaySnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PHS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PHS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingHolidaySnapshot()
    {
        var policy = Retry.DefaultPolicy("pending-holiday-snapshot");
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
