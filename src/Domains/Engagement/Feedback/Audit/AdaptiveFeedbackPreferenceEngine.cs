namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive feedback preference calculation.</summary>
public interface IAdaptiveFeedbackPreferenceFactor
{
    string Describe();
    double Adjust(AdaptiveFeedbackPreference item, double valueValue);
}

public sealed record TeamAdaptiveFeedbackPreferenceAdjuster(Dictionary<string, double> Factors) : IAdaptiveFeedbackPreferenceFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AdaptiveFeedbackPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveFeedbackPreferenceAdjuster(double Step) : IAdaptiveFeedbackPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveFeedbackPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdaptiveFeedbackPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveFeedbackPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveFeedbackPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveFeedbackPreferenceAdjuster(double Cap) : IAdaptiveFeedbackPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveFeedbackPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveFeedbackPreferenceScoreOffsetAdjuster(double Weight) : IAdaptiveFeedbackPreferenceFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AdaptiveFeedbackPreference item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentAdaptiveFeedbackPreferenceAdjuster(double Percent) : IAdaptiveFeedbackPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveFeedbackPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive feedback preference steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveFeedbackPreferenceEngine(List<IAdaptiveFeedbackPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveFeedbackPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveFeedbackPreferenceEngine DefaultAdaptiveFeedbackPreferenceEngine(Registry flags)
    {
        return new AdaptiveFeedbackPreferenceEngine(
            new List<IAdaptiveFeedbackPreferenceFactor>
            {
                new TeamAdaptiveFeedbackPreferenceAdjuster(new Dictionary<string, double> { ["central"] = 0.88 }),
                new RoundingAdaptiveFeedbackPreferenceAdjuster(0.1),
                new FlaggedAdaptiveFeedbackPreferenceAdjuster(flags, "adaptive-feedback-preference-default", -7.2),
                new CapAdaptiveFeedbackPreferenceAdjuster(49978.0),
                new AdaptiveFeedbackPreferenceScoreOffsetAdjuster(0.78),
                new PercentAdaptiveFeedbackPreferenceAdjuster(15.7),
            },
            new List<string>(),
            2.6,
            20638.6);
    }

    public double Price(AdaptiveFeedbackPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveFeedbackPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
