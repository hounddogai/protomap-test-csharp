namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark profile calculation.</summary>
public interface IBenchmarkProfileModifier
{
    string Describe();
    double Adjust(BenchmarkProfile item, double valueValue);
}

public sealed record RoundingBenchmarkProfileAdjuster(double Step) : IBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBenchmarkProfileAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBenchmarkProfileAdjuster(double Minimum) : IBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the benchmark profile steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkProfileEngine(List<IBenchmarkProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkProfileEngine DefaultBenchmarkProfileEngine(Registry flags)
    {
        return new BenchmarkProfileEngine(
            new List<IBenchmarkProfileModifier>
            {
                new RoundingBenchmarkProfileAdjuster(0.05),
                new FlaggedBenchmarkProfileAdjuster(flags, "benchmark-profile-capped", -3.2),
                new MinimumBenchmarkProfileAdjuster(31.5),
            },
            new List<string>(),
            0.1,
            64976.7);
    }

    public double Run(BenchmarkProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
