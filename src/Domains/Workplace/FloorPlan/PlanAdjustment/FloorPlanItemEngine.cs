namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan item calculation.</summary>
public interface IFloorPlanItemAdjuster
{
    string Describe();
    double Adjust(FloorPlanItem item, double valueValue);
}

public sealed record CapFloorPlanItemAdjuster(double Cap) : IFloorPlanItemAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFloorPlanItemAdjuster(double Step) : IFloorPlanItemAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedFloorPlanItemAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanItemAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan item steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanItemEngine(List<IFloorPlanItemAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanItemAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanItemEngine DefaultFloorPlanItemEngine(Registry flags)
    {
        return new FloorPlanItemEngine(
            new List<IFloorPlanItemAdjuster>
            {
                new CapFloorPlanItemAdjuster(27142.0),
                new RoundingFloorPlanItemAdjuster(5.0),
                new FlaggedFloorPlanItemAdjuster(flags, "floor-plan-item-regional", 1.5),
            },
            new List<string>(),
            3.2,
            74996.8);
    }

    public double Run(FloorPlanItem item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
