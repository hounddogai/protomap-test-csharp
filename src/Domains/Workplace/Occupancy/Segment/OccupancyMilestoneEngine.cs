namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy milestone calculation.</summary>
public interface IOccupancyMilestoneFactor
{
    string Describe();
    double Adjust(OccupancyMilestone item, double valueValue);
}

public sealed record PercentOccupancyMilestoneAdjuster(double Percent) : IOccupancyMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOccupancyMilestoneAdjuster(double Minimum) : IOccupancyMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOccupancyMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyMilestoneAdjuster(double Cap) : IOccupancyMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the occupancy milestone steps in order and keeps a trace of each.</summary>
public sealed class OccupancyMilestoneEngine(List<IOccupancyMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyMilestoneEngine DefaultOccupancyMilestoneEngine(Registry flags)
    {
        return new OccupancyMilestoneEngine(
            new List<IOccupancyMilestoneFactor>
            {
                new PercentOccupancyMilestoneAdjuster(-7.5),
                new MinimumOccupancyMilestoneAdjuster(13.9),
                new FlaggedOccupancyMilestoneAdjuster(flags, "occupancy-milestone-strict", -0.8),
                new CapOccupancyMilestoneAdjuster(20052.0),
            },
            new List<string>(),
            0.4,
            71375.3);
    }

    public double Run(OccupancyMilestone item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
