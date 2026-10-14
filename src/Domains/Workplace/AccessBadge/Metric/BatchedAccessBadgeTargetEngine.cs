namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched access badge target calculation.</summary>
public interface IBatchedAccessBadgeTargetAdjuster
{
    string Describe();
    double Adjust(BatchedAccessBadgeTarget item, double valueValue);
}

public sealed record MinimumBatchedAccessBadgeTargetAdjuster(double Minimum) : IBatchedAccessBadgeTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedAccessBadgeTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBatchedAccessBadgeTargetAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAccessBadgeTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAccessBadgeTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedAccessBadgeTargetHoursOffsetAdjuster(double Weight) : IBatchedAccessBadgeTargetAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(BatchedAccessBadgeTarget item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CountryBatchedAccessBadgeTargetAdjuster(Dictionary<string, double> Factors) : IBatchedAccessBadgeTargetAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BatchedAccessBadgeTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBatchedAccessBadgeTargetAdjuster(double Step) : IBatchedAccessBadgeTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedAccessBadgeTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the batched access badge target steps in order and keeps a trace of each.</summary>
public sealed class BatchedAccessBadgeTargetEngine(List<IBatchedAccessBadgeTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAccessBadgeTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAccessBadgeTargetEngine DefaultBatchedAccessBadgeTargetEngine(Registry flags)
    {
        return new BatchedAccessBadgeTargetEngine(
            new List<IBatchedAccessBadgeTargetAdjuster>
            {
                new MinimumBatchedAccessBadgeTargetAdjuster(26.8),
                new FlaggedBatchedAccessBadgeTargetAdjuster(flags, "batched-access-badge-target-legacy", 6.2),
                new BatchedAccessBadgeTargetHoursOffsetAdjuster(1.53),
                new CountryBatchedAccessBadgeTargetAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.07, ["west"] = 0.81, ["south"] = 0.74 }),
                new RoundingBatchedAccessBadgeTargetAdjuster(0.5),
            },
            new List<string>(),
            3.2,
            90461.5);
    }

    public double Compute(BatchedAccessBadgeTarget item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAccessBadgeTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
