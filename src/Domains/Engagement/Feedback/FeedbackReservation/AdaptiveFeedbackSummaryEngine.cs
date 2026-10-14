namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive feedback summary calculation.</summary>
public interface IAdaptiveFeedbackSummaryStep
{
    string Describe();
    double Adjust(AdaptiveFeedbackSummary item, double valueValue);
}

public sealed record RoundingAdaptiveFeedbackSummaryAdjuster(double Step) : IAdaptiveFeedbackSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveFeedbackSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveFeedbackSummaryAdjuster(double Percent) : IAdaptiveFeedbackSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveFeedbackSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveFeedbackSummaryAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveFeedbackSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveFeedbackSummary item, double valueValue)
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

public sealed record MinimumAdaptiveFeedbackSummaryAdjuster(double Minimum) : IAdaptiveFeedbackSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveFeedbackSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive feedback summary steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveFeedbackSummaryEngine(List<IAdaptiveFeedbackSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveFeedbackSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveFeedbackSummaryEngine DefaultAdaptiveFeedbackSummaryEngine()
    {
        return new AdaptiveFeedbackSummaryEngine(
            new List<IAdaptiveFeedbackSummaryStep>
            {
                new RoundingAdaptiveFeedbackSummaryAdjuster(0.5),
                new PercentAdaptiveFeedbackSummaryAdjuster(16.4),
                new TieredAdaptiveFeedbackSummaryAdjuster(
                    new List<double> { 1486.0, 4377.0, 5022.0 },
                    new List<double> { 0.8, 1.06, 1.12 }),
                new MinimumAdaptiveFeedbackSummaryAdjuster(6.3),
            },
            new List<string>(),
            0.2,
            53697.9);
    }

    public double Compute(AdaptiveFeedbackSummary item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveFeedbackSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
