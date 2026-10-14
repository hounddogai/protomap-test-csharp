namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack preference policy.</summary>
public static class PolicyAckPreferencePolicy
{
    /// <summary>Names the band a policy ack preference score falls in.</summary>
    public static string ClassifyPolicyAckPreference(double score)
    {
        if (score < 4.5)
        {
            return "elevated";
        }
        if (score < 37.9)
        {
            return "watch";
        }
        if (score < 90.3)
        {
            return "high";
        }
        return "ok";
    }

    public static string PolicyAckPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
