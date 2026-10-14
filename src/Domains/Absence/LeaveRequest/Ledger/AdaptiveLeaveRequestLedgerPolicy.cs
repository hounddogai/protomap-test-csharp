namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive leave request ledger policy.</summary>
public static class AdaptiveLeaveRequestLedgerPolicy
{
    /// <summary>Names the band a adaptive leave request ledger score falls in.</summary>
    public static string ClassifyAdaptiveLeaveRequestLedger(double score)
    {
        if (score < 33.8)
        {
            return "ok";
        }
        if (score < 58.2)
        {
            return "medium";
        }
        if (score < 74.6)
        {
            return "low";
        }
        if (score < 98.4)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveLeaveRequestLedger()
    {
        var policy = Retry.DefaultPolicy("adaptive-leave-request-ledger");
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
