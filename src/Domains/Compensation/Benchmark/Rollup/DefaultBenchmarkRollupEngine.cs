namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default benchmark rollup calculation.</summary>
public interface IDefaultBenchmarkRollupAdjuster
{
    string Describe();
    double Adjust(DefaultBenchmarkRollup item, double valueValue);
}

public sealed record TieredDefaultBenchmarkRollupAdjuster(List<double> Bounds, List<double> Rates) : IDefaultBenchmarkRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultBenchmarkRollup item, double valueValue)
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

public sealed record DefaultBenchmarkRollupCapacityOffsetAdjuster(double Weight) : IDefaultBenchmarkRollupAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(DefaultBenchmarkRollup item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingDefaultBenchmarkRollupAdjuster(double Step) : IDefaultBenchmarkRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultBenchmarkRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultBenchmarkRollupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultBenchmarkRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultBenchmarkRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultBenchmarkRollupAdjuster(double Cap) : IDefaultBenchmarkRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultBenchmarkRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default benchmark rollup steps in order and keeps a trace of each.</summary>
public sealed class DefaultBenchmarkRollupEngine(List<IDefaultBenchmarkRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultBenchmarkRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultBenchmarkRollupEngine DefaultDefaultBenchmarkRollupEngine(Registry flags)
    {
        return new DefaultBenchmarkRollupEngine(
            new List<IDefaultBenchmarkRollupAdjuster>
            {
                new TieredDefaultBenchmarkRollupAdjuster(
                    new List<double> { 1083.0, 1458.0, 1832.0, 5898.0 },
                    new List<double> { 1.16, 1.03, 1.02, 0.96 }),
                new DefaultBenchmarkRollupCapacityOffsetAdjuster(1.71),
                new RoundingDefaultBenchmarkRollupAdjuster(0.5),
                new FlaggedDefaultBenchmarkRollupAdjuster(flags, "default-benchmark-rollup-default", 3.1),
                new CapDefaultBenchmarkRollupAdjuster(31727.0),
            },
            new List<string>(),
            1.1,
            51043.1);
    }

    public double Compute(DefaultBenchmarkRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultBenchmarkRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
