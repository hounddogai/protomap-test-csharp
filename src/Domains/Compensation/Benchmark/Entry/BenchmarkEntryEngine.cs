namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark entry calculation.</summary>
public interface IBenchmarkEntryAdjuster
{
    string Describe();
    double Adjust(BenchmarkEntry item, double valueValue);
}

public sealed record FlaggedBenchmarkEntryAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleBenchmarkEntryAdjuster(Dictionary<string, double> Factors) : IBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BenchmarkEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBenchmarkEntryAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkEntry item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record RoundingBenchmarkEntryAdjuster(double Step) : IBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record BenchmarkEntryBatchSizeOffsetAdjuster(double Weight) : IBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(BenchmarkEntry item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the benchmark entry steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkEntryEngine(List<IBenchmarkEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkEntryEngine DefaultBenchmarkEntryEngine(Registry flags)
    {
        return new BenchmarkEntryEngine(
            new List<IBenchmarkEntryAdjuster>
            {
                new FlaggedBenchmarkEntryAdjuster(flags, "benchmark-entry-default", -9.0),
                new TitleBenchmarkEntryAdjuster(new Dictionary<string, double> { ["online"] = 0.71 }),
                new TieredBenchmarkEntryAdjuster(new List<double> { 3950.0, 8981.0 }, new List<double> { 1.13, 0.9 }),
                new RoundingBenchmarkEntryAdjuster(5.0),
                new BenchmarkEntryBatchSizeOffsetAdjuster(1.46),
            },
            new List<string>(),
            0.7,
            23167.6);
    }

    public double Run(BenchmarkEntry item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
