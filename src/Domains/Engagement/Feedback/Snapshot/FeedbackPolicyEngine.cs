namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback policy calculation.</summary>
public interface IFeedbackPolicyAdjuster
{
    string Describe();
    double Adjust(FeedbackPolicy item, double valueValue);
}

public sealed record TieredFeedbackPolicyAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackPolicy item, double valueValue)
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

public sealed record LocaleFeedbackPolicyAdjuster(Dictionary<string, double> Factors) : IFeedbackPolicyAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FeedbackPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFeedbackPolicyAdjuster(double Step) : IFeedbackPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the feedback policy steps in order and keeps a trace of each.</summary>
public sealed class FeedbackPolicyEngine(List<IFeedbackPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackPolicyEngine DefaultFeedbackPolicyEngine()
    {
        return new FeedbackPolicyEngine(
            new List<IFeedbackPolicyAdjuster>
            {
                new TieredFeedbackPolicyAdjuster(
                    new List<double> { 2197.0, 2650.0 },
                    new List<double> { 0.83, 0.88 }),
                new LocaleFeedbackPolicyAdjuster(new Dictionary<string, double> { ["online"] = 0.72 }),
                new RoundingFeedbackPolicyAdjuster(5.0),
            },
            new List<string>(),
            2.6,
            16594.2);
    }

    public double Price(FeedbackPolicy item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
