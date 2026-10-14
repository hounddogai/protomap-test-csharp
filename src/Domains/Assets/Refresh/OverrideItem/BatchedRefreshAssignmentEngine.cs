namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched refresh assignment calculation.</summary>
public interface IBatchedRefreshAssignmentAdjuster
{
    string Describe();
    double Adjust(BatchedRefreshAssignment item, double valueValue);
}

public sealed record FlaggedBatchedRefreshAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IBatchedRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedRefreshAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryBatchedRefreshAssignmentAdjuster(Dictionary<string, double> Factors) : IBatchedRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(BatchedRefreshAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBatchedRefreshAssignmentAdjuster(double Cap) : IBatchedRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedRefreshAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedRefreshAssignmentAdjuster(double Step) : IBatchedRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedRefreshAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the batched refresh assignment steps in order and keeps a trace of each.</summary>
public sealed class BatchedRefreshAssignmentEngine(List<IBatchedRefreshAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRefreshAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRefreshAssignmentEngine DefaultBatchedRefreshAssignmentEngine(Registry flags)
    {
        return new BatchedRefreshAssignmentEngine(
            new List<IBatchedRefreshAssignmentAdjuster>
            {
                new FlaggedBatchedRefreshAssignmentAdjuster(flags, "batched-refresh-assignment-primary", 7.2),
                new CategoryBatchedRefreshAssignmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.71, ["north"] = 1.26, ["retail"] = 1.04 }),
                new CapBatchedRefreshAssignmentAdjuster(33667.0),
                new RoundingBatchedRefreshAssignmentAdjuster(0.25),
            },
            new List<string>(),
            4.0,
            90417.5);
    }

    public double Run(BatchedRefreshAssignment item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRefreshAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
