namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback group calculation.</summary>
public interface IFeedbackGroupStep
{
    string Describe();
    double Adjust(FeedbackGroup item, double valueValue);
}

public sealed record CapFeedbackGroupAdjuster(double Cap) : IFeedbackGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FeedbackGroupAmountOffsetAdjuster(double Weight) : IFeedbackGroupStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(FeedbackGroup item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumFeedbackGroupAdjuster(double Minimum) : IFeedbackGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFeedbackGroupAdjuster(double Step) : IFeedbackGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFeedbackGroupAdjuster(double Percent) : IFeedbackGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the feedback group steps in order and keeps a trace of each.</summary>
public sealed class FeedbackGroupEngine(List<IFeedbackGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackGroupEngine DefaultFeedbackGroupEngine()
    {
        return new FeedbackGroupEngine(
            new List<IFeedbackGroupStep>
            {
                new CapFeedbackGroupAdjuster(4148.0),
                new FeedbackGroupAmountOffsetAdjuster(0.97),
                new MinimumFeedbackGroupAdjuster(6.8),
                new RoundingFeedbackGroupAdjuster(0.5),
                new PercentFeedbackGroupAdjuster(14.8),
            },
            new List<string>(),
            1.4,
            24095.1);
    }

    public double Price(FeedbackGroup item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
