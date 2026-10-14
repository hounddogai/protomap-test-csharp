namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled occupancy snapshot calculation.</summary>
public interface IScheduledOccupancySnapshotAdjuster
{
    string Describe();
    double Adjust(ScheduledOccupancySnapshot item, double valueValue);
}

public sealed record CapScheduledOccupancySnapshotAdjuster(double Cap) : IScheduledOccupancySnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledOccupancySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedScheduledOccupancySnapshotAdjuster(Registry Flags, string Flag, double Percent) : IScheduledOccupancySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledOccupancySnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameScheduledOccupancySnapshotAdjuster(Dictionary<string, double> Factors) : IScheduledOccupancySnapshotAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledOccupancySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingScheduledOccupancySnapshotAdjuster(double Step) : IScheduledOccupancySnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledOccupancySnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredScheduledOccupancySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IScheduledOccupancySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledOccupancySnapshot item, double valueValue)
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

/// <summary>Runs the scheduled occupancy snapshot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledOccupancySnapshotEngine(List<IScheduledOccupancySnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledOccupancySnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledOccupancySnapshotEngine DefaultScheduledOccupancySnapshotEngine(Registry flags)
    {
        return new ScheduledOccupancySnapshotEngine(
            new List<IScheduledOccupancySnapshotAdjuster>
            {
                new CapScheduledOccupancySnapshotAdjuster(16054.0),
                new FlaggedScheduledOccupancySnapshotAdjuster(flags, "scheduled-occupancy-snapshot-scheduled", 9.0),
                new NameScheduledOccupancySnapshotAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.98, ["online"] = 0.71, ["central"] = 1.26 }),
                new RoundingScheduledOccupancySnapshotAdjuster(0.5),
                new TieredScheduledOccupancySnapshotAdjuster(
                    new List<double> { 5724.0, 8400.0, 8628.0 },
                    new List<double> { 1.17, 1.04, 0.94 }),
            },
            new List<string>(),
            1.6,
            70332.8);
    }

    public double Compute(ScheduledOccupancySnapshot item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledOccupancySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
