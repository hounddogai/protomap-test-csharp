namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack bundle policy.</summary>
public static class PolicyAckBundlePolicy
{
    public static string PolicyAckBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a policy ack bundle score falls in.</summary>
    public static string ClassifyPolicyAckBundle(double score)
    {
        if (score < 13.7)
        {
            return "watch";
        }
        if (score < 54.5)
        {
            return "ok";
        }
        if (score < 62.2)
        {
            return "critical";
        }
        if (score < 96.7)
        {
            return "high";
        }
        return "minimal";
    }
}
