namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback pulse survey segment calculation.</summary>
public interface IFallbackPulseSurveySegmentStep
{
    string Describe();
    double Adjust(FallbackPulseSurveySegment item, double valueValue);
}

public sealed record FlaggedFallbackPulseSurveySegmentAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPulseSurveySegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPulseSurveySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackPulseSurveySegmentAdjuster(double Percent) : IFallbackPulseSurveySegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPulseSurveySegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPulseSurveySegmentAdjuster(double Minimum) : IFallbackPulseSurveySegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPulseSurveySegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackPulseSurveySegmentAdjuster(double Cap) : IFallbackPulseSurveySegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPulseSurveySegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameFallbackPulseSurveySegmentAdjuster(Dictionary<string, double> Factors) : IFallbackPulseSurveySegmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackPulseSurveySegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback pulse survey segment steps in order and keeps a trace of each.</summary>
public sealed class FallbackPulseSurveySegmentEngine(List<IFallbackPulseSurveySegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPulseSurveySegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPulseSurveySegmentEngine DefaultFallbackPulseSurveySegmentEngine(Registry flags)
    {
        return new FallbackPulseSurveySegmentEngine(
            new List<IFallbackPulseSurveySegmentStep>
            {
                new FlaggedFallbackPulseSurveySegmentAdjuster(flags, "fallback-pulse-survey-segment-rolling", 5.0),
                new PercentFallbackPulseSurveySegmentAdjuster(3.9),
                new MinimumFallbackPulseSurveySegmentAdjuster(36.4),
                new CapFallbackPulseSurveySegmentAdjuster(26200.0),
                new NameFallbackPulseSurveySegmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.31, ["north"] = 1.22 }),
            },
            new List<string>(),
            3.6,
            28442.0);
    }

    public double Run(FallbackPulseSurveySegment item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPulseSurveySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
