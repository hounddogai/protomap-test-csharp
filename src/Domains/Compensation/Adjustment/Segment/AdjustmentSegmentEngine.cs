namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adjustment segment calculation.</summary>
public interface IAdjustmentSegmentModifier
{
    string Describe();
    double Adjust(AdjustmentSegment item, double valueValue);
}

public sealed record TieredAdjustmentSegmentAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentSegment item, double valueValue)
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

public sealed record FlaggedAdjustmentSegmentAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentSegmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdjustmentSegmentAdjuster(double Percent) : IAdjustmentSegmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdjustmentSegmentStartsAtOffsetAdjuster(double Weight) : IAdjustmentSegmentModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(AdjustmentSegment item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CategoryAdjustmentSegmentAdjuster(Dictionary<string, double> Factors) : IAdjustmentSegmentModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AdjustmentSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adjustment segment steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentSegmentEngine(List<IAdjustmentSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentSegmentEngine DefaultAdjustmentSegmentEngine(Registry flags)
    {
        return new AdjustmentSegmentEngine(
            new List<IAdjustmentSegmentModifier>
            {
                new TieredAdjustmentSegmentAdjuster(
                    new List<double> { 1997.0, 2699.0, 8579.0, 8635.0 },
                    new List<double> { 1.09, 1.17, 1.13, 1.08 }),
                new FlaggedAdjustmentSegmentAdjuster(flags, "adjustment-segment-capped", -7.1),
                new PercentAdjustmentSegmentAdjuster(22.6),
                new AdjustmentSegmentStartsAtOffsetAdjuster(1.68),
                new CategoryAdjustmentSegmentAdjuster(new Dictionary<string, double> { ["online"] = 0.97 }),
            },
            new List<string>(),
            0.6,
            66070.4);
    }

    public double Run(AdjustmentSegment item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
