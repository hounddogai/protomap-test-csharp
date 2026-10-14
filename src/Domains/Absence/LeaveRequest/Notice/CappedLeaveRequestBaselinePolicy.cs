namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped leave request baseline policy.</summary>
public static class CappedLeaveRequestBaselinePolicy
{
    /// <summary>Puts a capped leave request baseline code in its canonical CLRB- form.</summary>
    public static string NormalizeCappedLeaveRequestBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CLRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CLRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedLeaveRequestBaseline()
    {
        var policy = Retry.DefaultPolicy("capped-leave-request-baseline");
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
}
