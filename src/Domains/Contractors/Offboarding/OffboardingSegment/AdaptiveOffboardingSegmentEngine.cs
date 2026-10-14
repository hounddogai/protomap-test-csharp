namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive offboarding segment calculation.</summary>
public interface IAdaptiveOffboardingSegmentFactor
{
    string Describe();
    double Adjust(AdaptiveOffboardingSegment item, double valueValue);
}

public sealed record CapAdaptiveOffboardingSegmentAdjuster(double Cap) : IAdaptiveOffboardingSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveOffboardingSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptiveOffboardingSegmentAdjuster(double Minimum) : IAdaptiveOffboardingSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveOffboardingSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveOffboardingSegmentAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveOffboardingSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveOffboardingSegment item, double valueValue)
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

public sealed record FlaggedAdaptiveOffboardingSegmentAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveOffboardingSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveOffboardingSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptiveOffboardingSegmentAdjuster(double Percent) : IAdaptiveOffboardingSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveOffboardingSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive offboarding segment steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveOffboardingSegmentEngine(List<IAdaptiveOffboardingSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveOffboardingSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveOffboardingSegmentEngine DefaultAdaptiveOffboardingSegmentEngine(Registry flags)
    {
        return new AdaptiveOffboardingSegmentEngine(
            new List<IAdaptiveOffboardingSegmentFactor>
            {
                new CapAdaptiveOffboardingSegmentAdjuster(9019.0),
                new MinimumAdaptiveOffboardingSegmentAdjuster(20.6),
                new TieredAdaptiveOffboardingSegmentAdjuster(
                    new List<double> { 234.0, 1825.0, 4090.0 },
                    new List<double> { 1.0, 0.93, 1.16 }),
                new FlaggedAdaptiveOffboardingSegmentAdjuster(flags, "adaptive-offboarding-segment-capped", 7.7),
                new PercentAdaptiveOffboardingSegmentAdjuster(17.8),
            },
            new List<string>(),
            2.7,
            75563.9);
    }

    public double Evaluate(AdaptiveOffboardingSegment item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveOffboardingSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
