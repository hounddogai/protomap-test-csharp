namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the feedback variance calculation.</summary>
public interface IFeedbackVarianceAdjuster
{
    string Describe();
    double Adjust(FeedbackVariance item, double valueValue);
}

public sealed record LocaleFeedbackVarianceAdjuster(Dictionary<string, double> Factors) : IFeedbackVarianceAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FeedbackVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FeedbackVarianceCostOffsetAdjuster(double Weight) : IFeedbackVarianceAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(FeedbackVariance item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record FlaggedFeedbackVarianceAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredFeedbackVarianceAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackVariance item, double valueValue)
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

/// <summary>Runs the feedback variance steps in order and keeps a trace of each.</summary>
public sealed class FeedbackVarianceEngine(List<IFeedbackVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackVarianceEngine DefaultFeedbackVarianceEngine(Registry flags)
    {
        return new FeedbackVarianceEngine(
            new List<IFeedbackVarianceAdjuster>
            {
                new LocaleFeedbackVarianceAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.81, ["north"] = 0.96 }),
                new FeedbackVarianceCostOffsetAdjuster(2.0),
                new FlaggedFeedbackVarianceAdjuster(flags, "feedback-variance-legacy", -8.2),
                new TieredFeedbackVarianceAdjuster(
                    new List<double> { 1880.0, 5058.0, 6699.0, 9495.0 },
                    new List<double> { 0.91, 0.98, 1.11, 0.9 }),
            },
            new List<string>(),
            2.9,
            75686.8);
    }

    public double Run(FeedbackVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
