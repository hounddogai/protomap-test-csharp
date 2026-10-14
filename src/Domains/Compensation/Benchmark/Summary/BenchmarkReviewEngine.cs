namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark review calculation.</summary>
public interface IBenchmarkReviewAdjuster
{
    string Describe();
    double Adjust(BenchmarkReview item, double valueValue);
}

public sealed record FlaggedBenchmarkReviewAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBenchmarkReviewAdjuster(double Minimum) : IBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBenchmarkReviewAdjuster(double Percent) : IBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentBenchmarkReviewAdjuster(Dictionary<string, double> Factors) : IBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(BenchmarkReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the benchmark review steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkReviewEngine(List<IBenchmarkReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkReviewEngine DefaultBenchmarkReviewEngine(Registry flags)
    {
        return new BenchmarkReviewEngine(
            new List<IBenchmarkReviewAdjuster>
            {
                new FlaggedBenchmarkReviewAdjuster(flags, "benchmark-review-fallback", 3.0),
                new MinimumBenchmarkReviewAdjuster(34.5),
                new PercentBenchmarkReviewAdjuster(-3.7),
                new DepartmentBenchmarkReviewAdjuster(new Dictionary<string, double> { ["retail"] = 1.36 }),
            },
            new List<string>(),
            1.8,
            29019.8);
    }

    public double Run(BenchmarkReview item)
    {
        var current = item.ProjectedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BenchmarkReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
