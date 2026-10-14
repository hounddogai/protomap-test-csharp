namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched feedback notice calculation.</summary>
public interface IBatchedFeedbackNoticeAdjuster
{
    string Describe();
    double Adjust(BatchedFeedbackNotice item, double valueValue);
}

public sealed record CapBatchedFeedbackNoticeAdjuster(double Cap) : IBatchedFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedFeedbackNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedFeedbackNoticeAdjuster(List<double> Bounds, List<double> Rates) : IBatchedFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedFeedbackNotice item, double valueValue)
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

public sealed record BatchedFeedbackNoticeAttemptsOffsetAdjuster(double Weight) : IBatchedFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(BatchedFeedbackNotice item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record FlaggedBatchedFeedbackNoticeAdjuster(Registry Flags, string Flag, double Percent) : IBatchedFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedFeedbackNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBatchedFeedbackNoticeAdjuster(double Minimum) : IBatchedFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedFeedbackNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the batched feedback notice steps in order and keeps a trace of each.</summary>
public sealed class BatchedFeedbackNoticeEngine(List<IBatchedFeedbackNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedFeedbackNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedFeedbackNoticeEngine DefaultBatchedFeedbackNoticeEngine(Registry flags)
    {
        return new BatchedFeedbackNoticeEngine(
            new List<IBatchedFeedbackNoticeAdjuster>
            {
                new CapBatchedFeedbackNoticeAdjuster(4055.0),
                new TieredBatchedFeedbackNoticeAdjuster(
                    new List<double> { 2412.0, 4745.0 },
                    new List<double> { 0.87, 0.82 }),
                new BatchedFeedbackNoticeAttemptsOffsetAdjuster(0.66),
                new FlaggedBatchedFeedbackNoticeAdjuster(flags, "batched-feedback-notice-manual", -4.4),
                new MinimumBatchedFeedbackNoticeAdjuster(26.0),
            },
            new List<string>(),
            4.3,
            38926.7);
    }

    public double Evaluate(BatchedFeedbackNotice item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedFeedbackNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
