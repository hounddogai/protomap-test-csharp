namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy mobility segment calculation.</summary>
public interface ILegacyMobilitySegmentAdjuster
{
    string Describe();
    double Adjust(LegacyMobilitySegment item, double valueValue);
}

public sealed record LegacyMobilitySegmentRatioOffsetAdjuster(double Weight) : ILegacyMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(LegacyMobilitySegment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record FlaggedLegacyMobilitySegmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyMobilitySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyMobilitySegmentAdjuster(List<double> Bounds, List<double> Rates) : ILegacyMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyMobilitySegment item, double valueValue)
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

public sealed record PercentLegacyMobilitySegmentAdjuster(double Percent) : ILegacyMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyMobilitySegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyMobilitySegmentAdjuster(double Step) : ILegacyMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyMobilitySegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLegacyMobilitySegmentAdjuster(double Minimum) : ILegacyMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyMobilitySegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy mobility segment steps in order and keeps a trace of each.</summary>
public sealed class LegacyMobilitySegmentEngine(List<ILegacyMobilitySegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyMobilitySegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyMobilitySegmentEngine DefaultLegacyMobilitySegmentEngine(Registry flags)
    {
        return new LegacyMobilitySegmentEngine(
            new List<ILegacyMobilitySegmentAdjuster>
            {
                new LegacyMobilitySegmentRatioOffsetAdjuster(0.39),
                new FlaggedLegacyMobilitySegmentAdjuster(flags, "legacy-mobility-segment-tiered", -5.6),
                new TieredLegacyMobilitySegmentAdjuster(
                    new List<double> { 3145.0, 5423.0, 7094.0 },
                    new List<double> { 1.01, 0.99, 1.11 }),
                new PercentLegacyMobilitySegmentAdjuster(-9.4),
                new RoundingLegacyMobilitySegmentAdjuster(0.1),
                new MinimumLegacyMobilitySegmentAdjuster(14.6),
            },
            new List<string>(),
            1.8,
            60190.2);
    }

    public double Run(LegacyMobilitySegment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyMobilitySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
