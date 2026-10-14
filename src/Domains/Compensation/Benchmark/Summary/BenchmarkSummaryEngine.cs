namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark summary calculation.</summary>
public interface IBenchmarkSummaryAdjuster
{
    string Describe();
    double Adjust(BenchmarkSummary item, double valueValue);
}

public sealed record MinimumBenchmarkSummaryAdjuster(double Minimum) : IBenchmarkSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBenchmarkSummaryAdjuster(double Percent) : IBenchmarkSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBenchmarkSummaryAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBenchmarkSummaryAdjuster(double Cap) : IBenchmarkSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the benchmark summary steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkSummaryEngine(List<IBenchmarkSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkSummaryEngine DefaultBenchmarkSummaryEngine(Registry flags)
    {
        return new BenchmarkSummaryEngine(
            new List<IBenchmarkSummaryAdjuster>
            {
                new MinimumBenchmarkSummaryAdjuster(14.5),
                new PercentBenchmarkSummaryAdjuster(12.4),
                new FlaggedBenchmarkSummaryAdjuster(flags, "benchmark-summary-weighted", -6.2),
                new CapBenchmarkSummaryAdjuster(36573.0),
            },
            new List<string>(),
            4.3,
            87962.6);
    }

    public double Compute(BenchmarkSummary item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
