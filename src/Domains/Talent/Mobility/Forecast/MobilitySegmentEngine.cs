namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the mobility segment calculation.</summary>
public interface IMobilitySegmentAdjuster
{
    string Describe();
    double Adjust(MobilitySegment item, double valueValue);
}

public sealed record MinimumMobilitySegmentAdjuster(double Minimum) : IMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilitySegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredMobilitySegmentAdjuster(List<double> Bounds, List<double> Rates) : IMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilitySegment item, double valueValue)
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

public sealed record MobilitySegmentRetriesOffsetAdjuster(double Weight) : IMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(MobilitySegment item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedMobilitySegmentAdjuster(Registry Flags, string Flag, double Percent) : IMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilitySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMobilitySegmentAdjuster(double Cap) : IMobilitySegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilitySegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameMobilitySegmentAdjuster(Dictionary<string, double> Factors) : IMobilitySegmentAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(MobilitySegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the mobility segment steps in order and keeps a trace of each.</summary>
public sealed class MobilitySegmentEngine(List<IMobilitySegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilitySegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilitySegmentEngine DefaultMobilitySegmentEngine(Registry flags)
    {
        return new MobilitySegmentEngine(
            new List<IMobilitySegmentAdjuster>
            {
                new MinimumMobilitySegmentAdjuster(37.8),
                new TieredMobilitySegmentAdjuster(
                    new List<double> { 978.0, 2955.0, 7516.0, 9713.0 },
                    new List<double> { 1.04, 1.1, 0.98, 0.93 }),
                new MobilitySegmentRetriesOffsetAdjuster(0.32),
                new FlaggedMobilitySegmentAdjuster(flags, "mobility-segment-primary", -2.2),
                new CapMobilitySegmentAdjuster(46509.0),
                new NameMobilitySegmentAdjuster(new Dictionary<string, double> { ["retail"] = 1.31 }),
            },
            new List<string>(),
            2.6,
            34855.6);
    }

    public double Price(MobilitySegment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MobilitySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
