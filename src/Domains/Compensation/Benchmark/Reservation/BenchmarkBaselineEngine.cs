namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark baseline calculation.</summary>
public interface IBenchmarkBaselineStep
{
    string Describe();
    double Adjust(BenchmarkBaseline item, double valueValue);
}

public sealed record FlaggedBenchmarkBaselineAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBenchmarkBaselineAdjuster(double Percent) : IBenchmarkBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBenchmarkBaselineAdjuster(double Step) : IBenchmarkBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the benchmark baseline steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkBaselineEngine(List<IBenchmarkBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkBaselineEngine DefaultBenchmarkBaselineEngine(Registry flags)
    {
        return new BenchmarkBaselineEngine(
            new List<IBenchmarkBaselineStep>
            {
                new FlaggedBenchmarkBaselineAdjuster(flags, "benchmark-baseline-scheduled", 9.2),
                new PercentBenchmarkBaselineAdjuster(1.2),
                new RoundingBenchmarkBaselineAdjuster(0.25),
            },
            new List<string>(),
            1.3,
            15910.6);
    }

    public double Compute(BenchmarkBaseline item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
