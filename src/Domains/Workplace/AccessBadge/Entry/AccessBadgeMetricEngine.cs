namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge metric calculation.</summary>
public interface IAccessBadgeMetricStep
{
    string Describe();
    double Adjust(AccessBadgeMetric item, double valueValue);
}

public sealed record AccessBadgeMetricRatioOffsetAdjuster(double Weight) : IAccessBadgeMetricStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(AccessBadgeMetric item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record FlaggedAccessBadgeMetricAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeMetricStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelAccessBadgeMetricAdjuster(Dictionary<string, double> Factors) : IAccessBadgeMetricStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AccessBadgeMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAccessBadgeMetricAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeMetricStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeMetric item, double valueValue)
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

public sealed record RoundingAccessBadgeMetricAdjuster(double Step) : IAccessBadgeMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the access badge metric steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeMetricEngine(List<IAccessBadgeMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeMetricEngine DefaultAccessBadgeMetricEngine(Registry flags)
    {
        return new AccessBadgeMetricEngine(
            new List<IAccessBadgeMetricStep>
            {
                new AccessBadgeMetricRatioOffsetAdjuster(0.13),
                new FlaggedAccessBadgeMetricAdjuster(flags, "access-badge-metric-lenient", -3.0),
                new LabelAccessBadgeMetricAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.34, ["west"] = 1.08 }),
                new TieredAccessBadgeMetricAdjuster(
                    new List<double> { 4420.0, 6647.0, 7875.0 },
                    new List<double> { 0.92, 0.95, 0.91 }),
                new RoundingAccessBadgeMetricAdjuster(1.0),
            },
            new List<string>(),
            2.9,
            73000.0);
    }

    public double Price(AccessBadgeMetric item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
