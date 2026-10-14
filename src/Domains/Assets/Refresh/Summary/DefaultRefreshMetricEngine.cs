namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default refresh metric calculation.</summary>
public interface IDefaultRefreshMetricFactor
{
    string Describe();
    double Adjust(DefaultRefreshMetric item, double valueValue);
}

public sealed record RoundingDefaultRefreshMetricAdjuster(double Step) : IDefaultRefreshMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultRefreshMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultRefreshMetricAdjuster(Registry Flags, string Flag, double Percent) : IDefaultRefreshMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultRefreshMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultRefreshMetricRetriesOffsetAdjuster(double Weight) : IDefaultRefreshMetricFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(DefaultRefreshMetric item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the default refresh metric steps in order and keeps a trace of each.</summary>
public sealed class DefaultRefreshMetricEngine(List<IDefaultRefreshMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultRefreshMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultRefreshMetricEngine DefaultDefaultRefreshMetricEngine(Registry flags)
    {
        return new DefaultRefreshMetricEngine(
            new List<IDefaultRefreshMetricFactor>
            {
                new RoundingDefaultRefreshMetricAdjuster(1.0),
                new FlaggedDefaultRefreshMetricAdjuster(flags, "default-refresh-metric-legacy", -8.7),
                new DefaultRefreshMetricRetriesOffsetAdjuster(0.26),
            },
            new List<string>(),
            3.7,
            11986.7);
    }

    public double Compute(DefaultRefreshMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultRefreshMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
