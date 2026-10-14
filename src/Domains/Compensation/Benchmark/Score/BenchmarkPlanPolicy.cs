namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark plan policy.</summary>
public static class BenchmarkPlanPolicy
{
    /// <summary>Names the band a benchmark plan score falls in.</summary>
    public static string ClassifyBenchmarkPlan(double score)
    {
        if (score < 25.6)
        {
            return "watch";
        }
        if (score < 43.5)
        {
            return "critical";
        }
        if (score < 59.3)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Puts a benchmark plan code in its canonical BP- form.</summary>
    public static string NormalizeBenchmarkPlanCode(string code)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkPlan()
    {
        var policy = Retry.DefaultPolicy("benchmark-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
