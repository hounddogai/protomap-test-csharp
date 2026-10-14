namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback reservation calculation.</summary>
public interface IFeedbackReservationStep
{
    string Describe();
    double Adjust(FeedbackReservation item, double valueValue);
}

public sealed record PercentFeedbackReservationAdjuster(double Percent) : IFeedbackReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFeedbackReservationAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FeedbackReservationMaximumOffsetAdjuster(double Weight) : IFeedbackReservationStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(FeedbackReservation item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record LabelFeedbackReservationAdjuster(Dictionary<string, double> Factors) : IFeedbackReservationStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FeedbackReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFeedbackReservationAdjuster(double Step) : IFeedbackReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFeedbackReservationAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackReservation item, double valueValue)
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

/// <summary>Runs the feedback reservation steps in order and keeps a trace of each.</summary>
public sealed class FeedbackReservationEngine(List<IFeedbackReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackReservationEngine DefaultFeedbackReservationEngine(Registry flags)
    {
        return new FeedbackReservationEngine(
            new List<IFeedbackReservationStep>
            {
                new PercentFeedbackReservationAdjuster(6.4),
                new FlaggedFeedbackReservationAdjuster(flags, "feedback-reservation-batched", -4.1),
                new FeedbackReservationMaximumOffsetAdjuster(0.53),
                new LabelFeedbackReservationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.07, ["south"] = 1.09, ["east"] = 1.08 }),
                new RoundingFeedbackReservationAdjuster(0.05),
                new TieredFeedbackReservationAdjuster(
                    new List<double> { 550.0, 7692.0 },
                    new List<double> { 1.1, 0.8 }),
            },
            new List<string>(),
            4.1,
            56252.5);
    }

    public double Evaluate(FeedbackReservation item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
