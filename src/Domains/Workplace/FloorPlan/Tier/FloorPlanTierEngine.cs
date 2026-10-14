namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the floor plan tier calculation.</summary>
public interface IFloorPlanTierModifier
{
    string Describe();
    double Adjust(FloorPlanTier item, double valueValue);
}

public sealed record FlaggedFloorPlanTierAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFloorPlanTierAdjuster(double Percent) : IFloorPlanTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameFloorPlanTierAdjuster(Dictionary<string, double> Factors) : IFloorPlanTierModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FloorPlanTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the floor plan tier steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanTierEngine(List<IFloorPlanTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanTierEngine DefaultFloorPlanTierEngine(Registry flags)
    {
        return new FloorPlanTierEngine(
            new List<IFloorPlanTierModifier>
            {
                new FlaggedFloorPlanTierAdjuster(flags, "floor-plan-tier-default", 1.3),
                new PercentFloorPlanTierAdjuster(3.0),
                new NameFloorPlanTierAdjuster(new Dictionary<string, double> { ["retail"] = 1.35 }),
            },
            new List<string>(),
            3.0,
            5416.7);
    }

    public double Run(FloorPlanTier item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
