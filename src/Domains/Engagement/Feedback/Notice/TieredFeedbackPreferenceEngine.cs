namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered feedback preference calculation.</summary>
public interface ITieredFeedbackPreferenceStep
{
    string Describe();
    double Adjust(TieredFeedbackPreference item, double valueValue);
}

public sealed record TieredTieredFeedbackPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ITieredFeedbackPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredFeedbackPreference item, double valueValue)
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

public sealed record MinimumTieredFeedbackPreferenceAdjuster(double Minimum) : ITieredFeedbackPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredFeedbackPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTieredFeedbackPreferenceAdjuster(double Percent) : ITieredFeedbackPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredFeedbackPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered feedback preference steps in order and keeps a trace of each.</summary>
public sealed class TieredFeedbackPreferenceEngine(List<ITieredFeedbackPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredFeedbackPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredFeedbackPreferenceEngine DefaultTieredFeedbackPreferenceEngine()
    {
        return new TieredFeedbackPreferenceEngine(
            new List<ITieredFeedbackPreferenceStep>
            {
                new TieredTieredFeedbackPreferenceAdjuster(
                    new List<double> { 1109.0, 5995.0 },
                    new List<double> { 1.06, 1.05 }),
                new MinimumTieredFeedbackPreferenceAdjuster(33.3),
                new PercentTieredFeedbackPreferenceAdjuster(6.5),
            },
            new List<string>(),
            3.3,
            57626.3);
    }

    public double Run(TieredFeedbackPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredFeedbackPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
