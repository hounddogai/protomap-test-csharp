namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled feedback baseline calculation.</summary>
public interface IScheduledFeedbackBaselineAdjuster
{
    string Describe();
    double Adjust(ScheduledFeedbackBaseline item, double valueValue);
}

public sealed record LabelScheduledFeedbackBaselineAdjuster(Dictionary<string, double> Factors) : IScheduledFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledFeedbackBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumScheduledFeedbackBaselineAdjuster(double Minimum) : IScheduledFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledFeedbackBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledFeedbackBaselineAdjuster(double Percent) : IScheduledFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledFeedbackBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledFeedbackBaselineAdjuster(List<double> Bounds, List<double> Rates) : IScheduledFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledFeedbackBaseline item, double valueValue)
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

public sealed record FlaggedScheduledFeedbackBaselineAdjuster(Registry Flags, string Flag, double Percent) : IScheduledFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledFeedbackBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled feedback baseline steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFeedbackBaselineEngine(List<IScheduledFeedbackBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFeedbackBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFeedbackBaselineEngine DefaultScheduledFeedbackBaselineEngine(Registry flags)
    {
        return new ScheduledFeedbackBaselineEngine(
            new List<IScheduledFeedbackBaselineAdjuster>
            {
                new LabelScheduledFeedbackBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.25, ["west"] = 1.08 }),
                new MinimumScheduledFeedbackBaselineAdjuster(49.0),
                new PercentScheduledFeedbackBaselineAdjuster(-11.3),
                new TieredScheduledFeedbackBaselineAdjuster(
                    new List<double> { 434.0, 3489.0, 6363.0 },
                    new List<double> { 0.9, 1.1, 1.14 }),
                new FlaggedScheduledFeedbackBaselineAdjuster(flags, "scheduled-feedback-baseline-manual", -9.8),
            },
            new List<string>(),
            2.9,
            25223.7);
    }

    public double Compute(ScheduledFeedbackBaseline item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFeedbackBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
