namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark notice policy.</summary>
public static class BenchmarkNoticePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkNotice()
    {
        var policy = Retry.DefaultPolicy("benchmark-notice");
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

    public static int BenchmarkNoticeSpan(int start, int end)
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

    public static string BenchmarkNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a benchmark notice score falls in.</summary>
    public static string ClassifyBenchmarkNotice(double score)
    {
        if (score < 35.3)
        {
            return "critical";
        }
        if (score < 48.9)
        {
            return "high";
        }
        return "low";
    }
}
