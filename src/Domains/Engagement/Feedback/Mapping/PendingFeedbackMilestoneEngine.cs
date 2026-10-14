namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending feedback milestone calculation.</summary>
public interface IPendingFeedbackMilestoneFactor
{
    string Describe();
    double Adjust(PendingFeedbackMilestone item, double valueValue);
}

public sealed record CapPendingFeedbackMilestoneAdjuster(double Cap) : IPendingFeedbackMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingFeedbackMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingFeedbackMilestoneAdjuster(double Percent) : IPendingFeedbackMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingFeedbackMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePendingFeedbackMilestoneAdjuster(Dictionary<string, double> Factors) : IPendingFeedbackMilestoneFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingFeedbackMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending feedback milestone steps in order and keeps a trace of each.</summary>
public sealed class PendingFeedbackMilestoneEngine(List<IPendingFeedbackMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingFeedbackMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingFeedbackMilestoneEngine DefaultPendingFeedbackMilestoneEngine()
    {
        return new PendingFeedbackMilestoneEngine(
            new List<IPendingFeedbackMilestoneFactor>
            {
                new CapPendingFeedbackMilestoneAdjuster(10111.0),
                new PercentPendingFeedbackMilestoneAdjuster(-9.9),
                new NamePendingFeedbackMilestoneAdjuster(new Dictionary<string, double> { ["south"] = 0.92 }),
            },
            new List<string>(),
            2.1,
            33374.5);
    }

    public double Price(PendingFeedbackMilestone item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingFeedbackMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
