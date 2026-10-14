namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding segment calculation.</summary>
public interface IOffboardingSegmentStep
{
    string Describe();
    double Adjust(OffboardingSegment item, double valueValue);
}

public sealed record CapOffboardingSegmentAdjuster(double Cap) : IOffboardingSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameOffboardingSegmentAdjuster(Dictionary<string, double> Factors) : IOffboardingSegmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(OffboardingSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumOffboardingSegmentAdjuster(double Minimum) : IOffboardingSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOffboardingSegmentAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding segment steps in order and keeps a trace of each.</summary>
public sealed class OffboardingSegmentEngine(List<IOffboardingSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingSegmentEngine DefaultOffboardingSegmentEngine(Registry flags)
    {
        return new OffboardingSegmentEngine(
            new List<IOffboardingSegmentStep>
            {
                new CapOffboardingSegmentAdjuster(28114.0),
                new NameOffboardingSegmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.71, ["south"] = 1.06 }),
                new MinimumOffboardingSegmentAdjuster(40.9),
                new FlaggedOffboardingSegmentAdjuster(flags, "offboarding-segment-lenient", 6.9),
            },
            new List<string>(),
            4.7,
            9881.9);
    }

    public double Run(OffboardingSegment item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
