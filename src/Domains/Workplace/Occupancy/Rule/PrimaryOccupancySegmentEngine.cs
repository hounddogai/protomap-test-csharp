namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary occupancy segment calculation.</summary>
public interface IPrimaryOccupancySegmentAdjuster
{
    string Describe();
    double Adjust(PrimaryOccupancySegment item, double valueValue);
}

public sealed record CapPrimaryOccupancySegmentAdjuster(double Cap) : IPrimaryOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryOccupancySegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryPrimaryOccupancySegmentAdjuster(Dictionary<string, double> Factors) : IPrimaryOccupancySegmentAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PrimaryOccupancySegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryOccupancySegmentAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryOccupancySegment item, double valueValue)
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

public sealed record FlaggedPrimaryOccupancySegmentAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryOccupancySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary occupancy segment steps in order and keeps a trace of each.</summary>
public sealed class PrimaryOccupancySegmentEngine(List<IPrimaryOccupancySegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryOccupancySegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryOccupancySegmentEngine DefaultPrimaryOccupancySegmentEngine(Registry flags)
    {
        return new PrimaryOccupancySegmentEngine(
            new List<IPrimaryOccupancySegmentAdjuster>
            {
                new CapPrimaryOccupancySegmentAdjuster(10403.0),
                new CountryPrimaryOccupancySegmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.13, ["online"] = 0.96, ["retail"] = 1.06 }),
                new TieredPrimaryOccupancySegmentAdjuster(
                    new List<double> { 788.0, 1534.0 },
                    new List<double> { 1.05, 1.16 }),
                new FlaggedPrimaryOccupancySegmentAdjuster(flags, "primary-occupancy-segment-seasonal", -2.3),
            },
            new List<string>(),
            0.2,
            54927.6);
    }

    public double Evaluate(PrimaryOccupancySegment item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryOccupancySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
