namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback template calculation.</summary>
public interface IFeedbackTemplateModifier
{
    string Describe();
    double Adjust(FeedbackTemplate item, double valueValue);
}

public sealed record NameFeedbackTemplateAdjuster(Dictionary<string, double> Factors) : IFeedbackTemplateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FeedbackTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFeedbackTemplateAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFeedbackTemplateAdjuster(double Step) : IFeedbackTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFeedbackTemplateAdjuster(double Cap) : IFeedbackTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FeedbackTemplateCreatedAtOffsetAdjuster(double Weight) : IFeedbackTemplateModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(FeedbackTemplate item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the feedback template steps in order and keeps a trace of each.</summary>
public sealed class FeedbackTemplateEngine(List<IFeedbackTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackTemplateEngine DefaultFeedbackTemplateEngine(Registry flags)
    {
        return new FeedbackTemplateEngine(
            new List<IFeedbackTemplateModifier>
            {
                new NameFeedbackTemplateAdjuster(new Dictionary<string, double> { ["retail"] = 1.11 }),
                new FlaggedFeedbackTemplateAdjuster(flags, "feedback-template-seasonal", -8.9),
                new RoundingFeedbackTemplateAdjuster(0.05),
                new CapFeedbackTemplateAdjuster(25327.0),
                new FeedbackTemplateCreatedAtOffsetAdjuster(0.02),
            },
            new List<string>(),
            3.9,
            97164.8);
    }

    public double Evaluate(FeedbackTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
