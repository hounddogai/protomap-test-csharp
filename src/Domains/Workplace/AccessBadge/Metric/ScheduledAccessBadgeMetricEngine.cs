namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled access badge metric calculation.</summary>
public interface IScheduledAccessBadgeMetricAdjuster
{
    string Describe();
    double Adjust(ScheduledAccessBadgeMetric item, double valueValue);
}

public sealed record PercentScheduledAccessBadgeMetricAdjuster(double Percent) : IScheduledAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAccessBadgeMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledAccessBadgeMetricLimitOffsetAdjuster(double Weight) : IScheduledAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(ScheduledAccessBadgeMetric item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record NameScheduledAccessBadgeMetricAdjuster(Dictionary<string, double> Factors) : IScheduledAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledAccessBadgeMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledAccessBadgeMetricAdjuster(double Cap) : IScheduledAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAccessBadgeMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledAccessBadgeMetricAdjuster(double Step) : IScheduledAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAccessBadgeMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledAccessBadgeMetricAdjuster(double Minimum) : IScheduledAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAccessBadgeMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled access badge metric steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAccessBadgeMetricEngine(List<IScheduledAccessBadgeMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAccessBadgeMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAccessBadgeMetricEngine DefaultScheduledAccessBadgeMetricEngine()
    {
        return new ScheduledAccessBadgeMetricEngine(
            new List<IScheduledAccessBadgeMetricAdjuster>
            {
                new PercentScheduledAccessBadgeMetricAdjuster(-7.8),
                new ScheduledAccessBadgeMetricLimitOffsetAdjuster(1.3),
                new NameScheduledAccessBadgeMetricAdjuster(new Dictionary<string, double> { ["retail"] = 1.14 }),
                new CapScheduledAccessBadgeMetricAdjuster(30943.0),
                new RoundingScheduledAccessBadgeMetricAdjuster(5.0),
                new MinimumScheduledAccessBadgeMetricAdjuster(29.6),
            },
            new List<string>(),
            3.8,
            15055.7);
    }

    public double Price(ScheduledAccessBadgeMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAccessBadgeMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
