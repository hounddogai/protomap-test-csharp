namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled policy ack quota policy.</summary>
public static class ScheduledPolicyAckQuotaPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPolicyAckQuota()
    {
        var policy = Retry.DefaultPolicy("scheduled-policy-ack-quota");
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

    public static string ScheduledPolicyAckQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int ScheduledPolicyAckQuotaSpan(int start, int end)
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

    /// <summary>Names the band a scheduled policy ack quota score falls in.</summary>
    public static string ClassifyScheduledPolicyAckQuota(double score)
    {
        if (score < 56.1)
        {
            return "watch";
        }
        if (score < 79.6)
        {
            return "critical";
        }
        if (score < 92.6)
        {
            return "high";
        }
        return "minimal";
    }
}
