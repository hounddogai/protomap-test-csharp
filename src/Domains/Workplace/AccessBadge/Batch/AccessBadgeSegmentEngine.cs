namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge segment calculation.</summary>
public interface IAccessBadgeSegmentModifier
{
    string Describe();
    double Adjust(AccessBadgeSegment item, double valueValue);
}

public sealed record FlaggedAccessBadgeSegmentAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeSegmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeSegmentAdjuster(double Step) : IAccessBadgeSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAccessBadgeSegmentAdjuster(double Percent) : IAccessBadgeSegmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAccessBadgeSegmentAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeSegment item, double valueValue)
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

/// <summary>Runs the access badge segment steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeSegmentEngine(List<IAccessBadgeSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeSegmentEngine DefaultAccessBadgeSegmentEngine(Registry flags)
    {
        return new AccessBadgeSegmentEngine(
            new List<IAccessBadgeSegmentModifier>
            {
                new FlaggedAccessBadgeSegmentAdjuster(flags, "access-badge-segment-adaptive", 8.5),
                new RoundingAccessBadgeSegmentAdjuster(0.25),
                new PercentAccessBadgeSegmentAdjuster(-8.9),
                new TieredAccessBadgeSegmentAdjuster(
                    new List<double> { 1854.0, 5209.0, 7257.0 },
                    new List<double> { 1.11, 1.14, 0.95 }),
            },
            new List<string>(),
            1.7,
            93223.2);
    }

    public double Run(AccessBadgeSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
