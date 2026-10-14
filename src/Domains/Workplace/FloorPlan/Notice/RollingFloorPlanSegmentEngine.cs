namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling floor plan segment calculation.</summary>
public interface IRollingFloorPlanSegmentAdjuster
{
    string Describe();
    double Adjust(RollingFloorPlanSegment item, double valueValue);
}

public sealed record RoundingRollingFloorPlanSegmentAdjuster(double Step) : IRollingFloorPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingFloorPlanSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameRollingFloorPlanSegmentAdjuster(Dictionary<string, double> Factors) : IRollingFloorPlanSegmentAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingFloorPlanSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRollingFloorPlanSegmentAdjuster(double Minimum) : IRollingFloorPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingFloorPlanSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRollingFloorPlanSegmentAdjuster(List<double> Bounds, List<double> Rates) : IRollingFloorPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingFloorPlanSegment item, double valueValue)
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

public sealed record FlaggedRollingFloorPlanSegmentAdjuster(Registry Flags, string Flag, double Percent) : IRollingFloorPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingFloorPlanSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling floor plan segment steps in order and keeps a trace of each.</summary>
public sealed class RollingFloorPlanSegmentEngine(List<IRollingFloorPlanSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFloorPlanSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFloorPlanSegmentEngine DefaultRollingFloorPlanSegmentEngine(Registry flags)
    {
        return new RollingFloorPlanSegmentEngine(
            new List<IRollingFloorPlanSegmentAdjuster>
            {
                new RoundingRollingFloorPlanSegmentAdjuster(5.0),
                new NameRollingFloorPlanSegmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.25, ["retail"] = 1.29, ["east"] = 1.07 }),
                new MinimumRollingFloorPlanSegmentAdjuster(45.3),
                new TieredRollingFloorPlanSegmentAdjuster(
                    new List<double> { 5100.0, 5765.0, 7917.0 },
                    new List<double> { 1.12, 0.91, 0.84 }),
                new FlaggedRollingFloorPlanSegmentAdjuster(flags, "rolling-floor-plan-segment-tiered", 9.4),
            },
            new List<string>(),
            4.8,
            55737.5);
    }

    public double Run(RollingFloorPlanSegment item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFloorPlanSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
