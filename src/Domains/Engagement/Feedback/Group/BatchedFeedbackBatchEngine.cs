namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched feedback batch calculation.</summary>
public interface IBatchedFeedbackBatchModifier
{
    string Describe();
    double Adjust(BatchedFeedbackBatch item, double valueValue);
}

public sealed record MinimumBatchedFeedbackBatchAdjuster(double Minimum) : IBatchedFeedbackBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedFeedbackBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BatchedFeedbackBatchAmountOffsetAdjuster(double Weight) : IBatchedFeedbackBatchModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BatchedFeedbackBatch item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentBatchedFeedbackBatchAdjuster(double Percent) : IBatchedFeedbackBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedFeedbackBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedFeedbackBatchAdjuster(double Cap) : IBatchedFeedbackBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedFeedbackBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBatchedFeedbackBatchAdjuster(Registry Flags, string Flag, double Percent) : IBatchedFeedbackBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedFeedbackBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched feedback batch steps in order and keeps a trace of each.</summary>
public sealed class BatchedFeedbackBatchEngine(List<IBatchedFeedbackBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedFeedbackBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedFeedbackBatchEngine DefaultBatchedFeedbackBatchEngine(Registry flags)
    {
        return new BatchedFeedbackBatchEngine(
            new List<IBatchedFeedbackBatchModifier>
            {
                new MinimumBatchedFeedbackBatchAdjuster(24.2),
                new BatchedFeedbackBatchAmountOffsetAdjuster(1.7),
                new PercentBatchedFeedbackBatchAdjuster(6.7),
                new CapBatchedFeedbackBatchAdjuster(9802.0),
                new FlaggedBatchedFeedbackBatchAdjuster(flags, "batched-feedback-batch-pending", 3.3),
            },
            new List<string>(),
            0.7,
            21927.8);
    }

    public double Compute(BatchedFeedbackBatch item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedFeedbackBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
