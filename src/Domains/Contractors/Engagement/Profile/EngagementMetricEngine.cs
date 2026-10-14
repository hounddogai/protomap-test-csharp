namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement metric calculation.</summary>
public interface IEngagementMetricModifier
{
    string Describe();
    double Adjust(EngagementMetric item, double valueValue);
}

public sealed record FlaggedEngagementMetricAdjuster(Registry Flags, string Flag, double Percent) : IEngagementMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementMetricAdjuster(double Minimum) : IEngagementMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapEngagementMetricAdjuster(double Cap) : IEngagementMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the engagement metric steps in order and keeps a trace of each.</summary>
public sealed class EngagementMetricEngine(List<IEngagementMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementMetricEngine DefaultEngagementMetricEngine(Registry flags)
    {
        return new EngagementMetricEngine(
            new List<IEngagementMetricModifier>
            {
                new FlaggedEngagementMetricAdjuster(flags, "engagement-metric-tiered", -3.0),
                new MinimumEngagementMetricAdjuster(4.6),
                new CapEngagementMetricAdjuster(7101.0),
            },
            new List<string>(),
            1.7,
            59918.7);
    }

    public double Compute(EngagementMetric item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
