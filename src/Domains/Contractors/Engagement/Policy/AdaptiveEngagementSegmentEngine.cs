namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive engagement segment calculation.</summary>
public interface IAdaptiveEngagementSegmentAdjuster
{
    string Describe();
    double Adjust(AdaptiveEngagementSegment item, double valueValue);
}

public sealed record PercentAdaptiveEngagementSegmentAdjuster(double Percent) : IAdaptiveEngagementSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveEngagementSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveEngagementSegmentAdjuster(double Cap) : IAdaptiveEngagementSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveEngagementSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveEngagementSegmentAdjuster(double Step) : IAdaptiveEngagementSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveEngagementSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive engagement segment steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveEngagementSegmentEngine(List<IAdaptiveEngagementSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveEngagementSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveEngagementSegmentEngine DefaultAdaptiveEngagementSegmentEngine()
    {
        return new AdaptiveEngagementSegmentEngine(
            new List<IAdaptiveEngagementSegmentAdjuster>
            {
                new PercentAdaptiveEngagementSegmentAdjuster(-16.4),
                new CapAdaptiveEngagementSegmentAdjuster(12984.0),
                new RoundingAdaptiveEngagementSegmentAdjuster(5.0),
            },
            new List<string>(),
            0.3,
            94853.9);
    }

    public double Run(AdaptiveEngagementSegment item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveEngagementSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
