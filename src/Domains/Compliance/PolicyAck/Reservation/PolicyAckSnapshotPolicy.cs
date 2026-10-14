namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy ack snapshot policy.</summary>
public static class PolicyAckSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckSnapshot()
    {
        var policy = Retry.DefaultPolicy("policy-ack-snapshot");
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

    public static double PolicyAckSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a policy ack snapshot score falls in.</summary>
    public static string ClassifyPolicyAckSnapshot(double score)
    {
        if (score < 25.6)
        {
            return "ok";
        }
        if (score < 57.4)
        {
            return "minimal";
        }
        if (score < 65.1)
        {
            return "watch";
        }
        return "critical";
    }
}
