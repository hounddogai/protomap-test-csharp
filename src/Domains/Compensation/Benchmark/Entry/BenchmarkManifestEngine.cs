namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark manifest calculation.</summary>
public interface IBenchmarkManifestStep
{
    string Describe();
    double Adjust(BenchmarkManifest item, double valueValue);
}

public sealed record TieredBenchmarkManifestAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkManifestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkManifest item, double valueValue)
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

public sealed record CapBenchmarkManifestAdjuster(double Cap) : IBenchmarkManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBenchmarkManifestAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkManifestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBenchmarkManifestAdjuster(double Step) : IBenchmarkManifestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelBenchmarkManifestAdjuster(Dictionary<string, double> Factors) : IBenchmarkManifestStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BenchmarkManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBenchmarkManifestAdjuster(double Percent) : IBenchmarkManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the benchmark manifest steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkManifestEngine(List<IBenchmarkManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkManifestEngine DefaultBenchmarkManifestEngine(Registry flags)
    {
        return new BenchmarkManifestEngine(
            new List<IBenchmarkManifestStep>
            {
                new TieredBenchmarkManifestAdjuster(
                    new List<double> { 1203.0, 1784.0, 1942.0, 6009.0 },
                    new List<double> { 0.99, 0.88, 1.04, 1.18 }),
                new CapBenchmarkManifestAdjuster(15970.0),
                new FlaggedBenchmarkManifestAdjuster(flags, "benchmark-manifest-rolling", -6.5),
                new RoundingBenchmarkManifestAdjuster(0.5),
                new LabelBenchmarkManifestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.78, ["east"] = 1.23, ["central"] = 0.7 }),
                new PercentBenchmarkManifestAdjuster(-20.7),
            },
            new List<string>(),
            2.2,
            14541.5);
    }

    public double Price(BenchmarkManifest item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
