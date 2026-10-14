namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback preference calculation.</summary>
public interface IFeedbackPreferenceAdjuster
{
    string Describe();
    double Adjust(FeedbackPreference item, double valueValue);
}

public sealed record FeedbackPreferenceMarginOffsetAdjuster(double Weight) : IFeedbackPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(FeedbackPreference item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingFeedbackPreferenceAdjuster(double Step) : IFeedbackPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFeedbackPreferenceAdjuster(double Percent) : IFeedbackPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the feedback preference steps in order and keeps a trace of each.</summary>
public sealed class FeedbackPreferenceEngine(List<IFeedbackPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackPreferenceEngine DefaultFeedbackPreferenceEngine()
    {
        return new FeedbackPreferenceEngine(
            new List<IFeedbackPreferenceAdjuster>
            {
                new FeedbackPreferenceMarginOffsetAdjuster(1.88),
                new RoundingFeedbackPreferenceAdjuster(0.05),
                new PercentFeedbackPreferenceAdjuster(-1.7),
            },
            new List<string>(),
            0.1,
            88499.8);
    }

    public double Price(FeedbackPreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
