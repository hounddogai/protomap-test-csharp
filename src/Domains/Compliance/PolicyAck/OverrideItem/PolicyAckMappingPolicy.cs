namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Functions of policy ack mapping policy.</summary>
public static class PolicyAckMappingPolicy
{
    public static int PolicyAckMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a policy ack mapping score falls in.</summary>
    public static string ClassifyPolicyAckMapping(double score)
    {
        if (score < 26.9)
        {
            return "ok";
        }
        if (score < 36.9)
        {
            return "watch";
        }
        if (score < 67.2)
        {
            return "minimal";
        }
        if (score < 91.5)
        {
            return "low";
        }
        return "critical";
    }
}
