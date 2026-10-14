namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the benchmark tier calculation.</summary>
public interface IBenchmarkTierStep
{
    string Describe();
    double Adjust(BenchmarkTier item, double valueValue);
}

public sealed record MinimumBenchmarkTierAdjuster(double Minimum) : IBenchmarkTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleBenchmarkTierAdjuster(Dictionary<string, double> Factors) : IBenchmarkTierStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BenchmarkTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBenchmarkTierAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBenchmarkTierAdjuster(double Percent) : IBenchmarkTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the benchmark tier steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkTierEngine(List<IBenchmarkTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkTierEngine DefaultBenchmarkTierEngine(Registry flags)
    {
        return new BenchmarkTierEngine(
            new List<IBenchmarkTierStep>
            {
                new MinimumBenchmarkTierAdjuster(43.8),
                new TitleBenchmarkTierAdjuster(new Dictionary<string, double> { ["south"] = 1.05 }),
                new FlaggedBenchmarkTierAdjuster(flags, "benchmark-tier-legacy", 6.8),
                new PercentBenchmarkTierAdjuster(-11.4),
            },
            new List<string>(),
            3.7,
            35750.4);
    }

    public double Compute(BenchmarkTier item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
