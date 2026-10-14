namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive leave request policy policy.</summary>
public static class AdaptiveLeaveRequestPolicyPolicy
{
    public static int AdaptiveLeaveRequestPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static List<string> MergeAdaptiveLeaveRequestPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive leave request policy score falls in.</summary>
    public static string ClassifyAdaptiveLeaveRequestPolicy(double score)
    {
        if (score < 15.2)
        {
            return "watch";
        }
        if (score < 54.3)
        {
            return "low";
        }
        if (score < 63.1)
        {
            return "critical";
        }
        if (score < 75.7)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveLeaveRequestPolicy()
    {
        var policy = Retry.DefaultPolicy("adaptive-leave-request-policy");
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
