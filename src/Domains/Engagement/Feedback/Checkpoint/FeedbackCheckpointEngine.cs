namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback checkpoint calculation.</summary>
public interface IFeedbackCheckpointStep
{
    string Describe();
    double Adjust(FeedbackCheckpoint item, double valueValue);
}

public sealed record FeedbackCheckpointVersionOffsetAdjuster(double Weight) : IFeedbackCheckpointStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(FeedbackCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record CapFeedbackCheckpointAdjuster(double Cap) : IFeedbackCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFeedbackCheckpointAdjuster(double Step) : IFeedbackCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFeedbackCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackCheckpoint item, double valueValue)
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

public sealed record MinimumFeedbackCheckpointAdjuster(double Minimum) : IFeedbackCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the feedback checkpoint steps in order and keeps a trace of each.</summary>
public sealed class FeedbackCheckpointEngine(List<IFeedbackCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackCheckpointEngine DefaultFeedbackCheckpointEngine()
    {
        return new FeedbackCheckpointEngine(
            new List<IFeedbackCheckpointStep>
            {
                new FeedbackCheckpointVersionOffsetAdjuster(1.64),
                new CapFeedbackCheckpointAdjuster(7800.0),
                new RoundingFeedbackCheckpointAdjuster(0.1),
                new TieredFeedbackCheckpointAdjuster(
                    new List<double> { 4167.0, 6218.0 },
                    new List<double> { 1.08, 1.11 }),
                new MinimumFeedbackCheckpointAdjuster(35.5),
            },
            new List<string>(),
            1.5,
            45619.2);
    }

    public double Compute(FeedbackCheckpoint item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
