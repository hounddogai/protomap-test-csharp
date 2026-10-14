namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback feedback reservation calculation.</summary>
public interface IFallbackFeedbackReservationStep
{
    string Describe();
    double Adjust(FallbackFeedbackReservation item, double valueValue);
}

public sealed record FlaggedFallbackFeedbackReservationAdjuster(Registry Flags, string Flag, double Percent) : IFallbackFeedbackReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackFeedbackReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredFallbackFeedbackReservationAdjuster(List<double> Bounds, List<double> Rates) : IFallbackFeedbackReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackFeedbackReservation item, double valueValue)
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

public sealed record FallbackFeedbackReservationWindowDaysOffsetAdjuster(double Weight) : IFallbackFeedbackReservationStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(FallbackFeedbackReservation item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record PercentFallbackFeedbackReservationAdjuster(double Percent) : IFallbackFeedbackReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackFeedbackReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackFeedbackReservationAdjuster(double Step) : IFallbackFeedbackReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackFeedbackReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback feedback reservation steps in order and keeps a trace of each.</summary>
public sealed class FallbackFeedbackReservationEngine(List<IFallbackFeedbackReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackFeedbackReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackFeedbackReservationEngine DefaultFallbackFeedbackReservationEngine(Registry flags)
    {
        return new FallbackFeedbackReservationEngine(
            new List<IFallbackFeedbackReservationStep>
            {
                new FlaggedFallbackFeedbackReservationAdjuster(flags, "fallback-feedback-reservation-tiered", -2.0),
                new TieredFallbackFeedbackReservationAdjuster(
                    new List<double> { 312.0, 623.0, 3855.0, 6163.0 },
                    new List<double> { 0.98, 1.05, 0.91, 0.97 }),
                new FallbackFeedbackReservationWindowDaysOffsetAdjuster(0.39),
                new PercentFallbackFeedbackReservationAdjuster(-8.9),
                new RoundingFallbackFeedbackReservationAdjuster(5.0),
            },
            new List<string>(),
            2.5,
            16429.2);
    }

    public double Evaluate(FallbackFeedbackReservation item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackFeedbackReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
