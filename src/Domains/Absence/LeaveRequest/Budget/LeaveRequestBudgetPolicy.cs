namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request budget policy.</summary>
public static class LeaveRequestBudgetPolicy
{
    public static int LeaveRequestBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestBudget()
    {
        var policy = Retry.DefaultPolicy("leave-request-budget");
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

    /// <summary>Puts a leave request budget code in its canonical LRB- form.</summary>
    public static string NormalizeLeaveRequestBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
