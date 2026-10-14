namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark proposal policy.</summary>
public static class BenchmarkProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkProposal()
    {
        var policy = Retry.DefaultPolicy("benchmark-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a benchmark proposal score falls in.</summary>
    public static string ClassifyBenchmarkProposal(double score)
    {
        if (score < 6.9)
        {
            return "watch";
        }
        if (score < 32.9)
        {
            return "high";
        }
        if (score < 47.7)
        {
            return "medium";
        }
        return "low";
    }

    /// <summary>Puts a benchmark proposal code in its canonical BP- form.</summary>
    public static string NormalizeBenchmarkProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
