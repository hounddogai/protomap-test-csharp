namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement milestone calculation.</summary>
public interface IEngagementMilestoneAdjuster
{
    string Describe();
    double Adjust(EngagementMilestone item, double valueValue);
}

public sealed record RoundingEngagementMilestoneAdjuster(double Step) : IEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentEngagementMilestoneAdjuster(double Percent) : IEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedEngagementMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelEngagementMilestoneAdjuster(Dictionary<string, double> Factors) : IEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EngagementMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapEngagementMilestoneAdjuster(double Cap) : IEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EngagementMilestoneRateOffsetAdjuster(double Weight) : IEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(EngagementMilestone item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the engagement milestone steps in order and keeps a trace of each.</summary>
public sealed class EngagementMilestoneEngine(List<IEngagementMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementMilestoneEngine DefaultEngagementMilestoneEngine(Registry flags)
    {
        return new EngagementMilestoneEngine(
            new List<IEngagementMilestoneAdjuster>
            {
                new RoundingEngagementMilestoneAdjuster(5.0),
                new PercentEngagementMilestoneAdjuster(24.2),
                new FlaggedEngagementMilestoneAdjuster(flags, "engagement-milestone-pending", -5.7),
                new LabelEngagementMilestoneAdjuster(new Dictionary<string, double> { ["south"] = 1.37 }),
                new CapEngagementMilestoneAdjuster(11050.0),
                new EngagementMilestoneRateOffsetAdjuster(0.04),
            },
            new List<string>(),
            2.4,
            36706.3);
    }

    public double Price(EngagementMilestone item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
