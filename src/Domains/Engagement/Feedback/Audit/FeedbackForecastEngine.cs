namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback forecast calculation.</summary>
public interface IFeedbackForecastModifier
{
    string Describe();
    double Adjust(FeedbackForecast item, double valueValue);
}

public sealed record LabelFeedbackForecastAdjuster(Dictionary<string, double> Factors) : IFeedbackForecastModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FeedbackForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFeedbackForecastAdjuster(double Step) : IFeedbackForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFeedbackForecastAdjuster(double Percent) : IFeedbackForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFeedbackForecastAdjuster(double Minimum) : IFeedbackForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFeedbackForecastAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFeedbackForecastAdjuster(double Cap) : IFeedbackForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the feedback forecast steps in order and keeps a trace of each.</summary>
public sealed class FeedbackForecastEngine(List<IFeedbackForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackForecastEngine DefaultFeedbackForecastEngine(Registry flags)
    {
        return new FeedbackForecastEngine(
            new List<IFeedbackForecastModifier>
            {
                new LabelFeedbackForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.8, ["east"] = 1.03, ["online"] = 1.28 }),
                new RoundingFeedbackForecastAdjuster(0.5),
                new PercentFeedbackForecastAdjuster(6.7),
                new MinimumFeedbackForecastAdjuster(30.5),
                new FlaggedFeedbackForecastAdjuster(flags, "feedback-forecast-tiered", 1.6),
                new CapFeedbackForecastAdjuster(4283.0),
            },
            new List<string>(),
            1.2,
            81829.3);
    }

    public double Price(FeedbackForecast item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
