namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling floor plan rollup calculation.</summary>
public interface IRollingFloorPlanRollupModifier
{
    string Describe();
    double Adjust(RollingFloorPlanRollup item, double valueValue);
}

public sealed record NameRollingFloorPlanRollupAdjuster(Dictionary<string, double> Factors) : IRollingFloorPlanRollupModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingFloorPlanRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRollingFloorPlanRollupAdjuster(Registry Flags, string Flag, double Percent) : IRollingFloorPlanRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingFloorPlanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingFloorPlanRollupAdjuster(double Cap) : IRollingFloorPlanRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingFloorPlanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling floor plan rollup steps in order and keeps a trace of each.</summary>
public sealed class RollingFloorPlanRollupEngine(List<IRollingFloorPlanRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFloorPlanRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFloorPlanRollupEngine DefaultRollingFloorPlanRollupEngine(Registry flags)
    {
        return new RollingFloorPlanRollupEngine(
            new List<IRollingFloorPlanRollupModifier>
            {
                new NameRollingFloorPlanRollupAdjuster(new Dictionary<string, double> { ["online"] = 1.2 }),
                new FlaggedRollingFloorPlanRollupAdjuster(flags, "rolling-floor-plan-rollup-weighted", -2.8),
                new CapRollingFloorPlanRollupAdjuster(20068.0),
            },
            new List<string>(),
            2.5,
            30173.5);
    }

    public double Evaluate(RollingFloorPlanRollup item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFloorPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
