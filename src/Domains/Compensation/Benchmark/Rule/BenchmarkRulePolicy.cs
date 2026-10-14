namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark rule policy.</summary>
public static class BenchmarkRulePolicy
{
    /// <summary>Puts a benchmark rule code in its canonical BR- form.</summary>
    public static string NormalizeBenchmarkRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkRule()
    {
        var policy = Retry.DefaultPolicy("benchmark-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a benchmark rule score falls in.</summary>
    public static string ClassifyBenchmarkRule(double score)
    {
        if (score < 47.0)
        {
            return "minimal";
        }
        if (score < 54.8)
        {
            return "low";
        }
        if (score < 56.8)
        {
            return "ok";
        }
        return "high";
    }

    public static int BenchmarkRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
