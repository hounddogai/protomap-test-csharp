namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark window calculation.</summary>
public interface IBenchmarkWindowModifier
{
    string Describe();
    double Adjust(BenchmarkWindow item, double valueValue);
}

public sealed record FlaggedBenchmarkWindowAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameBenchmarkWindowAdjuster(Dictionary<string, double> Factors) : IBenchmarkWindowModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BenchmarkWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBenchmarkWindowAdjuster(double Minimum) : IBenchmarkWindowModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBenchmarkWindowAdjuster(double Percent) : IBenchmarkWindowModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BenchmarkWindowRateOffsetAdjuster(double Weight) : IBenchmarkWindowModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(BenchmarkWindow item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the benchmark window steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkWindowEngine(List<IBenchmarkWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkWindowEngine DefaultBenchmarkWindowEngine(Registry flags)
    {
        return new BenchmarkWindowEngine(
            new List<IBenchmarkWindowModifier>
            {
                new FlaggedBenchmarkWindowAdjuster(flags, "benchmark-window-pending", -0.8),
                new NameBenchmarkWindowAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.37, ["central"] = 1.13 }),
                new MinimumBenchmarkWindowAdjuster(9.5),
                new PercentBenchmarkWindowAdjuster(-0.8),
                new BenchmarkWindowRateOffsetAdjuster(1.53),
            },
            new List<string>(),
            4.4,
            31559.5);
    }

    public double Price(BenchmarkWindow item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
