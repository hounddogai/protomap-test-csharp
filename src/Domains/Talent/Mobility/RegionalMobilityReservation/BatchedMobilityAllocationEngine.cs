namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched mobility allocation calculation.</summary>
public interface IBatchedMobilityAllocationModifier
{
    string Describe();
    double Adjust(BatchedMobilityAllocation item, double valueValue);
}

public sealed record BatchedMobilityAllocationUpdatedAtOffsetAdjuster(double Weight) : IBatchedMobilityAllocationModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(BatchedMobilityAllocation item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentBatchedMobilityAllocationAdjuster(double Percent) : IBatchedMobilityAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedMobilityAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedMobilityAllocationAdjuster(Registry Flags, string Flag, double Percent) : IBatchedMobilityAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedMobilityAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedMobilityAllocationAdjuster(double Step) : IBatchedMobilityAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedMobilityAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBatchedMobilityAllocationAdjuster(List<double> Bounds, List<double> Rates) : IBatchedMobilityAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedMobilityAllocation item, double valueValue)
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

/// <summary>Runs the batched mobility allocation steps in order and keeps a trace of each.</summary>
public sealed class BatchedMobilityAllocationEngine(List<IBatchedMobilityAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedMobilityAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedMobilityAllocationEngine DefaultBatchedMobilityAllocationEngine(Registry flags)
    {
        return new BatchedMobilityAllocationEngine(
            new List<IBatchedMobilityAllocationModifier>
            {
                new BatchedMobilityAllocationUpdatedAtOffsetAdjuster(0.71),
                new PercentBatchedMobilityAllocationAdjuster(-20.6),
                new FlaggedBatchedMobilityAllocationAdjuster(flags, "batched-mobility-allocation-pending", 3.5),
                new RoundingBatchedMobilityAllocationAdjuster(0.1),
                new TieredBatchedMobilityAllocationAdjuster(
                    new List<double> { 6918.0, 9470.0 },
                    new List<double> { 0.94, 1.17 }),
            },
            new List<string>(),
            2.5,
            22963.4);
    }

    public double Compute(BatchedMobilityAllocation item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedMobilityAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
