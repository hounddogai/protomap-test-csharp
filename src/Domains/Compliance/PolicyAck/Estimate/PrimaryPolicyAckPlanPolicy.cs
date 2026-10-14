namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary policy ack plan policy.</summary>
public static class PrimaryPolicyAckPlanPolicy
{
    public static double PrimaryPolicyAckPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 3.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PrimaryPolicyAckPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryPolicyAckPlan()
    {
        var policy = Retry.DefaultPolicy("primary-policy-ack-plan");
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
