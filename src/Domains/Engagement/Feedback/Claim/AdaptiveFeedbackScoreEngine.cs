namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive feedback score calculation.</summary>
public interface IAdaptiveFeedbackScoreStep
{
    string Describe();
    double Adjust(AdaptiveFeedbackScore item, double valueValue);
}

public sealed record FlaggedAdaptiveFeedbackScoreAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveFeedbackScoreStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveFeedbackScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryAdaptiveFeedbackScoreAdjuster(Dictionary<string, double> Factors) : IAdaptiveFeedbackScoreStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdaptiveFeedbackScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveFeedbackScoreAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveFeedbackScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveFeedbackScore item, double valueValue)
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

public sealed record MinimumAdaptiveFeedbackScoreAdjuster(double Minimum) : IAdaptiveFeedbackScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveFeedbackScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveFeedbackScoreBudgetOffsetAdjuster(double Weight) : IAdaptiveFeedbackScoreStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(AdaptiveFeedbackScore item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingAdaptiveFeedbackScoreAdjuster(double Step) : IAdaptiveFeedbackScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveFeedbackScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive feedback score steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveFeedbackScoreEngine(List<IAdaptiveFeedbackScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveFeedbackScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveFeedbackScoreEngine DefaultAdaptiveFeedbackScoreEngine(Registry flags)
    {
        return new AdaptiveFeedbackScoreEngine(
            new List<IAdaptiveFeedbackScoreStep>
            {
                new FlaggedAdaptiveFeedbackScoreAdjuster(flags, "adaptive-feedback-score-default", -4.4),
                new CountryAdaptiveFeedbackScoreAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.24, ["east"] = 0.95, ["south"] = 1.39 }),
                new TieredAdaptiveFeedbackScoreAdjuster(
                    new List<double> { 1796.0, 7139.0, 9221.0 },
                    new List<double> { 1.11, 1.12, 0.99 }),
                new MinimumAdaptiveFeedbackScoreAdjuster(35.1),
                new AdaptiveFeedbackScoreBudgetOffsetAdjuster(0.55),
                new RoundingAdaptiveFeedbackScoreAdjuster(5.0),
            },
            new List<string>(),
            2.6,
            69864.2);
    }

    public double Price(AdaptiveFeedbackScore item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveFeedbackScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
