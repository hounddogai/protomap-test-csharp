namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual reorg metric calculation.</summary>
public interface IManualReorgMetricAdjuster
{
    string Describe();
    double Adjust(ManualReorgMetric item, double valueValue);
}

public sealed record FlaggedManualReorgMetricAdjuster(Registry Flags, string Flag, double Percent) : IManualReorgMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualReorgMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualReorgMetricAdjuster(List<double> Bounds, List<double> Rates) : IManualReorgMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualReorgMetric item, double valueValue)
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

public sealed record PercentManualReorgMetricAdjuster(double Percent) : IManualReorgMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualReorgMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualReorgMetricAdjuster(double Minimum) : IManualReorgMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualReorgMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualReorgMetricAdjuster(double Cap) : IManualReorgMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualReorgMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual reorg metric steps in order and keeps a trace of each.</summary>
public sealed class ManualReorgMetricEngine(List<IManualReorgMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualReorgMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualReorgMetricEngine DefaultManualReorgMetricEngine(Registry flags)
    {
        return new ManualReorgMetricEngine(
            new List<IManualReorgMetricAdjuster>
            {
                new FlaggedManualReorgMetricAdjuster(flags, "manual-reorg-metric-legacy", 2.5),
                new TieredManualReorgMetricAdjuster(
                    new List<double> { 1197.0, 1370.0, 1796.0, 5845.0 },
                    new List<double> { 1.11, 1.13, 1.11, 0.98 }),
                new PercentManualReorgMetricAdjuster(18.0),
                new MinimumManualReorgMetricAdjuster(11.7),
                new CapManualReorgMetricAdjuster(31696.0),
            },
            new List<string>(),
            1.1,
            95640.6);
    }

    public double Price(ManualReorgMetric item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualReorgMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
