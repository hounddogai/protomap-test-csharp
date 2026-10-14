namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>One step of the batched leave request item calculation.</summary>
public interface IBatchedLeaveRequestItemFactor
{
    string Describe();
    double Adjust(BatchedLeaveRequestItem item, double valueValue);
}

public sealed record CategoryBatchedLeaveRequestItemAdjuster(Dictionary<string, double> Factors) : IBatchedLeaveRequestItemFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(BatchedLeaveRequestItem item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedLeaveRequestItemAdjuster(List<double> Bounds, List<double> Rates) : IBatchedLeaveRequestItemFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedLeaveRequestItem item, double valueValue)
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

public sealed record BatchedLeaveRequestItemQuantityOffsetAdjuster(double Weight) : IBatchedLeaveRequestItemFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(BatchedLeaveRequestItem item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumBatchedLeaveRequestItemAdjuster(double Minimum) : IBatchedLeaveRequestItemFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedLeaveRequestItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedLeaveRequestItemAdjuster(double Cap) : IBatchedLeaveRequestItemFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedLeaveRequestItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched leave request item steps in order and keeps a trace of each.</summary>
public sealed class BatchedLeaveRequestItemEngine(List<IBatchedLeaveRequestItemFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedLeaveRequestItemFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedLeaveRequestItemEngine DefaultBatchedLeaveRequestItemEngine()
    {
        return new BatchedLeaveRequestItemEngine(
            new List<IBatchedLeaveRequestItemFactor>
            {
                new CategoryBatchedLeaveRequestItemAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.31, ["east"] = 1.33 }),
                new TieredBatchedLeaveRequestItemAdjuster(
                    new List<double> { 2540.0, 3722.0, 6351.0, 6453.0 },
                    new List<double> { 1.11, 1.09, 0.98, 1.02 }),
                new BatchedLeaveRequestItemQuantityOffsetAdjuster(0.97),
                new MinimumBatchedLeaveRequestItemAdjuster(2.3),
                new CapBatchedLeaveRequestItemAdjuster(25016.0),
            },
            new List<string>(),
            4.6,
            44978.8);
    }

    public double Evaluate(BatchedLeaveRequestItem item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedLeaveRequestItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
