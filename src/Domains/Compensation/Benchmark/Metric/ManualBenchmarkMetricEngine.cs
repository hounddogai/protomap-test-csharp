namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual benchmark metric calculation.</summary>
public interface IManualBenchmarkMetricAdjuster
{
    string Describe();
    double Adjust(ManualBenchmarkMetric item, double valueValue);
}

public sealed record TieredManualBenchmarkMetricAdjuster(List<double> Bounds, List<double> Rates) : IManualBenchmarkMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualBenchmarkMetric item, double valueValue)
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

public sealed record CapManualBenchmarkMetricAdjuster(double Cap) : IManualBenchmarkMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualBenchmarkMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumManualBenchmarkMetricAdjuster(double Minimum) : IManualBenchmarkMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualBenchmarkMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelManualBenchmarkMetricAdjuster(Dictionary<string, double> Factors) : IManualBenchmarkMetricAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualBenchmarkMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingManualBenchmarkMetricAdjuster(double Step) : IManualBenchmarkMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBenchmarkMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualBenchmarkMetricAdjuster(Registry Flags, string Flag, double Percent) : IManualBenchmarkMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualBenchmarkMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual benchmark metric steps in order and keeps a trace of each.</summary>
public sealed class ManualBenchmarkMetricEngine(List<IManualBenchmarkMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBenchmarkMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBenchmarkMetricEngine DefaultManualBenchmarkMetricEngine(Registry flags)
    {
        return new ManualBenchmarkMetricEngine(
            new List<IManualBenchmarkMetricAdjuster>
            {
                new TieredManualBenchmarkMetricAdjuster(
                    new List<double> { 1256.0, 7886.0 },
                    new List<double> { 0.93, 1.11 }),
                new CapManualBenchmarkMetricAdjuster(29015.0),
                new MinimumManualBenchmarkMetricAdjuster(6.0),
                new LabelManualBenchmarkMetricAdjuster(new Dictionary<string, double> { ["online"] = 1.2 }),
                new RoundingManualBenchmarkMetricAdjuster(1.0),
                new FlaggedManualBenchmarkMetricAdjuster(flags, "manual-benchmark-metric-adaptive", -1.6),
            },
            new List<string>(),
            2.9,
            14601.7);
    }

    public double Evaluate(ManualBenchmarkMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualBenchmarkMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
