namespace Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Functions of rolling benchmark proposal policy.</summary>
public static class RollingBenchmarkProposalPolicy
{
    /// <summary>Puts a rolling benchmark proposal code in its canonical RBP- form.</summary>
    public static string NormalizeRollingBenchmarkProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a rolling benchmark proposal score falls in.</summary>
    public static string ClassifyRollingBenchmarkProposal(double score)
    {
        if (score < 46.2)
        {
            return "ok";
        }
        if (score < 74.0)
        {
            return "high";
        }
        if (score < 90.7)
        {
            return "critical";
        }
        return "watch";
    }
}
