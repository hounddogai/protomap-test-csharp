namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the benchmark digest calculation.</summary>
public interface IBenchmarkDigestFactor
{
    string Describe();
    double Adjust(BenchmarkDigest item, double valueValue);
}

public sealed record CapBenchmarkDigestAdjuster(double Cap) : IBenchmarkDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBenchmarkDigestAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BenchmarkDigestCostOffsetAdjuster(double Weight) : IBenchmarkDigestFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(BenchmarkDigest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the benchmark digest steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkDigestEngine(List<IBenchmarkDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkDigestEngine DefaultBenchmarkDigestEngine(Registry flags)
    {
        return new BenchmarkDigestEngine(
            new List<IBenchmarkDigestFactor>
            {
                new CapBenchmarkDigestAdjuster(36095.0),
                new FlaggedBenchmarkDigestAdjuster(flags, "benchmark-digest-default", 7.1),
                new BenchmarkDigestCostOffsetAdjuster(0.69),
            },
            new List<string>(),
            2.2,
            50971.2);
    }

    public double Price(BenchmarkDigest item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
