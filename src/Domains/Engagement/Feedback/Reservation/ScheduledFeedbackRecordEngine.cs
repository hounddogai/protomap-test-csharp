namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled feedback record calculation.</summary>
public interface IScheduledFeedbackRecordFactor
{
    string Describe();
    double Adjust(ScheduledFeedbackRecord item, double valueValue);
}

public sealed record MinimumScheduledFeedbackRecordAdjuster(double Minimum) : IScheduledFeedbackRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledFeedbackRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledFeedbackRecordRetriesOffsetAdjuster(double Weight) : IScheduledFeedbackRecordFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(ScheduledFeedbackRecord item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record SegmentScheduledFeedbackRecordAdjuster(Dictionary<string, double> Factors) : IScheduledFeedbackRecordFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ScheduledFeedbackRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledFeedbackRecordAdjuster(double Cap) : IScheduledFeedbackRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledFeedbackRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled feedback record steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFeedbackRecordEngine(List<IScheduledFeedbackRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFeedbackRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFeedbackRecordEngine DefaultScheduledFeedbackRecordEngine()
    {
        return new ScheduledFeedbackRecordEngine(
            new List<IScheduledFeedbackRecordFactor>
            {
                new MinimumScheduledFeedbackRecordAdjuster(47.9),
                new ScheduledFeedbackRecordRetriesOffsetAdjuster(1.24),
                new SegmentScheduledFeedbackRecordAdjuster(new Dictionary<string, double> { ["central"] = 1.03 }),
                new CapScheduledFeedbackRecordAdjuster(32511.0),
            },
            new List<string>(),
            3.8,
            59530.2);
    }

    public double Run(ScheduledFeedbackRecord item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFeedbackRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
