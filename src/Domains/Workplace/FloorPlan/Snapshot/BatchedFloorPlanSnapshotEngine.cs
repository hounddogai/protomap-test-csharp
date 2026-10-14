namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched floor plan snapshot calculation.</summary>
public interface IBatchedFloorPlanSnapshotModifier
{
    string Describe();
    double Adjust(BatchedFloorPlanSnapshot item, double valueValue);
}

public sealed record PercentBatchedFloorPlanSnapshotAdjuster(double Percent) : IBatchedFloorPlanSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedFloorPlanSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedFloorPlanSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IBatchedFloorPlanSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedFloorPlanSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryBatchedFloorPlanSnapshotAdjuster(Dictionary<string, double> Factors) : IBatchedFloorPlanSnapshotModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BatchedFloorPlanSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBatchedFloorPlanSnapshotAdjuster(double Step) : IBatchedFloorPlanSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedFloorPlanSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBatchedFloorPlanSnapshotAdjuster(double Cap) : IBatchedFloorPlanSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedFloorPlanSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched floor plan snapshot steps in order and keeps a trace of each.</summary>
public sealed class BatchedFloorPlanSnapshotEngine(List<IBatchedFloorPlanSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedFloorPlanSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedFloorPlanSnapshotEngine DefaultBatchedFloorPlanSnapshotEngine(Registry flags)
    {
        return new BatchedFloorPlanSnapshotEngine(
            new List<IBatchedFloorPlanSnapshotModifier>
            {
                new PercentBatchedFloorPlanSnapshotAdjuster(18.9),
                new FlaggedBatchedFloorPlanSnapshotAdjuster(flags, "batched-floor-plan-snapshot-rolling", -1.3),
                new CountryBatchedFloorPlanSnapshotAdjuster(new Dictionary<string, double> { ["east"] = 1.38 }),
                new RoundingBatchedFloorPlanSnapshotAdjuster(0.1),
                new CapBatchedFloorPlanSnapshotAdjuster(4400.0),
            },
            new List<string>(),
            3.9,
            74520.3);
    }

    public double Evaluate(BatchedFloorPlanSnapshot item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedFloorPlanSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
