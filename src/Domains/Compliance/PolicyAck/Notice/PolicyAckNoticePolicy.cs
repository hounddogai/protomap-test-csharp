namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy ack notice policy.</summary>
public static class PolicyAckNoticePolicy
{
    /// <summary>Names the band a policy ack notice score falls in.</summary>
    public static string ClassifyPolicyAckNotice(double score)
    {
        if (score < 4.1)
        {
            return "low";
        }
        if (score < 91.4)
        {
            return "minimal";
        }
        if (score < 92.5)
        {
            return "high";
        }
        if (score < 96.9)
        {
            return "critical";
        }
        return "elevated";
    }

    public static double PolicyAckNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PolicyAckNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckNotice()
    {
        var policy = Retry.DefaultPolicy("policy-ack-notice");
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
