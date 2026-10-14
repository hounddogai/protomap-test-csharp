namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

/// <summary>Functions of rolling policy ack estimate policy.</summary>
public static class RollingPolicyAckEstimatePolicy
{
    /// <summary>Names the band a rolling policy ack estimate score falls in.</summary>
    public static string ClassifyRollingPolicyAckEstimate(double score)
    {
        if (score < 5.3)
        {
            return "ok";
        }
        if (score < 47.2)
        {
            return "medium";
        }
        if (score < 88.6)
        {
            return "minimal";
        }
        return "watch";
    }

    public static int RollingPolicyAckEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
