namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Functions of strict policy ack claim policy.</summary>
public static class StrictPolicyAckClaimPolicy
{
    /// <summary>Puts a strict policy ack claim code in its canonical SPAC- form.</summary>
    public static string NormalizeStrictPolicyAckClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a strict policy ack claim score falls in.</summary>
    public static string ClassifyStrictPolicyAckClaim(double score)
    {
        if (score < 57.9)
        {
            return "medium";
        }
        if (score < 62.9)
        {
            return "elevated";
        }
        if (score < 70.6)
        {
            return "ok";
        }
        return "critical";
    }
}
