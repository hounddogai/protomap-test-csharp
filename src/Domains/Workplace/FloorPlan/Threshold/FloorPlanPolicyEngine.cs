namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the floor plan policy calculation.</summary>
public interface IFloorPlanPolicyFactor
{
    string Describe();
    double Adjust(FloorPlanPolicy item, double valueValue);
}

public sealed record MinimumFloorPlanPolicyAdjuster(double Minimum) : IFloorPlanPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FloorPlanPolicyRetriesOffsetAdjuster(double Weight) : IFloorPlanPolicyFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(FloorPlanPolicy item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedFloorPlanPolicyAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan policy steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanPolicyEngine(List<IFloorPlanPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanPolicyEngine DefaultFloorPlanPolicyEngine(Registry flags)
    {
        return new FloorPlanPolicyEngine(
            new List<IFloorPlanPolicyFactor>
            {
                new MinimumFloorPlanPolicyAdjuster(11.1),
                new FloorPlanPolicyRetriesOffsetAdjuster(0.09),
                new FlaggedFloorPlanPolicyAdjuster(flags, "floor-plan-policy-default", 4.0),
            },
            new List<string>(),
            2.9,
            90730.9);
    }

    public double Evaluate(FloorPlanPolicy item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
