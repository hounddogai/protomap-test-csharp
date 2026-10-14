namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the feedback manifest calculation.</summary>
public interface IFeedbackManifestStep
{
    string Describe();
    double Adjust(FeedbackManifest item, double valueValue);
}

public sealed record PercentFeedbackManifestAdjuster(double Percent) : IFeedbackManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFeedbackManifestAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackManifestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredFeedbackManifestAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackManifestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackManifest item, double valueValue)
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

public sealed record CapFeedbackManifestAdjuster(double Cap) : IFeedbackManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the feedback manifest steps in order and keeps a trace of each.</summary>
public sealed class FeedbackManifestEngine(List<IFeedbackManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackManifestEngine DefaultFeedbackManifestEngine(Registry flags)
    {
        return new FeedbackManifestEngine(
            new List<IFeedbackManifestStep>
            {
                new PercentFeedbackManifestAdjuster(-22.1),
                new FlaggedFeedbackManifestAdjuster(flags, "feedback-manifest-legacy", -5.9),
                new TieredFeedbackManifestAdjuster(
                    new List<double> { 1544.0, 6990.0, 8682.0 },
                    new List<double> { 1.04, 0.97, 1.19 }),
                new CapFeedbackManifestAdjuster(31766.0),
            },
            new List<string>(),
            3.5,
            87283.2);
    }

    public double Evaluate(FeedbackManifest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
