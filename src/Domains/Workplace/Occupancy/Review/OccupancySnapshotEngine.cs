namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy snapshot calculation.</summary>
public interface IOccupancySnapshotModifier
{
    string Describe();
    double Adjust(OccupancySnapshot item, double valueValue);
}

public sealed record RoundingOccupancySnapshotAdjuster(double Step) : IOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancySnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapOccupancySnapshotAdjuster(double Cap) : IOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentOccupancySnapshotAdjuster(Dictionary<string, double> Factors) : IOccupancySnapshotModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(OccupancySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the occupancy snapshot steps in order and keeps a trace of each.</summary>
public sealed class OccupancySnapshotEngine(List<IOccupancySnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancySnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancySnapshotEngine DefaultOccupancySnapshotEngine()
    {
        return new OccupancySnapshotEngine(
            new List<IOccupancySnapshotModifier>
            {
                new RoundingOccupancySnapshotAdjuster(0.25),
                new CapOccupancySnapshotAdjuster(28284.0),
                new SegmentOccupancySnapshotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.86, ["north"] = 0.96 }),
            },
            new List<string>(),
            0.4,
            63202.6);
    }

    public double Run(OccupancySnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
