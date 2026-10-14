namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request review calculation.</summary>
public interface ILeaveRequestReviewStep
{
    string Describe();
    double Adjust(LeaveRequestReview item, double valueValue);
}

public sealed record LeaveRequestReviewStartsAtOffsetAdjuster(double Weight) : ILeaveRequestReviewStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(LeaveRequestReview item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record MinimumLeaveRequestReviewAdjuster(double Minimum) : ILeaveRequestReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLeaveRequestReviewAdjuster(double Step) : ILeaveRequestReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLeaveRequestReviewAdjuster(double Cap) : ILeaveRequestReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelLeaveRequestReviewAdjuster(Dictionary<string, double> Factors) : ILeaveRequestReviewStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LeaveRequestReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the leave request review steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestReviewEngine(List<ILeaveRequestReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestReviewEngine DefaultLeaveRequestReviewEngine()
    {
        return new LeaveRequestReviewEngine(
            new List<ILeaveRequestReviewStep>
            {
                new LeaveRequestReviewStartsAtOffsetAdjuster(0.46),
                new MinimumLeaveRequestReviewAdjuster(1.4),
                new RoundingLeaveRequestReviewAdjuster(1.0),
                new CapLeaveRequestReviewAdjuster(18379.0),
                new ChannelLeaveRequestReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.89, ["east"] = 0.73 }),
            },
            new List<string>(),
            3.8,
            27123.6);
    }

    public double Run(LeaveRequestReview item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
