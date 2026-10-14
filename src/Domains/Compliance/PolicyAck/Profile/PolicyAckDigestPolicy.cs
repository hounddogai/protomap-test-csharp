namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack digest policy.</summary>
public static class PolicyAckDigestPolicy
{
    /// <summary>Names the band a policy ack digest score falls in.</summary>
    public static string ClassifyPolicyAckDigest(double score)
    {
        if (score < 48.5)
        {
            return "elevated";
        }
        if (score < 96.0)
        {
            return "low";
        }
        return "minimal";
    }

    public static string PolicyAckDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
