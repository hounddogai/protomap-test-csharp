namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled visitor metric calculation.</summary>
public interface IScheduledVisitorMetricFactor
{
    string Describe();
    double Adjust(ScheduledVisitorMetric item, double valueValue);
}

public sealed record MinimumScheduledVisitorMetricAdjuster(double Minimum) : IScheduledVisitorMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledVisitorMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledVisitorMetricRateOffsetAdjuster(double Weight) : IScheduledVisitorMetricFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(ScheduledVisitorMetric item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredScheduledVisitorMetricAdjuster(List<double> Bounds, List<double> Rates) : IScheduledVisitorMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledVisitorMetric item, double valueValue)
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

public sealed record FlaggedScheduledVisitorMetricAdjuster(Registry Flags, string Flag, double Percent) : IScheduledVisitorMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledVisitorMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamScheduledVisitorMetricAdjuster(Dictionary<string, double> Factors) : IScheduledVisitorMetricFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ScheduledVisitorMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled visitor metric steps in order and keeps a trace of each.</summary>
public sealed class ScheduledVisitorMetricEngine(List<IScheduledVisitorMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledVisitorMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledVisitorMetricEngine DefaultScheduledVisitorMetricEngine(Registry flags)
    {
        return new ScheduledVisitorMetricEngine(
            new List<IScheduledVisitorMetricFactor>
            {
                new MinimumScheduledVisitorMetricAdjuster(37.8),
                new ScheduledVisitorMetricRateOffsetAdjuster(0.52),
                new TieredScheduledVisitorMetricAdjuster(
                    new List<double> { 1822.0, 3783.0, 5157.0, 6658.0 },
                    new List<double> { 0.85, 0.98, 1.15, 0.94 }),
                new FlaggedScheduledVisitorMetricAdjuster(flags, "scheduled-visitor-metric-capped", 5.6),
                new TeamScheduledVisitorMetricAdjuster(new Dictionary<string, double> { ["north"] = 1.15 }),
            },
            new List<string>(),
            3.3,
            53350.6);
    }

    public double Run(ScheduledVisitorMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledVisitorMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
