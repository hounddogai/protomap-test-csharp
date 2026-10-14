namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered desk metric calculation.</summary>
public interface ITieredDeskMetricModifier
{
    string Describe();
    double Adjust(TieredDeskMetric item, double valueValue);
}

public sealed record CapTieredDeskMetricAdjuster(double Cap) : ITieredDeskMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredDeskMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTieredDeskMetricAdjuster(double Minimum) : ITieredDeskMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredDeskMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTieredDeskMetricAdjuster(Registry Flags, string Flag, double Percent) : ITieredDeskMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredDeskMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered desk metric steps in order and keeps a trace of each.</summary>
public sealed class TieredDeskMetricEngine(List<ITieredDeskMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredDeskMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredDeskMetricEngine DefaultTieredDeskMetricEngine(Registry flags)
    {
        return new TieredDeskMetricEngine(
            new List<ITieredDeskMetricModifier>
            {
                new CapTieredDeskMetricAdjuster(23175.0),
                new MinimumTieredDeskMetricAdjuster(8.4),
                new FlaggedTieredDeskMetricAdjuster(flags, "tiered-desk-metric-tiered", -6.3),
            },
            new List<string>(),
            1.0,
            39985.1);
    }

    public double Run(TieredDeskMetric item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredDeskMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
