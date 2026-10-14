namespace Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Functions of policy ack rollup policy.</summary>
public static class PolicyAckRollupPolicy
{
    /// <summary>Names the band a policy ack rollup score falls in.</summary>
    public static string ClassifyPolicyAckRollup(double score)
    {
        if (score < 74.6)
        {
            return "high";
        }
        if (score < 88.8)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Puts a policy ack rollup code in its canonical PAR- form.</summary>
    public static string NormalizePolicyAckRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
