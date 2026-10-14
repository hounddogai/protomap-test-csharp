namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered pool metric calculation.</summary>
public interface ITieredPoolMetricFactor
{
    string Describe();
    double Adjust(TieredPoolMetric item, double valueValue);
}

public sealed record TieredTieredPoolMetricAdjuster(List<double> Bounds, List<double> Rates) : ITieredPoolMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPoolMetric item, double valueValue)
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

public sealed record TieredPoolMetricRevisionOffsetAdjuster(double Weight) : ITieredPoolMetricFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(TieredPoolMetric item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record MinimumTieredPoolMetricAdjuster(double Minimum) : ITieredPoolMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredPoolMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTieredPoolMetricAdjuster(Registry Flags, string Flag, double Percent) : ITieredPoolMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPoolMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredPoolMetricAdjuster(double Step) : ITieredPoolMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPoolMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTieredPoolMetricAdjuster(double Percent) : ITieredPoolMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPoolMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered pool metric steps in order and keeps a trace of each.</summary>
public sealed class TieredPoolMetricEngine(List<ITieredPoolMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPoolMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPoolMetricEngine DefaultTieredPoolMetricEngine(Registry flags)
    {
        return new TieredPoolMetricEngine(
            new List<ITieredPoolMetricFactor>
            {
                new TieredTieredPoolMetricAdjuster(
                    new List<double> { 5308.0, 9509.0 },
                    new List<double> { 1.18, 1.02 }),
                new TieredPoolMetricRevisionOffsetAdjuster(0.12),
                new MinimumTieredPoolMetricAdjuster(13.7),
                new FlaggedTieredPoolMetricAdjuster(flags, "tiered-pool-metric-global", 2.2),
                new RoundingTieredPoolMetricAdjuster(0.1),
                new PercentTieredPoolMetricAdjuster(-17.1),
            },
            new List<string>(),
            1.4,
            28447.4);
    }

    public double Compute(TieredPoolMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredPoolMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
