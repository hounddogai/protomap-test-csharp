namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy segment calculation.</summary>
public interface IOccupancySegmentFactor
{
    string Describe();
    double Adjust(OccupancySegment item, double valueValue);
}

public sealed record CapOccupancySegmentAdjuster(double Cap) : IOccupancySegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancySegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentOccupancySegmentAdjuster(double Percent) : IOccupancySegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancySegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOccupancySegmentAdjuster(double Minimum) : IOccupancySegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancySegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredOccupancySegmentAdjuster(List<double> Bounds, List<double> Rates) : IOccupancySegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancySegment item, double valueValue)
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

public sealed record FlaggedOccupancySegmentAdjuster(Registry Flags, string Flag, double Percent) : IOccupancySegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingOccupancySegmentAdjuster(double Step) : IOccupancySegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancySegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the occupancy segment steps in order and keeps a trace of each.</summary>
public sealed class OccupancySegmentEngine(List<IOccupancySegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancySegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancySegmentEngine DefaultOccupancySegmentEngine(Registry flags)
    {
        return new OccupancySegmentEngine(
            new List<IOccupancySegmentFactor>
            {
                new CapOccupancySegmentAdjuster(38232.0),
                new PercentOccupancySegmentAdjuster(-6.5),
                new MinimumOccupancySegmentAdjuster(26.9),
                new TieredOccupancySegmentAdjuster(
                    new List<double> { 1356.0, 5666.0, 9655.0 },
                    new List<double> { 0.83, 0.94, 0.99 }),
                new FlaggedOccupancySegmentAdjuster(flags, "occupancy-segment-manual", 9.2),
                new RoundingOccupancySegmentAdjuster(0.05),
            },
            new List<string>(),
            0.2,
            20371.6);
    }

    public double Price(OccupancySegment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
