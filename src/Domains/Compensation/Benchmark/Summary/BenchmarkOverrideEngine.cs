namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the benchmark override calculation.</summary>
public interface IBenchmarkOverrideAdjuster
{
    string Describe();
    double Adjust(BenchmarkOverride item, double valueValue);
}

public sealed record MinimumBenchmarkOverrideAdjuster(double Minimum) : IBenchmarkOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBenchmarkOverrideAdjuster(double Percent) : IBenchmarkOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBenchmarkOverrideAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBenchmarkOverrideAdjuster(double Cap) : IBenchmarkOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the benchmark override steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkOverrideEngine(List<IBenchmarkOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkOverrideEngine DefaultBenchmarkOverrideEngine(Registry flags)
    {
        return new BenchmarkOverrideEngine(
            new List<IBenchmarkOverrideAdjuster>
            {
                new MinimumBenchmarkOverrideAdjuster(36.8),
                new PercentBenchmarkOverrideAdjuster(-1.8),
                new FlaggedBenchmarkOverrideAdjuster(flags, "benchmark-override-capped", -1.0),
                new CapBenchmarkOverrideAdjuster(38099.0),
            },
            new List<string>(),
            3.3,
            11665.1);
    }

    public double Evaluate(BenchmarkOverride item)
    {
        var current = item.NetExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BenchmarkOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
