namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy target calculation.</summary>
public interface IOccupancyTargetAdjuster
{
    string Describe();
    double Adjust(OccupancyTarget item, double valueValue);
}

public sealed record OccupancyTargetHoursOffsetAdjuster(double Weight) : IOccupancyTargetAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(OccupancyTarget item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record MinimumOccupancyTargetAdjuster(double Minimum) : IOccupancyTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOccupancyTargetAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the occupancy target steps in order and keeps a trace of each.</summary>
public sealed class OccupancyTargetEngine(List<IOccupancyTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyTargetEngine DefaultOccupancyTargetEngine(Registry flags)
    {
        return new OccupancyTargetEngine(
            new List<IOccupancyTargetAdjuster>
            {
                new OccupancyTargetHoursOffsetAdjuster(0.86),
                new MinimumOccupancyTargetAdjuster(34.4),
                new FlaggedOccupancyTargetAdjuster(flags, "occupancy-target-strict", -6.3),
            },
            new List<string>(),
            1.8,
            86504.8);
    }

    public double Compute(OccupancyTarget item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
