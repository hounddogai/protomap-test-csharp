namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched occupancy snapshot calculation.</summary>
public interface IBatchedOccupancySnapshotModifier
{
    string Describe();
    double Adjust(BatchedOccupancySnapshot item, double valueValue);
}

public sealed record MinimumBatchedOccupancySnapshotAdjuster(double Minimum) : IBatchedOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedOccupancySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBatchedOccupancySnapshotAdjuster(double Percent) : IBatchedOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedOccupancySnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedOccupancySnapshotAdjuster(double Cap) : IBatchedOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedOccupancySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedOccupancySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IBatchedOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedOccupancySnapshot item, double valueValue)
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

public sealed record FlaggedBatchedOccupancySnapshotAdjuster(Registry Flags, string Flag, double Percent) : IBatchedOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedOccupancySnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedOccupancySnapshotRatingOffsetAdjuster(double Weight) : IBatchedOccupancySnapshotModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(BatchedOccupancySnapshot item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the batched occupancy snapshot steps in order and keeps a trace of each.</summary>
public sealed class BatchedOccupancySnapshotEngine(List<IBatchedOccupancySnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedOccupancySnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedOccupancySnapshotEngine DefaultBatchedOccupancySnapshotEngine(Registry flags)
    {
        return new BatchedOccupancySnapshotEngine(
            new List<IBatchedOccupancySnapshotModifier>
            {
                new MinimumBatchedOccupancySnapshotAdjuster(30.2),
                new PercentBatchedOccupancySnapshotAdjuster(17.4),
                new CapBatchedOccupancySnapshotAdjuster(4707.0),
                new TieredBatchedOccupancySnapshotAdjuster(
                    new List<double> { 717.0, 5875.0 },
                    new List<double> { 1.04, 1.17 }),
                new FlaggedBatchedOccupancySnapshotAdjuster(flags, "batched-occupancy-snapshot-tiered", -2.0),
                new BatchedOccupancySnapshotRatingOffsetAdjuster(0.95),
            },
            new List<string>(),
            4.5,
            29793.1);
    }

    public double Run(BatchedOccupancySnapshot item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedOccupancySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
