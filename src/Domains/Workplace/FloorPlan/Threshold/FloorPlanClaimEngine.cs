namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the floor plan claim calculation.</summary>
public interface IFloorPlanClaimFactor
{
    string Describe();
    double Adjust(FloorPlanClaim item, double valueValue);
}

public sealed record FlaggedFloorPlanClaimAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFloorPlanClaimAdjuster(double Minimum) : IFloorPlanClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFloorPlanClaimAdjuster(double Percent) : IFloorPlanClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanClaimAdjuster(double Cap) : IFloorPlanClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the floor plan claim steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanClaimEngine(List<IFloorPlanClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanClaimEngine DefaultFloorPlanClaimEngine(Registry flags)
    {
        return new FloorPlanClaimEngine(
            new List<IFloorPlanClaimFactor>
            {
                new FlaggedFloorPlanClaimAdjuster(flags, "floor-plan-claim-default", 8.4),
                new MinimumFloorPlanClaimAdjuster(25.2),
                new PercentFloorPlanClaimAdjuster(-1.7),
                new CapFloorPlanClaimAdjuster(21092.0),
            },
            new List<string>(),
            0.5,
            56481.9);
    }

    public double Compute(FloorPlanClaim item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
