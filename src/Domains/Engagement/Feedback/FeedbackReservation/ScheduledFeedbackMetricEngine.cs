namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled feedback metric calculation.</summary>
public interface IScheduledFeedbackMetricModifier
{
    string Describe();
    double Adjust(ScheduledFeedbackMetric item, double valueValue);
}

public sealed record MinimumScheduledFeedbackMetricAdjuster(double Minimum) : IScheduledFeedbackMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledFeedbackMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceScheduledFeedbackMetricAdjuster(Dictionary<string, double> Factors) : IScheduledFeedbackMetricModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(ScheduledFeedbackMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingScheduledFeedbackMetricAdjuster(double Step) : IScheduledFeedbackMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledFeedbackMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledFeedbackMetricAdjuster(Registry Flags, string Flag, double Percent) : IScheduledFeedbackMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledFeedbackMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled feedback metric steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFeedbackMetricEngine(List<IScheduledFeedbackMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFeedbackMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFeedbackMetricEngine DefaultScheduledFeedbackMetricEngine(Registry flags)
    {
        return new ScheduledFeedbackMetricEngine(
            new List<IScheduledFeedbackMetricModifier>
            {
                new MinimumScheduledFeedbackMetricAdjuster(13.6),
                new SourceScheduledFeedbackMetricAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.81, ["central"] = 0.95 }),
                new RoundingScheduledFeedbackMetricAdjuster(0.05),
                new FlaggedScheduledFeedbackMetricAdjuster(flags, "scheduled-feedback-metric-batched", 6.2),
            },
            new List<string>(),
            0.9,
            58467.5);
    }

    public double Run(ScheduledFeedbackMetric item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFeedbackMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
