namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient feedback metric calculation.</summary>
public interface ILenientFeedbackMetricFactor
{
    string Describe();
    double Adjust(LenientFeedbackMetric item, double valueValue);
}

public sealed record CapLenientFeedbackMetricAdjuster(double Cap) : ILenientFeedbackMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientFeedbackMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLenientFeedbackMetricAdjuster(double Minimum) : ILenientFeedbackMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientFeedbackMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLenientFeedbackMetricAdjuster(List<double> Bounds, List<double> Rates) : ILenientFeedbackMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientFeedbackMetric item, double valueValue)
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

public sealed record FlaggedLenientFeedbackMetricAdjuster(Registry Flags, string Flag, double Percent) : ILenientFeedbackMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientFeedbackMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientFeedbackMetricUpdatedAtOffsetAdjuster(double Weight) : ILenientFeedbackMetricFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LenientFeedbackMetric item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentLenientFeedbackMetricAdjuster(double Percent) : ILenientFeedbackMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientFeedbackMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient feedback metric steps in order and keeps a trace of each.</summary>
public sealed class LenientFeedbackMetricEngine(List<ILenientFeedbackMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientFeedbackMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientFeedbackMetricEngine DefaultLenientFeedbackMetricEngine(Registry flags)
    {
        return new LenientFeedbackMetricEngine(
            new List<ILenientFeedbackMetricFactor>
            {
                new CapLenientFeedbackMetricAdjuster(49182.0),
                new MinimumLenientFeedbackMetricAdjuster(18.1),
                new TieredLenientFeedbackMetricAdjuster(
                    new List<double> { 439.0, 2112.0, 5342.0 },
                    new List<double> { 1.03, 1.2, 1.19 }),
                new FlaggedLenientFeedbackMetricAdjuster(flags, "lenient-feedback-metric-weighted", 3.5),
                new LenientFeedbackMetricUpdatedAtOffsetAdjuster(1.94),
                new PercentLenientFeedbackMetricAdjuster(4.3),
            },
            new List<string>(),
            3.3,
            63114.8);
    }

    public double Evaluate(LenientFeedbackMetric item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientFeedbackMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
