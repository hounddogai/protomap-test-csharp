namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive position profile calculation.</summary>
public interface IAdaptivePositionProfileAdjuster
{
    string Describe();
    double Adjust(AdaptivePositionProfile item, double valueValue);
}

public sealed record CapAdaptivePositionProfileAdjuster(double Cap) : IAdaptivePositionProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePositionProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptivePositionProfileAdjuster(double Minimum) : IAdaptivePositionProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePositionProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitAdaptivePositionProfileAdjuster(Dictionary<string, double> Factors) : IAdaptivePositionProfileAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AdaptivePositionProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdaptivePositionProfileThresholdOffsetAdjuster(double Weight) : IAdaptivePositionProfileAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(AdaptivePositionProfile item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedAdaptivePositionProfileAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePositionProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePositionProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptivePositionProfileAdjuster(double Percent) : IAdaptivePositionProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePositionProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive position profile steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePositionProfileEngine(List<IAdaptivePositionProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePositionProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePositionProfileEngine DefaultAdaptivePositionProfileEngine(Registry flags)
    {
        return new AdaptivePositionProfileEngine(
            new List<IAdaptivePositionProfileAdjuster>
            {
                new CapAdaptivePositionProfileAdjuster(28954.0),
                new MinimumAdaptivePositionProfileAdjuster(31.0),
                new UnitAdaptivePositionProfileAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.39, ["west"] = 1.26 }),
                new AdaptivePositionProfileThresholdOffsetAdjuster(0.77),
                new FlaggedAdaptivePositionProfileAdjuster(flags, "adaptive-position-profile-adaptive", 4.0),
                new PercentAdaptivePositionProfileAdjuster(-3.5),
            },
            new List<string>(),
            3.8,
            81692.9);
    }

    public double Evaluate(AdaptivePositionProfile item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePositionProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
