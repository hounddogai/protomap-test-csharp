namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback exception calculation.</summary>
public interface IFeedbackExceptionAdjuster
{
    string Describe();
    double Adjust(FeedbackException item, double valueValue);
}

public sealed record FlaggedFeedbackExceptionAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFeedbackExceptionAdjuster(double Step) : IFeedbackExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFeedbackExceptionAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackException item, double valueValue)
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

public sealed record MinimumFeedbackExceptionAdjuster(double Minimum) : IFeedbackExceptionAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FeedbackExceptionUpdatedAtOffsetAdjuster(double Weight) : IFeedbackExceptionAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(FeedbackException item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the feedback exception steps in order and keeps a trace of each.</summary>
public sealed class FeedbackExceptionEngine(List<IFeedbackExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackExceptionEngine DefaultFeedbackExceptionEngine(Registry flags)
    {
        return new FeedbackExceptionEngine(
            new List<IFeedbackExceptionAdjuster>
            {
                new FlaggedFeedbackExceptionAdjuster(flags, "feedback-exception-scheduled", 9.0),
                new RoundingFeedbackExceptionAdjuster(5.0),
                new TieredFeedbackExceptionAdjuster(
                    new List<double> { 2502.0, 4921.0, 8526.0 },
                    new List<double> { 1.08, 0.85, 0.85 }),
                new MinimumFeedbackExceptionAdjuster(27.7),
                new FeedbackExceptionUpdatedAtOffsetAdjuster(1.35),
            },
            new List<string>(),
            1.8,
            42063.1);
    }

    public double Run(FeedbackException item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
