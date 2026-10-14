namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback batch calculation.</summary>
public interface IFeedbackBatchModifier
{
    string Describe();
    double Adjust(FeedbackBatch item, double valueValue);
}

public sealed record CountryFeedbackBatchAdjuster(Dictionary<string, double> Factors) : IFeedbackBatchModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(FeedbackBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFeedbackBatchAdjuster(double Step) : IFeedbackBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFeedbackBatchAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackBatch item, double valueValue)
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

/// <summary>Runs the feedback batch steps in order and keeps a trace of each.</summary>
public sealed class FeedbackBatchEngine(List<IFeedbackBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackBatchEngine DefaultFeedbackBatchEngine()
    {
        return new FeedbackBatchEngine(
            new List<IFeedbackBatchModifier>
            {
                new CountryFeedbackBatchAdjuster(new Dictionary<string, double> { ["online"] = 1.35 }),
                new RoundingFeedbackBatchAdjuster(0.5),
                new TieredFeedbackBatchAdjuster(new List<double> { 7411.0, 9751.0 }, new List<double> { 0.83, 1.02 }),
            },
            new List<string>(),
            2.2,
            36366.2);
    }

    public double Price(FeedbackBatch item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
