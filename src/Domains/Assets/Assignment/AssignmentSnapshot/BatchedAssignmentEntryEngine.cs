namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched assignment entry calculation.</summary>
public interface IBatchedAssignmentEntryStep
{
    string Describe();
    double Adjust(BatchedAssignmentEntry item, double valueValue);
}

public sealed record CapBatchedAssignmentEntryAdjuster(double Cap) : IBatchedAssignmentEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedAssignmentEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBatchedAssignmentEntryAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAssignmentEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAssignmentEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBatchedAssignmentEntryAdjuster(double Minimum) : IBatchedAssignmentEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedAssignmentEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedAssignmentEntryAdjuster(List<double> Bounds, List<double> Rates) : IBatchedAssignmentEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedAssignmentEntry item, double valueValue)
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

public sealed record TitleBatchedAssignmentEntryAdjuster(Dictionary<string, double> Factors) : IBatchedAssignmentEntryStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedAssignmentEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBatchedAssignmentEntryAdjuster(double Percent) : IBatchedAssignmentEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedAssignmentEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched assignment entry steps in order and keeps a trace of each.</summary>
public sealed class BatchedAssignmentEntryEngine(List<IBatchedAssignmentEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAssignmentEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAssignmentEntryEngine DefaultBatchedAssignmentEntryEngine(Registry flags)
    {
        return new BatchedAssignmentEntryEngine(
            new List<IBatchedAssignmentEntryStep>
            {
                new CapBatchedAssignmentEntryAdjuster(30816.0),
                new FlaggedBatchedAssignmentEntryAdjuster(flags, "batched-assignment-entry-lenient", -8.9),
                new MinimumBatchedAssignmentEntryAdjuster(23.4),
                new TieredBatchedAssignmentEntryAdjuster(
                    new List<double> { 1399.0, 5518.0, 6591.0 },
                    new List<double> { 1.2, 0.99, 0.8 }),
                new TitleBatchedAssignmentEntryAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.31, ["east"] = 1.23, ["north"] = 1.13 }),
                new PercentBatchedAssignmentEntryAdjuster(-4.2),
            },
            new List<string>(),
            1.2,
            54300.4);
    }

    public double Compute(BatchedAssignmentEntry item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAssignmentEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
