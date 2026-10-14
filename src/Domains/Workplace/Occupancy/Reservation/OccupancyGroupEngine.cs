namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy group calculation.</summary>
public interface IOccupancyGroupAdjuster
{
    string Describe();
    double Adjust(OccupancyGroup item, double valueValue);
}

public sealed record FlaggedOccupancyGroupAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyGroupAdjuster(double Cap) : IOccupancyGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameOccupancyGroupAdjuster(Dictionary<string, double> Factors) : IOccupancyGroupAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(OccupancyGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentOccupancyGroupAdjuster(double Percent) : IOccupancyGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record OccupancyGroupMarginOffsetAdjuster(double Weight) : IOccupancyGroupAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(OccupancyGroup item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the occupancy group steps in order and keeps a trace of each.</summary>
public sealed class OccupancyGroupEngine(List<IOccupancyGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyGroupEngine DefaultOccupancyGroupEngine(Registry flags)
    {
        return new OccupancyGroupEngine(
            new List<IOccupancyGroupAdjuster>
            {
                new FlaggedOccupancyGroupAdjuster(flags, "occupancy-group-pending", 2.4),
                new CapOccupancyGroupAdjuster(2859.0),
                new NameOccupancyGroupAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.84, ["retail"] = 1.33, ["north"] = 0.74 }),
                new PercentOccupancyGroupAdjuster(7.6),
                new OccupancyGroupMarginOffsetAdjuster(1.54),
            },
            new List<string>(),
            3.3,
            96756.1);
    }

    public double Compute(OccupancyGroup item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
