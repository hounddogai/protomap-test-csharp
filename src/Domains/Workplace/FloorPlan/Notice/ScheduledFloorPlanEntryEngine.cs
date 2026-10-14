namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled floor plan entry calculation.</summary>
public interface IScheduledFloorPlanEntryFactor
{
    string Describe();
    double Adjust(ScheduledFloorPlanEntry item, double valueValue);
}

public sealed record FlaggedScheduledFloorPlanEntryAdjuster(Registry Flags, string Flag, double Percent) : IScheduledFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledFloorPlanEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledFloorPlanEntryAdjuster(double Step) : IScheduledFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledFloorPlanEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScheduledFloorPlanEntryAdjuster(double Percent) : IScheduledFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledFloorPlanEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledFloorPlanEntryAdjuster(double Minimum) : IScheduledFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledFloorPlanEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledFloorPlanEntryWeightOffsetAdjuster(double Weight) : IScheduledFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(ScheduledFloorPlanEntry item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapScheduledFloorPlanEntryAdjuster(double Cap) : IScheduledFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledFloorPlanEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled floor plan entry steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFloorPlanEntryEngine(List<IScheduledFloorPlanEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFloorPlanEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFloorPlanEntryEngine DefaultScheduledFloorPlanEntryEngine(Registry flags)
    {
        return new ScheduledFloorPlanEntryEngine(
            new List<IScheduledFloorPlanEntryFactor>
            {
                new FlaggedScheduledFloorPlanEntryAdjuster(flags, "scheduled-floor-plan-entry-capped", -5.3),
                new RoundingScheduledFloorPlanEntryAdjuster(1.0),
                new PercentScheduledFloorPlanEntryAdjuster(11.1),
                new MinimumScheduledFloorPlanEntryAdjuster(24.4),
                new ScheduledFloorPlanEntryWeightOffsetAdjuster(1.76),
                new CapScheduledFloorPlanEntryAdjuster(10814.0),
            },
            new List<string>(),
            4.0,
            83054.4);
    }

    public double Run(ScheduledFloorPlanEntry item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFloorPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
