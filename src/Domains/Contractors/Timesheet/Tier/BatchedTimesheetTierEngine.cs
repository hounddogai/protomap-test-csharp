namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched timesheet tier calculation.</summary>
public interface IBatchedTimesheetTierStep
{
    string Describe();
    double Adjust(BatchedTimesheetTier item, double valueValue);
}

public sealed record PercentBatchedTimesheetTierAdjuster(double Percent) : IBatchedTimesheetTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedTimesheetTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleBatchedTimesheetTierAdjuster(Dictionary<string, double> Factors) : IBatchedTimesheetTierStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(BatchedTimesheetTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedTimesheetTierAdjuster(List<double> Bounds, List<double> Rates) : IBatchedTimesheetTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedTimesheetTier item, double valueValue)
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

public sealed record FlaggedBatchedTimesheetTierAdjuster(Registry Flags, string Flag, double Percent) : IBatchedTimesheetTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedTimesheetTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBatchedTimesheetTierAdjuster(double Minimum) : IBatchedTimesheetTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedTimesheetTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the batched timesheet tier steps in order and keeps a trace of each.</summary>
public sealed class BatchedTimesheetTierEngine(List<IBatchedTimesheetTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedTimesheetTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedTimesheetTierEngine DefaultBatchedTimesheetTierEngine(Registry flags)
    {
        return new BatchedTimesheetTierEngine(
            new List<IBatchedTimesheetTierStep>
            {
                new PercentBatchedTimesheetTierAdjuster(-2.4),
                new LocaleBatchedTimesheetTierAdjuster(new Dictionary<string, double> { ["west"] = 0.8 }),
                new TieredBatchedTimesheetTierAdjuster(
                    new List<double> { 1722.0, 3928.0, 5149.0, 5290.0 },
                    new List<double> { 0.95, 1.07, 1.09, 0.97 }),
                new FlaggedBatchedTimesheetTierAdjuster(flags, "batched-timesheet-tier-tiered", -4.9),
                new MinimumBatchedTimesheetTierAdjuster(44.0),
            },
            new List<string>(),
            1.6,
            72131.2);
    }

    public double Evaluate(BatchedTimesheetTier item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedTimesheetTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
