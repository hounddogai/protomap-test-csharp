namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual benchmark allocation calculation.</summary>
public interface IManualBenchmarkAllocationAdjuster
{
    string Describe();
    double Adjust(ManualBenchmarkAllocation item, double valueValue);
}

public sealed record LabelManualBenchmarkAllocationAdjuster(Dictionary<string, double> Factors) : IManualBenchmarkAllocationAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualBenchmarkAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualBenchmarkAllocationAdjuster(Registry Flags, string Flag, double Percent) : IManualBenchmarkAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualBenchmarkAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualBenchmarkAllocationAdjuster(List<double> Bounds, List<double> Rates) : IManualBenchmarkAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualBenchmarkAllocation item, double valueValue)
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

public sealed record PercentManualBenchmarkAllocationAdjuster(double Percent) : IManualBenchmarkAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualBenchmarkAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualBenchmarkAllocationAdjuster(double Step) : IManualBenchmarkAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBenchmarkAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumManualBenchmarkAllocationAdjuster(double Minimum) : IManualBenchmarkAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualBenchmarkAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual benchmark allocation steps in order and keeps a trace of each.</summary>
public sealed class ManualBenchmarkAllocationEngine(List<IManualBenchmarkAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBenchmarkAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBenchmarkAllocationEngine DefaultManualBenchmarkAllocationEngine(Registry flags)
    {
        return new ManualBenchmarkAllocationEngine(
            new List<IManualBenchmarkAllocationAdjuster>
            {
                new LabelManualBenchmarkAllocationAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.8, ["retail"] = 1.29, ["south"] = 1.19 }),
                new FlaggedManualBenchmarkAllocationAdjuster(flags, "manual-benchmark-allocation-lenient", -3.4),
                new TieredManualBenchmarkAllocationAdjuster(
                    new List<double> { 3877.0, 4243.0, 9872.0 },
                    new List<double> { 0.89, 1.08, 1.0 }),
                new PercentManualBenchmarkAllocationAdjuster(24.8),
                new RoundingManualBenchmarkAllocationAdjuster(0.1),
                new MinimumManualBenchmarkAllocationAdjuster(33.0),
            },
            new List<string>(),
            0.1,
            85420.6);
    }

    public double Run(ManualBenchmarkAllocation item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBenchmarkAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
