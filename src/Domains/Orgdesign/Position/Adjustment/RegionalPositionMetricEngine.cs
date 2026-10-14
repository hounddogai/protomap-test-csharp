namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional position metric calculation.</summary>
public interface IRegionalPositionMetricModifier
{
    string Describe();
    double Adjust(RegionalPositionMetric item, double valueValue);
}

public sealed record RoundingRegionalPositionMetricAdjuster(double Step) : IRegionalPositionMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPositionMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalPositionMetricAdjuster(double Percent) : IRegionalPositionMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPositionMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalPositionMetricAdjuster(Registry Flags, string Flag, double Percent) : IRegionalPositionMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalPositionMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalPositionMetricAdjuster(double Cap) : IRegionalPositionMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPositionMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleRegionalPositionMetricAdjuster(Dictionary<string, double> Factors) : IRegionalPositionMetricModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RegionalPositionMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalPositionMetricDiscountOffsetAdjuster(double Weight) : IRegionalPositionMetricModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(RegionalPositionMetric item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the regional position metric steps in order and keeps a trace of each.</summary>
public sealed class RegionalPositionMetricEngine(List<IRegionalPositionMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPositionMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPositionMetricEngine DefaultRegionalPositionMetricEngine(Registry flags)
    {
        return new RegionalPositionMetricEngine(
            new List<IRegionalPositionMetricModifier>
            {
                new RoundingRegionalPositionMetricAdjuster(1.0),
                new PercentRegionalPositionMetricAdjuster(4.1),
                new FlaggedRegionalPositionMetricAdjuster(flags, "regional-position-metric-legacy", 8.0),
                new CapRegionalPositionMetricAdjuster(46650.0),
                new TitleRegionalPositionMetricAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.21, ["online"] = 0.93 }),
                new RegionalPositionMetricDiscountOffsetAdjuster(0.84),
            },
            new List<string>(),
            3.7,
            28436.9);
    }

    public double Compute(RegionalPositionMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPositionMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
