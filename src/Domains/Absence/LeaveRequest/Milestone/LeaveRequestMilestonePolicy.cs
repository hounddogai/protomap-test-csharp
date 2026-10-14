namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request milestone policy.</summary>
public static class LeaveRequestMilestonePolicy
{
    /// <summary>Puts a leave request milestone code in its canonical LRM- form.</summary>
    public static string NormalizeLeaveRequestMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestMilestone()
    {
        var policy = Retry.DefaultPolicy("leave-request-milestone");
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
}
