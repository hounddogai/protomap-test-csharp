namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy visitor milestone calculation.</summary>
public interface ILegacyVisitorMilestoneAdjuster
{
    string Describe();
    double Adjust(LegacyVisitorMilestone item, double valueValue);
}

public sealed record FlaggedLegacyVisitorMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ILegacyVisitorMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyVisitorMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyVisitorMilestoneAdjuster(double Cap) : ILegacyVisitorMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyVisitorMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitLegacyVisitorMilestoneAdjuster(Dictionary<string, double> Factors) : ILegacyVisitorMilestoneAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LegacyVisitorMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacyVisitorMilestoneThresholdOffsetAdjuster(double Weight) : ILegacyVisitorMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(LegacyVisitorMilestone item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record MinimumLegacyVisitorMilestoneAdjuster(double Minimum) : ILegacyVisitorMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyVisitorMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy visitor milestone steps in order and keeps a trace of each.</summary>
public sealed class LegacyVisitorMilestoneEngine(List<ILegacyVisitorMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyVisitorMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyVisitorMilestoneEngine DefaultLegacyVisitorMilestoneEngine(Registry flags)
    {
        return new LegacyVisitorMilestoneEngine(
            new List<ILegacyVisitorMilestoneAdjuster>
            {
                new FlaggedLegacyVisitorMilestoneAdjuster(flags, "legacy-visitor-milestone-tiered", 5.1),
                new CapLegacyVisitorMilestoneAdjuster(43491.0),
                new UnitLegacyVisitorMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.86, ["retail"] = 1.38 }),
                new LegacyVisitorMilestoneThresholdOffsetAdjuster(1.14),
                new MinimumLegacyVisitorMilestoneAdjuster(42.9),
            },
            new List<string>(),
            0.8,
            21396.4);
    }

    public double Price(LegacyVisitorMilestone item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyVisitorMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
