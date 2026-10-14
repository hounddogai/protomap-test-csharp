namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled floor plan snapshot calculation.</summary>
public interface IScheduledFloorPlanSnapshotAdjuster
{
    string Describe();
    double Adjust(ScheduledFloorPlanSnapshot item, double valueValue);
}

public sealed record TieredScheduledFloorPlanSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IScheduledFloorPlanSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledFloorPlanSnapshot item, double valueValue)
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

public sealed record PercentScheduledFloorPlanSnapshotAdjuster(double Percent) : IScheduledFloorPlanSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledFloorPlanSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledFloorPlanSnapshotAdjuster(double Cap) : IScheduledFloorPlanSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledFloorPlanSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledFloorPlanSnapshotAdjuster(double Step) : IScheduledFloorPlanSnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledFloorPlanSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledFloorPlanSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IScheduledFloorPlanSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledFloorPlanSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled floor plan snapshot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFloorPlanSnapshotEngine(List<IScheduledFloorPlanSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFloorPlanSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFloorPlanSnapshotEngine DefaultScheduledFloorPlanSnapshotEngine(Registry flags)
    {
        return new ScheduledFloorPlanSnapshotEngine(
            new List<IScheduledFloorPlanSnapshotAdjuster>
            {
                new TieredScheduledFloorPlanSnapshotAdjuster(
                    new List<double> { 6767.0, 9084.0 },
                    new List<double> { 1.0, 1.05 }),
                new PercentScheduledFloorPlanSnapshotAdjuster(-7.2),
                new CapScheduledFloorPlanSnapshotAdjuster(45778.0),
                new RoundingScheduledFloorPlanSnapshotAdjuster(0.1),
                new FlaggedScheduledFloorPlanSnapshotAdjuster(flags, "scheduled-floor-plan-snapshot-primary", 2.2),
            },
            new List<string>(),
            2.0,
            32928.3);
    }

    public double Evaluate(ScheduledFloorPlanSnapshot item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFloorPlanSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
