namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

/// <summary>Functions of fallback policy ack snapshot policy.</summary>
public static class FallbackPolicyAckSnapshotPolicy
{
    /// <summary>Puts a fallback policy ack snapshot code in its canonical FPAS- form.</summary>
    public static string NormalizeFallbackPolicyAckSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a fallback policy ack snapshot score falls in.</summary>
    public static string ClassifyFallbackPolicyAckSnapshot(double score)
    {
        if (score < 26.4)
        {
            return "ok";
        }
        if (score < 73.3)
        {
            return "minimal";
        }
        if (score < 87.5)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 6673.0, in order.</summary>
    public static int FallbackPolicyAckSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6673.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
