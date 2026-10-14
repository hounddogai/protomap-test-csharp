namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched offboarding allocation calculation.</summary>
public interface IBatchedOffboardingAllocationAdjuster
{
    string Describe();
    double Adjust(BatchedOffboardingAllocation item, double valueValue);
}

public sealed record PercentBatchedOffboardingAllocationAdjuster(double Percent) : IBatchedOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedOffboardingAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedOffboardingAllocationMarginOffsetAdjuster(double Weight) : IBatchedOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(BatchedOffboardingAllocation item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredBatchedOffboardingAllocationAdjuster(List<double> Bounds, List<double> Rates) : IBatchedOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedOffboardingAllocation item, double valueValue)
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

public sealed record FlaggedBatchedOffboardingAllocationAdjuster(Registry Flags, string Flag, double Percent) : IBatchedOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedOffboardingAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBatchedOffboardingAllocationAdjuster(double Minimum) : IBatchedOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedOffboardingAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the batched offboarding allocation steps in order and keeps a trace of each.</summary>
public sealed class BatchedOffboardingAllocationEngine(List<IBatchedOffboardingAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedOffboardingAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedOffboardingAllocationEngine DefaultBatchedOffboardingAllocationEngine(Registry flags)
    {
        return new BatchedOffboardingAllocationEngine(
            new List<IBatchedOffboardingAllocationAdjuster>
            {
                new PercentBatchedOffboardingAllocationAdjuster(4.4),
                new BatchedOffboardingAllocationMarginOffsetAdjuster(1.84),
                new TieredBatchedOffboardingAllocationAdjuster(
                    new List<double> { 3753.0, 7981.0, 8084.0 },
                    new List<double> { 0.9, 1.15, 0.94 }),
                new FlaggedBatchedOffboardingAllocationAdjuster(flags, "batched-offboarding-allocation-primary", 2.7),
                new MinimumBatchedOffboardingAllocationAdjuster(12.1),
            },
            new List<string>(),
            2.2,
            31399.7);
    }

    public double Evaluate(BatchedOffboardingAllocation item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedOffboardingAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
