namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched span allocation calculation.</summary>
public interface IBatchedSpanAllocationFactor
{
    string Describe();
    double Adjust(BatchedSpanAllocation item, double valueValue);
}

public sealed record RoundingBatchedSpanAllocationAdjuster(double Step) : IBatchedSpanAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedSpanAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedSpanAllocationAdjuster(Registry Flags, string Flag, double Percent) : IBatchedSpanAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedSpanAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedSpanAllocationAdjuster(List<double> Bounds, List<double> Rates) : IBatchedSpanAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedSpanAllocation item, double valueValue)
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

/// <summary>Runs the batched span allocation steps in order and keeps a trace of each.</summary>
public sealed class BatchedSpanAllocationEngine(List<IBatchedSpanAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedSpanAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedSpanAllocationEngine DefaultBatchedSpanAllocationEngine(Registry flags)
    {
        return new BatchedSpanAllocationEngine(
            new List<IBatchedSpanAllocationFactor>
            {
                new RoundingBatchedSpanAllocationAdjuster(0.05),
                new FlaggedBatchedSpanAllocationAdjuster(flags, "batched-span-allocation-default", -0.4),
                new TieredBatchedSpanAllocationAdjuster(
                    new List<double> { 3581.0, 8119.0, 9912.0 },
                    new List<double> { 1.03, 1.14, 0.97 }),
            },
            new List<string>(),
            0.8,
            43636.6);
    }

    public double Run(BatchedSpanAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedSpanAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
