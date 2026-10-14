namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg metric calculation.</summary>
public interface IReorgMetricFactor
{
    string Describe();
    double Adjust(ReorgMetric item, double valueValue);
}

public sealed record FlaggedReorgMetricAdjuster(Registry Flags, string Flag, double Percent) : IReorgMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumReorgMetricAdjuster(double Minimum) : IReorgMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ReorgMetricBatchSizeOffsetAdjuster(double Weight) : IReorgMetricFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ReorgMetric item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapReorgMetricAdjuster(double Cap) : IReorgMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the reorg metric steps in order and keeps a trace of each.</summary>
public sealed class ReorgMetricEngine(List<IReorgMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgMetricEngine DefaultReorgMetricEngine(Registry flags)
    {
        return new ReorgMetricEngine(
            new List<IReorgMetricFactor>
            {
                new FlaggedReorgMetricAdjuster(flags, "reorg-metric-global", -7.5),
                new MinimumReorgMetricAdjuster(3.8),
                new ReorgMetricBatchSizeOffsetAdjuster(0.5),
                new CapReorgMetricAdjuster(49701.0),
            },
            new List<string>(),
            4.1,
            19191.5);
    }

    public double Run(ReorgMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
