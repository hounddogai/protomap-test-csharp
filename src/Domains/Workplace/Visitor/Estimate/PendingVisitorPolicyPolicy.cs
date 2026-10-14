namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending visitor policy policy.</summary>
public static class PendingVisitorPolicyPolicy
{
    public static int PendingVisitorPolicySpan(int start, int end)
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

    /// <summary>Names the band a pending visitor policy score falls in.</summary>
    public static string ClassifyPendingVisitorPolicy(double score)
    {
        if (score < 54.9)
        {
            return "high";
        }
        if (score < 74.9)
        {
            return "critical";
        }
        if (score < 85.1)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingVisitorPolicy()
    {
        var policy = Retry.DefaultPolicy("pending-visitor-policy");
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
