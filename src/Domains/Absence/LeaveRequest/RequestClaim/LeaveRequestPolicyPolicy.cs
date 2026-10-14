namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request policy policy.</summary>
public static class LeaveRequestPolicyPolicy
{
    /// <summary>Puts a leave request policy code in its canonical LRP- form.</summary>
    public static string NormalizeLeaveRequestPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestPolicy()
    {
        var policy = Retry.DefaultPolicy("leave-request-policy");
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
