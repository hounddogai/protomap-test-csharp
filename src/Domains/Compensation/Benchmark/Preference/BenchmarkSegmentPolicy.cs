namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark segment policy.</summary>
public static class BenchmarkSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkSegment()
    {
        var policy = Retry.DefaultPolicy("benchmark-segment");
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

    public static List<string> MergeBenchmarkSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a benchmark segment score falls in.</summary>
    public static string ClassifyBenchmarkSegment(double score)
    {
        if (score < 11.0)
        {
            return "ok";
        }
        if (score < 72.7)
        {
            return "low";
        }
        if (score < 98.2)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 25529.0, in order.</summary>
    public static int BenchmarkSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25529.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
