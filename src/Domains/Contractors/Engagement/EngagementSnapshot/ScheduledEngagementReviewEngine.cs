namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>One step of the scheduled engagement review calculation.</summary>
public interface IScheduledEngagementReviewStep
{
    string Describe();
    double Adjust(ScheduledEngagementReview item, double valueValue);
}

public sealed record PercentScheduledEngagementReviewAdjuster(double Percent) : IScheduledEngagementReviewStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledEngagementReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelScheduledEngagementReviewAdjuster(Dictionary<string, double> Factors) : IScheduledEngagementReviewStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ScheduledEngagementReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledEngagementReviewAdjuster(double Cap) : IScheduledEngagementReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledEngagementReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledEngagementReviewAdjuster(double Minimum) : IScheduledEngagementReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledEngagementReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled engagement review steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEngagementReviewEngine(List<IScheduledEngagementReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEngagementReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEngagementReviewEngine DefaultScheduledEngagementReviewEngine()
    {
        return new ScheduledEngagementReviewEngine(
            new List<IScheduledEngagementReviewStep>
            {
                new PercentScheduledEngagementReviewAdjuster(24.3),
                new ChannelScheduledEngagementReviewAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.06, ["online"] = 0.8 }),
                new CapScheduledEngagementReviewAdjuster(24113.0),
                new MinimumScheduledEngagementReviewAdjuster(40.9),
            },
            new List<string>(),
            1.4,
            92004.0);
    }

    public double Evaluate(ScheduledEngagementReview item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEngagementReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
